using HarmonyLib;
using UnityEngine;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(GlowGrid), nameof(GlowGrid.GameGlowAt))]
    public static class GlowGrid_GameGlowAt
    {
        public static void Postfix(IntVec3 c, ref float __result)
        {
            if (WindowCache.WindowCells.ContainsKey(c))
                __result = Mathf.Max(__result, Mathf.Max(0f, WindowCache.Map.skyManager.CurSkyGlow));
        }
    }
}