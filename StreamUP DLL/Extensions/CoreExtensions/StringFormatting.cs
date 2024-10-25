using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace StreamUP
{
    public partial class StreamUpLib
    {


        public string ArgumentReplacement(string message, string productName = "General")
        {
            Regex regex = new Regex("%(.*?)(?::(.*?))?%");
            // Use Regex.Replace with a MatchEvaluator to replace each match
            string result = regex.Replace(message, match =>
            {
                // Extract the word inside the % symbols
                string word = match.Groups[1].Value;
                string format = match.Groups[2].Value;
                // Attempt to get the argument for this word
                if (_CPH.TryGetArg(word, out object arg))
                {
                    if (arg is IFormattable formattable)
                    {
                        // Use the specified format if provided, otherwise use the default format
                        string formatted = formattable.ToString(format, CultureInfo.CurrentCulture);
                        LogInfo($"Found {word}:{format}, Replaced with {formatted}");
                        return formatted;
                    }
                    else
                    {
                        // Return the argument value without formatting
                        LogInfo($"Found {word}:{format}, Replaced with {arg.ToString()}");
                        return arg.ToString();
                    }
                }
                else
                {
                    LogInfo($"Found {word}:{format}, Non Replaced, returned : {match.Value} ");
                    return match.Value; // Return the original %word% if no argument is found
                }
            });
            return result;


        }

        public string RawWithInputsRemoved(int inputsToRemove, bool decoded = false)
        {
            StringBuilder combinedInputs = new StringBuilder();
            if (decoded)
            {

                for (int i = inputsToRemove; _CPH.TryGetArg("inputUrlEncoded" + i, out string moreInput); i++)
                {
                    string textToAppend = Uri.UnescapeDataString(moreInput); ;
                    combinedInputs.Append(" ").Append(textToAppend);
                }
            }
            else
            {
                for (int i = inputsToRemove; _CPH.TryGetArg("input" + i, out string moreInput); i++)
                {
                    combinedInputs.Append(" ").Append(moreInput);
                }

            }
            return combinedInputs.Length > 0 ? combinedInputs.ToString().TrimStart() : string.Empty;

        }

        public bool RemoveUrlFromString(string inputText, string replacementText, out string outputText)
        {
            LogInfo($"Replacing Url from [{inputText}] with [{replacementText}]");

            // This pattern matches URLs starting with http://, https://, or ftp:// followed by any characters until a space is encountered
            string urlPattern = @"\b(http|https|ftp)://\S+";
            Regex urlRegex = new Regex(urlPattern, RegexOptions.IgnoreCase);

            outputText = urlRegex.Replace(inputText, replacementText);
            LogInfo($"Successfully replaced Url. Output string: [{outputText}]");
            return true;
        }
    }
}