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
using System.CodeDom;

namespace StreamUP
{

    public static class TimedVIP
    {
        public static bool TimedVipError(this IInlineInvokeProxy CPH, int code, string message)
        {
            CPH.SetArgument("errorCode", code);
            CPH.SetArgument("errorMessage", message);
            CPH.SUWriteLog($"[Timed Vip] ERR {code} - {message}");
            CPH.TriggerCodeEvent("timedVipFail");
            return true;
        }
        public static int CurrentVipCount(this IInlineInvokeProxy CPH)
        {
            List<GroupUser> groupsUsers = CPH.UsersInGroup("Timed VIPs");
            int count = groupsUsers.Count;
            return count;

        }
        public static bool TimedVipUserCheck(this IInlineInvokeProxy CPH, string userId, bool inGroup, bool mod, bool allowMods, bool vip, bool allowVIPs, int maxVips, bool allowAdding, bool usePoints, long cost, long points)
        {


           if (inGroup && !allowAdding)
        {
            CPH.TimedVipError(1, "User can not add anymore time.");
            return false;
        }

        if (mod && !allowMods)
        {
            CPH.TimedVipError(2, "User is a Moderator and Moderators are not allowed to redeem.");
            return false;
        }

        if (!inGroup && vip && !allowVIPs)
        {
            CPH.TimedVipError(3, "User is already a Vip Permanently and VIPs are not allowed to redeem.");
            return false;
        }

        int vips = CPH.CurrentVipCount();
        if (vips >= maxVips)
        {
            CPH.TimedVipError(4, "You already have the max number of Allowed Users in the Group.");
            return false;
        }

        if (usePoints && (cost > points))
        {
            CPH.TimedVipError(5, "You can not afford to redeem this.");
            return false;
        }

        return true;
        }
        public static int TimedVipDaysLeft(this IInlineInvokeProxy CPH, DateTime expireDate)
        {
            DateTime today = DateTime.Now;
            TimeSpan nod = expireDate - today;
            int days = (int)nod.TotalDays;
            CPH.SetArgument("daysLeft", days);
            return days;
        }

        public static int TimedVipHoursLeft(this IInlineInvokeProxy CPH, DateTime expireDate)
        {
            DateTime today = DateTime.Now;
            TimeSpan nod = expireDate - today;
            int hours = (int)nod.TotalHours;
            CPH.SetArgument("hoursLeft", hours);
            return hours;
        }


    
    }

    public static class TerrierDartsHelperMethods
    {
        
        public static void SUSBSendPlatformMessage(this IInlineInvokeProxy CPH, string platform, string message, bool botAccount)
        {
            if (platform == "twitch")
            {
                CPH.SendMessage(message, botAccount);
            }
            if (platform == "youtube")
            {
                CPH.SendYouTubeMessage(message, botAccount);
            }
        }


        public static string SURawInputWithInputsRemoved(this IInlineInvokeProxy CPH, int inputsToRemove)
        {
            StringBuilder combinedInputs = new StringBuilder();
            for (int i = inputsToRemove; CPH.TryGetArg("input" + i, out string moreInput); i++)


            {
                combinedInputs.Append(" ").Append(moreInput);
            }
           return combinedInputs.Length > 0 ? combinedInputs.ToString().TrimStart() : string.Empty;
        }

        public static string SURawInputWithInputsRemovedEncoded(this IInlineInvokeProxy CPH, int inputsToRemove)
        {
             StringBuilder combinedInputs = new StringBuilder();
            for (int i = inputsToRemove; CPH.TryGetArg("inputUrlEncoded" + i, out string moreInput); i++)


            {
                combinedInputs.Append(" ").Append(moreInput);
            }
           return combinedInputs.Length > 0 ? combinedInputs.ToString().TrimStart() : string.Empty;
        }
        public static string SURawInputWithInputsRemovedMAC(this IInlineInvokeProxy CPH, int inputsToRemove)
        {
            StringBuilder combinedInputs = new StringBuilder();

            for (int i = inputsToRemove; CPH.TryGetArg("inputUrlEncoded" + i, out string moreInput); i++)
            {   
                string textToAppend = "";
                if (moreInput.StartsWith("!"))
                {
                        textToAppend = moreInput;
                }
                else
                {
                    CPH.TryGetArg("input" + i, out string plainInput);
                    textToAppend = plainInput;
                }

                combinedInputs.Append(" ").Append(textToAppend);
            }

            return combinedInputs.Length > 0 ? combinedInputs.ToString().TrimStart() : string.Empty;
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

        public static string SUpwnyyReplacement(this IInlineInvokeProxy CPH, string message)
    {
        Regex regex = new Regex("%(.*?)(?::(.*?))?%");
        // Use Regex.Replace with a MatchEvaluator to replace each match
        string result = regex.Replace(message, match =>
        {
            // Extract the word inside the % symbols
            string word = match.Groups[1].Value;
            string format = match.Groups[2].Value;

            // Attempt to get the argument for this word
            if (CPH.TryGetArg(word, out object arg))
            {
                if (arg is IFormattable formattable)
                {
                    // Use the specified format if provided, otherwise use the default format
                    return formattable.ToString(format, CultureInfo.CurrentCulture);
                }
                else
                {
                    // Return the argument value without formatting
                    return arg.ToString();
                }
            }
            else
            {
                return match.Value; // Return the original %word% if no argument is found
            }
        });
        return result;
    }


    }




}