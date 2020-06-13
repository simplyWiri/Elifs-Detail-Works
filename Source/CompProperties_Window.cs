using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;


namespace ElifsDecorations
{
    public class CompProperties_Window : CompProperties
    {
        public CompProperties_Window()
        {
            compClass = typeof(CompWindow);
        }

        public bool ajar = true;
        public bool closed = true;
        public bool open = true;
        
        public int radius = -1;
        public int x = 3;
        public int y = 3;
    }
}
