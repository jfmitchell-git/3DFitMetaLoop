using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaLoop.Common.PlatformCommon.Utils
{
    public class RandomItem
    {
        private static System.Random randomInstance = new System.Random();
        public object Object { get; set; }
        public int Probability { get; set; }
        public RandomItem(object obj, int probability)
        {
            Object = obj;
            Probability = probability;
        }
        public void Dispose()
        {
            Object = null;
        }

        public static RandomItem GetRandomItemByProbability(List<RandomItem> items)
        {
            int minValue = 0;
            int maxValue = 0;

            items = items.OrderBy(y => y.Probability).ToList();

            foreach (RandomItem item in items)
            {
                maxValue += item.Probability;
            }

            float randomNumber = GetRandomNumberByMinMax(minValue, maxValue - 1, true);
            int baseProbability = minValue;
            int loopCount = 0;

            foreach (RandomItem item in items)
            {
                if ((randomNumber >= baseProbability) && (randomNumber < (baseProbability + item.Probability)))
                {
                    return item;
                }

                baseProbability = baseProbability + item.Probability;
                loopCount++;
            }

            return null;
        }


        /// <summary>
        /// Deals the random based on odds.
        /// </summary>
        /// <param name="odds">The odds.</param>
        /// <param name="baseOdds">The base odds.</param>
        /// <returns></returns>
        public static bool DealRandomBasedOnOdds(int odds, int baseOdds)
        {
            List<RandomItem> items = new List<RandomItem>();
            items.Add(new RandomItem(1, odds));
            items.Add(new RandomItem(0, baseOdds - odds));
            return (int)GetRandomItemByProbability(items).Object == 1;

        }

        public static int GetRandomNumberByMinMax(int minimum, int maximum, bool useStaticInstance = true)
        {
            if (useStaticInstance)
            {
                return randomInstance.Next(minimum, maximum + 1);
            }
            else
            {
                return new System.Random().Next(minimum, maximum);
            }

        }
    }
}
