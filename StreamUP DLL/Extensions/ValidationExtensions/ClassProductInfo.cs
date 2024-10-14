using System;

namespace StreamUP
{
        [Serializable]
        public class ProductInfo
        {
            public string ProductName { get; set; } = string.Empty;
            public string ProductNumber { get; set; } = string.Empty;
            public Version RequiredLibraryVersion { get; set; } = new Version (0, 0, 0, 0);
            public string SceneName { get; set; } = string.Empty;
            public string SettingsAction { get; set; } = string.Empty;
            public string SourceNameVersionCheck { get; set; } = string.Empty;
            public Version SourceNameVersionNumber { get; set; } = new Version (0, 0, 0, 0);
            public Version ProductVersionNumber { get; set; } = new Version (0, 0, 0, 0);
        }
    
}
