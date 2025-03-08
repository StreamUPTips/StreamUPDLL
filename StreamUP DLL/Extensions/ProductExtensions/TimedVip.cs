using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
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


        //! Fix These
        public bool TimedVipRemoveMethod(string user)
        {
            _CPH.RemoveUserFromGroup(user, Platform.Twitch, "Timed Vips");
            _CPH.TwitchRemoveVip(user);
            _CPH.UnsetTwitchUserVar(user, "timedVipExpiryDate", true);
            LogInfo($"[Timed Vip] {user} has been removed.");
            return true;
        }



        // public DateTime TimedVipAddTimeMethod(string user, int timeToAdd)
        // {
        //     TwitchUserInfoEx userInfo = CPH.TwitchGetExtendedUserInfoByLogin(user);
        //     string userId = userInfo.UserId;
        //     string displayName = userInfo.UserName;
        //     CPH.SetArgument("vipUser", displayName);
        //     TimedVIPSettings productSettings = JsonConvert.DeserializeObject<TimedVIPSettings>(CPH.GetGlobalVar<string>($"sup0100_ProductSettings", true));
        //     string type = productSettings.TimedVIPType;
        //     DateTime usersExpireDate = CPH.GetTwitchUserVarById<DateTime?>(userId, "timedVipExpiryDate", true) ?? DateTime.Now;
        //     DateTime newExpiry;
        //     if (type == "Hours")
        //     {
        //         newExpiry = usersExpireDate.AddHours(timeToAdd);
        //     }
        //     else
        //     {
        //         newExpiry = usersExpireDate.AddDays(timeToAdd);

        //     }

        //     return newExpiry;

        // }

    }
}