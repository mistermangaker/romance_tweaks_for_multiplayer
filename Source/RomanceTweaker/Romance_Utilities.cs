using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RomanceTweaker
{
    [DefOf]
    public static class InternalDefof
    {
        public static ThoughtDef MMM_FlirtingAttempt;
        public static ThoughtDef MMM_FlirtingAttemptOnMe;
        public static ThoughtDef MMM_FlirtyWords_Mood;
        public static RulePackDef MMM_FlirtyWords_Lovers;
        public static RulePackDef MMM_FlirtyWords_AttemptSucceeded;
        public static RulePackDef MMM_FlirtyWords_AttemptSucceeded_NowLovers;
        public static RulePackDef MMM_FlirtyWords_AttemptFailed;


        //public static JobDef MMM_DoLovinForIdle;

        static InternalDefof()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefof));
        }
    }

    public static class Romance_Utilities
    {
        public static bool ResolveRomanceAttemptSettings(Pawn initiator, Pawn recipient)
        {
            bool mustBeMutual = RomanceMod.settings.alwaysAttemptRomanceThresholdMustBeMutual;
            bool recipientOverThreshold = (recipient.relations.OpinionOf(initiator) >= RomanceMod.settings.alwayAttemptRomanceThreshold);
            bool initiatorOverThreshold = (initiator.relations.OpinionOf(recipient) >= RomanceMod.settings.alwayAttemptRomanceThreshold);
            bool CantBeCheating = RomanceMod.settings.alwayAttemptRomanceThresholdCantBeCheating;
            if (recipientOverThreshold && initiatorOverThreshold)
            {
                return CantBeCheating ? Romance_Utilities.IsThisRomanceAttemptCheating(initiator, recipient) : true;
            }
            if (!mustBeMutual && recipientOverThreshold)
            {
                return CantBeCheating ? Romance_Utilities.IsThisRomanceAttemptCheating(initiator, recipient) : true;
            }
            return false;

        }
        public static bool ResolveRomanceSettings(Pawn initiator, Pawn recipient, out float endresult)
        {
            if(ResolveRomanceSettings(initiator, recipient))
            {
                endresult = 1f;
                return true;
            }
            endresult = 0f;
            return false;

        }
        public static bool ResolveRomanceSettings(Pawn initiator, Pawn recipient)
        {
            bool mustBeMutual = RomanceMod.settings.alwaySucceedRomanceThresholdMustBeMutual;
            bool CantBeCheating = RomanceMod.settings.alwaySucceedRomanceThresholdCantBeCheating;
            bool recipientOverThreshold = (recipient.relations.OpinionOf(initiator) >= RomanceMod.settings.alwaySucceedRomanceThreshold);
            bool initiatorOverThreshold = (initiator.relations.OpinionOf(recipient) >= RomanceMod.settings.alwaySucceedRomanceThreshold);
            if (recipientOverThreshold && initiatorOverThreshold)
            {
                return CantBeCheating ? Romance_Utilities.IsThisRomanceAttemptCheating(initiator, recipient) : true;
            }
            if (!mustBeMutual && recipientOverThreshold)
            {
                return CantBeCheating ? Romance_Utilities.IsThisRomanceAttemptCheating(initiator, recipient) : true;
            }
            return false;
        }

        public static bool IsThisRomanceAttemptCheating(Pawn initiator, Pawn recipient)
        {
            if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                return false;
            }
            if (!LovePartnerRelationUtility.HasAnyLovePartner(initiator, false) && !LovePartnerRelationUtility.HasAnyLovePartner(recipient, false))
            {
                return false;
            }
            return true;
            
        }
    }
}
