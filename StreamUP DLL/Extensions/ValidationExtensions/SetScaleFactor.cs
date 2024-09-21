using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        private bool SetScaleFactor(int baseWidth, Dictionary<string, object> productSettings, string productNumber)
        {
            // Define the reference width
            const int referenceWidth = 1920;

            // Calculate the scale factor
            float scaleFactor = (float)baseWidth / referenceWidth;

            // Add the scale factor to the productSettings dictionary
            if (productSettings.ContainsKey("ScaleFactor"))
            {
                productSettings["ScaleFactor"] = scaleFactor;
            }
            else
            {
                productSettings.Add("ScaleFactor", scaleFactor);
            }

            // Optionally, log the scale factor for debugging
            LogInfo($"Scale factor calculated: {scaleFactor}");

            // Save the updated productSettings back to CPH global variable
            _CPH.SetGlobalVar($"{productNumber}_ProductSettings", productSettings, true);

            return true;
        }
    }
}
