using System;
using System.Reflection;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool CheckStreamUpLibraryVersion(Version minimumRequiredVersion)
        {
            LogInfo("Getting current StreamUP library version");
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (currentVersion < minimumRequiredVersion)
            {
                string errorMessage = "StreamUP.dll file is not the required version";
                string actionMessage = "Please download it via the 'StreamUP_Library_Updater.exe' that was bundled in with this product.\n\nYou should have placed it in the root of your Streamer.Bot folder.";
                LogError("StreamUP.dll file is not the required version");
                MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            LogInfo($"StreamUP library version is sufficient. currentVersion=[{currentVersion}], minimumRequiredVersion=[{minimumRequiredVersion}]");
            return true;
        }    

    }
}
