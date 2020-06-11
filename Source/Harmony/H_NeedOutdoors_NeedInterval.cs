using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;

namespace ElifsDecorations
{
    [HarmonyPatch(typeof(Need_Outdoors), nameof(Need_Outdoors.NeedInterval))]
    public static class NeedOutdoors_NeedInterval
    {
        // pawns shouldn't desire going outdoors when they are next to a window!

        // num *= 0.2f; <- corresponding C#
        // IL_00e1: ldloc.1
        // IL_00e2: ldc.r4 0.2 <- We look for this code
        // IL_00e7: mul
        // IL_00e8: stloc.1 <- we want to insert AdjustNum after this code

        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var instAsList = instructions.ToList();
            int Index = instAsList.FirstIndexOf(inst => inst.opcode == OpCodes.Ldc_R4 && (float)inst.operand == 0.2f);
            Index = Index + 2;
            for (int i = 0; i < instAsList.Count(); i++)
            {
                if (i == Index)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_2); // roofdef
                    yield return new CodeInstruction(OpCodes.Ldarg_0); // Need_Outdoors
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Need_Outdoors), "pawn")); // Need_Outdoors.pawn
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 1); // ref num
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NeedOutdoors_NeedInterval), nameof(AdjustNum))); // AdjustNum(roofdef, pawn, ref num);
                }
                yield return instAsList[i];
            }
        }

        public static void AdjustNum(RoofDef roofDef, Pawn pawn, ref float num)
        {
            if (roofDef != null && !pawn.Map.GetComponent<MapComponent_Windows>().WindowCells.ContainsKey(pawn.Position))
            {
                num = (num < 0f) ? num * 0.9f : num / 0.9f;
                return;
            }
        }
    }
}