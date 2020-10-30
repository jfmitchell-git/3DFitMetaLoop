using MetaLoop.Common.PlatformCommon.Data.Schema.Types;
using MetaLoop.Common.PlatformCommon.State;
using MetaLoop.Common.PlatformCommon;
using MetaLoop.Common.PlatformCommon.Data;
using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaLoop.Common.PlatformCommon.Settings;

namespace MetaLoop.Common.PlatformCommon.Data.Schema
{
    public partial class ShopOffer
    {
        public int LevelMin { get; set; }
        public int LevelMax { get; set; }
        public int AgeMin { get; set; }
        public int AgeMax { get; set; }
        public int LowSoftCurrencyThreshold { get; set; }
        public int LowHardCurrencyThreshold { get; set; }
        public int LowEnergyThreshold { get; set; }
        public bool EvaluateRequirement(MetaDataStateBase state, DateTime utcNow)
        {

#if !BACKOFFICE
            if (Duration > 0 && utcNow < EndTime && CanBeListedInOffers)
            {
                return true;
            }
#endif

            if (LevelMin > 0 && LevelMax > 0)
            {
                if (state.PlayerLevel.LevelId < LevelMin || state.PlayerLevel.LevelId > LevelMax) return false;
            }

            if (AgeMin > 0 && AgeMax > 0)
            {
                if (AgeMin < state.LoginCalendar.Count || state.LoginCalendar.Count > AgeMax) return false;
            }


            if (LowSoftCurrencyThreshold > 0)
            {
                if (state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaStateSettings._SoftCurrencyId)) > LowSoftCurrencyThreshold)
                {
                    return false;
                }
            }

            if (LowHardCurrencyThreshold > 0)
            {
                if (state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaStateSettings._HardCurrencyId)) > LowHardCurrencyThreshold)
                {
                    return false;
                }
            }

            if (LowEnergyThreshold > 0)
            {
                if (state.Consumables.GetConsumableAmount(Consumable.GetByName(MetaStateSettings._EnergyId)) > LowEnergyThreshold)
                {
                    return false;
                }
            }

            return true;
        }
    }


    public partial class ShopOffer : PurchasableItem
    {

        private int id;
        [PrimaryKey, AutoIncrement]
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                //Load relationships here...
            }
        }

        public int Duration { get; set; }
        public int ResetAfter = 168;
        public int DisplayTagTypeVal { get; set; }
        public string MainUiImage { get; set; }
        public string ShopImage { get; set; }
        public string TitleResourceKey { get; set; }
        public string DescResourceKey { get; set; }

        public string StartTimeString { get; set; }
        public string EndTimeString { get; set; }
        public int TimerDurationInHours { get; set; }

        public int RedeemMaxCount { get; set; }
        public int ShowDaysBefore { get; set; }

        public int ShowPopupEveryHours { get; set; }
        public int Priority { get; set; }
        public DisplayTagType DisplayTagType { get; set; }

        public string PlacementsPriority { get; set; }

        public List<ShopOfferPlacementType> GetPlacementPriorities()
        {
            List<ShopOfferPlacementType> result = new List<ShopOfferPlacementType>();
            if (PlacementsPriority == null) return result;
            var allString = PlacementsPriority.Split(';');
            foreach (var placement in allString)
            {
                try { result.Add((ShopOfferPlacementType)Enum.Parse(typeof(ShopOfferPlacementType), placement)); } catch { };
            }
            return result;
        }


        [IgnoreCodeFirst, Ignore]
        public DateTime StartTime
        {
            get
            {

                if (!string.IsNullOrEmpty(StartTimeString))
                {
                    DateTime myDate = DateTime.ParseExact(StartTimeString, MetaStateSettings._DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    return myDate;
                }
                else
                {
                    return new DateTime(2000, 01, 01);
                }


            }
        }

        [IgnoreCodeFirst, Ignore]
        public DateTime EndTime
        {
            get
            {
                if (!string.IsNullOrEmpty(StartTimeString))
                {
                    DateTime myDate = DateTime.ParseExact(EndTimeString, MetaStateSettings._DateTimeFormat, System.Globalization.CultureInfo.InvariantCulture);
                    return myDate;
                }
#if !BACKOFFICE
                else if (Duration > 0 && ResetAfter == 0)
                {
                    if (ActivationDate == DateTime.MinValue)
                    {
                        return DateTime.UtcNow.AddHours(Duration);
                    }
                    else
                    {
                        return ActivationDate.AddHours(Duration);
                    }

                }

                else if (Duration > 0 && ResetAfter > 0)
                {

                    if (ActivationDate == DateTime.MinValue)
                    {
                        return DateTime.UtcNow.AddHours(Duration);
                    }
                    else
                    {
                        if (MetaDataStateBase.Current.GetServerUTCDatetime() > ActivationDate.AddHours(Duration))
                        {
                            if (MetaDataStateBase.Current.GetServerUTCDatetime() > ActivationDate.AddHours(Duration).AddHours(ResetAfter))
                            {
                                return DateTime.UtcNow.AddHours(Duration);
                            }
                            else
                            {
                                return ActivationDate.AddHours(Duration);
                            }
                        }
                        else
                        {
                            return ActivationDate.AddHours(Duration);
                        }
                    }


                }
#endif
                else
                {
                    return DateTime.UtcNow.AddDays(7);
                }
            }
        }


#if BACKOFFICE
        public static List<ShopOffer> GetActiveOffers(DateTime UtcNow)
        {
            return DataLayer.Instance.GetTable<ShopOffer>().Where(y => UtcNow >= y.StartTime && UtcNow < y.EndTime).OrderBy(y => y.Priority).OrderByDescending(y => y.StartTime).ToList();
        }
#endif

#if !BACKOFFICE

        public static List<ShopOffer> GetActiveOffers(DateTime UtcNow)
        {
            return DataLayer.Instance.GetTable<ShopOffer>().Where(y => y.EvaluateRequirement(MetaDataStateBase.Current, UtcNow) && UtcNow >= y.StartTime && UtcNow < y.EndTime && !y.ReachedMaxRedeemCount).OrderBy(y => y.Priority).OrderByDescending(y => y.StartTime).ToList();
        }


        public OfferDataStateEntry GetLastOfferDataState()
        {
            return MetaDataStateBase.Current.OfferDataState.AllOffersState.Where(y => y.OfferId == InternalId).LastOrDefault();
        }

        [IgnoreCodeFirst, Ignore]
        public DateTime LastTimeShown
        {
            get
            {
                var offerState = GetLastOfferDataState();
                if (offerState != null) return offerState.LastTimeShown;
                return DateTime.MinValue;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public DateTime ActivationDate
        {
            get
            {
                var offerState = GetLastOfferDataState();
                if (offerState != null) return offerState.ActivationDate;
                return DateTime.MinValue;
            }
        }


        public bool ReachedMaxRedeemCount
        {
            get
            {
                if (this.RedeemMaxCount == 0) return false;
                if (this.PurchaseCount >= RedeemMaxCount) return true;
                return false;
            }
        }

        public int PurchaseCount
        {
            get
            {
#if UNITY_EDITOR
                return MetaDataStateBase.Current.ShopState.AllTransactions.Where(y => y.InternalId == this.InternalId).Count();
#else
                
                return MetaDataStateBase.Current.ShopState.AllTransactions.Where(y => y.InternalId == this.InternalId).Count();
                //return gamedata.GameData.Current.MetaDataState.ShopState.AllTransactions.Where(y => y.InternalId == this.InternalId && y.Result == GenericRequestRewardResultType.Success).Count();
#endif
            }
        }


        public bool CanBeListedInOffers
        {
            get
            {
                if (TimerDurationInHours > 0)
                {
                    var offerState = GetLastOfferDataState();

                    if (offerState != null)
                    {
                        var Timeleft = offerState.ActivationDate.AddHours(TimerDurationInHours) - MetaDataStateBase.Current.GetServerUTCDatetime();

                        if (Timeleft.TotalSeconds < 0)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (Duration > 0)
                {
                    if (ActivationDate == DateTime.MinValue)
                    {
                        return false;
                    }
                    else
                    {
                        if (MetaDataStateBase.Current.GetServerUTCDatetime() > ActivationDate.AddHours(Duration))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }

                return true;
            }
        }

        [IgnoreCodeFirst, Ignore]
        public bool IsAvailabeForMainScreenDisplay
        {
            get
            {
                if (ShowPopupEveryHours > 0)
                {
                    var offerState = GetLastOfferDataState();
                    if (offerState != null)
                    {
                        if (DateTime.UtcNow.Subtract(offerState.LastTimeShown).TotalHours >= this.ShowPopupEveryHours)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public void MarkAsShown(ShopOfferPlacementType placement, bool popup)
        {
            MetaDataStateBase.Current.OfferDataState.MarkAsShown(this.InternalId, placement, this, popup);
        }
#endif
    }
}
