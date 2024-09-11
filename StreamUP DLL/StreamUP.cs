using System.IO;
using Streamer.bot.Plugin.Interface;

namespace StreamUP
{

    public partial class StreamUpLib
    {
        private IInlineInvokeProxy _CPH;
        private string _ProductIdentifier;

        public StreamUpLib(IInlineInvokeProxy cph, string productIdentifier)
        {
            _CPH = cph;
            _ProductIdentifier = productIdentifier;
        }

        public StreamUpLib(IInlineInvokeProxy cph)
            : this(cph, "UNKNOWN") // Calls the other constructor with a default value
        {
        }
    }
}