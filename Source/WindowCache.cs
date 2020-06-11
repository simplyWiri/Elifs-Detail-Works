using System.Collections.Generic;
using Verse;

namespace ElifsDecorations
{
    public static class WindowCache
    {
        public static Map Map => Find.CurrentMap;
        private static Map map = null;
        private static MapComponent_Windows windowComponent = null;

        public static MapComponent_Windows WindowComponent
        {
            get
            {
                var locMap = Map;
                if (!(locMap == null) && (map != locMap || windowComponent == null))
                {
                    // we need to dirty our lighting overlay, otherwise we will see tiles that have been lit from the prev map
                    LightingOverlay_Regenerate.dirty = true; 

                    map = locMap;
                    windowComponent = map.GetComponent<MapComponent_Windows>();
                }
                return windowComponent;
            }
        }

        public static Dictionary<IntVec3, List<Building_Window>> WindowCells
        {
            get
            {
                return WindowComponent.WindowCells;
            }
        }

        public static List<Building_Window> Windows
        {
            get
            {
                return WindowComponent.windows;
            }
        }
    }
}