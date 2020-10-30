using MetaLoop.Common.PlatformCommon.Data.Schema;
using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.State
{

    [Serializable]
    public class PlacementTypeState
    {
        public PlacementTypeState() { }
        public ShopOfferPlacementType PlacementType;
        public DateTime LastTimeShown;
    }

    [Serializable]
    public class OfferDataStateEntry
    {
        public OfferDataStateEntry() { }
        public string OfferId;
        public DateTime LastTimeShown;
        public DateTime ActivationDate;

    }

    [Serializable]
    public class OfferDataState
    {
        public List<OfferDataStateEntry> AllOffersState;
        public List<PlacementTypeState> AllPlacementTypeState;



        public DateTime LastSupplyRefresh;

        private void Init()
        {
            if (AllOffersState == null) AllOffersState = new List<OfferDataStateEntry>();
            if (AllPlacementTypeState == null) AllPlacementTypeState = new List<PlacementTypeState>();
        }

        public OfferDataState()
        {
            Init();
        }

        [OnDeserializing]
        private void SetValuesOnDeserializing(StreamingContext context)
        {
            Init();
        }

        public void MarkAsShown(string offerId, ShopOfferPlacementType placement, ShopOffer shopOffer, bool popup)
        {
            OfferDataStateEntry entry = AllOffersState.Where(y => y.OfferId == offerId).SingleOrDefault();
            PlacementTypeState entryPlacement = AllPlacementTypeState.Where(y => y.PlacementType == placement).SingleOrDefault();
            if (entry == null)
            {
                entry = new OfferDataStateEntry();
                entry.OfferId = offerId;
                entry.ActivationDate = DateTime.UtcNow;
                entry.LastTimeShown = DateTime.MinValue;
                AllOffersState.Add(entry);
            }
            else
            {
                if (shopOffer.Duration > 0)
                {
                    if (MetaDataStateBase.Current.GetServerUTCDatetime() > entry.ActivationDate.AddHours(shopOffer.Duration))
                    {
                        entry.ActivationDate = DateTime.UtcNow;
                    }
                }

            }

            if (popup)
            {
                if (entryPlacement == null)
                {
                    entryPlacement = new PlacementTypeState();
                    entryPlacement.PlacementType = placement;
                    AllPlacementTypeState.Add(entryPlacement);
                }
                entryPlacement.LastTimeShown = DateTime.UtcNow;

                entry.LastTimeShown = DateTime.UtcNow;
            }


        }

        public DateTime LastPopupShownTime
        {
            get
            {
                var latest = AllOffersState.OrderByDescending(y => y.LastTimeShown).FirstOrDefault();
                if (latest == null)
                {
                    return new DateTime();
                }
                else
                {
                    return latest.LastTimeShown;
                }
            }
        }


    

      
    }
}
