using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Common.Events;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace StreamUP {

    public static class EventTriggerExtensions 
    {
        // Get Twitch profile picture
        public static string SUSBGetTwitchProfilePicture(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, string productNumber, int userType)
        {
            // Load log string
            string logName = "EventTriggerExtensions-SUSBGetTwitchProfilePicture";
            CPH.SUWriteLog("Method Started", logName);

            // pull requested sizes '50, 70, 150, 300'
            int userImageSize = CPH.GetGlobalVar<int>($"{productNumber}_ProfileImageSize");

            if (userImageSize <= 0)
            {
                userImageSize = 300;
            }

            // Get profile picture
            string userCheck = "";
            switch (userType)
            {
                case 0:
                    userCheck = "userId";
                    break;
                case 1:
                    userCheck = "recipientId";
                    break;
            }
            var userInfo = CPH.TwitchGetExtendedUserInfoById(sbArgs[$"{userCheck}"].ToString());
            string originalImage = userInfo.ProfileImageUrl;
            string basePattern = "300x300";

            // Replace the base pattern with the new size
            string newSizePattern = $"{userImageSize}x{userImageSize}";
            string image = Regex.Replace(originalImage, basePattern, newSizePattern);

            // Log created image URL
            CPH.SUWriteLog($"Created {newSizePattern} profile image: image=[{image}]", logName);

            CPH.SUWriteLog("Method complete", logName);
            return image;
        }
            
        // Check if youtube profile pic arg exists
        private static string SUSBCheckYouTubeProfileImageArgs(this IInlineInvokeProxy CPH)
        {
            if (!CPH.TryGetArg("userProfileUrl", out string profileImage))
            {
                profileImage = "https://www.tea-tron.com/antorodriguez/blog/wp-content/uploads/2016/04/Image-Not-Found1.png";
            }
            return profileImage;
        }

        // Process SB events
        public static TriggerData SUSBProcessEvent(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, string productNumber) {
            // Load baseInfo var
            var baseInfo = new TriggerData();

            // Load misc vars
            decimal amount;
            string currency;
            string localCurrency;

            switch (CPH.GetEventType())
            {
                case EventType.CommandTriggered:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    string command = sbArgs["command"].ToString();
                    if (baseInfo.Message.StartsWith(command))
                    {
                        baseInfo.Message = baseInfo.Message.Substring(command.Length + 1).TrimStart();
                    }           
                    baseInfo.User = sbArgs["user"].ToString();
                    switch (sbArgs["commandSource"].ToString())
                    {
                        case "twitch":
                            baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                            break;
                        case "youtube":
                            baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                            break;
                    }
                    break;
                case EventType.TwitchAnnouncement:
                    baseInfo.Message = sbArgs["messageStripped"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchBotWhisper:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchChatMessage:
                    baseInfo.Message = sbArgs["messageStripped"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchCheer:
                    baseInfo.Amount = int.Parse(sbArgs["bits"].ToString());
                    baseInfo.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    baseInfo.Message = sbArgs["messageCheermotesStripped"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchFirstWord:
                    baseInfo.Message = sbArgs["messageStripped"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchFollow:
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchGiftBomb:
                    baseInfo.Amount = int.Parse(sbArgs["gifts"].ToString());
                    baseInfo.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.TotalAmount = int.Parse(sbArgs["totalGifts"].ToString());
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchGiftSub:
                    baseInfo.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    baseInfo.MonthsAmount = int.Parse(sbArgs["cumulativeMonths"].ToString());
                    baseInfo.MonthsGifted = int.Parse(sbArgs["monthsGifted"].ToString());
                    baseInfo.Receiver = sbArgs["recipientUser"].ToString();
                    baseInfo.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 1);
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.TotalAmount = int.Parse(sbArgs["totalSubsGifted"].ToString());
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchReSub:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    baseInfo.MonthsTotal = int.Parse(sbArgs["cumulative"].ToString());
                    baseInfo.MonthsStreak = int.Parse(sbArgs["monthStreak"].ToString());
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchRewardRedemption:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);

                    break;
                case EventType.TwitchSub:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchShoutoutCreated:
                    baseInfo.User = sbArgs["targetUserDisplayName"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.TwitchWhisper:
                    baseInfo.Message = sbArgs["rawInput"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productNumber, 0);
                    break;
                case EventType.YouTubeFirstWords:
                    baseInfo.Message = sbArgs["message"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = sbArgs["userProfileUrl"].ToString();
                    break;
                case EventType.YouTubeGiftMembershipReceived:
                    baseInfo.Receiver = sbArgs["user"].ToString();
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.User = sbArgs["gifterUser"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);         
                    break;
                case EventType.YouTubeMembershipGift:
                    baseInfo.Amount = int.Parse(sbArgs["count"].ToString());
                    baseInfo.Tier = sbArgs["tier"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeMemberMileStone:
                    baseInfo.Message = sbArgs["message"].ToString();
                    baseInfo.MonthsTotal = int.Parse(sbArgs["months"].ToString());
                    baseInfo.Tier = sbArgs["levelName"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeMessage:
                    baseInfo.Message = sbArgs["message"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = sbArgs["userProfileUrl"].ToString();
                    break;
                case EventType.YouTubeNewSponsor:
                    baseInfo.Tier = sbArgs["levelName"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeNewSubscriber:
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeSuperChat:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = CPH.GetGlobalVar<string>("sup000_LocalCurrencyCode", true);
                    baseInfo.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    baseInfo.Message = sbArgs["message"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeSuperSticker:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = CPH.GetGlobalVar<string>("sup000_LocalCurrencyCode", true);
                    baseInfo.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                default:
                    CPH.SUWriteLog($"The trigger method [{CPH.GetEventType().ToString()}] is not supported. Feel free to open a ticket and the StreamUP team will try and add it.");
                    break;
            }

            // Fix message string
            if (!string.IsNullOrEmpty(baseInfo.Message)) {
                baseInfo.Message = baseInfo.Message
                    .Replace("\\n", "")
                    .Replace("\\r", "")
                    .Replace("\\t", "");
            }

            LoadCustomMessage1Vars(CPH, productNumber, baseInfo);

            return baseInfo;
        }
        private static void LoadCustomMessage1Vars(this IInlineInvokeProxy CPH, string productNumber, TriggerData baseInfo)
        {
            baseInfo.CustomMessage1 = CPH.GetGlobalVar<string>($"{productNumber}_{CPH.GetEventType().ToString()}CustomMessage1");
            if (!string.IsNullOrEmpty(baseInfo.CustomMessage1))
            {
                baseInfo.CustomMessage1 = baseInfo.CustomMessage1
                    .Replace("%user%", baseInfo.User)
                    .Replace("%message%", baseInfo.Message)
                    .Replace("%tier%", baseInfo.Tier)
                    .Replace("%amount%", baseInfo.Amount.ToString())
                    .Replace("%amountCurrency%", baseInfo.AmountCurrency)
                    .Replace("%monthsAmount%", baseInfo.MonthsAmount.ToString())
                    .Replace("%monthsGifted%", baseInfo.MonthsGifted.ToString())
                    .Replace("%monthsStreak%", baseInfo.MonthsStreak.ToString())
                    .Replace("%monthsTotal%", baseInfo.MonthsTotal.ToString())
                    .Replace("%reveiver%", baseInfo.Receiver)
                    .Replace("%totalAmount%", baseInfo.TotalAmount.ToString());
            }            
        }

        // Queue system
        public static bool SUSBSaveTriggerQueueToGlobalVar(this IInlineInvokeProxy CPH, Queue<TriggerData> myQueue, string varName, bool persisted)
        {
            string logName = "EventTriggerExtensions-SUSBSaveTriggerQueueToGlobalVar";
            CPH.SUWriteLog("Method Started", logName);

            // Serialize the queue to a JSON string
            var array = myQueue.ToArray();
            string jsonString = JsonConvert.SerializeObject(array);

            // Optionally, log the jsonString to verify its content
            CPH.SUWriteLog($"Serialized JSON: {jsonString}", logName);

            // Set the global variable
            CPH.SetGlobalVar(varName, jsonString, persisted);
            CPH.SUWriteLog("Method Complete", logName);

            return true;
        }
        public static Queue<TriggerData> SUSBGetTriggerQueueFromGlobalVar(this IInlineInvokeProxy CPH, string varName, bool persisted)
        {
            // Load log string
            string logName = "EventTriggerExtensions-SUSBGetTriggerQueueFromGlobalVar";
            CPH.SUWriteLog("Method Started", logName);

            // Retrieve the JSON string from the global variable
            string jsonString = CPH.GetGlobalVar<string>(varName, persisted);

            if (string.IsNullOrEmpty(jsonString))
            {
                CPH.SUWriteLog("JSON string is null or empty, returning a new empty Queue<TriggerData>.", logName);
                return new Queue<TriggerData>();
            }

            // Try to convert the JSON string back to a List<TriggerData>, then to a Queue
            try
            {
                var list = JsonConvert.DeserializeObject<List<TriggerData>>(jsonString);
                Queue<TriggerData> myQueue = new Queue<TriggerData>(list);

                CPH.SUWriteLog("Successfully deserialized and converted to Queue<TriggerData>.", logName);
                return myQueue;
            }
            catch (JsonException e)
            {
                CPH.SUWriteLog($"Failed to deserialize JSON: {e.Message}", logName);
                // Handle error (e.g., by returning an empty queue or re-throwing the exception)
                return new Queue<TriggerData>(); // Return an empty queue as a fallback
            }
        }


    }
        // SB events trigger data
        public class TriggerData 
        {
            public int Amount { get; set; }
            public string AmountCurrency { get; set; }
            public bool Anonymous { get; set; }
            public string CustomMessage1 { get; set; }
            public string EventSource { get; set; }
            public string EventType { get; set; }
            public string Message { get; set; }
            public int MonthsAmount { get; set; }
            public int MonthsGifted { get; set; }
            public int MonthsStreak { get; set; }
            public int MonthsTotal { get; set; }
            public string Receiver { get; set; }
            public string ReceiverImage { get; set; }
            public string Tier { get; set; }
            public int TotalAmount { get; set; }
            public string User { get; set; }
            public string UserImage { get; set; }
        }
}

