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

        public void SendMessageToPlatform(string message, Platform platform, bool bot = true, string broadcastId = null)
        {
            bool result = platform switch
            {
                Platform.Twitch => SendToTwitch(message, bot),
                Platform.YouTube => SendToYouTube(message, bot, broadcastId),
                Platform.Trovo => SendToTrovo(message, bot),
                Platform.Kick => SendToKick(message, bot),
                _ => false
            };




        }

        public void SendMessageBack(string message, bool bot = true, string broadcastId = null)
        {
            _CPH.TryGetArg("userType", out string platformString);
            Enum.TryParse(platformString, true, out Platform platform);
            SendMessageToPlatform(message, platform, bot, broadcastId);
        }


        private bool SendToTwitch(string msg, bool bot)
        {
            _CPH.SendMessage(msg, bot, true);
            return true;
        }
        private bool SendToYouTube(string msg, bool bot, string id)
        {
            _CPH.SendYouTubeMessage(msg, bot, true, id);
            return true;
        }
        private bool SendToTrovo(string msg, bool bot)
        {
            _CPH.SendTrovoMessage(msg, bot, true);
            return true;
        }
        private bool SendToKick(string msg, bool bot)
        {
            _CPH.SendKickMessage(msg, bot, true);
            return true;
        }


    }
}