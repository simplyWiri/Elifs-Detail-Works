using HarmonyLib;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(RoofGrid), nameof(RoofGrid.SetRoof))]
    public static class RoofGrid_SetRoof
    {
        // when a new roof is placed, notify windows that enclose that cell to update
        public static void Prefix(IntVec3 c, RoofDef def)
        {
            MapComponent_Windows mapComp = WindowCache.WindowComponent;

            if (mapComp?.WindowCells?.ContainsKey(c) ?? false)
                mapComp?.DirtyCells?.Add(c);
        }
    }
}