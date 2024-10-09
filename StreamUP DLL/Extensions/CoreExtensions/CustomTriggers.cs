using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class CustomTrigger
    {
        public string Name { get; set; }
        public string Code { get; set; }

        public string[] TriggerCategory { get; set; }

        public CustomTrigger(string name, string code, string[] triggerCategory)
        {
            Name = name;
            Code = code;
            TriggerCategory = triggerCategory;
        }

    }
    public partial class StreamUpLib
    {

        public void SetCustomTriggers(List<CustomTrigger> customTriggers)
        {
            foreach (var customTrigger in customTriggers)
            {
                _CPH.RegisterCustomTrigger(customTrigger.Name, customTrigger.Code, customTrigger.TriggerCategory);
                LogInfo($"[Custom Triggers] {customTrigger.Name}, {customTrigger.Code} = [{customTrigger.TriggerCategory}]");
            }
        }

        public void SetGenericCurrencyTriggers()
        {
            string[] categories = { "Currency" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Generic Fail", "currencyFail" ,categories),
                //new("Points Updated","pointsUpdate" ,categories)
            };
            SetCustomTriggers(customTriggers);


        }

        public void SetTriggersForSlots()
        {
            string[] categories = { "Currency", "Slots" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new("Slots All-In Loss", "slotsAllInLoss", categories),
            new("Slots All-in Win", "slotsAllInWin", categories),
            new("Slots Any Loss", "slotsAnyLoss", categories),
            new("Slots Any Win", "slotsAnyWin", categories),
            new("Slots Loss", "slotsLoss", categories),
            new("Slots Win", "slotsWin", categories),
             };
            SetCustomTriggers(customTriggers);

        }

        public void SetTriggersForRaffle()
        {
            string[] categories = { "Currency", "Raffle" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new("Raffle Start", "raffleStart", categories),
            new("Raffle End (All Winners)", "raffleEndAllWinners", categories),
            new("Raffle End (Single Winner)", "raffleEndSingleWinner", categories),
            new("Raffle Join", "raffleJoinSuccess", categories),

             };
            SetCustomTriggers(customTriggers);

        }

        public void SetTriggersForGamble()
        {
            string[] categories = { "Currency", "Gamble" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new("Gamble All In Loss", "gambleAllInLoss",categories),
            new("Gamble All In Win", "gambleAllInWin",categories),
            new("Gamble All In Push", "gambleAllInPush",categories),
            new("Gamble Any Loss", "gambleAnyLoss",categories),
            new("Gamble Any Win", "gambleAnyWin",categories),
            new("Gamble Any Push", "gambleAnyPush",categories),
            new("Gamble Loss", "gambleLoss",categories),
            new("Gamble Win", "gambleWin",categories),
            new("Gamble Push", "gamblePush",categories),

             };
            SetCustomTriggers(customTriggers);

        }

        public void SetTriggersForHeist()
        {
            string[] categories = { "Currency", "Heist" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
            new("Heist Start", "heistStart",categories),
            new("User Joined", "heistUserJoined",categories),
            new("Heist End", "heistEnding",categories),
            new("All Users Lost", "heistAllUsersLost",categories),
            new("No User Win", "heistNoUsersWon",categories),
            new("Single User Win", "heistSingleWin",categories),
            new("Single User Loss", "heistSingleLoss",categories),
            new("Users Lost", "heistUsersLost",categories),
            new("Users Won", "heistUsersWon",categories),
            new("Everyone Won", "heistEveryoneWon",categories),
            
             };
            SetCustomTriggers(customTriggers);

        }

        public void SetTriggersForTimedVIP()
        {
            string[] categories = { "Timed VIP" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("New VIP", "timedVipnewVip", categories),
                new("Time Added",  "timedViptimeAdded", categories),
                new("Timed VIP Fail", "timedVipFail", categories),
                new("Timed VIP Time Left", "timedVipTimeLeft", categories),
                new("VIP Removed", "timedVipUserRemoved", categories)
            };
            SetCustomTriggers(customTriggers);
        }



    }
}