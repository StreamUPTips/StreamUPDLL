using System;
using System.Collections.Generic;
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
        public string UserName {get; set;}
        public string UserId {get; set;}
        public Platform Platform {get; set;}
    }
 

    
}