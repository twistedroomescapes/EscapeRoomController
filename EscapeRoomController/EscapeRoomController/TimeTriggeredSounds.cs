using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EscapeRoomController
{
    public class TimeTriggeredSounds
    {
        public bool Triggered { get; set; }
        public string SoundFile { get; set; }

        public TimeTriggeredSounds(string fileName)
        {
            SoundFile = fileName;
        }
    }
}
