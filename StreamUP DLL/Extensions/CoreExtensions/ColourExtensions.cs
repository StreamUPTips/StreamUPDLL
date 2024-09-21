using System;
using System.Drawing;

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
    }
}