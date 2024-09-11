using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public string GetStreamerBotFolder() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
