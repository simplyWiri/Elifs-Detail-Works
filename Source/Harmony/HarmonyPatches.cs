using HarmonyLib;
using System.Reflection;
using Verse;

namespace ElifsDecorations
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static Harmony instance = null;

        public static Harmony Instance
        {
            get
            {
                if (instance == null)
                    instance = new Harmony("Elif.Decorations");
                return instance;
            }
        }

        static HarmonyPatches()
        {
            Instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}