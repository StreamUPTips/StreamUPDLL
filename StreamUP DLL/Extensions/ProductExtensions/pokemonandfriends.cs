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

        public string GetPokemonDataById(int id, string streamUPStreamerKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.streamup.tips/pokemon/index/{id}");
            request.Headers.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var response = _httpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                LogError($"GetPokemonDataById: ({response.StatusCode}) error.");
                return "{}";
            }
            var pokemonJson = response.Content.ReadAsStringAsync().Result;
            return pokemonJson;
        }

        public string GetPokemonDataByName(string name, string streamUPStreamerKey)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.streamup.tips/pokemon/name/{name}");
            request.Headers.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var response = _httpClient.SendAsync(request).Result;
            if (!response.IsSuccessStatusCode)
            {
                LogError($"GetPokemonDataByName: ({response.StatusCode}) error.");
                return "{}";
            }
            var pokemonJson = response.Content.ReadAsStringAsync().Result;
            return pokemonJson;
        }

        public string GetPokemonDataFromScreen()
        {
            string pokemonInfoJson = _CPH.GetGlobalVar<string>("pafCurrentPokemonJson");
            return pokemonInfoJson;
            // Implementation to retrieve Pokémon data from screen
        }

        public string CatchPokemon(JObject catchInfo, string streamUPStreamerKey)
        {
            var data = new StringContent(catchInfo.ToString(), Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.streamup.tips/pokemon/catch?language=en");
            request.Headers.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            request.Content = data;
            var catchResult = _httpClient.SendAsync(request).Result;
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
        public string PokemonSpawn(string streamUPStreamerKey, int pafGenerations, bool pafSpecial, string pafLanguage)
        {
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
            var request = new HttpRequestMessage(HttpMethod.Get, pokeURI);
            request.Headers.Add("X-StreamUP-Streamer-Key", streamUPStreamerKey);
            var pokemonInfoResponse = _httpClient.SendAsync(request).Result;
            string pokemonInfoJson = string.Empty;
            if (pokemonInfoResponse.IsSuccessStatusCode)
            {
                pokemonInfoJson = pokemonInfoResponse.Content.ReadAsStringAsync().Result;
            }
            else
            {
                if (pokemonInfoResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    LogError("Pokémon and Friends: Not authorized to make requests, please check your StreamUP Streamer key!");
                }
                else
                {
                    LogError($"Pokémon and Friends: An ({pokemonInfoResponse.StatusCode}) error occurred.");
                }
                return "{}";
            }
            return pokemonInfoJson;
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
