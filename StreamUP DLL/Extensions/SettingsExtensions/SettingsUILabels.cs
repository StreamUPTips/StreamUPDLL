using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamUP
{

    public static class SettingsUILabels
    {
        //# General
        public static string Donation = CreateSettingsLabel(new Dictionary<string, string>
                                        {
                                            {"amountCurrency", "Donation Amount"},
                                            {"message", "Donation Message"},
                                            {"user", "Donators Name"}
                                        });

        //# Twitch
        public static string TwitchCheer = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"amount", "Cheer Amount"},
                                                {"message", "Cheer Message"},
                                                {"user", "Cheerers Name"}
                                            });

        public static string TwitchFollow = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"user", "Followers Name"}
                                            });

        public static string TwitchRaid = CreateSettingsLabel(new Dictionary<string, string>
                                        {
                                            {"amount", "Viewer Amount"},
                                            {"user", "Raiders Name"}
                                        });

        public static string TwitchSub = CreateSettingsLabel(new Dictionary<string, string>
                                        {
                                            {"message", "Subscribers Message"},
                                            {"tier", "Sub Tier"},
                                            {"user", "Subscribers Name"}
                                        });

        public static string TwitchReSub = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"message", "Sub Message"},
                                                {"monthsTotal", "Total Months Subscribed"},
                                                {"monthsStreak", "Current Sub Streak"},
                                                {"tier", "Sub Tier"},
                                                {"user", "Subscribers Name"}
                                            });

        public static string TwitchGiftSub = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"monthsTotal", "Total Months Subbed"},
                                                {"monthsGifted", "Amount Of Months Gifted"},
                                                {"receiver", "Sub Receiver"},
                                                {"tier", "Sub Tier"},
                                                {"totalAmount", "Total Subs Gifted"},
                                                {"user", "Gifters Name"}
                                            });

        public static string TwitchGiftBomb = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"amount", "Amount Of Subs Gifted"},
                                                {"tier", "Sub Tier"},
                                                {"totalAmount", "Total Subs Gifted"},
                                                {"user", "Gifters Name"}
                                            });

        public static string TwitchFirstWords = CreateSettingsLabel(new Dictionary<string, string>
                                            {
                                                {"message", "Chat Message"},
                                                {"user", "Chatters Name"}
                                            });

        public static string TwitchRewardRedemption = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"message", "Chat Message"},
                                                        {"user", "Chatters Name"}
                                                    });

        public static string TwitchShoutoutCreated = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"receiver", "User Being Shoutout"},
                                                        {"user", "User Executing Shoutout"}
                                                    });

        public static string TwitchUserBanned = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"reason", "Reason For Ban"},
                                                    {"receiver", "User That Is Banned"},
                                                    {"user", "User Executing The Ban"}
                                                });

        public static string TwitchUserTimedOut = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"duration", "Duration Of Time Out"},
                                                    {"reason", "Reason For Time Out"},
                                                    {"receiver", "User That Is Banned"},
                                                    {"user", "User Executing The Timeout"}
                                                });

        public static string TwitchWatchStreak = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"user", "User with streak streak"},
                                                    {"amount", "Current stream streak amount"}
                                                });

        public static string TwitchHypeTrainLevelUp = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"contributors", "Amount of contributors"},
                                                        {"level", "Hype Train level"},
                                                        {"top.bits.user", "Highest Cheerer Username"},
                                                        {"top.bits.total", "Highest Cheerer Amount"}
                                                    });

        public static string TwitchHypeTrainEnd = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"contributors", "Amount of contributors"},
                                                        {"level", "Hype Train level"},
                                                        {"percent", "percent"},
                                                        {"top.bits.user", "Highest Cheerer Username"},
                                                        {"top.bits.total", "Highest Cheerer Amount"},
                                                        {"top.subscription.user", "Highest Sub Gifter"},
                                                        {"top.subscription.total", "Highest Sub Amount (in point value) [T1 = 500, T2 = 1000, T3 = 2500]"}
                                                    });

        //# YouTube
        public static string YouTubeFirstWords = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"message", "Chat Message"},
                                                    {"user", "Chatters Name"}
                                                });

        public static string YouTubeNewSubscriber = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"user", "Username"}
                                                    });

        public static string YouTubeNewSponsor = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"tier", "Membership tier"},
                                                    {"user", "Username"}
                                                });

        public static string YouTubeMemberMileStone = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"message", "Chat Message"},
                                                        {"monthsTotal", "Total Months Being A Member"},
                                                        {"tier", "Membership Tier"},
                                                        {"user", "Username"}
                                                    });

        public static string YouTubeMembershipGift = CreateSettingsLabel(new Dictionary<string, string>
                                                    {
                                                        {"amount", "Amount Of Memberships Gifted"},
                                                        {"tier", "Membership Tier"},
                                                        {"user", "Username"}
                                                    });

        public static string YouTubeSuperChat = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"amountCurrency", "SuperChat Amount"},
                                                    {"message", "Chat Message"},
                                                    {"user", "Username"}
                                                });

        public static string YouTubeSuperSticker = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"amountCurrency", "SuperSticker Amount"},
                                                    {"user", "Username"}
                                                });

        public static string YouTubeUserBanned = CreateSettingsLabel(new Dictionary<string, string>
                                                {
                                                    {"duration", "Duration Of Ban"},
                                                    {"banType", "Ban Type"},
                                                    {"user", "Username"}
                                                });

        //# Products

        public static string CurrencyCore = CreateSettingsLabel(new Dictionary<string, string>{
                                                                {"user", "The User who used the command"},
                                                                {"points","The Number of points a person has"},
                                                                {"pointsName", "The name of the points"},
                                                                {"target", "The user whose points are been adjusted"},
                                                                {"addAmount", "Amount that is been added"},
                                                                {"setAmount", "The amount the points are been set to"},
                                                                {"count", "The amount of users been adjusted."},
                                                                });

        public static string ModAdded = CreateSettingsLabel(new Dictionary<string, string>{
                                                                {"user", "The one who uses the command"},
                                                                {"commandName", "The name of said command"},
                                                                {"countName","The name of said counter"},
                                                                {"countValue", "The Value of said counter"}
                                                                });
        public static string CustomTimed = CreateSettingsLabel(new Dictionary<string, string>{
                                                                {"user", "The User who used the command"},
                                                                {"timerId","The ID of the Timer"},
                                                                {"timerTime", "How often the Timer will post"},
                                                                {"timerMessage", "The Message of the Timer"},
                                                                });



        public static string CreateSettingsLabel(Dictionary<string, string> data)
        {
            string dataString = "Variables Available: \n";

            foreach (KeyValuePair<string, string> variable in data)
            {

                dataString += $"%{variable.Key}% = {variable.Value} \n";


            }

            return dataString;
        }
    }

}
