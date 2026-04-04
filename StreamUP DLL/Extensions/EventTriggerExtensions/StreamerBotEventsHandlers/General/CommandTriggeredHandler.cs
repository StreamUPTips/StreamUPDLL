using System.Collections.Generic;

namespace StreamUP
{
    public class CommandTriggeredHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;
            
            // Get message
            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "rawInput", "No arg 'rawInput' found");

            // Get command that was triggers
            string command = SUP.GetValueOrDefault<string>(sbArgs, "command", "No arg 'command' found");

            // Trim message if it starts with the command
            if (triggerData.Message.StartsWith(command))
            {
                triggerData.Message = triggerData.Message.Substring(command.Length + 1).TrimStart();
            }           

            // Set username
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");

            // Get platform source of the command
            string commandSource = SUP.GetValueOrDefault<string>(sbArgs, "commandSource", null);

            // Get profile picture
            switch (commandSource)
            {
                case "twitch":
                    triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);
                    break;
                case "youtube":
                    triggerData.UserImage = SUP.GetYouTubeProfilePicture(sbArgs);                
                    break;
            }
            return triggerData;
        }
    }
}
