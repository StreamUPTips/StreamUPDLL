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
using System.Data.Common;

namespace StreamUP
{

    

    public static class TerrierDartsHelperMethods
    {


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

        //! Im fairly sure these arent been used in any public products but ive left them for now
       [Obsolete]
        public static bool SUSBRefund(this IInlineInvokeProxy CPH, bool refund = false, string productName = "General")
        {

            StreamUpLib SUP = new StreamUpLib(CPH, productName);
            SUP.Refund(refund, productName);
            return true;
        }

        [Obsolete]
        public static bool SUSBFulfill(this IInlineInvokeProxy CPH, bool fulfill = false, string productName = "General")
        {


            StreamUpLib SUP = new StreamUpLib(CPH, productName);
            SUP.Refund(fulfill, productName);
            return true;
        }
        [Obsolete]
        public static string SUpwnyyReplacement(this IInlineInvokeProxy CPH, string message, string productName = "General")
        {
            StreamUpLib SUP = new StreamUpLib(CPH, productName);
            var result = SUP.ArgumentReplacement(message, productName);
            return result;
        }
        [Obsolete]
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


    }




}
