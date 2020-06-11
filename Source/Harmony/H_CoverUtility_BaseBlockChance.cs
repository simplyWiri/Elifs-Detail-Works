using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(CoverUtility), nameof(CoverUtility.BaseBlockChance), new Type[] { typeof(Thing) })]
    public static class CoverUtility_BaseBlockChance
    {
        // if our window is open... it provides cover
        public static void Postfix(Thing thing, ref float __result)
        {
            if (thing is Building_Window)
                __result = (thing as Building_Window).WindowComp.state != State.Closed ? 0.7f : __result;
        }
    }
}