using Streamer.bot.Plugin.Interface.Enums;

namespace StreamUP
{
    public partial class StreamUpLib
    {

        public void SendMessageToAll(string message, bool bot = true)
        {

            _CPH.SendMessage(message,bot);
            _CPH.SendYouTubeMessage(message,bot);
          
        }

        public void SendMessageToPlatform(string message, Platform platform, bool bot = true , string broadcastId = null )
        {
                if(platform == Platform.Twitch)
                {
                     _CPH.SendMessage(message,bot);
           
                }
                if(platform == Platform.YouTube)
                {
                     _CPH.SendYouTubeMessage(message,bot,broadcastId);
                }

        }


    }

}