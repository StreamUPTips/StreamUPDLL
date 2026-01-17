using System;
using System.Collections.Generic;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        //# Split text based on width
        /// <summary>
        /// [OBSOLETE] Use ObsFitTextToSize instead. This method passes through to the new implementation.
        /// </summary>
        [Obsolete("Use ObsFitTextToSize instead")]
        public string ObsSplitTextOnWidth(
            string productNumber,
            int obsConnection,
            OBSSceneType parentSourceType,
            string sceneName,
            string sourceName,
            string rawText,
            int maxWidth,
            int maxHeight
        )
        {
            // Pass through to new method (productNumber and parentSourceType are unused)
            return ObsFitTextToSize(
                sceneName,
                sourceName,
                rawText,
                maxWidth,
                maxHeight,
                obsConnection
            );
        }

        /// <summary>
        /// [OBSOLETE] Use ObsWrapTextToWidth instead. This method passes through to the new implementation.
        /// </summary>
        [Obsolete("Use ObsWrapTextToWidth instead")]
        private List<string> ObsSplitTextIntoLines(
            string productNumber,
            int obsConnection,
            int maxWidth,
            string message,
            OBSSceneType parentSourceType,
            string sceneName,
            string sourceName
        )
        {
            // Pass through to new method (productNumber and parentSourceType are unused)
            return ObsWrapTextToWidth(sceneName, sourceName, message, maxWidth, obsConnection);
        }
    }
}