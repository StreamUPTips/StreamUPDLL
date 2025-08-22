using Streamer.bot.Plugin.Interface;

namespace StreamUP
{

    public partial class StreamUpLib
    {
        private IInlineInvokeProxy _CPH;
        private string _ProductIdentifier;
        private bool _DebugMode;

        public StreamUpLib(IInlineInvokeProxy cph, string productIdentifier)
        {
            _CPH = cph;
            _ProductIdentifier = productIdentifier;
            _DebugMode = cph.GetGlobalVar<bool>("sup000_DebugMode", true);
        }

        public StreamUpLib(IInlineInvokeProxy cph)
            : this(cph, "UNKNOWN") // Calls the other constructor with a default value
        {
        }
    }
}