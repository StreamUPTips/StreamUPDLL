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
        public static bool TimedVipUserCheck(this IInlineInvokeProxy CPH, string userId, bool mod, bool vip, int maxVips, bool allowAdding, bool usePoints, int cost)
        {


            long points = CPH.GetUserPointsById(userId, Platform.Twitch);


            if (CPH.UserIdInGroup(userId, Platform.Twitch, "Timed VIPs") && !allowAdding)
            {

                CPH.TimedVipError(1, "User can not add anymore time.");
                return false;

            }

            if (mod)
            {
                CPH.TimedVipError(2, "User is a Moderator so can not redeem");
                return false;
            }

            if (!CPH.UserIdInGroup(userId, Platform.Twitch, "Timed VIPs") && vip)
            {
                CPH.TimedVipError(3, "User is already a Vip Permanently so can not redeem");
                return false;
            }


            int vips = CPH.CurrentVipCount();
            if (vips >= maxVips)
            {

                CPH.TimedVipError(4, "You already have the max number of Vips.");
                return false;
            }

            if (usePoints && cost > points)
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

          /*public static List<StreamUpSetting> AddSettingsToUI(this IInlineInvokeProxy CPH)
    {
        List<StreamUpSetting> settings = new List<StreamUpSetting>();
        settings.AddRange(CPH.SUSettingsCreateHeading());
        settings.AddRange(CPH.SUSettingsCreateInteger("RaffleDefaultPrize"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleMinPrize"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleMaxPrize"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleLength"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleCost"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleMaxEntries"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleWinnerPerEntries"));

        
        setting.AddRange(CPH.SUSettingsCreateBoolean("RaffleMultiWinners"));
        setting.AddRange(CPH.SUSettingsCreateInteger("RaffleSharePrize"));
        
        
        
        return settings;
    }
    */
    }




}