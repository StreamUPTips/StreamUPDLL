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

        public long GetEventPoints(EventType eventType, Platform platform, CurrencySettings settings)
        {
            //# Load Event Settings
            long returnValue = 0;
            
            //! Generic Events Settings
            int minChatPoints = settings.Generic_MinChat;
            int maxChatPoints = settings.Generic_MaxChat;
            int cooldownChatPoints = settings.Generic_ChatCooldown;
            int pointsFirstWords = settings.Generic_FirstWords;
            int pointsPresentViewers = settings.Generic_PresentViewers;
            int pointsPerDollarTipped = settings.Generic_DollarTipped;
            //! Twitch Events Settings
            int pointsPerBit = settings.Twitch_Bits;
            int pointsPerT1 = settings.Twitch_T1;
            int pointsPerT2 = settings.Twitch_T2;
            int pointsPerT3 = settings.Twitch_T3;
            int pointsPerMonth = settings.Twitch_Month;
            int pointsPerRaid = settings.Twitch_Raid;
            int pointsPerViewer = settings.Twitch_Viewer;
            int pointsPerFollow = settings.Twitch_Follow;
            int pointsPerGiftSub = settings.Twitch_Gift;
            bool pointsPerGiftBoost = settings.Twitch_BoostGifts;
            //! Youtube Event Settings
            int pointsPerSub = settings.YouTube_Sub;
            Dictionary<string, string> pointsYouTubeMembers = settings.Youtube_Memberships;
            int pointsPerMember = settings.YouTube_Months;
            int pointsPerMemberMonth = settings.YouTube_Months;
            int pointsPerGiftedMember = settings.YouTube_Gift;
            bool pointsPerMemberBoost = settings.YouTube_BoostGifts;
            //! Kick Event Settings
            int pointsPerKickFollow = settings.Kick_Follower;
            int pointsPerKickSub = settings.Kick_Sub;
            int pointsPerKickMonth = settings.Kick_Month;
            int pointsPerKickGift = settings.Kick_Gift;

            //! Pre-declared Vars
            string tier;
            string membershipName;
            long points;
            long memberPoints;
            int monthsSubbed;
            int gifts;

            switch (eventType)
            {
                //? Generic Events
                case EventType.TwitchPresentViewers:
                case EventType.YouTubePresentViewers:
                case EventType.KickPresentViewers:
                    returnValue = pointsPresentViewers;
                    break;
                case EventType.TwitchChatMessage:
                case EventType.YouTubeMessage:
                case EventType.KickChatMessage:    
                    int min = minChatPoints;
                    int max = maxChatPoints;
                    returnValue = _CPH.Between(min, max);
                    break;
                case EventType.TwitchFirstWord:
                case EventType.YouTubeFirstWords:
                case EventType.KickFirstWords:
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
                    gifts = TryGetArgOrDefault("gifts", 1);
                    points = 0;
                    if (pointsPerMemberBoost)
                    {
                        if (pointsYouTubeMembers.TryGetValue(membershipName, out string memberPointsString2) && long.TryParse(memberPointsString2, out long memberPointsValue2))
                        {
                            points = memberPointsValue2;
                        }
                        else
                        {
                            points = pointsPerMember;
                        }
                    }

                    returnValue = (pointsPerGiftedMember + points) * gifts;
                    break;
                case EventType.YouTubeNewSponsor:
                case EventType.YouTubeMemberMileStone:
                    int months = TryGetArgOrDefault("months", 1);
                    membershipName = TryGetArgOrDefault("levelName", "notFoundErrorXYZ123");
                    // Get points from dictionary or default to setting
                    if (pointsYouTubeMembers.TryGetValue(membershipName, out string memberPointsString) && long.TryParse(memberPointsString, out long memberPointsValue))
                    {
                        points = memberPointsValue;
                    }
                    else
                    {
                        points = pointsPerMember;
                    }
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
                    gifts = TryGetArgOrDefault("gifts", 1);
                    returnValue = pointsPerKickGift * gifts;
                    break;

                default:
                    break;

            }

            return returnValue;
        }
    }
}