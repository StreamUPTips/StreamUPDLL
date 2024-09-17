using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        
        public bool Refund(bool refund = false, string productName = "General")
        {
            _CPH.TryGetArg("rewardId", out string reward);
            _CPH.TryGetArg("redemptionId", out string redemption);
            if (reward == null || redemption == null)
            {
                LogError($"Reward {reward} not refunded.");
                return false;
            }
            if (refund)
            {
                LogInfo($"Reward {reward} refunded.");
                _CPH.TwitchRedemptionCancel(reward, redemption);
            }

            return true;
        }

        public bool Fulfill(bool fulfill = false, string productName = "General")
        {


            _CPH.TryGetArg("rewardId", out string reward);
            _CPH.TryGetArg("redemptionId", out string redemption);
            if (reward == null || redemption == null)
            {
                LogError($"Reward {reward} not fulfilled.");
                return false;
            }
            if (fulfill)
            {
                LogInfo($"Reward {reward} fulfilled.");
                _CPH.TwitchRedemptionFulfill(reward, redemption);
            }

            return true;
        }
    }
}