using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamUP
{

    public static class SettingsUILabels
    {
        //# Misc
        public static string Donation = "%amountCurrency% = Donation Amount\n" +
                                        "%message% = Donation Message\n" +
                                        "%user% = Donators Name";



        //# Twitch
        public static string TwitchCheer = "%amount% = Cheer Amount\n" +
                                            "%message% = Cheer Message\n" +
                                            "%user% = Cheerers Name";

        public static string TwitchFollow = "%user% = Followers Name";

        public static string TwitchRaid = "%amount% = Viewer Amount\n" +
                                            "%user% = Raiders Name";

        public static string TwitchSub = "%message% = Subscribers Message\n" +
                                            "%tier% = Sub Tier\n" +
                                            "%user% = Subscribers Name";

        public static string TwitchReSub = "%message% = Sub Message\n" +
                                            "%monthsTotal% = Total Months Subscribed\n" +
                                            "%monthsStreak% = Current Sub Streak\n" +
                                            "%tier% = Sub Tier\n" +
                                            "%user% = Subscribers Name";

        public static string TwitchGiftSub = "%monthsTotal% = Total Months Subbed\n" +
                                                "%monthsGifted% = Amount Of Months Gifted\n" +
                                                "%receiver% = Sub Receiver\n" +
                                                "%tier% = Sub Tier\n" +
                                                "%totalAmount% = Total Subs Gifted\n" +
                                                "%user% = Gifters Name";

        public static string TwitchGiftBomb = "%amount% = Amount Of Subs Gifted\n" +
                                                "%tier% = Sub Tier\n" +
                                                "%totalAmount% = Total Subs Gifted\n" +
                                                "%user% = Gifters Name";

        public static string TwitchFirstWords = "%message% = Chat Message\n" +
                                                "%user% = Chatters Name";


        public static string TwitchRewardRedemption = "%message% = Chat Message\n" +
                                                        "%user% = Chatters Name";

        public static string TwitchShoutoutCreated = "%receiver% = User Being Shoutout\n" +
                                                        "%user% = User Executing Shoutout";

        public static string TwitchUserBanned = "%reason% = Reason For Ban\n" +
                                                "%receiver% = User That Is Banned\n" +
                                                "%user% = User Executing The Ban";

        public static string TwitchUserTimedOut = "%duration% = Duration Of Time Out\n" +
                                                    "%reason% = Reason For Time Out\n" +
                                                    "%receiver% = User That Is Banned\n" +
                                                    "%user% = User Executing The Timeout";

        public static string TwitchWatchStreak = "%user% = User with streak streak\n" +
                                                    "%amount% = Current stream streak amount";

        public static string TwitchHypeTrainLevelUp = "%contributors% = Amount of contributors\n" +
                                                        "%level% = Hype Train level\n" +
                                                        "%top.bits.user% = Highest Cheerer Username\n" +
                                                        "%top.bits.total% = Highest Cheerer Amount";

        public static string TwitchHypeTrainEnd = "%contributors% = Amount of contributors\n" +
                                                    "%level% = Hype Train level\n" +
                                                    "%percent% = percent\n" +
                                                    "%top.bits.user% = Highest Cheerer Username\n" +
                                                    "%top.bits.total% = Highest Cheerer Amount\n" +
                                                    "%top.subscription.user% = Highest Sub Gifter\n" +
                                                    "%top.subscription.total% = Highest Sub Amount (in point value) [T1 = 500, T2 = 1000, T3 = 2500]";



        //# YouTube                                    
        public static string YouTubeFirstWords = "%message% = Chat Message\n" +
                                                    "%user% = Chatters Name";

        public static string YouTubeNewSubscriber = "%user% = Username";

        public static string YouTubeNewSponsor = "%tier% = Membership tier\n" +
                                                    "%user% = Username";

        public static string YouTubeMemberMileStone = "%message% = Chat Message\n" +
                                                        "%monthsTotal% = Total Months Being A Member\n" +
                                                        "%tier% = Membership Tier\n" +
                                                        "%user% = Username";

        public static string YouTubeMembershipGift = "%amount% = Amount Of Memberships Gifted\n" +
                                                        "%tier% = Membership Tier\n" +
                                                        "%user% = Username";

        public static string YouTubeSuperChat = "%amountCurrency% = SuperChat Amount\n" +
                                                "%message% = Chat Message\n" +
                                                "%user% = Username";

        public static string YouTubeSuperSticker = "%amountCurrency% = SuperSticker Amount\n" +
                                                    "%user% = Username";

        public static string YouTubeUserBanned = "%duration% = Duration Of Ban\n" +
                                                    "%banType% = Ban Type\n" +
                                                    "%user% = Username";

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
