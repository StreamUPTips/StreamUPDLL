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
            bool isMember = false;
            bool isVip = false;
            bool isSubscriber = false;
            switch (platform)
            {
                case Platform.Twitch:
                    _CPH.TryGetArg("isVip", out isVip);
                    _CPH.TryGetArg("isSubscribed", out isSubscriber);
                    break;
                case Platform.YouTube:
                    _CPH.TryGetArg("userIsSponser", out isMember);
                    break;
            }

            if (broadcasterId == userId)
            {
                return 4;
            }

            if (isMod)
            {
                return 3;
            }

            if (isVip || isSubscriber || isMember)
            {
                return 2;
            }

            return 1;

        }
    }
}