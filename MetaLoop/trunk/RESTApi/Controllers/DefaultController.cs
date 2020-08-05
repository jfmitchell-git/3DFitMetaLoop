using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public string Get(string method, string param1, string param2)
        {
            return "";
            //Response.ContentType = "application/json; charset=utf-8";

            //bool stack = false;
            //string functionId = method;

            //if (functionId.IndexOf("/stack") >= 0)
            //{
            //    stack = true;
            //    functionId = functionId.Replace("/stack", string.Empty);
            //}

            //string[] urlParams = null;

            //if (functionId.IndexOf(@"/") > 0)
            //{
            //    var allParams = functionId.Split('/').ToList();
            //    allParams.RemoveAt(0);

            //    urlParams = allParams.ToArray();
            //    functionId = functionId.Split('/')[0];
            //}


            //switch (functionId)
            //{
            //    case "":
            //    case "Status":

            //        bool showStatus = true;
            //        CloudScriptResponse statusResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Success, Method = functionId };
            //        statusResponse.Params.Add("DataVersion", DataLayer.Instance.GetTable<DataVersion>().First().Version.ToString());
            //        statusResponse.Params.Add("Environment", PlayFabSettings.PlayFabEnvironment);
            //        statusResponse.Params.Add("UptimeMinutes", Convert.ToUInt32((DateTime.UtcNow - Global.UpTimeStart).TotalMinutes).ToString());
            //        statusResponse.Params.Add("InstanceId", Global.InstanceId);
            //        return JsonConvert.SerializeObject(statusResponse);


            //    case "ResetPlayerData":

            //        CloudScriptResponse resetDataResponse;

            //        if (ResetPlayerData(urlParams[0]))
            //        {
            //            resetDataResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Success, ErrorMessage = "", Method = functionId };
            //        }
            //        else
            //        {
            //            resetDataResponse = new CloudScriptResponse() { ResponseCode = ResponseCode.Error, ErrorMessage = string.Format("Reset data for user {0} failed.", urlParams[0]), Method = functionId };
            //        }

            //        Response.Write(JsonConvert.SerializeObject(resetDataResponse));

            //        break;


            //    default:

            //        CloudScriptResponse response = null;

            //        if (ApiController.ApiMethods.ContainsKey(functionId))
            //        {
            //            StreamReader stream = new StreamReader(Request.InputStream);
            //            string jsonRequest = stream.ReadToEnd();

            //            if (stack)
            //            {
            //                CloudScriptRequestStack requests = JsonConvert.DeserializeObject<CloudScriptRequestStack>(jsonRequest);
            //                response = ApiController.ApiMethods[functionId].ExecuteStack(requests);
            //            }
            //            else
            //            {

            //                CloudScriptRequest request = JsonConvert.DeserializeObject<CloudScriptRequest>(jsonRequest);
            //                response = ApiController.ApiMethods[functionId].Execute(request, urlParams);
            //            }

            //        }
            //        else
            //        {
            //            response = new CloudScriptResponse() { ResponseCode = ResponseCode.ProtocolError, ErrorMessage = "Invalid method.", Method = functionId };
            //        }

            //        Response.Write(JsonConvert.SerializeObject(response));

            //        break;

            //}


            //Response.End();

        }

    }
}