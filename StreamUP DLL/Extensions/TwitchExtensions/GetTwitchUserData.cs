using System;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public int GetUserRoleId()
    {
        _CPH.TryGetArg("userType", out string source);
        Enum.TryParse(source, true, out Platform platform);
        _CPH.TryGetArg("userId", out string userId);
        _CPH.TryGetArg("broadcastUserId", out string broadcasterId);
        _CPH.TryGetArg("isModerator", out bool isMod);
        if (platform == Platform.Twitch)
        {
            _CPH.TryGetArg("isVip", out bool isVip);
            _CPH.TryGetArg("isSubscribed", out bool isSubscriber);
            if (broadcasterId == userId)
            {
                return 4;
            }

            if (isMod)
            {
                return 3;
            }

            if (isVip || isSubscriber)
            {
                return 2;
            }

            return 1;
        }
        else if (platform == Platform.YouTube)
        {
            _CPH.TryGetArg("userIsSponser", out bool isMemeber);
            if (broadcasterId == userId)
            {
                return 4;
            }

            if (isMod)
            {
                return 3;
            }

            if (isMemeber)
            {
                return 2;
            }

            return 1;
        }

        return 1;
    }
    }
}