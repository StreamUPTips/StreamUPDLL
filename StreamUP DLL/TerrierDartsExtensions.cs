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




}