
using System.Drawing;
using Newtonsoft.Json.Linq;
using Streamer.bot.Plugin.Interface;

namespace StreamUP
{
    partial class StreamUpLib
    {

        public string CreateInput(string sceneName, string sourceName, string sourceKind, JArray sourceSettings, bool enabled, int connection)
        {
            var request = new

            {

                sceneName = sceneName,
                inputName = sourceName,
                inputKind = sourceKind,
                inputSettings = new
                {
                    sourceSettings
                },
                sceneItemEnabled = false



            };

            JToken data = JObject.Parse(_CPH.ObsSendRaw("CreateInput", request.ToString(), 0));
            string sceneItemId = data["sceneItemId"].ToString();
            return sceneItemId;
        }

        public bool RemoveInput(string sourceName, int connection)
        {

            var request = new
            {

                inputName = sourceName
            };

           JObject.Parse(_CPH.ObsSendRaw("RemoveInput", request.ToString(), 0));
            return true;
        }



}
    }