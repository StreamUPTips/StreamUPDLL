
using System;
using System.Collections.Generic;

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



  partial class StreamUpLib
  {
    public void SetTriggersForModAddedCommands()
    {
      string[] commands = { "StreamUP", "Mod Added Commands", "Commands" };
      string[] counts = { "StreamUP", "Mod Added Commands", "Commands" };
      List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Command Triggered", "macCommandTriggered", commands),
                new("Command Added", "macCommandAdded", commands),
                new("Command Edited", "macCommandEdited", commands),
                new("Command Deleted", "macCommandDeleted", commands),
                new("Command Fail", "macCommandFailed", commands),
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
  }

}