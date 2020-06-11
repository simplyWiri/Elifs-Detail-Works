using HarmonyLib;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(ThingGrid), nameof(ThingGrid.RegisterInCell))]
    public static class ThingGrid_Register
    {
        // update windows if required because a new 'Thing' has been placed nearby
        public static void Prefix(Thing t, IntVec3 c)
        {
            if (!(t is Building && t.def.passability == Traversability.Impassable))
                return;

            var mapComp = WindowCache.WindowComponent;

            if (mapComp?.WindowCells?.ContainsKey(c) ?? false)
                mapComp?.DirtyCells?.Add(c);
        }
    }

    [HarmonyPatch(typeof(ThingGrid), nameof(ThingGrid.DeregisterInCell))]
    public static class ThingGrid_Deregister
    {
        public static void Prefix(Thing t, IntVec3 c)
        {
            if (!(t is Building && t.def.passability == Traversability.Impassable))
                return;

            var mapComp = WindowCache.WindowComponent;

            if (mapComp?.WindowCells?.ContainsKey(c) ?? false)
                mapComp?.DirtyCells?.Add(c);
        }
    }
}