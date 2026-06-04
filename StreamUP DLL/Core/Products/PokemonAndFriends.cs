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
                new("Pokémon Status Applied", "pafStatusApplied", commands),
                new("Pokémon Status Wore Off", "pafStatusWoreOff", commands),
                new("Any Ball Thrown", "pafAnyBallThrown", commands),
                new("Pokeball Thrown", "pafPokeballThrown", commands),
                new("Greatball Thrown", "pafGreatballThrown", commands),
                new("Ultraball Thrown", "pafUltraballThrown", commands),
                new("Masterball Thrown", "pafMasterballThrown", commands),
                new("Catch Failed/Escaped", "pafCatchFailed", commands),
                new("Pokémon Error", "pafError", commands),
                new("Pokedex Checked", "pafPokedexChecked", commands)



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

        public string CatchPokemon(JObject catchInfo, string streamUPStreamerKey, int multiplier = 1, int ball = 0, int status = 0)
        {
            var data = new StringContent(catchInfo.ToString(), Encoding.UTF8, "application/json");
            double catchValue = (multiplier + ball + status) / 100;
            LogInfo($"Attempting to catch Pokémon with multiplier {multiplier}, ball {ball} and status {status} = catchValue {catchValue}");
            LogInfo($"Catch Info: {catchInfo}");
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.streamup.tips/pokemon/catch?language=en&catchValue={catchValue}");
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
        public string PokemonSpawn(string streamUPStreamerKey, int pafGenerations, bool pafSpecial, string pafLanguage,  int shinyChance = 256)
        {
            string pokeURI = $"https://api.streamup.tips/pokemon/random?generation={pafGenerations}&special={pafSpecial}&language={pafLanguage}&shinyChance={shinyChance}";
            if (_CPH.TryGetArg<string>("rawInput", out string selectedPokemon) && !string.IsNullOrEmpty(selectedPokemon))
            {
                if (int.TryParse(selectedPokemon, out int pokeId))
                {
                    pokeURI = $"https://api.streamup.tips/pokemon/index/{pokeId}?shinyChance={shinyChance}";
                }
                else
                {
                    pokeURI = $"https://api.streamup.tips/pokemon/name/{selectedPokemon}?shinyChance={shinyChance}";
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
            //4 - Ball is not Available.
        }

        public string GetBallName(string ballType)
        {
            return ballType switch
            {
                "poke" => "PokéBall",
                "great" => "Great Ball",
                "ultra" => "Ultra Ball",
                "master" => "Master Ball",
                _ => "Unknown Ball"
            };
        }

        public string GetBallURL(string ball)
        {
            return ball switch
            {
                "poke" => "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/poke-ball.png",
                "great" => "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/great-ball.png",
                "ultra" => "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/ultra-ball.png",
                "master" => "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/master-ball.png",
                _ => "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/items/poke-ball.png"
            };
        }
    }




    public class PokemonSettings
    {
        [JsonProperty("poke_af_api_key")]
        public string Poke_af_api_key { get; set; } = "";
        [JsonProperty("poke_af_gens")]
        public string Poke_af_gens { get; set; } = "2";
        [JsonProperty("poke_af_lang_select")]
        public string Poke_af_lang_select { get; set; } = "en";
        [JsonProperty("poke_af_catch_multi")]
        public int Poke_af_catch_multi { get; set; } = 1;
        [JsonProperty("poke_af_sprite_type")]
        public string Poke_af_sprite_type { get; set; } = "Default";
        [JsonProperty("poke_af_shiny_chance")]
        public int Poke_af_shiny_chance { get; set; } = 265;
        [JsonProperty("poke_af_flee_chance")]
        public double Poke_af_flee_chance { get; set; } = 50.0;
        [JsonProperty("poke_af_seasonal")]
        public bool Poke_af_seasonal { get; set; } = false;
        [JsonProperty("poke_af_channel_redeem")]
        public string Poke_af_channel_redeem { get; set; } = "";
        [JsonProperty("poke_af_status_boost")]
        public int Poke_af_status_boost { get; set; } = 20;
        [JsonProperty("poke_af_status_effects")]
        public List<string> Poke_af_status_effects { get; set; } = new List<string>();
        [JsonProperty("poke_af_effect_off")]
        public int Poke_af_effect_off { get; set; } = 50;
        [JsonProperty("poke_af_enable_poke")]
        public bool Poke_af_enable_poke { get; set; } = true;
        [JsonProperty("poke_af_pokeball")]
        public int Poke_af_pokeball { get; set; } = 0;
        [JsonProperty("poke_af_pokeball_use")]
        public int Poke_af_pokeball_use { get; set; } = 65;
        [JsonProperty("poke_af_enable_great")]
        public bool Poke_af_enable_great { get; set; } = true;
        [JsonProperty("poke_af_great_ball")]
        public int Poke_af_great_ball { get; set; } = 0;
        [JsonProperty("poke_af_greatball_use")]
        public int Poke_af_greatball_use { get; set; } = 20;
        [JsonProperty("poke_af_enable_ultra")]
        public bool Poke_af_enable_ultra { get; set; } = true;
        [JsonProperty("poke_af_ultra_ball")]
        public int Poke_af_ultra_ball { get; set; } = 0;
        [JsonProperty("poke_af_ultraball_use")]
        public int Poke_af_ultraball_use { get; set; } = 10;
        [JsonProperty("poke_af_enable_master")]
        public bool Poke_af_enable_master { get; set; } = true;
        [JsonProperty("poke_af_master_ball")]
        public int Poke_af_master_ball { get; set; } = 100;
        [JsonProperty("poke_af_masterball_use")]
        public int Poke_af_masterball_use { get; set; } = 5;
    }
}
