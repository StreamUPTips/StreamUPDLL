using System;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public string GetStreamerBotFolder() {
            return AppDomain.CurrentDomain.BaseDirectory;
        }
    }
}
