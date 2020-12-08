using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi
{
    public class PlayFabApiMethodBase : IPlayFabApiMethod
    {
        public bool IsClientValidRequest(CloudScriptRequest r)
        {
            return !string.IsNullOrEmpty(r.UserId) || !string.IsNullOrEmpty(r.EntityId);
        }


        public bool IsClientValidRequest(CloudScriptRequestStack r)
        {
            return !string.IsNullOrEmpty(r.UserId) || !string.IsNullOrEmpty(r.EntityId);
        }

        public virtual async Task<CloudScriptResponse> ExecuteAsync(CloudScriptRequest request, string[] urlArguments)
        {
            throw new NotImplementedException();
        }

        public virtual async Task<CloudScriptResponse> ExecuteStackAsync(CloudScriptRequestStack request)
        {
            throw new NotImplementedException();
        }

        public virtual CloudScriptResponse ExecuteStack(CloudScriptRequestStack request)
        {
            throw new NotImplementedException();
        }

        public virtual CloudScriptResponse Execute(CloudScriptRequest request, string[] urlArguments)
        {
            throw new NotImplementedException();
        }

    }
}
