using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RomanceTweaker
{
   
    [StaticConstructorOnStartup]
    public class HarmonyPatches
    {
     
	
        static HarmonyPatches()
        {
            new Harmony("RomanceTweaker").PatchAll(Assembly.GetExecutingAssembly());
        }
        //[HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
        [HarmonyPatch]
        public class RomanticInteractionPatch
        {
            [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
            [HarmonyPrefix]
            public static bool RandomSelectionPrefix(Pawn initiator, Pawn recipient, ref float __result)
            {
                
                if (recipient.relations.OpinionOf(initiator) < RomanceMod.settings.minLevelToStartRomanceAttempt || !RelationsUtility.AttractedToGender(recipient, initiator.gender) || !RelationsUtility.AttractedToGender(initiator, recipient.gender)) 
                {
                   
                    __result = 0f;
                    return false;
                }
                if (!RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual)
                {
                    
                    if (recipient.relations.OpinionOf(initiator) < RomanceMod.settings.alwayAttemptRomanceThreshold || initiator.relations.OpinionOf(recipient) < RomanceMod.settings.alwayAttemptRomanceThreshold)
                    {
                      
                        __result = 10f;
                        return false;
                    }
                }
                else
                {
                   
                    if (recipient.relations.OpinionOf(initiator) < RomanceMod.settings.alwayAttemptRomanceThreshold && initiator.relations.OpinionOf(recipient) < RomanceMod.settings.alwayAttemptRomanceThreshold)
                    {
                        
                        __result = 10f;
                    return false;
                    }
                }
          
                return true;
            }

            [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "RandomSelectionWeight")]
           [HarmonyPostfix]
            public static float RandomSelectionWeight_patch(float __result, Pawn initiator, Pawn recipient)
            {

                if (__result == 0f)
                {

                    return __result;
                }
                
                float romancechancefactor = RomanceMod.settings.romanceAttemptFactorGeneral;
                if (!LovePartnerRelationUtility.HasAnyLovePartner(initiator,false)&&!LovePartnerRelationUtility.HasAnyLovePartner(recipient, false))
                {

                    romancechancefactor *= RomanceMod.settings.romanceAttemptFactorSingle;
                }
                if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
                {

                    romancechancefactor *= RomanceMod.settings.romanceAttemptFactorCheating;
                }

                return __result * romancechancefactor;

            }

           [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
           [HarmonyPostfix]
            public static float SuccessChance_patch(float __result, Pawn initiator, Pawn recipient)
            {

                float successchancefactor = RomanceMod.settings.romanceSuccessChanceFactorGeneral;
                if (!LovePartnerRelationUtility.HasAnyLovePartner(initiator, false) && !LovePartnerRelationUtility.HasAnyLovePartner(recipient, false))
                {

                    successchancefactor *= RomanceMod.settings.romanceSuccessChanceFactorSingle;
                }
                if (!LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
                {

                    successchancefactor *= RomanceMod.settings.romanceSuccessChanceFactorCheating;

                }
                //Log.Message($"[RomTw] Romance Success {initiator.Name.ToStringShort} -> {recipient.Name.ToStringShort} : {__result} -> {__result * successchancefactor}");
                
                return __result * successchancefactor;
            }

         [HarmonyPatch(typeof(InteractionWorker_RomanceAttempt), "SuccessChance")]
           [HarmonyPrefix]
            public static bool SuccessChance_Prefix(Pawn initiator, Pawn recipient, ref float __result)
            {

                if (!RomanceMod.settings.alwaySucceedRomanceThresholdMustBeMutual)
                {

                    if (recipient.relations.OpinionOf(initiator) > RomanceMod.settings.alwaySucceedRomanceThreshold)
                    {
                        //Log.Message("guarrenteed success");
                        __result = 1f;
                        return false;
                    }
                }
                else
                {

                    if (recipient.relations.OpinionOf(initiator) > RomanceMod.settings.alwaySucceedRomanceThreshold && initiator.relations.OpinionOf(recipient) > RomanceMod.settings.alwaySucceedRomanceThreshold)
                    {
                        //Log.Message("guarrenteed success");
                        __result = 1f;
                        return false;
                    }
                }

                return true;

            }

        }

        


    }




}
