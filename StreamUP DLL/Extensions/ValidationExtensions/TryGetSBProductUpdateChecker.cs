using System.Diagnostics;
using System.Windows.Forms;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public bool TryGetSBProductUpdateChecker()
        {
            // Check if user has already been prompted for the update checker this launch
            if (_CPH.GetGlobalVar<bool>("sup000_UpdateCheckerPrompted", false))
            {
                return true;
            }

            // Check if Update checker action exists
            if (!_CPH.ActionExists("StreamUP Tools • Update Checker"))
            {
                string errorMessage = "The StreamUP Update Checker for Streamer.Bot is not installed";

                string actionMessage = 
                @"You can download it from the StreamUP website.
                Would you like to open the link now?";

                LogError(errorMessage);



                DialogResult result = MessageBox.Show($"{errorMessage}\n\n{actionMessage}", "StreamUP Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    Process.Start("https://streamup.tips/product/update-checker");
                }
                _CPH.SetGlobalVar("sup000_UpdateCheckerPrompted", true, false);
            }
            else
            {
                LogInfo("StreamUP update checker for Streamer.Bot is installed");
            }

            return true;
        }
    

    }
}
