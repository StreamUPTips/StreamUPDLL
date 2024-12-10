using System;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool DSISetBackgroundFilters(double cornerRadius, double width, double height, double scaleFactor, int obsConnection)
        {
            LogDebug("Setting background filters...");

            // Set widget padding
            double xPadding = 50 * scaleFactor;
            double yPadding = 20 * scaleFactor;
            LogDebug($"Applied scale factor to padding. X=[{xPadding}], Y=[{yPadding}]");

            // Set final background size
            double maskWidth = width + xPadding;
            double maskHeight = height + yPadding - (4 * scaleFactor);
            if (maskHeight < 72 * scaleFactor)
            {
                maskHeight = 72 * scaleFactor;
            }
            LogDebug($"Calculated final background size. Width=[{maskWidth}], Height=[{maskHeight}]");

            // Retrieve current widget size from OBS
            var (currentWidth, currentHeight) = GetCurrentWidgetSize(obsConnection);
            LogDebug($"Retrieved current widget size: Width=[{currentWidth}], Height=[{currentHeight}]");

            // Calculate adjustments for bounce animation
            double widthAdjustment = CalculateAdjustment(maskWidth, currentWidth, scaleFactor);
            double heightAdjustment = CalculateAdjustment(maskHeight, currentHeight, scaleFactor);
            double bounceWidth = CalculateBounceDimension(maskWidth, currentWidth, widthAdjustment, scaleFactor);
            double bounceHeight = CalculateBounceDimension(maskHeight, currentHeight, heightAdjustment, scaleFactor);
            LogDebug($"Calculated adjustments and bounce dimensions. bounceWidth=[{bounceWidth}] bounceHeight=[{bounceHeight}]");

            // Special condition: if bounceHeight equals MinDimensionThreshold, set it to 68 * scaleFactor
            if (bounceHeight == (72 * scaleFactor) && bounceHeight != currentHeight)
            {
                bounceHeight = 68 * scaleFactor;
                LogDebug($"Adjusted bounce height to minimum threshold. bounceHeight=[{bounceHeight}]");
            }

            // Update filter settings for bounce and end state of animation
            double maskPosX = 960 * scaleFactor;
            double maskPosY = 540 * scaleFactor;
            if (maskHeight > (72 * scaleFactor))
            {
                maskPosY += (maskHeight - (72 * scaleFactor)) / 2;
            }

            // Set filter settings for bounce animation
            var newSizeBounceSettings = new JObject
        {
            { "position_x", maskPosX },
            { "position_y", CalculateBouncePosY(bounceHeight, maskHeight, maskPosY) },
            { "rectangle_corner_radius", cornerRadius },
            { "rectangle_width", bounceWidth },
            { "rectangle_height", bounceHeight }
        };
            SetObsSourceFilterSettings("DSI • BG", "New Size Bounce", newSizeBounceSettings, obsConnection);

            // Set filter settings for end state of animation
            var newSizeSettings = new JObject
        {
            { "position_x", maskPosX },
            { "position_y", maskPosY },
            { "rectangle_corner_radius", cornerRadius },
            { "rectangle_width", maskWidth },
            { "rectangle_height", maskHeight }
        };
            SetObsSourceFilterSettings("DSI • BG", "New Size", newSizeSettings, obsConnection);
            _CPH.Wait(50);

            LogDebug("Background filters set successfully.");
            return true;
        }

        private (double width, double height) GetCurrentWidgetSize(int obsConnection)
        {
            GetObsSourceFilterSettings("DSI • BG", "Advanced Mask", obsConnection, out JObject obsResponse);
            double rectangleWidth = obsResponse["rectangle_width"].ToObject<double>();
            double rectangleHeight = obsResponse["rectangle_height"].ToObject<double>();
            return (rectangleWidth, rectangleHeight);
        }

        private double CalculateAdjustment(double newDimension, double currentDimension, double scaleFactor)
        {
            double sizeDifference = Math.Abs(newDimension - currentDimension);
            double bouncePercentage = 0.05;
            double adjustment = sizeDifference * bouncePercentage * scaleFactor;
            double minAdjustment = (72 * scaleFactor) * 0.1;
            double maxAdjustment = (72 * scaleFactor) * 0.5;
            return Clamp(adjustment, minAdjustment, maxAdjustment);
        }

        private double CalculateBounceDimension(double newDimension, double currentDimension, double adjustment, double scaleFactor)
        {
            double dimension = newDimension;
            if (newDimension > currentDimension)
            {
                dimension += adjustment;
            }
            else if (newDimension < currentDimension)
            {
                dimension -= adjustment;
            }
            dimension = dimension * scaleFactor;
            return Math.Max(dimension, 72 * scaleFactor);
        }

        private double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private double CalculateBouncePosY(double bounceHeight, double newHeight, double maskPosY)
        {
            if (bounceHeight != newHeight)
            {
                double heightDiff = bounceHeight - newHeight;
                return maskPosY + heightDiff / 2;
            }
            return maskPosY;
        }
    }
}