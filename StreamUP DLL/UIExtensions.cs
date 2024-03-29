using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class UIExtensions {

        public static void SUUIShowErrorMessage(this IInlineInvokeProxy CPH, string message)
        {
            MessageBox.Show(
                message,
                "StreamUP Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }  

        public static DialogResult SUUIShowYesNoWarningMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message, 
                "StreamUP Warning", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning
                );
            return result;
        }
    }
}
