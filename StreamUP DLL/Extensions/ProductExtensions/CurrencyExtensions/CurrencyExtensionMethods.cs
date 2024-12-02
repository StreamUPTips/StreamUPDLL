using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
       
        public long GetBetSize(string input, long currentPoints, long minBet, long maxBet, long defaultBet, bool rangeError)
        {
           
            if (maxBet == 0 )
            {
                maxBet = long.MaxValue;
            }

            long betSize = 0;

            if (input.ToLower() == "all")
            {
                betSize = currentPoints;
            }

            else if (long.TryParse(input, out long num))
            {
                betSize = Math.Abs(num); //Turns Positive if negative
            }

            else if (Regex.IsMatch(input, @"\d+%"))
            {
                int percentage = Convert.ToInt32(input.Replace("%", string.Empty));
                betSize = (long)((double)currentPoints / 100 * percentage);
            }

            else if (Regex.IsMatch(input, @"\d+[mM]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("m", string.Empty));
                betSize = value * 1000000;
            }

            else if (Regex.IsMatch(input, @"\d+[kK]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("k", string.Empty));
                betSize = value * 1000;
            }
            else
            {
                //Default Bet  if nothing entered
            }

            if (betSize == 0)
            {

                 betSize = defaultBet;
            }

            if (betSize > currentPoints)
            {
                betSize = currentPoints;
            }

            //num
            if (betSize > maxBet)
            {
                if(rangeError)
                {
                    return -1;
                }
                else
                {
                betSize = maxBet;
                }
            }

            if (betSize < minBet)
            {
                if(rangeError)
                {
                    return -2;
                }
                else
                {
                betSize = minBet;
                }
            }

            if (betSize > currentPoints)
            {
                betSize = 0;
            }

            return betSize;
        }

         public long GetPrizeSize(string input, long minPrize, long maxPrize, long defaultPrize)
        {
            if (maxPrize == 0)
            {
                maxPrize = long.MaxValue;
            }

            long betPrize = 0;

           if (long.TryParse(input, out long num))
            {
                betPrize = Math.Abs(num); //Turns Positive if negative
            }

            else if (Regex.IsMatch(input, @"\d+[mM]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("m", string.Empty));
                betPrize = value * 1000000;
            }

            else if (Regex.IsMatch(input, @"\d+[kK]"))
            {
                int value = Convert.ToInt32(input.ToLower().Replace("k", string.Empty));
                betPrize = value * 1000;
            }
            else
            {
                betPrize = defaultPrize;
            }

            //num
            if (betPrize > maxPrize)
            {
                betPrize = maxPrize;
            }

            if (betPrize < minPrize)
            {
                betPrize = minPrize;
            }

            if (betPrize == 0)
            {
                betPrize = defaultPrize;
            }


            return betPrize;
        }


        public bool CanUserAffordById(string userId, long cost, Platform platform, string varName = "points")
        {
            long currentPoints = GetUserPointsById(userId, platform, varName);
            if (currentPoints < cost)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public bool CanUserAffordByName(string user, long cost, Platform platform, string varName = "points")
        {
            long currentPoints = GetUserPointsByUser(user, platform, varName);
            if (currentPoints < cost)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
    }
}