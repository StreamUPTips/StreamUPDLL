using Streamer.bot.Plugin.Interface;
using System;

namespace StreamUP
{

    public partial class StreamUpLib
    {
        private IInlineInvokeProxy _CPH;
        private string _ProductIdentifier;
        private bool _DebugMode;

        // Track if static components have been initialized
        private static bool _staticInitialized = false;
        private static readonly object _initLock = new object();

        public StreamUpLib(IInlineInvokeProxy cph, string productIdentifier)
        {
            _CPH = cph;
            _ProductIdentifier = productIdentifier;
            _DebugMode = cph.GetGlobalVar<bool>("sup000_DebugMode", true);

            // Initialize static components once (thread-safe)
            InitializeStaticComponents();
        }

        public StreamUpLib(IInlineInvokeProxy cph)
            : this(cph, "UNKNOWN") // Calls the other constructor with a default value
        {
        }

        /// <summary>
        /// Initialize static components (ProductConfigRegistry, ProductValidationCache)
        /// This is called once per Streamer.bot session, thread-safe.
        /// </summary>
        private void InitializeStaticComponents()
        {
            if (_staticInitialized)
            {
                return;
            }

            lock (_initLock)
            {
                if (_staticInitialized)
                {
                    return;
                }

                try
                {
                    string sbFolder = GetStreamerBotFolder();

                    // Initialize the product config registry
                    ProductConfigRegistry.Initialize(sbFolder);

                    // Initialize the validation cache
                    ProductValidationCache.Initialize(sbFolder);

                    _staticInitialized = true;
                }
                catch (Exception)
                {
                    // Silent failure - methods will handle missing initialization gracefully
                }
            }
        }
    }
}