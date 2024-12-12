using Newtonsoft.Json;
using System;

public class VersionConverter : JsonConverter<Version>
{
    public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        string versionString = (string)reader.Value;
        return Version.Parse(versionString);
    }
}

namespace StreamUP
{
    [Serializable]
    public class ProductInfo
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductNumber { get; set; } = string.Empty;

        [JsonConverter(typeof(VersionConverter))]
        public Version RequiredLibraryVersion { get; set; } = new Version(0, 0, 0, 0);

        public string SceneName { get; set; } = string.Empty;
        public string SettingsAction { get; set; } = string.Empty;
        public string SourceNameVersionCheck { get; set; } = string.Empty;

        [JsonConverter(typeof(VersionConverter))]
        public Version SourceNameVersionNumber { get; set; } = new Version(0, 0, 0, 0);

        [JsonConverter(typeof(VersionConverter))]
        public Version ProductVersionNumber { get; set; } = new Version(0, 0, 0, 0);
    }
}
