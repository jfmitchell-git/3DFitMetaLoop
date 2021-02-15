#if BACKOFFICE

using MetaLoop.Common.PlatformCommon.PlayFabClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaLoop.Common.PlatformCommon.Server
{
    public interface IPlayFabApiMethod
    {
        CloudScriptResponse Execute(CloudScriptRequest request, string[] urlArguments);
        CloudScriptResponse ExecuteStack(CloudScriptRequestStack request);


        Task<CloudScriptResponse> ExecuteStackAsync(CloudScriptRequestStack request);
        Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments);
    }

}

#endif
