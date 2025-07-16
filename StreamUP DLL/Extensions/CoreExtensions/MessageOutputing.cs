using System;
using Streamer.bot.Plugin.Interface.Enums;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public void SendMessageToAll(string message, bool bot = true)
        {

            _CPH.SendMessage(message, bot);
            _CPH.SendYouTubeMessage(message, bot, true, null); //SendYouTubeMessage(string message, bool useBot = true, bool fallback = true, string broadcastId = null)
            _CPH.SendTrovoMessage(message, bot, true);
            _CPH.SendKickMessage(message, bot, true);
          
        }

        public void SendMessageToPlatform(string message, Platform platform, bool bot = true , string broadcastId = null )
        {
               bool result = platform switch
            {
                Platform.Twitch => _CPH.SendMessage(message, bot,true);
                Platform.YouTube =>  _CPH.SendYouTubeMessage(message,bot, true, broadcastId),
                Platform.Trovo =>   _CPH.SendTrovoMessage(message, bot, true),
                Platform.Kick =>  _CPH.SendKickMessage(message, bot, true)
                _ => null
            };
       
      
           

        }

         public void SendMessageBack(string message, bool bot = true, string broadcastId = null)
        {
            _CPH.TryGetArg("userType", out string platformString);
            Enum.TryParse(platformString, true, out Platform platform);
            SendMessageToPlatform(message, platform, bot, broadcastId);
        }
    }

}