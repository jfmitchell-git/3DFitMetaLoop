using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.PlayFabClient;
using MetaLoop.Common.PlatformCommon.Server;
using MetaLoopDemo.Meta;
using MetaLoopDemo.Meta.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetaLoop.RESTApi.ApiMethods
{
    public class UpgradeTier : IPlayFabApiMethod
    {

        public CloudScriptResponse Execute(CloudScriptRequest request, string[] urlArguments)
        {
            string troopId = request.CloudScriptMethod.Params["troopId"];

            var troopData = ((MetaDataState)(MetaDataState.Current)).TroopsData.Where(y => y.TroopId == troopId).SingleOrDefault();

            var nextTier = DataLayer.Instance.GetTable<TierData>().Where(y => (int)y.Tier == (int)(troopData.CurrentTier) + 1).SingleOrDefault();

            if (MetaDataState.Current.Consumables.CheckBalances(nextTier.Cost.ConsumableCostItems))
            {
                MetaDataState.Current.Consumables.SpendConsumables(nextTier.Cost.ConsumableCostItems);
                return new CloudScriptResponse() { ResponseCode = ResponseCode.Success };
            }
            else
            {
                return new CloudScriptResponse() { ResponseCode = ResponseCode.Error };
            }
        }

        public CloudScriptResponse ExecuteStack(CloudScriptRequestStack request)
        {
            throw new NotImplementedException();

        }
    }

}
