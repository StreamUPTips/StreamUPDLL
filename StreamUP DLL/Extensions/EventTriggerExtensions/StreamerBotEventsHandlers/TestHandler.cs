using System;
using System.Collections.Generic;

namespace StreamUP
{
    public class TestHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();

            // Load test data
            triggerData.AlertMessage = "This is a test trigger";
            triggerData.Amount = 69;
            triggerData.AmountCurrency = "£4.20";
            triggerData.AmountCurrencyDouble = 4.20;
            triggerData.AmountCurrencyDecimal = 4.20M;
            triggerData.Anonymous = false;
            triggerData.BanDuration = 69;
            triggerData.BanType = "You were too awesome";
            triggerData.Donation = false;
            triggerData.EventSource = "Test";
            triggerData.EventType = "Test";
            triggerData.Message = "This is a test trigger";
            triggerData.MonthsGifted = 1;
            triggerData.MonthsStreak = 69;
            triggerData.MonthsTotal = 88;
            triggerData.Tier = "tier 3";
            triggerData.TotalAmount = 420;

            // Create list to get random StreamUP member
            List<string> usernames = new List<string> { "Andilippi", "WaldoAndFriends", "Silverlink", "TerrierDarts"};
            Random random = new Random();

            // Get random member for User data
            int index = random.Next(usernames.Count);
            triggerData.User = usernames[index];
            triggerData.UserImage = SUP.GetTwitchProfilePictureFromUsername(triggerData.User);
            usernames.RemoveAt(index);

            // Get random member for Receiver data
            index = random.Next(usernames.Count);
            triggerData.Receiver = usernames[index];
            triggerData.ReceiverImage = SUP.GetTwitchProfilePictureFromUsername(triggerData.Receiver);                   
            return triggerData;
        }
    }
}
