using System.CodeDom;
using System.Collections.Generic;
using Streamer.bot.Plugin.Interface;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    partial class StreamUpLib
    {

      private static readonly Dictionary<int, string> _errorMessages = new Dictionary<int, string>
    {
        { 4501, "User can not afford to do this" },
        { 4502, "User’s request is over the limit" },
        { 4503, "User’s request is under the limit" },
        { 5001, "Free for all is currently active" },
        { 5002, "Free for all is not currently active" },
        { 5003, "User has already joined the Free for all" },
        { 5004, "Free for all is full" },
        { 5005, "Nobody Joined the Free for all" },
        { 5201, "Raffle is currently active" },
        { 5202, "Raffle is not currently active" },
        { 5203, "User has max tickets" },
        { 5204, "Nobody joined the Raffle" },
        { 5501, "Heist is currently active" },
        { 5502, "Heist is not active" },
        { 5503, "Heist is Full" },
        { 5504, "User has already joined the Heist" },
        { 5505, "Nobody Joined the Heist" } 
    };
        public bool CurrencyError(int code)
        {
            string error = _errorMessages.TryGetValue(code, out var message) ? message : "Unknown Error";
            _CPH.TryGetArg("gameName",out string game);
            _CPH.SetArgument("failReason", error);
            _CPH.SetArgument("failCode", code);
            LogError($"[{game}] {code} - {message}");
            _CPH.TriggerCodeEvent("currencyFail");
            return true;
        }



    }
}

