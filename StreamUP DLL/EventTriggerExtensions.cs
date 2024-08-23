using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Common.Events;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Diagnostics;
using System.Text;
using Streamer.bot.Plugin.Interface.Model;
using System.Globalization;
using Streamer.bot.Plugin.Interface.Enums;

namespace StreamUP {

    public static class EventTriggerExtensions 
    {
        // Process SB events
        public static TriggerData SUSBProcessEvent(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, ProductInfo productInfo, Dictionary<string, object> productSettings, string settingsGlobalName = "ProductSettings") 
        {
            string logName = $"{productInfo.ProductNumber}::SUSBProcessEvent";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // Load baseInfo var
            var triggerData = new TriggerData();

            // Load misc vars
            decimal amount;
            double amountCurrencyDouble;
            string currency;
            string localCurrency;
            string cleanedAmount;
            string defaultDonationImageUrl;
            string triggerType = CPH.GetEventType().ToString();
            triggerData.EventType = triggerType;
            
            // Try load other product settings
            string otherSettingsLoad = CPH.GetGlobalVar<string>($"{productInfo.ProductNumber}_{settingsGlobalName}", true);
            Dictionary<string, object> otherSettingsDict = null;
            if (!string.IsNullOrEmpty(otherSettingsLoad) || otherSettingsLoad != "null")
            {
                otherSettingsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(otherSettingsLoad);
            }

            CPH.SUWriteLog($"Processing trigger type [{triggerType}]", logName);

            switch (CPH.GetEventType())
            {
                // TEST
                case EventType.Test:
                    triggerData.AlertMessage = "This is a test trigger";
                    triggerData.Amount = 69;
                    triggerData.AmountCurrency = "£4.20";
                    triggerData.AmountCurrencyDouble = 4.20;
                    triggerData.Anonymous = false;
                    triggerData.BanDuration = 69;
                    triggerData.BanType = "You were too awesome";
                    triggerData.Donation = false;
                    triggerData.EventSource = "Test";
                    triggerData.EventType = "Test";
                    triggerData.Message = "This is a test trigger";
                    triggerData.MonthsGifted = 1;
                    triggerData.MonthsStreak = 69;
                    triggerData.MonthsTotal = 88;
                    triggerData.Tier = "tier 3";
                    triggerData.TotalAmount = 420;

                    List<string> usernames = new List<string> { "Andilippi", "WaldoAndFriends", "Silverlink" };
                    Random random = new Random();
                    int index = random.Next(usernames.Count);
                    triggerData.User = usernames[index];
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings, triggerData.User);
                    usernames.RemoveAt(index);

                    index = random.Next(usernames.Count);
                    triggerData.Receiver = usernames[index];
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.recipientId, productSettings, triggerData.Receiver);                   
                    break;                
                // CORE
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
                            triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                            break;
                        case "youtube":
                            triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                            break;
                    }
                    break;
                case EventType.HotKeyPress:                   
                    break;
                case EventType.ChatWindowAction:
                case 0:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["displayName"].ToString();
                    switch (sbArgs["userType"].ToString())
                    {
                        case "twitch":
                            triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                            break;
                        case "youtube":
                            triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                            break;
                    }
                    break;
                case EventType.SpeechToTextCommand:
                    triggerData.Message = sbArgs["spokenText"].ToString();
                    break;
                case EventType.StreamDeckAction:
                    break;
                case EventType.TimedAction:
                    break;
                // DONATIONS
                case EventType.FourthwallDonation:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["username"].ToString();
                    triggerData.Message = sbArgs["message"].ToString();

                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        amount = decimal.Parse(sbArgs["amount"].ToString());
                        currency = sbArgs["currency"].ToString();
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }    
                    }

                    defaultDonationImageUrl = "https://fourthwall.com/homepage/static/logo-aae6bab7310025c5a3da5ed8acd67a8d.png";
                    triggerData.UserImage = SUSBGetDonationUserImage(CPH, productInfo, triggerType, defaultDonationImageUrl);                
                    break;
                case EventType.KofiDonation:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["from"].ToString();
                    triggerData.Message = sbArgs["message"].ToString();

                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();
                        amount = decimal.Parse(sbArgs["amount"].ToString());
                        currency = sbArgs["currency"].ToString();
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }                     
                    }

                    defaultDonationImageUrl = "https://wiki.streamer.bot/ko-fi_icon_rgb_rounded.png";
                    triggerData.UserImage = SUSBGetDonationUserImage(CPH, productInfo, triggerType, defaultDonationImageUrl);                
                    break;
                case EventType.StreamElementsTip:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["tipUsername"].ToString();
                    triggerData.Message = sbArgs["tipMessage"].ToString();

                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        amount = decimal.Parse(sbArgs["tipAmount"].ToString());
                        currency = sbArgs["tipCurrency"].ToString();
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();         
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }                     
                    }

                    defaultDonationImageUrl = "https://streamer.bot/img/integrations/streamelements.png";
                    triggerData.UserImage = SUSBGetDonationUserImage(CPH, productInfo, triggerType, defaultDonationImageUrl);                
                    break;
                case EventType.StreamlabsDonation:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["donationFrom"].ToString();
                    triggerData.Message = sbArgs["donationMessage"].ToString();

                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        amount = decimal.Parse(sbArgs["donationAmount"].ToString());
                        currency = sbArgs["donationCurrency"].ToString();
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }                     
                    }

                    defaultDonationImageUrl = "https://streamer.bot/img/integrations/streamlabs.png";
                    triggerData.UserImage = SUSBGetDonationUserImage(CPH, productInfo, triggerType, defaultDonationImageUrl);                
                    break;
                case EventType.TipeeeStreamDonation:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["username"].ToString();
                    triggerData.Message = sbArgs["message"].ToString();

                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        amount = decimal.Parse(sbArgs["amount"].ToString());
                        currency = sbArgs["currency"].ToString();
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }                     
                    }

                    defaultDonationImageUrl = "https://streamer.bot/img/integrations/tipeestream.png";
                    triggerData.UserImage = SUSBGetDonationUserImage(CPH, productInfo, triggerType, defaultDonationImageUrl);                
                    break;
                case EventType.DonorDriveDonation:
                    triggerData.Donation = true;
                    triggerData.User = sbArgs["donorName"].ToString();
                    triggerData.Message = sbArgs["donorMessage"].ToString();
                    if (productSettings.ContainsKey("LocalCurrencyCode"))
                    {
                        amount = decimal.Parse(sbArgs["donorAmount"].ToString());
                        currency = "USD"; // I'm forcing this to USD as I assume that all currency is in USD from DonorDrive
                        localCurrency = productSettings["LocalCurrencyCode"].ToString();
                        triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                        cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                        if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                        {
                            triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                        }                     
                    }

                    CPH.TryGetArg("donorAvatarUrl", out string donorAvatarUrlInput);
                    string donorAvatarUrl = (!string.IsNullOrEmpty(donorAvatarUrlInput)) ? donorAvatarUrlInput : "https://static.donordrive.com/clients/try/img/avatar-constituent-default.gif";
                    triggerData.UserImage = donorAvatarUrl;
                    break;
                // TWITCH
                case EventType.TwitchAnnouncement:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchBotWhisper:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchChatMessage:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchCheer:
                    triggerData.Amount = int.Parse(sbArgs["bits"].ToString());
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.Message = sbArgs["messageCheermotesStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchFirstWord:
                    triggerData.Message = sbArgs["messageStripped"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchFollow:
                    if (productSettings.ContainsKey("TwitchFollowAnonymous"))
                    {
                        if (bool.Parse(productSettings["TwitchFollowAnonymous"].ToString()))
                        {
                            triggerData.User = productSettings["TwitchFollowAnonymousName"].ToString();
                            triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.broadcastUserId, productSettings);
                        }
                        else
                        {
                            triggerData.User = sbArgs["user"].ToString();
                            triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                        }
                    }
                    else
                    {
                        triggerData.User = sbArgs["user"].ToString();
                        triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    }

                    break;
                case EventType.TwitchGiftBomb:
                    triggerData.Amount = int.Parse(sbArgs["gifts"].ToString());
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.TotalAmount = int.Parse(sbArgs["totalGifts"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchGiftSub:
                    triggerData.Anonymous = bool.Parse(sbArgs["anonymous"].ToString());
                    triggerData.MonthsTotal = int.Parse(sbArgs["cumulativeMonths"].ToString());
                    triggerData.MonthsGifted = int.Parse(sbArgs["monthsGifted"].ToString());
                    triggerData.Receiver = sbArgs["recipientUser"].ToString();
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.recipientId, productSettings);
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.TotalAmount = int.Parse(sbArgs["totalSubsGifted"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    break;
                case EventType.TwitchRaid:
                    triggerData.Amount = int.Parse(sbArgs["viewers"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;         
                case EventType.TwitchReSub:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.MonthsTotal = int.Parse(sbArgs["cumulative"].ToString());
                    triggerData.MonthsStreak = int.Parse(sbArgs["monthStreak"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchRewardRedemption:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;

                /*
                case EventType.TwitchAutomaticRewardRedemption:
                    switch (sbArgs["rewardType"].ToString())
                    {
                        case "chosen_sub_emote_unlock":
                            break;
                        case "send_highlighted_message":
                            break;
                    }
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                */
                
                case EventType.TwitchSub:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                case EventType.TwitchShoutoutCreated:
                    triggerData.Receiver = sbArgs["targetUserDisplayName"].ToString();
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.targetUserId, productSettings);
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    break;
                case EventType.TwitchUserBanned:
                    triggerData.BanType = string.IsNullOrEmpty(sbArgs["reason"].ToString()) ? "No Reason" : sbArgs["reason"].ToString();
                    triggerData.Receiver = sbArgs["user"].ToString();
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    triggerData.User = sbArgs["createdByDisplayName"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.createdById, productSettings);
                    break;
                case EventType.TwitchUserTimedOut:
                    triggerData.BanDuration = int.Parse(sbArgs["duration"].ToString());
                    triggerData.BanType = string.IsNullOrEmpty(sbArgs["reason"].ToString()) ? "No Reason" : sbArgs["reason"].ToString();
                    triggerData.Receiver = sbArgs["user"].ToString();
                    triggerData.ReceiverImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    triggerData.User = sbArgs["createdByDisplayName"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.createdById, productSettings);
                    break;
                case EventType.TwitchWatchStreak:
                    triggerData.Amount = int.Parse(sbArgs["watchStreak"].ToString());
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, TwitchProfilePictureUserType.userId, productSettings);
                    break;
                case EventType.TwitchWhisper:
                    triggerData.Message = sbArgs["rawInput"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = CPH.SUSBGetTwitchProfilePicture(sbArgs, productInfo.ProductNumber, 0, productSettings);
                    break;
                // YOUTUBE
                case EventType.YouTubeFirstWords:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeGiftMembershipReceived:
                    triggerData.Receiver = sbArgs["user"].ToString();
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["gifterUser"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeMembershipGift:
                    triggerData.Amount = int.Parse(sbArgs["count"].ToString());
                    triggerData.Tier = sbArgs["tier"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeMemberMileStone:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.MonthsTotal = int.Parse(sbArgs["months"].ToString());
                    triggerData.Tier = sbArgs["levelName"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeMessage:
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeNewSponsor:
                    triggerData.Tier = sbArgs["levelName"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeNewSubscriber:
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeSuperChat:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = productSettings["LocalCurrencyCode"].ToString();
                    triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                    if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                    {
                        triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                    }                     
                    triggerData.Message = sbArgs["message"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeSuperSticker:
                    amount = decimal.Parse(sbArgs["microAmount"].ToString())/1000000;
                    currency = sbArgs["currencyCode"].ToString();
                    localCurrency = productSettings["LocalCurrencyCode"].ToString();
                    triggerData.AmountCurrency = CPH.SUConvertCurrency(amount, currency, localCurrency);
                    cleanedAmount = Regex.Replace(triggerData.AmountCurrency, @"[^\d.-]", "");
                    if (double.TryParse(cleanedAmount, NumberStyles.Any, CultureInfo.InvariantCulture, out amountCurrencyDouble))
                    {
                        triggerData.AmountCurrencyDouble = amountCurrencyDouble;
                    }                     
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                case EventType.YouTubeUserBanned:
                    triggerData.BanDuration = int.Parse(sbArgs["banDuration"].ToString());
                    triggerData.BanType = sbArgs["banType"].ToString();
                    triggerData.User = sbArgs["user"].ToString();
                    triggerData.UserImage = SUSBCheckYouTubeProfileImageArgs(CPH, productInfo.ProductNumber);                
                    break;
                default:
                    CPH.SUWriteLog($"The trigger method [{triggerType}] is not supported. Feel free to open a ticket in the StreamUP Discord and the StreamUP team will try and add it.");
                    DialogResult result = CPH.SUUIShowWarningYesNoMessage($"The trigger method [{triggerType}] is not supported.\n\nFeel free to open a ticket in the StreamUP Discord and the StreamUP team will try and add it.\n\nWould you like a link to the Discord Server?");
                    if (result == DialogResult.Yes)
                    {
                        Process.Start("https://discord.com/invite/RnDKRaVCEu?");
                    }
                    CPH.SUWriteLog("METHOD FAILED", logName);
                    return null;
            }

            // Escape message string for OBS
            if (!string.IsNullOrEmpty(triggerData.Message)) {
                triggerData.Message = triggerData.Message
                    .Replace("\\n", "")
                    .Replace("\\r", "")
                    .Replace("\\t", "");
            }

            ReplaceAlertMessageVarsTriggerData(CPH, triggerData, productInfo, productSettings);

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return triggerData;
        }
        
        private static void ReplaceAlertMessageVarsTriggerData(this IInlineInvokeProxy CPH, TriggerData triggerData, ProductInfo productInfo, Dictionary<string, object> productSettings)
        {
            string logName = $"{productInfo.ProductNumber}::ReplaceAlertMessageVarsTriggerData";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string triggerType = CPH.GetEventType().ToString();
            if (triggerData.Donation)
            {
                triggerType = "Donation";
            }
            
            string eventTypeKey = triggerType + "AlertMessage";
            if (productSettings.TryGetValue(eventTypeKey, out object value))
            {
                string customMessageValue = value.ToString();
                CPH.SUWriteLog($"Custom message value found: [{customMessageValue}]");

                StringBuilder builder = new StringBuilder(customMessageValue);
                builder.Replace("%user%", triggerData.User)
                .Replace("%message%", triggerData.Message)
                .Replace("%tier%", triggerData.Tier)
                .Replace("%amount%", triggerData.Amount.ToString())
                .Replace("%amountCurrency%", triggerData.AmountCurrency)
                .Replace("%banType%", triggerData.BanType)
                .Replace("%duration%", triggerData.BanDuration.ToString())
                .Replace("%monthsGifted%", triggerData.MonthsGifted.ToString())
                .Replace("%monthsStreak%", triggerData.MonthsStreak.ToString())
                .Replace("%monthsTotal%", triggerData.MonthsTotal.ToString())
                .Replace("%reason%", triggerData.BanType)
                .Replace("%receiver%", triggerData.Receiver)
                .Replace("%totalAmount%", triggerData.TotalAmount.ToString());

                triggerData.AlertMessage = builder.ToString();
                
            }
            else
            {
                CPH.SUWriteLog("No custom message configuration found for event type: " + eventTypeKey);
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
        }

        private static string SUSBGetDonationUserImage(this IInlineInvokeProxy CPH, ProductInfo productInfo, string triggerType, string defaultDonationImageUrl)
        {
            string logName = $"{productInfo.ProductNumber}::SUSBGetDonationUserImage";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string userImage = defaultDonationImageUrl;

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return userImage;
        }               

        public static string SUSBGetTwitchProfilePicture(this IInlineInvokeProxy CPH, IDictionary<string, object> sbArgs, string productNumber, TwitchProfilePictureUserType userType, Dictionary<string, object> productSettings, string testUser = "")
        {
            string logName = $"{productNumber}::SUSBGetTwitchProfilePicture";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            // pull requested sizes '50, 70, 150, 300'
            int userImageSize = 300;

            // Attempt to retrieve 'ProfilePictureSize' from productSettings
            if (productSettings.TryGetValue("ProfilePictureSize", out object value) && int.TryParse(value.ToString(), out int size))
            {
                if (size > 0)
                {
                    userImageSize = size;
                }
                else
                {
                    CPH.SUWriteLog("Invalid ProfilePictureSize provided. Using default size: 300");
                }
            }
            else
            {
                CPH.SUWriteLog("ProfilePictureSize not set. Using default size: 300");
            }

            TwitchUserInfoEx userInfo;
            if (!string.IsNullOrEmpty(testUser))
            {
                userInfo = CPH.TwitchGetExtendedUserInfoByLogin(testUser);
            }
            else
            {
                userInfo = CPH.TwitchGetExtendedUserInfoById(sbArgs[$"{userType}"].ToString());
            }

            string originalImage = userInfo.ProfileImageUrl;
            string basePattern = "300x300";

            // Replace the base pattern with the new size
            string newSizePattern = $"{userImageSize}x{userImageSize}";
            string image = Regex.Replace(originalImage, basePattern, newSizePattern);

            CPH.SUWriteLog($"Created {newSizePattern} profile image: image=[{image}]", logName);
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return image;
        }
            
        public static string SUSBCheckYouTubeProfileImageArgs(this IInlineInvokeProxy CPH, string productNumber)
        {
            string logName = $"{productNumber}::SUSBCheckYouTubeProfileImageArgs";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            if (!CPH.TryGetArg("userProfileUrl", out string profileImage))
            {
                if (!CPH.TryGetArg("profileImageUrl", out profileImage))
                {
                    CPH.SUWriteLog($"User Profile Url args do not exist. Setting to default YouTube logo", logName);
                    profileImage = "https://upload.wikimedia.org/wikipedia/commons/e/ef/Youtube_logo.png";
                }
            }

            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return profileImage;
        }

        public static string SUSBReplaceMessageVarsFromTriggerData(this IInlineInvokeProxy CPH, TriggerData triggerData, ProductInfo productInfo, Dictionary<string, object> productSettings, string inputMessage)
        {
            string logName = $"{productInfo.ProductNumber}::SUSBReplaceMessageVarsFromTriggerData";
            CPH.SUWriteLog("METHOD STARTED!", logName);

            string customMessageValue = inputMessage;

            StringBuilder builder = new StringBuilder(customMessageValue);
            builder.Replace("%user%", triggerData.User)
            .Replace("%message%", triggerData.Message)
            .Replace("%tier%", triggerData.Tier)
            .Replace("%amount%", triggerData.Amount.ToString())
            .Replace("%amountCurrency%", triggerData.AmountCurrency)
            .Replace("%banType%", triggerData.BanType)
            .Replace("%duration%", triggerData.BanDuration.ToString())
            .Replace("%monthsGifted%", triggerData.MonthsGifted.ToString())
            .Replace("%monthsStreak%", triggerData.MonthsStreak.ToString())
            .Replace("%monthsTotal%", triggerData.MonthsTotal.ToString())
            .Replace("%reason%", triggerData.BanType)
            .Replace("%receiver%", triggerData.Receiver)
            .Replace("%totalAmount%", triggerData.TotalAmount.ToString());

            string outputMessage = builder.ToString();
            triggerData.AlertMessage = outputMessage;
            
            CPH.SUWriteLog("METHOD COMPLETED SUCCESSFULLY!", logName);
            return outputMessage;
        }

        public static void SUSBSendMessage(this IInlineInvokeProxy CPH, ProductInfo productInfo, string message, bool botAccount, StreamingPlatform streamingPlatform = StreamingPlatform.All)
        {
            if (streamingPlatform == StreamingPlatform.All || streamingPlatform == StreamingPlatform.Twitch)
            {
                CPH.SendMessage(message, botAccount);
            }

            if (streamingPlatform == StreamingPlatform.All || streamingPlatform == StreamingPlatform.YouTube)
            {
                CPH.SendYouTubeMessage(message, botAccount);
            }
            //CPH.SendTrovoMessage(message, botAccount);
        }

        public static void SUSBSendPlatformMessage(this IInlineInvokeProxy CPH, string platform, string message, bool botAccount)
        {
            if(platform == "twitch")
            {
                CPH.SendMessage(message, botAccount);
            }
            if(platform == "youtube")
            {
                CPH.SendYouTubeMessage(message, botAccount);
            }
        }

        
        public static string SURawInputWithInputsRemoved(this IInlineInvokeProxy CPH, int inputsToRemove)
        {
            string textRemove = "";
            for (int i = inputsToRemove; CPH.TryGetArg("input" + i, out string moreInput); i++)
            {
                textRemove += moreInput;
            }
            return textRemove;
        }

          public static bool SUSBRefund(this IInlineInvokeProxy CPH, bool refund = false)
        {
            CPH.TryGetArg("rewardId", out string reward);
            CPH.TryGetArg("redemptionId", out string redemption);
            if (reward == null || redemption == null)
            {
                return true;
            }
            if (refund)
            {

                CPH.TwitchRedemptionCancel(reward, redemption);
            }

            return true;
        }

          public static bool SUSBFulfill(this IInlineInvokeProxy CPH, bool fulfill = false)
        {

           
            CPH.TryGetArg("rewardId", out string reward);
            CPH.TryGetArg("redemptionId", out string redemption);
            if (reward == null || redemption == null)
            {
                return true;
            }
            if (fulfill)
            {

                CPH.TwitchRedemptionFulfill(reward, redemption);
            }

            return true;
        }



        public static T SUSBTryGetArgOrDefault<T>(this IInlineInvokeProxy CPH, string key, T defaultValue = default)
        {
            if (CPH.TryGetArg(key, out object value))
            {
                try
                {
                    if (value is T variable)
                    {
                        return variable;
                    }
                    else if (typeof(T) == typeof(string) && value is Guid guidValue)
                    {
                        return (T)(object)guidValue.ToString(); 
                    }
                    else if (value is IConvertible)
                    {
                        return (T)Convert.ChangeType(value, typeof(T));
                    }
                    else
                    {
                        CPH.SUWriteLog($"Key '{key}' contains a value of type '{value.GetType()}' which cannot be converted to type '{typeof(T)}'.", "Type Conversion Error");
                        return defaultValue;
                    }
                }
                catch (InvalidCastException ex)
                {
                    CPH.SUWriteLog($"Error converting key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Conversion Error");
                    return defaultValue;
                }
                catch (FormatException ex)
                {
                    CPH.SUWriteLog($"Error formatting key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Format Error");
                    return defaultValue;
                }
                catch (Exception ex)
                {
                    CPH.SUWriteLog($"Unexpected error with key '{key}' to type '{typeof(T)}': {ex.Message}", $"{typeof(T).Name} Unexpected Error");
                    return defaultValue;
                }
            }
            else
            {
                CPH.SUWriteLog($"Key '{key}' not found in arguments.", "Key Not Found");
                return defaultValue;
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

                CPH.SUWriteLog("Successfully deserialised and converted to Queue<TriggerData>.", logName);
                return myQueue;
            }
            catch (JsonException e)
            {
                CPH.SUWriteLog($"Failed to deserialise JSON: {e.Message}", logName);
                // Handle error (e.g., by returning an empty queue or re-throwing the exception)
                return new Queue<TriggerData>(); // Return an empty queue as a fallback
            }
        }

    }

    public enum StreamingPlatform
    {
        All = 0,
        Twitch = 1,
        YouTube = 2
    }

    public enum TwitchProfilePictureUserType
    {
        userId = 0,   
        recipientId = 1,
        createdById = 2,
        targetUserId = 3,
        broadcastUserId = 4
    }


    // SB events trigger data
    public class TriggerData 
    {
        public string AlertMessage { get; set; } = null;
        public int Amount { get; set; } = 0;
        public string AmountCurrency { get; set; } = null;
        public double AmountCurrencyDouble { get; set; } = 0;
        public bool Anonymous { get; set; } = false;
        public int BanDuration { get; set; } = 0;
        public string BanType { get; set; } = null;
        public bool Donation { get; set; } = false;
        public string EventSource { get; set; } = null;
        public string EventType { get; set; } = null;
        public string Message { get; set; } = null;
        public int MonthsGifted { get; set; } = 0;
        public int MonthsStreak { get; set; } = 0;
        public int MonthsTotal { get; set; } = 0;
        public string Receiver { get; set; } = null;
        public string ReceiverImage { get; set; } = null;
        public string Tier { get; set; } = null;
        public int TotalAmount { get; set; } = 0;
        public string User { get; set; } = null;
        public string UserImage { get; set; } = null;
    }


}

