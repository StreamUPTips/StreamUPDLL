using System.Collections.Generic;
using System.Drawing.Drawing2D;
using Microsoft.SqlServer.Server;
using Streamer.bot.Common.Events;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public double GetTwitchValue(TriggerData triggerData, string myCode)
        {

            double amount = 0.00;
            double bitCost = 0.01;
            double tierOne = 3.00;
            double tierTwo = 5.00;
            double tierThree = 12.50;
            
            string tier = triggerData.Tier;
            EventType eventType = _CPH.GetEventType();
            int duration = triggerData.MonthDuration == 0 ? 1 : triggerData.MonthDuration;

            switch (eventType)
            {

                case EventType.TwitchCheer:
                    amount = bitCost * triggerData.Amount;
                    break;
                case EventType.TwitchSub:
                case EventType.TwitchReSub:
                    
                    if (tier == "tier 1" || tier == "prime")
                    {
                        amount = tierOne * duration;
                    }
                    if (tier == "tier 2")
                    {
                        amount = tierTwo * duration;
                    }
                    if (tier == "tier 3" )
                    {
                        amount = tierThree * duration;
                    }

                    break;
                case EventType.TwitchGiftSub:
                    
                   if (tier == "tier 1")
                    {
                        amount = tierOne * duration;
                    }
                    if (tier == "tier 2")
                    {
                        amount = tierTwo * duration;
                    }
                    if (tier == "tier 3")
                    {
                        amount = tierThree * duration;
                    }
                    break;
                case EventType.TwitchGiftBomb:
                    
                    if (tier == "tier 1")
                    {
                        amount = tierOne * triggerData.Amount;
                    }
                    if (tier == "tier 2")
                    {
                        amount = tierTwo * triggerData.Amount;
                    }
                    if (tier == "tier 3")
                    {
                        amount = tierThree * triggerData.Amount;
                    }
                    break;




            }


            ConvertCurrency((decimal)amount, myCode, "USD", out decimal convertedAmount);
            return (double)convertedAmount;

        }
    }
}