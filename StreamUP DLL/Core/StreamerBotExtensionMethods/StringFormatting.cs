using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

        public bool ContainsIgnoreCase(List<string> users, string user)
        {
            bool containsIgnoreCase = users.Any(item => string.Equals(item, user, StringComparison.OrdinalIgnoreCase));
            return containsIgnoreCase;
        }

        public string ToOrdinalSuffix(string word)
        {
            if (word.EndsWith("11") || word.EndsWith("12") || word.EndsWith("13"))
            {
                return word + "th";
            }

            return word + (word.EndsWith("1") ? "st" :
                           word.EndsWith("2") ? "nd" :
                           word.EndsWith("3") ? "rd" : "th");
        }

        public string RemoveDiacritics(string text)
        {
            string normalizedString = text.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }

            LogInfo($"[Remove Diacritics] {text} == {stringBuilder}");
            return stringBuilder.ToString();
        }


    }
}