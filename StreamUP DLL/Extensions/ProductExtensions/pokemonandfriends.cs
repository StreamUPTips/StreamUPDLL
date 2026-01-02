using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using Streamer.bot.Plugin.Interface;
using System.Globalization;
using System.Net;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using Streamer.bot.Plugin.Interface.Model;
using System.CodeDom;
using System.Data.Common;
using System.Net.Http;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public void SetTriggersForPokemon()
        {
            string[] commands = { "StreamUP", "Pokemon and Friends" };

            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Pokémon Spawned", "pafSpawned", commands),
                new("Pokémon Shiny Spawned", "pafShinySpawned", commands),
                new("Pokémon Captured", "pafCaptured", commands),
                new("Pokémon Fled", "pafFled", commands),
                new("Any Ball Thrown", "pafAnyBallThrown", commands),
                //new("Pokeball Thrown", "pafPokeballThrown", commands),
                //new("Greatball Thrown", "pafGreatballThrown", commands),
                //new("Ultraball Thrown", "pafUltraballThrown", commands),
                //new("Masterball Thrown", "pafMasterballThrown", commands),
                new("Catch Failed/Escaped", "pafCatchFailed", commands),
                new("Pokémon Error", "pafError", commands),
                new("Pokedex Checked", "pafPokedexChecked", commands)
                //Inventory?
                //CatchModifier?


            };
            SetCustomTriggers(customTriggers);
        }

        public JObject GetPokemonDataById(int id, string streamUPStreamerKey)
        {
            // Implementation to retrieve Pokémon data by ID
            _httpClient.DefaultRequestHeaders.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var pokemonJson = _httpClient.GetStringAsync("https://api.streamup.tips/pokemon/index/" + id).Result;
            return JObject.Parse(pokemonJson);
        }

        public JObject GetPokemonDataByName(string name, string streamUPStreamerKey)
        {
            // Implementation to retrieve Pokémon data by name
            _httpClient.DefaultRequestHeaders.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var pokemonJson = _httpClient.GetStringAsync("https://api.streamup.tips/pokemon/name/" + name).Result;
            return JObject.Parse(pokemonJson);
        }

        public JObject GetPokemonDataFromScreen()
        {
            string pokemonInfoJson = _CPH.GetGlobalVar<string>("pafCurrentPokemonJson");
            JObject pokemon = JObject.Parse(pokemonInfoJson);
            return pokemon;
            // Implementation to retrieve Pokémon data from screen
        }

        public string CatchPokemon(JObject catchInfo, string streamUPStreamerKey)
        {
            // Implementation to catch a Pokémon
            var data = new StringContent(catchInfo.ToString(), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var catchResult = _httpClient.PostAsync("https://api.streamup.tips/pokemon/catch?language=en", data).Result;
            if (catchResult.IsSuccessStatusCode)
            {
                var response = catchResult.Content.ReadAsStringAsync().Result;
                bool caught = bool.Parse(response);
                if (caught)
                {
                    return "caught";
                }
                else
                {
                    return "escaped";
                }
            }
            if (catchResult.StatusCode == HttpStatusCode.Unauthorized)
            {
                return "unauthorized";
            }

            return catchResult.StatusCode.ToString();
        }
        public JObject PokemonSpawn(string streamUPStreamerKey, int pafGenerations, bool pafSpecial, string pafLanguage)
        {
            _httpClient.DefaultRequestHeaders.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            string pokeURI = $"https://api.streamup.tips/pokemon/random?generation={pafGenerations}&special={pafSpecial}&language={pafLanguage}";
            if (_CPH.TryGetArg<string>("rawInput", out string selectedPokemon) && !string.IsNullOrEmpty(selectedPokemon))
            {
                if (int.TryParse(selectedPokemon, out int pokeId))
                {
                    pokeURI = $"https://api.streamup.tips/pokemon/index/{pokeId}";
                }
                else
                {
                    pokeURI = $"https://api.streamup.tips/pokemon/name/{selectedPokemon}";
                }
            }

            var pokemonInfoResponse = _httpClient.GetAsync(pokeURI).Result;
            string pokemonInfoJson = string.Empty;
            if (pokemonInfoResponse.IsSuccessStatusCode)
            {
                pokemonInfoJson = pokemonInfoResponse.Content.ReadAsStringAsync().Result;
            }
            else
            {
                // Handle unsuccessful response
                if (pokemonInfoResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LogError("Pokémon and Friends: Not authorized to make requests, please check your StreamUP Streamer key!");
                }
                else
                {
                    LogError($"Pokémon and Friends: An ({pokemonInfoResponse.StatusCode}) error occurred.");
                }
                return new JObject();
            }
            JObject pokemon = JObject.Parse(pokemonInfoJson.ToString());
            return pokemon;
        }
        public bool PokemonAndFriendsFail(int code, string message)
        {
            _CPH.SetArgument("errorCode", code);
            _CPH.SetArgument("errorMessage", message);
            LogError($"[PAF] • {code} - {message}");
            _CPH.TriggerCodeEvent("pafError");
            return true;
            //! - Errors
            //1 - No Valid Input or Pokemon on screen. Please check and try again later.
            //2 - A catch is already in progress. Please wait for it to complete before trying again.
            //3 - No Pokemon on screen.
        }
    }
}
