using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(SectionLayer_LightingOverlay), nameof(SectionLayer_LightingOverlay.Regenerate))]
    public static class LightingOverlay_Regenerate
    {
        // what we do here... find all of the cells which are being lit by our windows
        // pretend that there is no roof above them, so the regular lighting is applied
        // reset back to what is was before afterwards

        private static Dictionary<int, RoofDef> changedRoofs = new Dictionary<int, RoofDef>();
        private static FieldInfo roofGridInfo = AccessTools.Field(typeof(RoofGrid), "roofGrid");
        public static bool dirty = true;
        private static RoofDef[] roofRef = null;

        public static void Prefix()
        {
            var locMap = WindowCache.Map;
            var component = WindowCache.WindowComponent;

            if (dirty) // this signifies that we have changed maps, lets get the (new) roofs for the map
            {
                roofRef = (RoofDef[])roofGridInfo.GetValue(locMap.roofGrid);

                RecalcLight(locMap, component);

                dirty = false;
            }

            if (WindowCache.WindowComponent?.dirty ?? false)
                RecalcLight(locMap, component);
            else
                foreach (KeyValuePair<int, RoofDef> entry in changedRoofs)
                    roofRef[entry.Key] = null;
        }

        private static void RecalcLight(Map map, MapComponent_Windows comp)
        {
            changedRoofs = new Dictionary<int, RoofDef>();

            // hypothetically we don't need to redo all of this, we could simply get the tiles. Todo: Check overhead of this
            // not much overhead, but it could be done! maybe if complaints are raised
            foreach (IntVec3 cell in comp.WindowCells.Keys)
            {
                var index = map.cellIndices.CellToIndex(cell);
                changedRoofs.Add(index, roofRef[index]);
                roofRef[index] = null;
            }

            // we have validated our cache, lets make sure we don't do it needlessly
            WindowCache.WindowComponent.dirty = false;
        }

        public static void Postfix()
        {
            foreach (KeyValuePair<int, RoofDef> entry in changedRoofs)
                roofRef[entry.Key] = entry.Value;
        }
    }
}