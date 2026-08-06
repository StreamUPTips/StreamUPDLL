
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StreamUP
{


  [Serializable()]
  public class CustomTimed
  {
    public int Id { get; set; }
    public int Time { get; set; }
    public DateTime NextRun { get; set; }
    public string Message { get; set; }
  }





public class ModAddedCommand
{
    [JsonProperty("command")]
    public string Command { get; set; } = string.Empty;
    [JsonProperty("output")]
    public int Output { get; set; } = 0;
    [JsonProperty("user_cooldown")]
    public int User_cooldown { get; set; } = 0;
    [JsonProperty("global_cooldown")]
    public int Global_cooldown { get; set; } = 0;
    [JsonProperty("permission")]
    public int Permission { get; set; } = 0;
    [JsonProperty("volume")]
    public int Volume { get; set; } = 0;
}


public class ModAddedCommandSettings
{
    [JsonProperty("mac_command_table")]
    public List<ModAddedCommand> Mac_command_table { get; set; } = new List<ModAddedCommand>();
    [JsonProperty("mac_allow_globals")]
    public bool Mac_allow_globals { get; set; } = false;
    [JsonProperty("mac_allow_sounds")]
    public bool Mac_allow_sounds { get; set; } = false;
    [JsonProperty("mac_fetch_url")]
    public bool Mac_fetch_url { get; set; } = false;
    [JsonProperty("mac_banned_commands")]
    public List<string> Mac_banned_commands { get; set; } = new List<string>();
    [JsonProperty("mac_global_cooldown")]
    public int Mac_global_cooldown { get; set; } = 30;
    [JsonProperty("mac_user_cooldown")]
    public int Mac_user_cooldown { get; set; } = 60;
    [JsonProperty("mac_permission")]
    public int Mac_permission { get; set; } = 1;
    [JsonProperty("mac_volume")]
    public int Mac_volume { get; set; } = 0;
    [JsonProperty("mac_counts_table")]
    public Dictionary<string, int> Mac_counts_table { get; set; } = new Dictionary<string, int>();
}





  partial class StreamUpLib
  {

    public void SetTriggersForModAddedCommands()
    {
      string[] commands = { "StreamUP", "Mod Added Commands", "Commands" };
      string[] counts = { "StreamUP", "Mod Added Commands", "Counts" };
      List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Command Triggered", "macCommandTriggered", commands),
                new("Command Added", "macCommandAdded", commands),
                new("Command Edited", "macCommandEdited", commands),
                new("Command Deleted", "macCommandDeleted", commands),
                new("Command Fail", "macCommandFailed", commands),
                new("Command Cooldown", "macCommandOnCooldown", commands),
                new("Command No Permisson", "macPermissionDenied", commands),
                new("Command Permission Updated", "macPermissionUpdated", commands),
                new("Command Cooldown Updated", "macCooldownUpdated", commands),
                new("Count Failed", "macCountFailed", counts),
                new("Count Updated", "macCountUpdated", counts),
                new("Count Check", "macCountCheck", counts),


            };
      SetCustomTriggers(customTriggers);
    }

    public bool ModAddedCommandFail(int code, string message)
    {
      _CPH.SetArgument("errorCode", code);
      _CPH.SetArgument("errorMessage", message);
      LogError($"{code} - {message}");
      _CPH.TriggerCodeEvent("macCommandFailed");
      return true;
    }


    public bool DoesModAddedExist(List<ModAddedCommand> commands, string command)
    {
      return commands.Exists(c => c.Command.Equals(command, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsBannedCommand(List<string> banned, string command)
    {
      return banned.Exists(c => c.Equals(command, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string> GetSound(string command, string message, bool allow)
    {
      if (allow)
      {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StreamUP", "Sounds", $"{command}.mp3");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        Regex regex = new Regex(@"sound{([^}]+)}");
        var matches = regex.Matches(message);
        foreach (Match match in matches)
        {
          string url = match.Groups[1].Value;
          using (var response = _httpClient.GetAsync(url).Result)
          {
            if (response.IsSuccessStatusCode)
            {
              using (var stream = await _httpClient.GetStreamAsync(url))
              using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
              {
                await stream.CopyToAsync(fs);
              }
            }
            else
            {
              LogError($"Failed to download: {url} (Status: {response.StatusCode})");
            }
          }

          message = message.Replace(url, command);
        }
      }

      return message;
    }

  }
}