using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StreamUP
{
    partial class StreamUpLib
    {
        
        public JObject ReadJsonFromFile(string path)
        {
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "{}\n");
            }
            string json = File.ReadAllText(path);
            return JObject.Parse(json);
        }

        public bool ParseJsonToArguments(string jsonString, string prefix = "parse")
        {
            //? Heavily Inspired by the Work done by Happy Plotter!
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                _CPH.LogInfo("Provided JSON string is empty.");
                return false;
            }

            _CPH.LogDebug("Starting JSON parsing.");

            using (var reader = new JsonTextReader(new StringReader(jsonString)))
            {
                while (reader.Read())
                {
                    if (reader.TokenType != JsonToken.PropertyName && reader.Value != null)
                    {
                        string argumentName = $"{prefix}.{reader.Path}"; // Keeps [] in paths //! This is where we control the name of the Argument so if you dont want the [] we remove them here
                        _CPH.LogDebug($"{argumentName} = {reader.Value}");
                        _CPH.SetArgument(argumentName, reader.Value.ToString());
                    }
                }
            }

            return true;
        }
    }
}