using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Common.Events;
using System.Text.RegularExpressions;

namespace StreamUP {

    public static class EventTriggerExtensions 
    {
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
                    baseInfo.Amount = int.Parse(sbArgs["bits"].ToString());
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
                    baseInfo.AmountCurrency = CPH.SUCurrencyConverter(amount, currency, localCurrency);
                    baseInfo.Message = sbArgs["message"].ToString();
                    baseInfo.User = sbArgs["user"].ToString();
                    baseInfo.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH);                
                    break;
                case EventType.YouTubeSuperSticker:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = CPH.GetGlobalVar<string>("sup000_LocalCurrencyCode", true);
                    baseInfo.AmountCurrency = CPH.SUCurrencyConverter(amount, currency, localCurrency);
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
            return baseInfo;
        }

        private static string SUSBCheckYouTubeProfileImageArgs(this IInlineInvokeProxy CPH)
        {
            if (!CPH.TryGetArg("userProfileUrl", out string profileImage))
            {
                profileImage = "https://www.tea-tron.com/antorodriguez/blog/wp-content/uploads/2016/04/Image-Not-Found1.png";
            }
            return profileImage;
        }

    }

        public class TriggerData 
        {
            public int Amount { get; set; }
            public string AmountCurrency { get; set; }
            public bool Anonymous { get; set; }
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

