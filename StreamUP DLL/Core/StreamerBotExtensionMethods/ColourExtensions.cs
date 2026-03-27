using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool GetContrastingColourLong(long inputColour, out long contrastingColourLong)
        {
            LogInfo($"Getting Streamer.Bot global variable");

            // Extract ARGB
            Color color = Color.FromArgb((int)inputColour);

            // Convert RGB to YIQ luminance
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255;

            // Decide contrasting color based on luminance
            contrastingColourLong = luminance >= 0.5 ? 4278190080L : 4294967295L; // Black or White

            LogInfo($"Successfully retrieved contrasting colour");
            return true;
        }

        public bool GetRandomColourHex(out string hexColour)
        {
            Random random = new Random();
            int red = random.Next(256);
            int green = random.Next(256);
            int blue = random.Next(256);

            // Convert RGB values to a hexadecimal string
            hexColour = $"#{red:X2}{green:X2}{blue:X2}";

            return true;
        }

        public bool GetObsColour(string inputColour, out long obsColourLong)
        {
            obsColourLong = 0;

            LogInfo($"Converting colour '{inputColour}' to OBS ARGB long");

            if (string.IsNullOrWhiteSpace(inputColour))
            {
                LogError("GetObsColour: input was null or empty");
                return false;
            }

            try
            {
                Color color;
                var s = inputColour.Trim();

                // rgb(...) or rgba(...)
                if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
                {
                    var matches = Regex.Matches(s, "\\d+");
                    if (matches.Count < 3)
                    {
                        LogError($"GetObsColour: failed to parse rgb/rgba string '{s}'");
                        return false;
                    }

                    int r = int.Parse(matches[0].Value, CultureInfo.InvariantCulture);
                    int g = int.Parse(matches[1].Value, CultureInfo.InvariantCulture);
                    int b = int.Parse(matches[2].Value, CultureInfo.InvariantCulture);
                    int a = 255;
                    if (matches.Count >= 4)
                    {
                        a = int.Parse(matches[3].Value, CultureInfo.InvariantCulture);
                        if (a >= 0 && a <= 1)
                        {
                            // treat as normalized alpha 0-1
                            a = (int)Math.Round(a * 255.0);
                        }
                    }

                    color = Color.FromArgb(ClampByte(a), ClampByte(r), ClampByte(g), ClampByte(b));
                }
                else if (s.StartsWith("#"))
                {
                    var hex = s.Substring(1);
                    if (hex.Length == 3)
                    {
                        // e.g. #rgb -> rrggbb
                        hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
                    }
                    else if (hex.Length == 4)
                    {
                        // e.g. #argb -> aarrggbb
                        hex = string.Concat(
                            hex[0],
                            hex[0],
                            hex[1],
                            hex[1],
                            hex[2],
                            hex[2],
                            hex[3],
                            hex[3]
                        );
                    }

                    if (hex.Length == 6)
                    {
                        int r = int.Parse(
                            hex.Substring(0, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        int g = int.Parse(
                            hex.Substring(2, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        int b = int.Parse(
                            hex.Substring(4, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        color = Color.FromArgb(255, r, g, b);
                    }
                    else if (hex.Length == 8)
                    {
                        int a = int.Parse(
                            hex.Substring(0, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        int r = int.Parse(
                            hex.Substring(2, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        int g = int.Parse(
                            hex.Substring(4, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        int b = int.Parse(
                            hex.Substring(6, 2),
                            NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture
                        );
                        color = Color.FromArgb(a, r, g, b);
                    }
                    else
                    {
                        LogError($"GetObsColour: unsupported hex length '{hex.Length}' for '{s}'");
                        return false;
                    }
                }
                else
                {
                    // Try named colour or html-like values
                    try
                    {
                        color = ColorTranslator.FromHtml(s);
                    }
                    catch
                    {
                        // Try CSV "r,g,b" or "r,g,b,a"
                        var parts = s.Split(
                            new[] { ',', ' ' },
                            StringSplitOptions.RemoveEmptyEntries
                        );
                        if (parts.Length >= 3)
                        {
                            int r = int.Parse(parts[0], CultureInfo.InvariantCulture);
                            int g = int.Parse(parts[1], CultureInfo.InvariantCulture);
                            int b = int.Parse(parts[2], CultureInfo.InvariantCulture);
                            int a = 255;
                            if (parts.Length >= 4)
                            {
                                a = int.Parse(parts[3], CultureInfo.InvariantCulture);
                            }

                            color = Color.FromArgb(
                                ClampByte(a),
                                ClampByte(r),
                                ClampByte(g),
                                ClampByte(b)
                            );
                        }
                        else
                        {
                            LogError($"GetObsColour: could not parse colour string '{s}'");
                            return false;
                        }
                    }
                }

                obsColourLong = (long)(uint)color.ToArgb();
                LogInfo($"Converted '{inputColour}' -> 0x{((uint)color.ToArgb()):X8}");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"GetObsColour: exception converting '{inputColour}': {ex.Message}");
                return false;
            }
        }

        private static int ClampByte(int value)
        {
            if (value < 0)
                return 0;
            if (value > 255)
                return 255;
            return value;
        }
    }
}
