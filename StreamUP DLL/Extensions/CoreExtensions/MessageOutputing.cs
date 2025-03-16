using System;
using Streamer.bot.Plugin.Interface.Enums;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public void SendMessageToAll(string message, bool bot = true)
        {

            _CPH.SendMessage(message,bot);
            _CPH.SendYouTubeMessage(message,bot,true, null); //SendYouTubeMessage(string message, bool useBot = true, bool fallback = true, string broadcastId = null)
          
        }

        public void SendMessageToPlatform(string message, Platform platform, bool bot = true , string broadcastId = null )
        {
                if(platform == Platform.Twitch)
                {
                     _CPH.SendMessage(message,bot);
           
                }
                if(platform == Platform.YouTube)
                {
                     
                     _CPH.SendYouTubeMessage(message,bot, true, broadcastId); //SendYouTubeMessage(string message, bool useBot = true, bool fallback = true, string broadcastId = null)
                }

        }

         public void SendMessageBack(string message, bool bot = true, string broadcastId = null)
        {

            _CPH.TryGetArg("userType", out string platformString);
            Enum.TryParse(platformString, true, out Platform platform);
            if (platform == Platform.Twitch)
            {
                _CPH.SendMessage(message, bot);

            }
            if (platform == Platform.YouTube)
            {
               _CPH.SendYouTubeMessage(message,bot, true, broadcastId);
            }

        }



    }

}