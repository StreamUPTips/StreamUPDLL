using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using Streamer.bot.Plugin.Interface.Model;
using LiteDB.Engine;
using System.CodeDom;
using System.Data.Common;

namespace StreamUP
{

    public partial class StreamUPExtensions {
        public IInlineInvokeProxy _CPH;

        public StreamUPExtensions (IInlineInvokeProxy CPH) {
            _CPH = CPH;
        }

        public bool TestMe (out string banana) {
            banana = "Hello!";
            _CPH.LogInfo(banana);
            return true;
        }
        
    }    
}