using UnityEngine;
using Verse;

namespace ElifsDecorations
{
    public class ElifsDecorationsMod : Mod
    {
        private ElifsDecorationsSettings settings;

        public ElifsDecorationsMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<ElifsDecorationsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            ElifsDecorationsSettings.DoWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "ElifsDecorations";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }
    }
}