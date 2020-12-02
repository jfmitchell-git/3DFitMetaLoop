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
        public bool IsClientValidRequest
        {
            get
            {

#if DEBUG
                //CurrentUserId = "E5FFC164A1183E0E";
                //return true;
#endif
                return !string.IsNullOrEmpty(CurrentUserId) || !string.IsNullOrEmpty(CurrentEntity);
            }
        }
        public string CurrentUserId { get; set; }
        public string CurrentEntity { get; set; }

        

        public void LoadContext(CloudScriptRequest r)
        {
            if (r != null)
            {
                CurrentUserId = r.UserId;
                CurrentEntity = r.EntityId;
            }
        }

        public void LoadContext(CloudScriptRequestStack r)
        {
            if (r != null)
            {
                CurrentUserId = r.UserId;
                CurrentEntity = r.EntityId;
            }
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
