using System;
using System.Collections.Generic;
using System.ComponentModel;
using Streamer.bot.Common.Events;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public long GetEventPoints(EventType eventType, Platform platform)
        {
            //# Load Event Settings
            long returnValue = 0;
            //! Generic Events Settings
            int minChatPoints = GetSetting<int>("minChatPoints", 10);
            int maxChatPoints = GetSetting<int>("maxChatPoints", 15);
            int cooldownChatPoints = GetSetting<int>("cooldownChatPoints", 15);
            int pointsFirstWords = GetSetting<int>("pointsFirstWords", 50);
            int pointsPresentViewers = GetSetting<int>("pointsPresentViewers", 50);
            int pointsPerDollarTipped = GetSetting<int>("pointsPerDollarTipped", 200);
            //! Twitch Events Settings
            int pointsPerBit = GetSetting<int>("pointsPerBit", 1);
            int pointsPerT1 = GetSetting<int>("pointsPerT1", 300);
            int pointsPerT2 = GetSetting<int>("pointsPerT2", 700);
            int pointsPerT3 = GetSetting<int>("pointsPerT3", 1900);
            int pointsPerMonth = GetSetting<int>("pointsPerMonth", 10);
            int pointsPerRaid = GetSetting<int>("pointsPerRaid", 250);
            int pointsPerViewer = GetSetting<int>("pointsPerViewer", 5);
            int pointsPerFollow = GetSetting<int>("pointsPerFollow", 100);
            int pointsPerGiftSub = GetSetting<int>("pointsPerGiftSub", 200);
            bool pointsPerGiftBoost = GetSetting<bool>("pointsPerGiftBoost", false);
            //! Youtube Event Settings
            int pointsPerSub = GetSetting<int>("pointsPerSub", 100);
            Dictionary<string, int> pointsYouTubeMembers = GetSetting<Dictionary<string, int>>("pointsYouTubeMembers", []);
            int pointsPerMember = GetSetting<int>("pointsPerMember", 500);
            int pointsPerMemberMonth = GetSetting<int>("pointsPerMemberMonth", 10);
            int pointsPerGiftedMember = GetSetting<int>("pointsPerGiftedMember", 500);
            bool pointsPerMemberBoost = GetSetting<bool>("pointsPerMemberBoost", false);
            //! Kick Event Settings
            int pointsPerKickFollow = GetSetting<int>("pointsPerKickFollow", 100);
            int pointsPerKickSub = GetSetting<int>("pointsPerKickSub", 300);
            int pointsPerKickMonth = GetSetting<int>("pointsPerKickSubMonth", 10);
            int pointsPerKickGift = GetSetting<int>("pointsPerKickGift", 300);
            //! Trovo Event Settings
            //Follow
            //Raid
            //Sub
            //Resub
            //Gift
            //MassGift
            //Todo Spells?

            //! Pre-declared Vars
            string tier;
            string membershipName;
            int points;
            int memberPoints;
            int monthsSubbed;

            switch (eventType)
            {
                //? Generic Events
                case EventType.TwitchPresentViewers:
                case EventType.YouTubePresentViewers:
                case EventType.TrovoPresentViewers:
                case EventType.KickPresentViewers:
                    returnValue = pointsPresentViewers;
                    break;
                case EventType.TwitchChatMessage:
                case EventType.YouTubeMessage:
                case EventType.KickChatMessage:
                case EventType.TrovoChatMessage:
                    int min = minChatPoints;
                    int max = maxChatPoints;
                    returnValue = _CPH.Between(min, max);
                    break;
                case EventType.TwitchFirstWord:
                case EventType.YouTubeFirstWords:
                case EventType.KickFirstWords:
                case EventType.TrovoFirstWords:
                    returnValue = pointsFirstWords;
                    break;
                //? Twitch Events
                case EventType.TwitchCheer:
                    int bits = TryGetArgOrDefault("bits", 100);
                    returnValue = pointsPerBit * bits;
                    break;
                case EventType.TwitchSub:
                case EventType.TwitchReSub:
                    tier = TryGetArgOrDefault("tier", "tier 1");
                    monthsSubbed = TryGetArgOrDefault("cumulative", 1);
                    int subBaseReward = pointsPerT1;
                    if (tier == "tier 2")
                    {
                        subBaseReward = pointsPerT2;
                    }

                    if (tier == "tier 3")
                    {
                        subBaseReward = pointsPerT3;
                    }

                    int monthReward = pointsPerMonth * monthsSubbed;
                    returnValue = subBaseReward + monthReward;
                    break;
                case EventType.TwitchGiftSub:
                case EventType.TwitchGiftBomb:
                    int giftSubBase = pointsPerGiftSub;
                    int giftSubAmount = TryGetArgOrDefault("gifts", 1);
                    int giftBoost = 0;
                    if (pointsPerGiftBoost)
                    {
                        tier = TryGetArgOrDefault("tier", "tier 1");
                        giftBoost = pointsPerT1;
                        if (tier == "tier 2")
                        {
                            giftBoost = pointsPerT2;
                        }

                        if (tier == "tier 3")
                        {
                            giftBoost = pointsPerT3;
                        }
                    }

                    returnValue = (giftSubBase + giftBoost) * giftSubAmount;
                    break;
                case EventType.TwitchFollow:
                    returnValue = pointsPerFollow;
                    break;
                case EventType.TwitchRaid:
                    int raidBaseReward = pointsPerRaid;
                    int viewerBonus = pointsPerViewer;
                    int viewers = TryGetArgOrDefault("viewers", 1);
                    returnValue = raidBaseReward + (viewers * viewerBonus);
                    break;


                //? Youtube Events
                case EventType.YouTubeMembershipGift:
                    membershipName = TryGetArgOrDefault("tier", "notFoundErrorXYZ123");
                    int gifts = TryGetArgOrDefault("gifts", 1);
                    points = 0;
                    if (pointsPerMemberBoost)
                    {
                        points = pointsYouTubeMembers.TryGetValue(membershipName, out memberPoints) ? memberPoints : pointsPerMember;
                    }

                    returnValue = (pointsPerGiftedMember + points) * gifts;
                    break;
                case EventType.YouTubeNewSponsor:
                case EventType.YouTubeMemberMileStone:
                    int months = TryGetArgOrDefault("months", 1);
                    membershipName = TryGetArgOrDefault("levelName", "notFoundErrorXYZ123");
                    // Get points from dictionary or default to setting
                    points = pointsYouTubeMembers.TryGetValue(membershipName, out memberPoints) ? memberPoints : pointsPerMember;
                    // Calculate points to add
                    returnValue = points + (pointsPerMemberMonth * months);
                    break;
                case EventType.YouTubeNewSubscriber:
                    returnValue = pointsPerSub;
                    break;
                case EventType.YouTubeSuperChat:
                case EventType.YouTubeSuperSticker:
                    decimal amount = TryGetArgOrDefault("amount", 1);
                    string fromCode = TryGetArgOrDefault("currencyCode", "USD");
                    TryGetCurrencyExchangeRate(fromCode.ToLower(), "usd", out decimal exchangeRate);
                    decimal amountInUSD = amount / exchangeRate;
                    returnValue = (int)Math.Floor(amountInUSD) * pointsPerDollarTipped;
                    break;
                //? Kick Events
                case EventType.KickFollow:
                    returnValue = pointsPerKickFollow;
                    break;
                case EventType.KickSubscription:
                case EventType.KickResubscription:
                    monthsSubbed = TryGetArgOrDefault("monthsSubscribed", 1);
                    returnValue = pointsPerKickSub + (monthsSubbed * pointsPerKickMonth);
                    break;
                case EventType.KickGiftSubscription:
                case EventType.KickMassGiftSubscription:
                    returnValue = pointsPerFollow;
                    break;

                //? Trovo Events
                case EventType.TrovoFollow:
                case EventType.TrovoSpellCast:
                case EventType.TrovoCustomSpellCast:
                case EventType.TrovoRaid:
                case EventType.TrovoSubscription:
                case EventType.TrovoResubscription:
                case EventType.TrovoGiftSubscription:
                case EventType.TrovoMassGiftSubscription:

                default:
                    break;

            }

            return returnValue;
        }
    }
}