using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class TimedVipUser //This was an idea i had Not but forgotten what i wanted to do with it so imma just leave it for now
    {
        public string UserName { get; set; }
        public string UserId { get; set; }
        public Enum Platform {get; set;}
        public DateTime ExpirationDate { get; set; }
    }
    public partial class StreamUpLib
    {
    
        public bool TimedVipError(int code, string message)
        {
            _CPH.SetArgument("errorCode", code);
            _CPH.SetArgument("errorMessage", message);
            LogError($"{code} - {message}");
            _CPH.TriggerCodeEvent("timedVipFail");
            return true;
        }
        public int CurrentVipCount()
        {
            List<GroupUser> groupsUsers = _CPH.UsersInGroup("Timed VIPs");
            int count = groupsUsers.Count;
            return count;

        }
        public bool TimedVipUserCheck(string userId, bool inGroup, bool mod, bool allowMods, bool vip, bool allowVIPs, int maxVips, bool allowAdding, bool usePoints, long cost, long points)
        {


            if (inGroup && !allowAdding)
            {
                TimedVipError(1, "User can not add anymore time.");
                return false;
            }

            if (mod && !allowMods)
            {
                TimedVipError(2, "User is a Moderator and Moderators are not allowed to redeem.");
                return false;
            }

            if (!inGroup && vip && !allowVIPs)
            {
                TimedVipError(3, "User is already a Vip Permanently and VIPs are not allowed to redeem.");
                return false;
            }

            int vips = CurrentVipCount();
            if (vips >= maxVips)
            {
                TimedVipError(4, "You already have the max number of Allowed Users in the Group.");
                return false;
            }

            if (usePoints && (cost > points))
            {
                TimedVipError(5, "You can not afford to redeem this.");
                return false;
            }

            return true;
        }
        public int TimedVipDaysLeft(DateTime expireDate)
        {
            DateTime today = DateTime.Now;
            TimeSpan nod = expireDate - today;
            int days = (int)nod.TotalDays;
            _CPH.SetArgument("daysLeft", days);
            return days;
        }

        public int TimedVipHoursLeft(DateTime expireDate)
        {
            DateTime today = DateTime.Now;
            TimeSpan nod = expireDate - today;
            int hours = (int)nod.TotalHours;
            _CPH.SetArgument("hoursLeft", hours);
            return hours;
        }

        public DateTime TimedVipAddTime(string userId, Platform platform, int timeToAdd, string type)
        {
            DateTime expiryDate = GetUserVariableById<DateTime>(userId, "timedVipExpiryDate", platform, true, DateTime.Now);
            if (expiryDate == DateTime.MinValue || expiryDate < DateTime.Now)
            {
                expiryDate = DateTime.Now;
            }
            _CPH.LogDebug($"Current Expiry Date for UserId: {userId} on Platform: {platform} is: {expiryDate}");
            if (type == "Hours")
            {
                expiryDate = expiryDate.AddHours(timeToAdd);
                _CPH.LogDebug($"Current Expiry HOURS for UserId: {userId} on Platform: {platform} is: {expiryDate}");
            }
            else
            {
                expiryDate = expiryDate.AddDays(timeToAdd);
                _CPH.LogDebug($"Current Expiry DAYS for UserId: {userId} on Platform: {platform} is: {expiryDate}");

            }
            _CPH.LogDebug($"Adding {timeToAdd} {type} to UserId: {userId} on Platform: {platform}. New Expiry Date: {expiryDate}");
            SetUserVariableById(userId, "timedVipExpiryDate", expiryDate, platform, true);
            return expiryDate;
        }

    }
}