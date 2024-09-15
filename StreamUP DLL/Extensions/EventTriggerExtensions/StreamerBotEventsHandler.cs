using System.Collections.Generic;
using Streamer.bot.Common.Events;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        // StreamerBot Event Handler

        public interface IEventHandler
        {
            TriggerData HandleEvent(IDictionary<string, object> sbArgs);
        }

        private static readonly Dictionary<EventType, IEventHandler> EventHandlers = new Dictionary<EventType, IEventHandler>
        {
            { EventType.TwitchChatMessage, new TwitchChatMessageHandler() },
            // etc...
        };

        public enum StreamingPlatform
        {
            All = 0,
            Twitch = 1,
            YouTube = 2
        }

        public class TriggerData
        {
            public string AlertMessage { get; set; } = null;
            public int Amount { get; set; } = -1;
            public string AmountCurrency { get; set; } = null;
            public double AmountCurrencyDouble { get; set; } = -1;
            public bool Anonymous { get; set; } = false;
            public int BanDuration { get; set; } = -1;
            public string BanType { get; set; } = null;
            public bool Donation { get; set; } = false;
            public string EventSource { get; set; } = null;
            public string EventType { get; set; } = null;
            public string Message { get; set; } = null;
            public int MonthsGifted { get; set; } = -1;
            public int MonthsStreak { get; set; } = -1;
            public int MonthsTotal { get; set; } = -1;
            public string Receiver { get; set; } = null;
            public string ReceiverImage { get; set; } = null;
            public string Tier { get; set; } = null;
            public int TotalAmount { get; set; } = -1;
            public string User { get; set; } = null;
            public string UserImage { get; set; } = null;
        }

    }
}
