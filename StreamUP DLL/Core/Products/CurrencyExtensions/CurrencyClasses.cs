using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class LeaderboardUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public long Points { get; set; }
        public int Position { get; set; }
    }

    public class YouTubeUser
    {
        public string UserId { get; set; }
        public string UserName { get; set; }

    }

    [Serializable()]
    public class CurrencyUser
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public Platform Platform { get; set; }
    }

    public class CurrencySettings
    {
        [JsonProperty("Points_Name")]
        public string Points_Name { get; set; } = "Points";
        [JsonProperty("Points_Var")]
        public string Points_Var { get; set; } = "points";
        [JsonProperty("Default_Points")]
        public int Default_Points { get; set; } = 200;
        [JsonProperty("User_Boosts")]
        public bool User_Boosts { get; set; } = false;
        [JsonProperty("Boosted_Group")]
        public string Boosted_Group { get; set; } = "StreamUP Currency Boosted Users";
        [JsonProperty("Sub_Boost")]
        public bool Sub_Boost { get; set; } = false;
        [JsonProperty("Boost_Amount")]
        public double Boost_Amount { get; set; } = 1.5;
        [JsonProperty("Exclude_Users")]
        public bool Exclude_Users { get; set; } = false;
        [JsonProperty("Excluded_Group")]
        public string Excluded_Group { get; set; } = "Points Excluded";
        [JsonProperty("Generic_MinChat")]
        public int Generic_MinChat { get; set; } = 10;
        [JsonProperty("Generic_MaxChat")]
        public int Generic_MaxChat { get; set; } = 20;
        [JsonProperty("Generic_ChatCooldown")]
        public int Generic_ChatCooldown { get; set; } = 30;
        [JsonProperty("Generic_FirstWords")]
        public int Generic_FirstWords { get; set; } = 50;
        [JsonProperty("Generic_PresentViewers")]
        public int Generic_PresentViewers { get; set; } = 25;
        [JsonProperty("Generic_DollarTipped")]
        public int Generic_DollarTipped { get; set; } = 100;
        [JsonProperty("Twitch_Enable")]
        public bool Twitch_Enable { get; set; } = false;
        [JsonProperty("Twitch_Follow")]
        public int Twitch_Follow { get; set; } = 100;
        [JsonProperty("Twitch_Bits")]
        public int Twitch_Bits { get; set; } = 1;
        [JsonProperty("Twitch_Raid")]
        public int Twitch_Raid { get; set; } = 200;
        [JsonProperty("Twitch_Viewer")]
        public int Twitch_Viewer { get; set; } = 10;
        [JsonProperty("Twitch_T1")]
        public int Twitch_T1 { get; set; } = 500;
        [JsonProperty("Twitch_T2")]
        public int Twitch_T2 { get; set; } = 800;
        [JsonProperty("Twitch_T3")]
        public int Twitch_T3 { get; set; } = 2000;
        [JsonProperty("Twitch_Month")]
        public int Twitch_Month { get; set; } = 10;
        [JsonProperty("Twitch_Gift")]
        public int Twitch_Gift { get; set; } = 500;
        [JsonProperty("Twitch_BoostGifts")]
        public bool Twitch_BoostGifts { get; set; } = true;
        [JsonProperty("YouTube_Enable")]
        public bool YouTube_Enable { get; set; } = false;
        [JsonProperty("YouTube_Sub")]
        public int YouTube_Sub { get; set; } = 100;
        [JsonProperty("Youtube_Memberships")]
        public Dictionary<string, string> Youtube_Memberships { get; set; } = new Dictionary<string, string>();
        [JsonProperty("YouTube_NotFound")]
        public int YouTube_NotFound { get; set; } = 500;
        [JsonProperty("YouTube_Months")]
        public int YouTube_Months { get; set; } = 0;
        [JsonProperty("YouTube_Gift")]
        public int YouTube_Gift { get; set; } = 500;
        [JsonProperty("YouTube_BoostGifts")]
        public bool YouTube_BoostGifts { get; set; } = false;
        [JsonProperty("Kick_Enable")]
        public bool Kick_Enable { get; set; } = false;
        [JsonProperty("Kick_Follower")]
        public int Kick_Follower { get; set; } = 100;
        [JsonProperty("Kick_Sub")]
        public int Kick_Sub { get; set; } = 300;
        [JsonProperty("Kick_Month")]
        public int Kick_Month { get; set; } = 10;
        [JsonProperty("Kick_Gift")]
        public int Kick_Gift { get; set; } = 300;
    }

}