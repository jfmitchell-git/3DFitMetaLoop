﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoop.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MetaLoop.RESTApi.Controllers
{
    [Route("api/{method}/{param1}/{param2}")]
    [Route("api/{method}/{param1}")]
    [Route("api/{method}")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        [HttpPost]
        public async Task<string> Get(string method, string param1, string param2)
        {
            await PlayFabApiHandler.ValidateEntityToken();

            //Response.ContentType = "application/json; charset=utf-8";

            string result = string.Empty;

            bool isStackCall = param1 != null && param1.ToLower() == "stack";

            switch (method)
            {
                case "":
                case "Status":

                    CloudScriptResponse statusResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Success, Method = method };
                    statusResponse.Params.Add("DataVersion", DataLayer.Instance.GetTable<DataVersion>().First().Version.ToString());
                    statusResponse.Params.Add("Environment", PlayFabSettings.PlayFabEnvironment);
                    statusResponse.Params.Add("UptimeMinutes", Convert.ToUInt32((DateTime.UtcNow - Startup.UpTimeStart).TotalMinutes).ToString());
                    statusResponse.Params.Add("InstanceId", Startup.InstanceId);
                    statusResponse.Params.Add("InstanceId2", Startup.InstanceId);
                    return JsonConvert.SerializeObject(statusResponse);

                case "ResetPlayerData":

                    CloudScriptResponse resetDataResponse;

                    //TODO: Implement ResetPlayerData

                    resetDataResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Error, ErrorMessage = string.Format("Reset data for user {0} failed.", param1), Method = method };

                    result = JsonConvert.SerializeObject(resetDataResponse);

                    break;


                default:

                    CloudScriptResponse response = null;

                    if (ApiController.ApiMethods.ContainsKey(method))
                    {

                        string jsonRequest;
                        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                        {
                            jsonRequest = await reader.ReadToEndAsync();
                        }

                  

                        if (isStackCall)
                        {
                            CloudScriptRequestStack requests = JsonConvert.DeserializeObject<CloudScriptRequestStack>(jsonRequest);
                            response = ApiController.ApiMethods[method].ExecuteStack(requests);
                        }
                        else
                        {
                            CloudScriptRequest request = JsonConvert.DeserializeObject<CloudScriptRequest>(jsonRequest);
                            response = ApiController.ApiMethods[method].Execute(request, new string[] { param1, param2 });
                        }

                    }
                    else
                    {
                        response = new CloudScriptResponse() { ResponseCode = ResponseCode.ProtocolError, ErrorMessage = "Invalid method.", Method = method };
                    }

                    result = JsonConvert.SerializeObject(response);

                    break;

            }

            return result;
        }

    }
}