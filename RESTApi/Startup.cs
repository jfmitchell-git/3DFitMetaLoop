using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dryginstudios.bioinc.meta;
using MetaLoop.Common.DataEngine;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoop.Common.PlatformCommon.Settings;
using MetaLoop.Common.PlayFabWrapper;
using MetaLoop.GameLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MetaLoop.RESTApi
{
    public class Startup
    {
        public static DateTime UpTimeStart;
        public static string InstanceId;
        public static string AppVersion;
        public static List<ApiErrorDetail> ApiErrors = new List<ApiErrorDetail>();
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            PlayFabApiHandler.UseEntityFiles = true;

            //Required to force assembly loading order. 
            _MetaStateSettings.Init();

            InstanceId = Guid.NewGuid().ToString();

            //Initialize MetaLoop methods reflection engine. 
            ApiController.Init();

            //Init DataLayer.
            var rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DataLayer.Instance.Init(rootDir + @"\" + MetaStateSettings._DatabaseName);

            var appVersionFileName = rootDir + @"\AppVersion.txt";

            if (System.IO.File.Exists(appVersionFileName))
            {
                AppVersion = System.IO.File.ReadAllText(appVersionFileName);
            }
            else
            {
                throw new Exception("Mission AppVersion.txt file.");
            }

            //Init CloudTable if any.



            //Set PlayFab settings.
            PlayFab.PlayFabSettings.staticSettings.TitleId = MetaLoop.Configuration.PlayFabSettings.TitleId;
            PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey = MetaLoop.Configuration.PlayFabSettings.DeveloperSecretKey;

            //Statistics
            UpTimeStart = DateTime.UtcNow;

            ShopManager.Init();


        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "application/json; charset=utf-8";
                    var exception = context.Features.Get<IExceptionHandlerPathFeature>().Error;
                 
                    var newError = new ApiErrorDetail(exception);
                    if (ApiErrors.Count > 50) Startup.ApiErrors.Remove(Startup.ApiErrors.LastOrDefault());
                    ApiErrors.Insert(0, newError);

                    CloudScriptResponse statusResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Error, Method = context.Request.Path, ErrorMessage = exception.Message };
                    statusResponse.Params.Add("InstanceId", Startup.InstanceId);
                    string output = JsonConvert.SerializeObject(statusResponse);
                    await context.Response.WriteAsync(output);
               
                });
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
