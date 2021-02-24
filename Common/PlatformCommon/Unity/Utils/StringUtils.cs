using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Unity.Utils
{

    public class StringUtils
    {

        public static string FirstCharToLower(string input)
        {
            return input.First().ToString().ToLower() + input.Substring(1);
        }

        public static string FirstCharToUpper(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }


        public static string MillisecondsToSecondString(int value)
        {
            return (value / 1000).ToString("0.00");
        }

        public static string FloatToString(float num, int numOfDecimal)
        {
            double value = Math.Round(num, numOfDecimal);
            return value.ToString();
        }


        public static string FormatDate(DateTime date)
        {
            return date.Day + " " + date.ToString("MMM") + " " + date.Year;
        }




        public static string GetDateFormatted(TimeSpan time,string dayString, string hourString,string minuteString,bool showSec = true,bool showHourMinimum = true,bool showMinuteOnDays = false)
        {
            string returnValue = "";
            bool secondRemoved = false;

            if (time.Days > 0)
            {
                returnValue =  string.Format(dayString, time.Days,time.Hours, time.Minutes,time.Seconds);

                if(!showMinuteOnDays)
                {
                    var tmp = returnValue.Split(new string[] { " " }, StringSplitOptions.None).ToList();

                    if(tmp.Count > 4)
                    {
                        tmp.RemoveAt(tmp.Count - 1);
                        tmp.RemoveAt(tmp.Count - 1);
                        tmp.RemoveAt(tmp.Count - 1);
                        tmp.RemoveAt(tmp.Count - 1);
                    } else
                    {
                        tmp.RemoveAt(tmp.Count - 1);
                        tmp.RemoveAt(tmp.Count - 1);
                    }
                    

                    returnValue = String.Join(" ", tmp.ToArray());
                    secondRemoved = true;
                }

            } else if(time.Hours > 0 || showHourMinimum)
            {

                returnValue = string.Format(hourString, time.Hours, time.Minutes, time.Seconds);
                
            } else
            {
                returnValue = string.Format(minuteString, time.Minutes, time.Seconds);

            }

            if (!showSec && !secondRemoved)
            {
                var tmp = returnValue.Split(new string[] { " " }, StringSplitOptions.None).ToList();


                if(tmp.Count > 3 && !showMinuteOnDays)
                {
                    
                    tmp.RemoveAt(tmp.Count - 1);
                    tmp.RemoveAt(tmp.Count - 1);
                } else
                {
                    tmp.RemoveAt(tmp.Count - 1);
                }
                
                returnValue = String.Join(" ", tmp.ToArray());
            }

            return returnValue;

        }

        




    }
}
