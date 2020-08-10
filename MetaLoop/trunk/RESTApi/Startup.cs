using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoopDemo.Meta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MetaLoop.RESTApi
{
    public class Startup
    {
        public static DateTime UpTimeStart;
        public static string InstanceId;
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //Required to force assembly loading order. 
            new MetaSettings().InitForReflection();

            InstanceId = Guid.NewGuid().ToString();

            //Initialize MetaLoop methods reflection engine. 
            ApiController.Init();

            //Init DataLayer.
            var rootDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            DataLayer.Instance.Init(rootDir + @"\" + MetaSettings.DatabaseName);


            //Init CloudTable if any.



            //Set PlayFab settings.
            PlayFab.PlayFabSettings.staticSettings.TitleId = MetaLoop.Configuration.PlayFabSettings.TitleId;
            PlayFab.PlayFabSettings.staticSettings.DeveloperSecretKey = MetaLoop.Configuration.PlayFabSettings.DeveloperSecretKey;

            //Statistics
            UpTimeStart = DateTime.UtcNow;


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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
