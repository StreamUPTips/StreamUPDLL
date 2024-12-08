
using System;

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
    
}