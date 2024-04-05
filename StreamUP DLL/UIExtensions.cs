using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class UIExtensions {

        public static DialogResult SUUIShowErrorOKMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message,
                "StreamUP Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return result;
        }  

        public static DialogResult SUUIShowWarningYesNoMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message, 
                "StreamUP Warning", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Warning
                );
            return result;
        }

        public static DialogResult SUUIShowInformationOKMessage(this IInlineInvokeProxy CPH, string message)
        {
            DialogResult result = MessageBox.Show(
                message, 
                "StreamUP Notification", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information
                );
            return result;
        }
    }
}
