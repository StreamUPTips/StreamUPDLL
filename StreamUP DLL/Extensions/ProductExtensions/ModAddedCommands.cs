
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
      List<CustomTrigger> customTriggers = new List<CustomTrigger>
            {
                new("Command Triggered", "macCommandTriggered", commands),
                new("Command Added", "macCommandAdded", commands),
                new("Command Edited", "macCommandEdited", commands),
                new("Command Deleted", "macCommandDeleted", commands),
               
            };
      SetCustomTriggers(customTriggers);
    }
  }

}