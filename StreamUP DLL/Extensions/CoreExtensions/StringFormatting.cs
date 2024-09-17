using System;
using System.Globalization;
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


    }
}