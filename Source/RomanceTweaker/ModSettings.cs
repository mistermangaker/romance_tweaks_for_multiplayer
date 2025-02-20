using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;
using Verse.Sound;
using Verse.Noise;
using Multiplayer.API;
using System.Xml.Linq;


namespace RomanceTweaker
{
    [StaticConstructorOnStartup]
    public static class multiplayerstartup
    {
        static multiplayerstartup()
        {
            if (!MP.enabled) return;

            // This is where the magic happens and your attributes
            // auto register, similar to Harmony's PatchAll.
            MP.RegisterAll();

            // You can choose to not auto register and do it manually
            // with the MP.Register* methods.

            // Use MP.IsInMultiplayer to act upon it in other places
            // user can have it enabled and not be in session
        }

        

    }




    public class RomanceMod : Mod
    {


        public static RomanceModSettings settings;
        public RomanceMod(ModContentPack content) 
            : base(content)
        {
            settings = GetSettings<RomanceModSettings>();
            //new Harmony("RomanceTweaker").PatchAll();

        }

        public override string SettingsCategory()
        {
            return "RomanceTweaker".Translate();
        }
        private static void DoSettingsLine(Listing_Standard listing, string name, ref float val, float min, float max, float minlevel = 0f)
        {
            Rect headerRect = listing.GetRect(20f);
            Rect bottombits = listing.GetRect(16f);

            Rect sliders = bottombits.RightPart(0.7f).Rounded();
            Rect numWidget = bottombits.LeftPart(0.1f).Rounded();
            Rect centered = numWidget.CenteredOnXIn(numWidget);
            Widgets.Label(headerRect, ("RomanceTweaker." + name).Translate());
            TooltipHandler.TipRegion(headerRect, ("RomanceTweaker." + name + ".description").Translate());
            TooltipHandler.TipRegion(sliders, ("RomanceTweaker." + name + ".description").Translate());
            listing.GapLine();
            string buffer = $"{val:F2}";
            Widgets.TextFieldNumeric(centered, ref val, ref buffer, min, max);
            float num = Widgets.HorizontalSlider(sliders, val, min, max, middleAlignment: true);
            {
                
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                    val = num;
                
                if ((val < minlevel))
                {
                    val = minlevel;
                }

            }
            listing.Gap(2f);
        }
        private static void DoSettingsLine(Listing_Standard listing, string name, ref int val, float min, float max, ref int minlevel)
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Rect headerRect = listing.GetRect(20f);
            Rect bottombits = listing.GetRect(16f);

            Rect sliders = bottombits.RightPart(0.7f).Rounded();
            Rect numWidget = bottombits.LeftPart(0.1f).Rounded();
            Rect centered = numWidget.CenteredOnXIn(numWidget);

            Widgets.Label(headerRect, ("RomanceTweaker." + name).Translate());
            TooltipHandler.TipRegion(headerRect, ("RomanceTweaker." + name + ".description").Translate());
            TooltipHandler.TipRegion(sliders, ("RomanceTweaker." + name + ".description").Translate());
            listing.GapLine();
            string buffer = $"{val:F2}";
            Widgets.TextFieldNumeric(centered, ref val, ref buffer, min, max);
            float num = Widgets.HorizontalSlider(sliders, val, min, max, middleAlignment: true);
            {

                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val = (int)num;

                if ((val < minlevel))
                {
                    val = minlevel;
                }

            }
            listing.Gap(2f);
        }

        private static void DoSettingsLine(Listing_Standard listing, string name, ref int val, int min, int max )
        {
            Text.Anchor = TextAnchor.UpperLeft;
            Rect headerRect = listing.GetRect(20f);
            Rect bottombits = listing.GetRect(16f);
            Rect sliders = bottombits.RightPart(0.7f).Rounded();
            Rect numWidget = bottombits.LeftPart(0.1f).Rounded();
            Rect centered = numWidget.CenteredOnXIn(numWidget);
            Widgets.Label(headerRect, ("RomanceTweaker." + name).Translate());
            TooltipHandler.TipRegion(headerRect, ("RomanceTweaker." + name + ".description").Translate());
            TooltipHandler.TipRegion(sliders, ("RomanceTweaker." + name + ".description").Translate());
            listing.GapLine();
            string buffer = $"{val:F2}";
            Widgets.TextFieldNumeric(centered, ref val, ref buffer, min, max);
            float num = Widgets.HorizontalSlider(sliders, val, min, max, middleAlignment: true);
            {
                
                    SoundDefOf.DragSlider.PlayOneShotOnCamera();
                    val = (int)num;
                

            }
            listing.Gap(2f);
        }
        // todo add a second option for two bools
        private static void DoSettingsLineWithBool(Listing_Standard listing, string name, ref int val, int min, int max, string boolname, ref bool option, ref int minlevel)
        {
            Rect headerRect = listing.GetRect(18f);
            Rect bottombits = listing.GetRect(18f);

            Rect leftside = bottombits.RightPart(0.5f).Rounded();
            Rect sliders = leftside.RightPart(0.9f).Rounded();
            Rect rightSide = bottombits.LeftPart(0.5f).Rounded();
            Listing_Standard LS = new Listing_Standard();
            LS.Begin(rightSide);
            LS.CheckboxLabeled(("RomanceTweaker." + boolname).Translate(), ref option, ("RomanceTweaker." + boolname + ".description").Translate());
            LS.End();
            Rect numWidget = leftside.LeftPart(0.1f).Rounded();
            Rect centered = numWidget.CenteredOnXIn(numWidget);
            Widgets.Label(headerRect, ("RomanceTweaker." + name).Translate());
            TooltipHandler.TipRegion(headerRect, ("RomanceTweaker." + name + ".description").Translate());
            TooltipHandler.TipRegion(leftside, ("RomanceTweaker." + name + ".description").Translate());
            listing.GapLine();
            string buffer = $"{val}";
            Widgets.TextFieldNumeric(centered, ref val, ref buffer, min, max);
            float num = Widgets.HorizontalSlider(sliders, val, min, max, middleAlignment: true);
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val = (int)num;
                if ((val < minlevel))
                {
                    val = minlevel;
                }
            }
            listing.Gap(2f);
        }
        private static void DoSettingsLineWithBool(Listing_Standard listing, string boolname, ref bool option)
        {
            Rect bottombits = listing.GetRect(18f);
            Listing_Standard LS = new Listing_Standard();
            LS.Begin(bottombits);
            LS.CheckboxLabeled((boolname).Translate(), ref option, ("RomanceTweaker." + boolname + ".description").Translate());
            LS.End();


            listing.Gap(2f);
        }

      
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard LS = new Listing_Standard();
            LS.Begin(inRect);
            DoSettingsLine(LS, "minLevelToStartRomanceAttempt", ref RomanceMod.settings.minLevelToStartRomanceAttempt, 0, 100);
            LS.CheckboxLabeled(("RomanceTweaker.romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist").Translate(), ref RomanceMod.settings.romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist, ("RomanceTweaker.romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist.description").Translate());
            //DoSettingsLineWithBool(LS,"romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist".Translate(), ref RomanceMod.settings.romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist);
            DoSettingsLine(LS, "romanceSuccessChanceFactorGeneral", ref RomanceMod.settings.romanceSuccessChanceFactorGeneral, 0f, 10f);
            DoSettingsLine(LS, "romanceSuccessChanceFactorSingle", ref RomanceMod.settings.romanceSuccessChanceFactorSingle, 0f, 10f);
            DoSettingsLine(LS, "romanceSuccessChanceFactorCheating", ref RomanceMod.settings.romanceSuccessChanceFactorCheating, 0f, 10f);
            DoSettingsLine(LS, "alwaySucceedRomanceThreshold", ref RomanceMod.settings.alwaySucceedRomanceThreshold, 0, 100, ref RomanceMod.settings.minLevelToStartRomanceAttempt);
            //DoSettingsLineWithBool(LS, "alwaySucceedRomanceThreshold", ref RomanceMod.settings.alwaySucceedRomanceThreshold, 0, 100, "alwaySucceedRomanceThresholdMustBeMutual", ref RomanceMod.settings.alwaySucceedRomanceThresholdMustBeMutual, ref RomanceMod.settings.minLevelToStartRomanceAttempt);
            LS.CheckboxLabeled(("RomanceTweaker.alwaySucceedRomanceThresholdMustBeMutual").Translate(), ref RomanceMod.settings.alwaySucceedRomanceThresholdMustBeMutual, ("RomanceTweaker.alwaySucceedRomanceThresholdMustBeMutual.description").Translate());
            LS.CheckboxLabeled(("RomanceTweaker.alwaySucceedRomanceThresholdCantBeCheating").Translate(), ref RomanceMod.settings.alwayAttemptRomanceThresholdCantBeCheating, ("RomanceTweaker.alwaySucceedRomanceThresholdCantBeCheating.description").Translate());
            //DoSettingsLineWithBool(LS, "alwaysAttemptRomanceThresholdMustBeMutual".Translate(), ref RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual);
            //DoSettingsLineWithBool(LS, "alwayAttemptRomanceThresholdCantBeCheating".Translate(), ref RomanceMod.settings.alwayAttemptRomanceThresholdCantBeCheating);

            
            DoSettingsLine(LS, "romanceAttemptFactorGeneral", ref RomanceMod.settings.romanceAttemptFactorGeneral, 0f, 10f);
            DoSettingsLine(LS, "romanceAttemptFactorSingle", ref RomanceMod.settings.romanceAttemptFactorSingle, 0f, 10f);
            DoSettingsLine(LS, "romanceAttemptFactorCheating", ref RomanceMod.settings.romanceAttemptFactorCheating, 0f, 10f);
            DoSettingsLine(LS, "alwayAttemptRomanceThreshold", ref RomanceMod.settings.alwayAttemptRomanceThreshold, 0, 100, ref RomanceMod.settings.minLevelToStartRomanceAttempt);
            //DoSettingsLineWithBool(LS, "alwayAttemptRomanceThreshold", ref RomanceMod.settings.alwayAttemptRomanceThreshold, 0, 100, "alwaysAttemptRomanceThresholdMustBeMutual", ref RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual, ref RomanceMod.settings.minLevelToStartRomanceAttempt);
            LS.CheckboxLabeled(("RomanceTweaker.alwaysAttemptRomanceThresholdMustBeMutual").Translate(), ref RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual, ("RomanceTweaker.alwaysAttemptRomanceThresholdMustBeMutual.description").Translate());
            LS.CheckboxLabeled(("RomanceTweaker.alwayAttemptRomanceThresholdCantBeCheating").Translate(), ref RomanceMod.settings.alwayAttemptRomanceThresholdCantBeCheating, ("RomanceTweaker.alwayAttemptRomanceThresholdCantBeCheating.description").Translate());

            //DoSettingsLineWithBool(LS, "alwaysAttemptRomanceThresholdMustBeMutual".Translate(), ref RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual);
            //DoSettingsLineWithBool(LS, "alwayAttemptRomanceThresholdCantBeCheating".Translate(), ref RomanceMod.settings.alwayAttemptRomanceThresholdCantBeCheating);


            LS.End();
            base.DoSettingsWindowContents(inRect);
        }


    }



    public class RomanceModSettings : ModSettings
    {
        public bool EnableDebugglogging = false;



        public float romanceAttemptFactorGeneral = 1.2f;

        public float romanceAttemptFactorSingle = 1.5f;

        public float romanceAttemptFactorCheating = 1f;

        public int alwayAttemptRomanceThreshold = 80;
        public bool alwaysAttemptRomanceThresholdMustBeMutual = false;
        public bool alwayAttemptRomanceThresholdCantBeCheating = true;


        public float romanceSuccessChanceFactorGeneral = 1.2f;

        public float romanceSuccessChanceFactorSingle = 1.5f;

        public float romanceSuccessChanceFactorCheating = 1f;

        public int alwaySucceedRomanceThreshold = 80;
        public bool alwaySucceedRomanceThresholdMustBeMutual = false;
        public bool alwaySucceedRomanceThresholdCantBeCheating = true;


        public int minLevelToStartRomanceAttempt = 20;

        public bool romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist = true;


        public float couplesTryingForBabyWillDoLovinForRecreationAgressivenessFactor = 1f;
        public bool couplesTryingForBabyWillDoLovinForRecreation = false;

        [SyncMethod]
        public override void ExposeData()
        {
            
            Scribe_Values.Look(ref alwaySucceedRomanceThreshold, "alwaySucceedRomanceThreshold", 80);
            Scribe_Values.Look(ref romanceSuccessChanceFactorGeneral, "romanceSuccessChanceFactorGeneral", 1.2f);
            Scribe_Values.Look(ref romanceSuccessChanceFactorSingle, "romanceSuccessChanceFactorSingle", 1.5f);
            Scribe_Values.Look(ref romanceSuccessChanceFactorCheating, "romanceSuccessChanceFactorCheating", 1f);

            Scribe_Values.Look(ref alwayAttemptRomanceThreshold, "alwayAttemptRomanceThreshold", 80);
            Scribe_Values.Look(ref romanceAttemptFactorGeneral, "romanceAttemptFactorGeneral", 1.2f);
            Scribe_Values.Look(ref romanceAttemptFactorSingle, "romanceAttemptFactorSingle", 1.5f);
            Scribe_Values.Look(ref romanceAttemptFactorCheating, "romanceAttemptFactorCheating", 1f);


            Scribe_Values.Look(ref minLevelToStartRomanceAttempt, "minLevelToStartRomanceAttempt", 20);

            Scribe_Values.Look(ref alwaysAttemptRomanceThresholdMustBeMutual, "alwaysAttemptRomanceThresholdMustBeMutual", defaultValue: false);
            Scribe_Values.Look(ref alwaySucceedRomanceThresholdMustBeMutual, "alwaySucceedRomanceThresholdMustBeMutual", defaultValue: false);
            Scribe_Values.Look(ref alwaySucceedRomanceThresholdCantBeCheating, "alwaySucceedRomanceThresholdCantBeCheating", defaultValue: true);
            Scribe_Values.Look(ref alwayAttemptRomanceThresholdCantBeCheating, "alwayAttemptRomanceThresholdCantBeCheating", defaultValue: true);
            Scribe_Values.Look(ref romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist, "romanceAttemptsAlwaysSucceedIfPreviousRomanceAttemptsExist", defaultValue: true);
            base.ExposeData();
        }

    }
}
