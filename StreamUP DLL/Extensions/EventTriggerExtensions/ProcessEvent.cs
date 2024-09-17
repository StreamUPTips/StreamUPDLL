using System.Collections.Generic;
using Streamer.bot.Common.Events;

namespace StreamUP
{
    public interface IEventHandler
    {
        TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib streamUpLib);
    }

    public partial class StreamUpLib
    {
        public static readonly Dictionary<EventType, IEventHandler> EventHandlers = new()
        {
            // Twitch Events
            { EventType.TwitchAnnouncement, new TwitchAnnouncementHandler() },
            { EventType.TwitchBotWhisper, new TwitchBotWhisperHandler() },
            { EventType.TwitchChatMessage, new TwitchChatMessageHandler() },
            { EventType.TwitchCheer, new TwitchCheerHandler() },
            { EventType.TwitchFirstWord, new TwitchFirstWordHandler() },
            { EventType.TwitchFollow, new TwitchFollowHandler() },
            { EventType.TwitchGiftBomb, new TwitchGiftBombHandler() },
            { EventType.TwitchGiftSub, new TwitchGiftSubHandler() },
            { EventType.TwitchRaid, new TwitchRaidHandler() },
            { EventType.TwitchReSub, new TwitchReSubHandler() },
            { EventType.TwitchRewardRedemption, new TwitchRewardRedemptionHandler() },
            { EventType.TwitchShoutoutCreated, new TwitchShoutoutCreatedHandler() },
            { EventType.TwitchSub, new TwitchSubHandler() },
            { EventType.TwitchUserBanned, new TwitchUserBannedHandler() },
            { EventType.TwitchUserTimedOut, new TwitchUserTimedOutHandler() },
            { EventType.TwitchWatchStreak, new TwitchWatchStreakHandler() },
            { EventType.TwitchWhisper, new TwitchWhisperHandler() },

            // YouTube Events
            { EventType.YouTubeFirstWords, new YouTubeFirstWordsHandler() },
            { EventType.YouTubeGiftMembershipReceived, new YouTubeGiftMembershipReceivedHandler() },
            { EventType.YouTubeMemberMileStone, new YouTubeMemberMileStoneHandler() },
            { EventType.YouTubeMembershipGift, new YouTubeMembershipGiftHandler() },
            { EventType.YouTubeMessage, new YouTubeMessageHandler() },
            { EventType.YouTubeNewSponsor, new YouTubeNewSponsorHandler() },
            { EventType.YouTubeNewSubscriber, new YouTubeNewSubscriberHandler() },
            { EventType.YouTubeSuperChat, new YouTubeSuperChatHandler() },
            { EventType.YouTubeSuperSticker, new YouTubeSuperStickerHandler() },
            { EventType.YouTubeUserBanned, new YouTubeUserBannedHandler() }
        };

        public TriggerData ProcessEvent(IDictionary<string, object> sbArgs)
        {
            var eventType = _CPH.GetEventType();

            if (EventHandlers.TryGetValue(eventType, out IEventHandler handler))
            {
                return handler.HandleEvent(sbArgs, this);
            }

            return null;
        }
    }

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
        public decimal AmountCurrencyDecimal { get; internal set; }
    }
}
