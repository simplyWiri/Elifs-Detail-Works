using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class H_StatWorker_GetValueUnfinalized
    {
        public static void Postfix(StatRequest req, StatDef ___stat, ref float __result)
        {
            if (___stat != StatDefOf.Beauty || !req.HasThing) return;

            if (req.Thing is Building_Window window)
                __result += window?.WindowComp?.CachedBeauty ?? 0;
        }
    }
}
