using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP {

    public static class UIExtensions {

        public static void SUShowErrorMessage(this IInlineInvokeProxy CPH, string message)
        {
            MessageBox.Show(
                message,
                "StreamUP Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }    

    }
}
