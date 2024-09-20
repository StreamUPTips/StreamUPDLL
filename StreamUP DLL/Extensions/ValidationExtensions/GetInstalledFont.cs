using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool GetInstalledFonts(List<(string fontName, string fontFile, string fontUrl)> requiredFonts)
        {
            bool allFontsInstalled = true;

            foreach (var font in requiredFonts)
            {
                string fontName = font.fontName;
                string fontFile = font.fontFile;
                string fontUrl = font.fontUrl;

                // Get the list of installed font families
                if (!FontFamily.Families.Any(ff => ff.Name.Equals(fontName, StringComparison.OrdinalIgnoreCase)))
                {
                    LogFontWarning(fontName, fontFile, fontUrl);
                    allFontsInstalled = false;
                    continue;
                }

                // Check font file in default and AppData font directories
                if (!CheckFontFileExists(fontFile))
                {
                    LogFontWarning(fontName, fontFile, fontUrl);
                    allFontsInstalled = false;
                }
            }

            LogInfo(allFontsInstalled ? "All required fonts are installed." : "One or more required fonts are not installed.");
            return allFontsInstalled;
        }

        private void LogFontWarning(string fontName, string fontFile, string fontUrl)
        {
            LogError($"The font [{fontFile}] is not installed. The product may not look or function properly");

            DialogResult result = MessageBox.Show($"The font '{fontFile}' is not installed. The product may not function properly.\n\nWould you like to go to the fonts download page now? Once installed make sure to restart OBS.", "StreamUP Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                Process.Start(fontUrl);
            }
        }

        private bool CheckFontFileExists(string fontFile)
        {
            string[] fontDirectories = {
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts), // Default Fonts
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "Fonts") // AppData Fonts
            };

            foreach (string directory in fontDirectories)
            {
                LogInfo($"Searching for '{fontFile}' in: [{directory}]");
                if (Directory.EnumerateFiles(directory, $"{Path.GetFileNameWithoutExtension(fontFile)}.*")
                    .Any(file => Path.GetFileName(file).Equals(fontFile, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }    
    }

}
