using System;
using System.Collections.Generic;
using System.Globalization;
using Streamer.bot.Plugin.Interface;


namespace StreamUP
{
    
    public static class SetTriggers //Give your Class a Name
    {
        [Obsolete]
        public static void SetCustomTriggers(this IInlineInvokeProxy CPH, List<CustomTrigger> customTriggers)
        {
            foreach (var customTrigger in customTriggers)
            {
                CPH.RegisterCustomTrigger(customTrigger.Name, customTrigger.Code, customTrigger.TriggerCategory);
                CPH.SUWriteLog($"[Custom Triggers] {customTrigger.Name}, {customTrigger.Code} = [{customTrigger.TriggerCategory}]");
            }
        }

        [Obsolete]
        public static void SetGenericCurrencyTriggers(this IInlineInvokeProxy CPH)
        {
            string[] categories = { "Currency" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new CustomTrigger("Generic Fail", "currencyFail" ,categories),
                //new CustomTrigger("Points Updated","pointsUpdate" ,categories)
            };
            CPH.SetCustomTriggers(customTriggers);


        }

        [Obsolete]
        public static void SetTriggersForSlots(this IInlineInvokeProxy CPH)
        {
            string[] categories = { "Currency", "Slots" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new CustomTrigger("Slots All-In Loss", "slotsAllInLoss", categories),
            new CustomTrigger("Slots All-in Win", "slotsAllInWin", categories),
            new CustomTrigger("Slots Any Loss", "slotsAnyLoss", categories),
            new CustomTrigger("Slots Any Win", "slotsAnyWin", categories),
            new CustomTrigger("Slots Loss", "slotsLoss", categories),
            new CustomTrigger("Slots Win", "slotsWin", categories),
             };
            CPH.SetCustomTriggers(customTriggers);

        }

        [Obsolete]
        public static void SetTriggersForRaffle(this IInlineInvokeProxy CPH)
        {
            string[] categories = { "Currency", "Raffle" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new CustomTrigger("Raffle Start", "raffleStart", categories),
            new CustomTrigger("Raffle End (All Winners)", "raffleEndAllWinners", categories),
            new CustomTrigger("Raffle End (Single Winner)", "raffleEndSingleWinner", categories),
            new CustomTrigger("Raffle Join", "raffleJoinSuccess", categories),
           
             };
            CPH.SetCustomTriggers(customTriggers);

        }

        [Obsolete]
        public static void SetTriggersForGamble(this IInlineInvokeProxy CPH)
        {
            string[] categories = { "Currency", "Gamble" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new CustomTrigger("Gamble All In Loss", "gambleAllInLoss",categories),
            new CustomTrigger("Gamble All In Win", "gambleAllInWin",categories),
            new CustomTrigger("Gamble All In Push", "gambleAllInPush",categories),
            new CustomTrigger("Gamble Any Loss", "gambleAnyLoss",categories),
            new CustomTrigger("Gamble Any Win", "gambleAnyWin",categories),
            new CustomTrigger("Gamble Any Push", "gambleAnyPush",categories),
            new CustomTrigger("Gamble Loss", "gambleLoss",categories),
            new CustomTrigger("Gamble Win", "gambleWin",categories),
            new CustomTrigger("Gamble Push", "gamblePush",categories),
            
             };
            CPH.SetCustomTriggers(customTriggers);

        }

        [Obsolete]
        public static void SetTriggersForTimedVIP(this IInlineInvokeProxy CPH)
        {
            string [] categories = {"Timed VIP"};
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new CustomTrigger("New VIP", "timedVipnewVip", categories),
                new CustomTrigger("Time Added",  "timedViptimeAdded", categories),
                new CustomTrigger("Timed VIP Fail", "timedVipFail", categories),
                new CustomTrigger("Timed VIP Time Left", "timedVipTimeLeft", categories),
                new CustomTrigger("VIP Removed", "timedVipUserRemoved", categories)
            };
            CPH.SetCustomTriggers(customTriggers);
        }

    }
}