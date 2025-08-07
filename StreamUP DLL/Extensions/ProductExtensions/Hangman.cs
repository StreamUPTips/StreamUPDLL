using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Streamer.bot.Plugin.Interface.Enums;
using Streamer.bot.Plugin.Interface.Model;

namespace StreamUP
{
    public class Definition
    {
        public string definition { get; set; }
        public List<object> synonyms { get; set; }
        public List<object> antonyms { get; set; }
    }

    public class License
    {
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Meaning
    {
        public string partOfSpeech { get; set; }
        public List<Definition> definitions { get; set; }
        public List<object> synonyms { get; set; }
        public List<object> antonyms { get; set; }
    }

    public class Hangman
    {
        public string word { get; set; }
        public List<object> phonetics { get; set; }
        public List<Meaning> meanings { get; set; }
        public License license { get; set; }
        public List<string> sourceUrls { get; set; }
    }
    public partial class StreamUpLib
    {

        public async Task<string> GetDefinitionAsync(string word)
        {
            var dictUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
            string wordDefinition;
            string dictString = await _httpClient.GetStringAsync(dictUrl);
            List<Hangman> json = JsonConvert.DeserializeObject<List<Hangman>>(dictString);
            wordDefinition = json[0]?.meanings[0]?.definitions[0]?.definition ?? " ";
            return wordDefinition;
        }
        public void SetTriggersForHangman()
        {
            string[] categories = { "StreamUP", "Hangman" };
            List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Game Started", "hangmanStart", categories),
                new("Game Ended", "hangmanEnd", categories),
                new("Game Lost", "hangmanLost", categories),
                new("Game Won", "hangmanWon", categories),
                new("Correct Guess", "hangmanCorrect", categories),
                new("Incorrect Guess", "hangmanIncorrect", categories),
                new("Fail/Error", "hangmanFail", categories)
            };
            SetCustomTriggers(customTriggers);
        }

        public int GetScore(string userId, Platform platform)
        {
            int score;
            int defaultRank = GetSetting<int>("hangmanRanking", 1500);
            score = GetUserVariableById<int>(userId, "hangmanRanking", platform, true, defaultRank);
            return score;
        }

        public void AddScore(string userId, int scoreToAdd, Platform platform)
        {
            int currentScore = GetScore(userId, platform);
            int newScore = currentScore + scoreToAdd;
            SetUserVariableById(userId, "hangmanRanking", newScore, platform, true);
            _CPH.SetArgument("oldScore", currentScore);
            _CPH.SetArgument("newScore", newScore);
            _CPH.SetArgument("scoreToAdd", scoreToAdd);
        }



    }
}