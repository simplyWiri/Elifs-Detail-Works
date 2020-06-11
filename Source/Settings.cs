using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace ElifsDecorations
{
    public enum WindowFocalType : int 
    {
        Circular, Rectangular
    }

    public class ElifsDecorationsSettings : ModSettings
    {
        public static bool BeautyEnabled = true;
        public static bool Flickable = true;
        public static bool CanShootThrough = true;
        public static bool CanTransmitTemperature = true;
        public static WindowFocalType focalType = WindowFocalType.Circular;

        public static void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            Heading(listing, "defaultheading".TranslateSimple());
            listing.CheckboxLabeled("enablebeauty".TranslateSimple(), ref BeautyEnabled);
            listing.CheckboxLabeled("flickable".TranslateSimple(), ref Flickable);
            listing.CheckboxLabeled("shootthrough".TranslateSimple(), ref CanShootThrough);
            listing.CheckboxLabeled("temptransmit".TranslateSimple(), ref CanTransmitTemperature);

            Heading(listing, "focaltype".TranslateSimple());
            if (Widgets.RadioButtonLabeled(listing.GetRect(30f), "circular".TranslateSimple(), focalType == WindowFocalType.Circular))
            {
                focalType = WindowFocalType.Circular;
                WindowCache.WindowComponent.UpdateAllWindowCells();
            }
            if (Widgets.RadioButtonLabeled(listing.GetRect(30f), "rectangular".TranslateSimple(), focalType == WindowFocalType.Rectangular))
            {
                focalType = WindowFocalType.Rectangular;
                WindowCache.WindowComponent.UpdateAllWindowCells();
            }

            listing.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref BeautyEnabled, "BeautyEnabled", true);
            Scribe_Values.Look(ref Flickable, "Flickable", true);
            Scribe_Values.Look(ref CanShootThrough, "CanShootThrough", true);
            Scribe_Values.Look(ref CanTransmitTemperature, "CanTransmitTemperature", true);
            Scribe_Values.Look(ref focalType, "FocalType", WindowFocalType.Circular);
        }

        public static void ResetFont()
        {
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static void Heading(Listing_Standard listing, string label)
        {
            Heading(listing.GetRect(30), label);
        }

        public static void Heading(Rect rect, string label)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect, label);
            ResetFont();
        }
    }
}