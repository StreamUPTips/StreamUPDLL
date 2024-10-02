using System.Collections.Generic;

namespace StreamUP
{
    public class TwitchReSubHandler : IEventHandler
    {
        public TriggerData HandleEvent(IDictionary<string, object> sbArgs, StreamUpLib SUP)
        {
            var triggerData = new TriggerData();
            var userType = StreamUpLib.TwitchUserType.userId;

            triggerData.Message = SUP.GetValueOrDefault<string>(sbArgs, "rawInput", "No arg 'rawInput' found");
            triggerData.MonthsTotal = SUP.GetValueOrDefault<int>(sbArgs, "cumulative", -1);
            triggerData.MonthsStreak = SUP.GetValueOrDefault<int>(sbArgs, "monthStreak", -1);
            triggerData.Tier = SUP.GetValueOrDefault<string>(sbArgs, "tier", "No arg 'tier' found");
            triggerData.User = SUP.GetValueOrDefault<string>(sbArgs, "user", "No arg 'user' found");
            triggerData.UserImage = SUP.GetTwitchProfilePicture(sbArgs, userType);   
            triggerData.MonthTenure =  SUP.GetValueOrDefault<int>(sbArgs, "multiMonthTenure", 1);
            triggerData.MonthDuration =   SUP.GetValueOrDefault<int>(sbArgs, "multiMonthDuration", 1);
            triggerData.IsMultiMonth =  SUP.GetValueOrDefault<bool>(sbArgs, "isMultiMonth", false);      
            return triggerData;
        }
    }
}
