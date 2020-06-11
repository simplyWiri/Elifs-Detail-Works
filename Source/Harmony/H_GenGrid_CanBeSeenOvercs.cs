using HarmonyLib;
using System;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(GenGrid), nameof(GenGrid.CanBeSeenOver), new Type[] { typeof(Building) })]
    public static class GenGrid_CanBeSeenOver
    {
        // if our window is open, we can shoot out of it
        public static void Postfix(Building b, ref bool __result)
        {
            if (ElifsDecorationsSettings.CanShootThrough && b is Building_Window && (b as Building_Window).WindowComp.state == State.Ajar)
                __result = true;
        }
    }
}