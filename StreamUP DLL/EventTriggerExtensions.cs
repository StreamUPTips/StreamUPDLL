using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Common.Events;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;

namespace StreamUP {

    public static class EventTriggerExtensions 
    {
        // Process SB events
        public static TriggerData SUSBProcessEvent(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, ProductInfo productInfo, Dictionary<string, object> productSettings) 
        {
            string logName = $"{productInfo.ProductNumber}::SUSBProcessEvent";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Load baseInfo var
            var triggerData = new TriggerData();

            // Load misc vars
            decimal amount;
            string currency;
            string localCurrency;

            switch (CPH.GetEventType())
            {
                case EventType.CommandTriggered:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    string command = sbArgs["command"].ToString();
                    if (triggerData.Message.StartsWith(command))
                    {
                        triggerData.Message = triggerData.Message.Substring(command.Length + 1).TrimStart();
                    }           
                    triggerData.User = sbArgs["user"].ToString();
                    switch (sbArgs["commandSource"].ToString())
                    {
                        case "twitch":
                            triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                            break;
                        case "youtube":
                            triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                            break;
                    }
                    break;
                case EventType.TwitchAnnouncement:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchBotWhisper:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchChatMessage:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchCheer:
                    triggerData.Amount = int.Parse(sbArgs["bits"].ToString());
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.Message = sbArgs["messageCheermotesStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchFirstWord:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchFollow:
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchGiftBomb:
                    triggerData.Amount = int.Parse(sbArgs["gifts"].ToString());
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.TotalAmount = int.Parse(sbArgs["totalGifts"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchGiftSub:
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.MonthsAmount = int.Parse(sbArgs["cumulativeMonths"].ToString());
                    triggerData.MonthsGifted = int.Parse(sbArgs["monthsGifted"].ToString());
                    triggerData.Receiver = sbArgs["recipientUser"].ToString();
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 1);
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.TotalAmount = int.Parse(sbArgs["totalSubsGifted"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchReSub:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.MonthsTotal = int.Parse(sbArgs["cumulative"].ToString());
                    triggerData.MonthsStreak = int.Parse(sbArgs["monthStreak"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchRewardRedemption:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);

                    break;
                case EventType.TwitchSub:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchShoutoutCreated:
                    triggerData.User = sbArgs["targetUserDisplayName"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.TwitchWhisper:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0);
                    break;
                case EventType.YouTubeFirstWords:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = sbArgs["userProfileUrl"].ToString();
                    break;
                case EventType.YouTubeGiftMembershipReceived:
                    triggerData.Receiver = sbArgs["user"].ToString();
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["gifterUser"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);         
                    break;
                case EventType.YouTubeMembershipGift:
                    triggerData.Amount = int.Parse(sbArgs["count"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeMemberMileStone:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.MonthsTotal = int.Parse(sbArgs["months"].ToString());
                    triggerData.Tier = sbArgs["levelName"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeMessage:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = sbArgs["userProfileUrl"].ToString();
                    break;
                case EventType.YouTubeNewSponsor:
                    triggerData.Tier = sbArgs["levelName"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeNewSubscriber:
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeSuperChat:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = productSettings["LocalCurrencyCode"].ToString();
                    triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeSuperSticker:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = productSettings["LocalCurrencyCode"].ToString();
                    triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                default:
                    CPH.SUWriteLog($"The trigger method [{CPH.GetEventType().ToString()}] is not supported. Feel free to open a ticket in the StreamUP Discord and the StreamUP team will try and add it.");
                    DialogResult result = CPH.SUUIShowWarningYesNoMessage($"The trigger method [{CPH.GetEventType().ToString()}] is not supported.\n\nFeel free to open a ticket in the StreamUP Discord and the StreamUP team will try and add it.\n\nWould you like a link to the Discord Server?");
                    if (result == DialogResult.Yes)
                    {
                        Process.Start("https://discord.com/invite/RnDKRaVCEu?");
                    }
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return null;
            }

            // Fix message string
            if (!string.IsNullOrEmpty(triggerData.Message)) {
                triggerData.Message = triggerData.Message
                    .Replace("\\n", "")
                    .Replace("\\r", "")
                    .Replace("\\t", "");
            }

            
            LoadCustomMessage1Vars(CPH, triggerData, productSettings);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return triggerData;
        }
        private static void LoadCustomMessage1Vars(this IInlineInvokeProxy CPH, TriggerData triggerData, Dictionary<string, object> productSettings)
        {
            string triggerType = CPH.GetEventType().ToString();
            string eventTypeKey = triggerType + "CustomMessage1";
            string customMessageValue = null;
            CPH.SUWriteLog($"Checking if productSettings contains the key [{eventTypeKey}]");
            if (productSettings.ContainsKey(eventTypeKey))
            {
                customMessageValue = productSettings[eventTypeKey].ToString();
            }

            CPH.SUWriteLog($"customMessageValue=[{customMessageValue}]");
            if (!string.IsNullOrEmpty(customMessageValue))
            {
                triggerData.CustomMessage1 = customMessageValue
                    .Replace("%user%", triggerData.User)
                    .Replace("%message%", triggerData.Message)
                    .Replace("%tier%", triggerData.Tier)
                    .Replace("%amount%", triggerData.Amount.ToString())
                    .Replace("%amountCurrency%", triggerData.AmountCurrency)
                    .Replace("%monthsAmount%", triggerData.MonthsAmount.ToString())
                    .Replace("%monthsGifted%", triggerData.MonthsGifted.ToString())
                    .Replace("%monthsStreak%", triggerData.MonthsStreak.ToString())
                    .Replace("%monthsTotal%", triggerData.MonthsTotal.ToString())
                    .Replace("%reveiver%", triggerData.Receiver)
                    .Replace("%totalAmount%", triggerData.TotalAmount.ToString());
            }            
        }



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

