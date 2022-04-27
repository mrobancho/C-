//Author: Marlon Robancho
using System.Collections.Generic;

namespace System_Dependencies
{
    class Program
    {
        public string Name { get; set; }
        public List<string> ProgramsThisDependsOn { get; set; } = new List<string>();
        public List<string> ProgramsDependsOnThis { get; set; } = new List<string>();
        public bool isInstalled { get; set; } = false;
        public bool isManuallyInstalled { get; set; } = false;
    }
}
