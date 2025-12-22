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
                new("Pokeball Thrown", "pafPokeballThrown", commands),
                new("Greatball Thrown", "pafGreatballThrown", commands),
                new("Ultraball Thrown", "pafUltraballThrown", commands),
                new("Masterball Thrown", "pafMasterballThrown", commands),
                new("Catch Failed", "pafCatchFailed", commands),
                new("Pokémon Error", "pafError", commands),
                new("Pokedex Checked", "pafPokedexChecked", commands)
                //Inventory?
                //CatchModifier?


            };
            SetCustomTriggers(customTriggers);
        }

        public bool PokemonAndFriendsFail(int code, string message)
        {
            _CPH.SetArgument("errorCode", code);
            _CPH.SetArgument("errorMessage", message);
            LogError($"[PAF] • {code} - {message}");
            _CPH.TriggerCodeEvent("pafError");
            return true;
        }
    }
}
