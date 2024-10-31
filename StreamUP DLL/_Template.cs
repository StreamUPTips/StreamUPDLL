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
using Streamer.bot.Plugin.Interface.Model;
using System.CodeDom;
using System.Data.Common;

namespace StreamUP
{
    public partial class StreamUPExtensions
    {
        public bool MethodName (Dictionary<string, object> parameters, out string varName)
        {
            varName = "Whatever";
            return true;
        }
    }
}