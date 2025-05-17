
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    partial class StreamUpLib
    {
        public (string Type, string StarSign) HoroscopeParseCommand(string command)
        {
            // Log the original command
            LogDebug($"Original command: {command}");

            // Clean the command
            command = command.ToLower().Trim();
            LogDebug($"Command after trim: {command}");

            // Define valid star signs
            var validStarSigns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "aries", "taurus", "gemini", "cancer", "leo", "virgo",
        "libra", "scorpio", "sagittarius", "capricorn", "aquarius", "pisces"
    };

            // Check for prefixes
            if (command.StartsWith("weekly"))
            {
                string sign = command.Substring(6); // Remove "weekly" prefix
                LogDebug($"Weekly prefix detected, extracted sign: '{sign}'");
                bool isValid = validStarSigns.Contains(sign);
                LogDebug($"Is '{sign}' valid: {isValid}");
                return isValid ? ("Weekly", sign) : (null, null);
            }
            else if (command.StartsWith("monthly"))
            {
                string sign = command.Substring(7); // Remove "monthly" prefix
                LogDebug($"Monthly prefix detected, extracted sign: '{sign}'");
                bool isValid = validStarSigns.Contains(sign);
                LogDebug($"Is '{sign}' valid: {isValid}");
                return isValid ? ("Monthly", sign) : (null, null);
            }
            else if (validStarSigns.Contains(command))
            {
                LogDebug($"No prefix detected, using command as sign: '{command}'");
                return ("Daily", command);
            }

            LogDebug($"Command '{command}' didn't match any pattern");
            return (null, null);
        }

        public string HoroscopeBuildApiUrl(string horoscopeType, string starSign)
        {
            // Format the star sign with first letter capitalised
            string formattedSign = char.ToUpper(starSign[0]) + starSign.Substring(1).ToLower();

            switch (horoscopeType.ToLower())
            {
                case "daily":
                    return $"https://horoscope-app-api.vercel.app/api/v1/get-horoscope/daily?sign={formattedSign}&day=TODAY";
                case "weekly":
                    return $"https://horoscope-app-api.vercel.app/api/v1/get-horoscope/weekly?sign={formattedSign}";
                case "monthly":
                    return $"https://horoscope-app-api.vercel.app/api/v1/get-horoscope/monthly?sign={formattedSign}";
                default:
                    LogError($"Invalid horoscope type: {horoscopeType}");
                    return string.Empty;
            }
        }

        public string HoroscopeGet(string apiUrl)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, apiUrl);

                HttpResponseMessage result = _httpClient.SendAsync(request).Result;
                if (!result.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Request failed with status code: {(int)result.StatusCode} - {result.ReasonPhrase}");
                }

                string content = result.Content.ReadAsStringAsync().Result;

                // Parse JSON response
                JObject json = JObject.Parse(content);

                // Extract data safely
                string horoscopeText = json["data"]?["horoscope_data"]?.ToString() ?? "Unable to retrieve horoscope.";

                // Remove premium horoscope ads if needed
                horoscopeText = RemovePremiumHoroscopeAds(horoscopeText);

                // Format message spacing
                horoscopeText = HoroscopeFormatMessageSpacing(horoscopeText);
                
                return horoscopeText;
            }
            catch (HttpRequestException error)
            {
                LogError($"HTTP request error: {error.Message}");
                return "Unable to retrieve horoscope at this time.";
            }
            catch (Exception ex)
            {
                LogError($"General error: {ex.Message}");
                return "Unable to retrieve horoscope at this time.";
            }
        }

        private string RemovePremiumHoroscopeAds(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Pattern to match any month name followed by "Premium Horoscope"
            string pattern = @"(January|February|March|April|May|June|July|August|September|October|November|December)\s+Premium\s+Horoscope";

            // Replace the pattern with empty string
            return System.Text.RegularExpressions.Regex.Replace(text, pattern, "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private string HoroscopeFormatMessageSpacing(string message)
        {
            // Fix punctuation that doesn't have a space after it but is followed by a letter
            return System.Text.RegularExpressions.Regex.Replace(
                message,
                @"([\.!?;:,])([a-zA-Z])",
                "$1 $2"
            );
        }
    }
}

// Horoscope API provider
// https://horoscope-app-api.vercel.app/
// Copyright (c) 2021 Ashutosh Krishna