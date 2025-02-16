using Multiplayer.API;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static System.Net.Mime.MediaTypeNames;

namespace RomanceTweaker
{
    public class InteractionWorker_FlirtyWords : InteractionWorker
    {

        private const float MinRomanceChanceForRomanceAttempt = 0.15f;

        public const int MinOpinionForRomanceAttempt = 5;

        private const float BaseSelectionWeight = 1.15f;

        private const float BaseSuccessChance = 0.6f;

        public const float TryRomanceSuccessChance = 1f;

        private const int TryRomanceCooldownTicks = 900000;


        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            if (TutorSystem.TutorialMode)
            {
                return 0f;
            }
            if (initiator.DevelopmentalStage.Juvenile() || recipient.DevelopmentalStage.Juvenile())
            {
                return 0f;
            }
            if (initiator.Inhumanized())
            {
                return 0f;
            }
            if (recipient.relations.OpinionOf(initiator) < RomanceMod.settings.minLevelToStartRomanceAttempt || !RelationsUtility.AttractedToGender(recipient, initiator.gender) || !RelationsUtility.AttractedToGender(initiator, recipient.gender))
            {

                return 0f;
            }
            float num = initiator.relations.SecondaryRomanceChanceFactor(recipient);
            if (num < 0.15f)
            {
                return 0f;
            }
            return Romance_Utilities.ResolveRomanceAttemptSettings(initiator, recipient) ? 10f : 0f;
    
        }


        [SyncMethod]
        public static bool RomanceAttemptSuccess(Pawn initiator, Pawn recipient, float baseChance, out float endResult)
        {
            Log.Message("STARTING CALCULATION");
            
            endResult = baseChance;
            string text = "step1: "+ endResult.ToString();
            // resolves the game settings for instant successes
            Log.Message(text);
            if (Romance_Utilities.ResolveRomanceSettings(initiator, recipient))
            {
                return Romance_Utilities.ResolveRomanceSettings(initiator, recipient, out endResult);
            }

            // if a pychic bond is able to be made then the romance will succeed anyway
            if (CanCreatePsychicBondBetween(initiator, recipient))
            {
                if (initiator.IsQuestHelper() || recipient.IsQuestHelper())
                {
                    endResult = 0f;
                    return false;
                }
                endResult = 1f;
                return true;
            }

            float successchancefactor = RomanceMod.settings.romanceSuccessChanceFactorGeneral;
            text += "Generalsuccesschancefactor: " + successchancefactor;

            //successchancefactor *= OpinionFactor(initiator, recipient);
            if (!Romance_Utilities.IsThisRomanceAttemptCheating(initiator, recipient))
            {

                successchancefactor *= RomanceMod.settings.romanceSuccessChanceFactorSingle;
                text += "Singlesuccesschancefactor: " + successchancefactor;
            }
            else
            {

                successchancefactor *= RomanceMod.settings.romanceSuccessChanceFactorCheating;
                text += "Cheatingsuccesschancefactor: " + successchancefactor;
                successchancefactor *= PartnerFactor(initiator, recipient);
                text += "successchancefactor: " + successchancefactor;

            }

            endResult  *=  (recipient.relations.SecondaryRomanceChanceFactor(initiator) * OpinionFactor(initiator, recipient) * PartnerFactor(initiator, recipient) * successchancefactor);
            endResult = Mathf.Clamp01(endResult);
            

            Log.Message(text);
            
            return (Rand.Value < endResult);

        }



        private static float PartnerFactor(Pawn initiator, Pawn recipient)
        {
            float num = 1f;
            if (!new HistoryEvent(recipient.GetHistoryEventForLoveRelationCountPlusOne(), recipient.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
            {
                Pawn pawn = null;
                if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Lover);
                    num = 0.6f;
                }
                else if (recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance, (Pawn x) => !x.Dead) != null)
                {
                    pawn = recipient.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Fiance);
                    num = 0.1f;
                }
                else if (recipient.GetSpouseCount(includeDead: false) > 0)
                {
                    pawn = recipient.GetMostLikedSpouseRelation().otherPawn;
                    num = 0.3f;
                }
                if (pawn != null)
                {
                    num *= Mathf.InverseLerp(100f, 0f, recipient.relations.OpinionOf(pawn));
                    num *= Mathf.Clamp01(1f - recipient.relations.SecondaryRomanceChanceFactor(pawn));
                }
            }
            return num;
        }

        private static float OpinionFactor(Pawn initiator, Pawn recipient)
        {
            return Mathf.InverseLerp(5f, 100f, recipient.relations.OpinionOf(initiator));
        }

        [SyncMethod]
        public static bool GenerateFlirtThought(Pawn initiator, Pawn recipient)
        {
            Log.Message("GenerateFlirtThought: " + Mathf.InverseLerp(-10f, 100f, recipient.relations.OpinionOf(initiator)));
            return (Rand.Value < Mathf.InverseLerp(-10f, 100f, recipient.relations.OpinionOf(initiator)));
        }



        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            if (RomanceMod.settings.EnableDebugglogging)
            {
                Log.Message("STARTING INTERACTION");
            }
            if (LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                if (initiator.needs.mood != null)
                {
                    initiator.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtingAttempt, recipient);
                    initiator.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtyWords_Mood, recipient);
                }
                if (recipient.needs.mood != null)
                {
                    recipient.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtingAttemptOnMe, initiator);
                    recipient.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtyWords_Mood, initiator);
                }
                extraSentencePacks.Add(InternalDefof.MMM_FlirtyWords_Lovers);
                letterText = null;
                letterLabel = null;
                letterDef = null;
                lookTargets = null;
            }

            float endResult = 0f;
            bool success = RomanceAttemptSuccess(initiator, recipient, 1f,out  endResult);
            if (RomanceMod.settings.EnableDebugglogging)
            {
                Log.Message(endResult);
            }
            if (success && !LovePartnerRelationUtility.LovePartnerRelationExists(initiator, recipient))
            {
                BreakLoverAndFianceRelations(initiator, out var oldLoversAndFiances);
                BreakLoverAndFianceRelations(recipient, out var oldLoversAndFiances2);
                RemoveBrokeUpAndFailedRomanceThoughts(initiator, recipient);
                RemoveBrokeUpAndFailedRomanceThoughts(recipient, initiator);
                for (int i = 0; i < oldLoversAndFiances.Count; i++)
                {
                    TryAddCheaterThought(oldLoversAndFiances[i], initiator);
                }
                for (int j = 0; j < oldLoversAndFiances2.Count; j++)
                {
                    TryAddCheaterThought(oldLoversAndFiances2[j], recipient);
                }
                initiator.relations.TryRemoveDirectRelation(PawnRelationDefOf.ExLover, recipient);
                initiator.relations.AddDirectRelation(PawnRelationDefOf.Lover, recipient);
                TaleRecorder.RecordTale(TaleDefOf.BecameLover, initiator, recipient);
                bool createdBond = false;
                if (CanCreatePsychicBondBetween(initiator, recipient))
                {
                    createdBond = TryCreatePsychicBondBetween(initiator, recipient);
                }
                if (PawnUtility.ShouldSendNotificationAbout(initiator) || PawnUtility.ShouldSendNotificationAbout(recipient))
                {
                    GetNewLoversLetter(initiator, recipient, oldLoversAndFiances, oldLoversAndFiances2, createdBond, out letterText, out letterLabel, out letterDef, out lookTargets);
                }
                else
                {
                    letterText = null;
                    letterLabel = null;
                    letterDef = null;
                    lookTargets = null;
                }
                extraSentencePacks.Add(InternalDefof.MMM_FlirtyWords_AttemptSucceeded_NowLovers);
                LovePartnerRelationUtility.TryToShareBed(initiator, recipient);

            }
            else
            {
               bool flirt = GenerateFlirtThought(initiator, recipient);
                if (flirt)
                {
                    if (initiator.needs.mood != null)
                    {
                        initiator.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtingAttempt, recipient);
                    }
                    if (recipient.needs.mood != null)
                    {
                        recipient.needs.mood.thoughts.memories.TryGainMemory(InternalDefof.MMM_FlirtingAttemptOnMe, initiator);
                    }
                    extraSentencePacks.Add(InternalDefof.MMM_FlirtyWords_AttemptSucceeded);
                    letterText = null;
                    letterLabel = null;
                    letterDef = null;
                    lookTargets = null;
                    
                }
                else
                {
                    if (initiator.needs.mood != null)
                    {
                        initiator.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RebuffedMyRomanceAttempt, recipient);
                    }
                    if (recipient.needs.mood != null)
                    {
                        recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMe, initiator);
                    }
                    if (recipient.needs.mood != null && recipient.relations.OpinionOf(initiator) <= 0)
                    {
                        recipient.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, initiator);
                    }
                    extraSentencePacks.Add(InternalDefof.MMM_FlirtyWords_AttemptFailed);
                    letterText = null;
                    letterLabel = null;
                    letterDef = null;
                    lookTargets = null;
                }

            }

        }


        public static bool CanCreatePsychicBondBetween(Pawn initiator, Pawn recipient)
        {
            if (!ModsConfig.BiotechActive)
            {
                return false;
            }
            Gene_PsychicBonding gene_PsychicBonding = initiator.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
            Gene_PsychicBonding gene_PsychicBonding2 = recipient.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
            if (gene_PsychicBonding == null && gene_PsychicBonding2 == null)
            {
                return false;
            }
            if (gene_PsychicBonding == null || gene_PsychicBonding.CanBondToNewPawn)
            {
                return gene_PsychicBonding2?.CanBondToNewPawn ?? true;
            }
            return false;
        }

        public static bool TryCreatePsychicBondBetween(Pawn initiator, Pawn recipient)
        {
            Gene_PsychicBonding gene_PsychicBonding = initiator.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
            Gene_PsychicBonding gene_PsychicBonding2 = recipient.genes?.GetFirstGeneOfType<Gene_PsychicBonding>();
            if (gene_PsychicBonding != null && gene_PsychicBonding2 != null && (!gene_PsychicBonding.CanBondToNewPawn || !gene_PsychicBonding2.CanBondToNewPawn))
            {
                return false;
            }
            if (gene_PsychicBonding != null && gene_PsychicBonding.CanBondToNewPawn)
            {
                gene_PsychicBonding.BondTo(recipient);
                return true;
            }
            if (gene_PsychicBonding2 != null && gene_PsychicBonding2.CanBondToNewPawn)
            {
                gene_PsychicBonding2.BondTo(initiator);
                return true;
            }
            return false;
        }

        private void RemoveBrokeUpAndFailedRomanceThoughts(Pawn pawn, Pawn otherPawn)
        {
            if (pawn.needs.mood != null)
            {
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.BrokeUpWithMe, otherPawn);
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMe, otherPawn);
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDefWhereOtherPawnIs(ThoughtDefOf.FailedRomanceAttemptOnMeLowOpinionMood, otherPawn);
            }
        }

        private void BreakLoverAndFianceRelations(Pawn pawn, out List<Pawn> oldLoversAndFiances)
        {
            oldLoversAndFiances = new List<Pawn>();
            int num = 200;
            while (num > 0 && !new HistoryEvent(pawn.GetHistoryEventForLoveRelationCountPlusOne(), pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
            {
                Pawn pawn2 = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn, (DirectPawnRelation r) => r.def == PawnRelationDefOf.Lover);
                if (pawn2 != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Lover, pawn2);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn2);
                    oldLoversAndFiances.Add(pawn2);
                    num--;
                    continue;
                }
                Pawn pawn3 = LovePartnerRelationUtility.ExistingLeastLikedPawnWithRelation(pawn, (DirectPawnRelation r) => r.def == PawnRelationDefOf.Fiance);
                if (pawn3 != null)
                {
                    pawn.relations.RemoveDirectRelation(PawnRelationDefOf.Fiance, pawn3);
                    pawn.relations.AddDirectRelation(PawnRelationDefOf.ExLover, pawn3);
                    oldLoversAndFiances.Add(pawn3);
                    num--;
                    continue;
                }
                break;
            }
        }

        private void TryAddCheaterThought(Pawn pawn, Pawn cheater)
        {
            if (!pawn.Dead && pawn.needs.mood != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.CheatedOnMe, cheater);
            }
        }

        private void GetNewLoversLetter(Pawn initiator, Pawn recipient, List<Pawn> initiatorOldLoversAndFiances, List<Pawn> recipientOldLoversAndFiances, bool createdBond, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            bool flag = false;
            HistoryEvent ev = new HistoryEvent(initiator.GetHistoryEventLoveRelationCount(), initiator.Named(HistoryEventArgsNames.Doer));
            HistoryEvent ev2 = new HistoryEvent(recipient.GetHistoryEventLoveRelationCount(), recipient.Named(HistoryEventArgsNames.Doer));
            if (!ev.DoerWillingToDo() || !ev2.DoerWillingToDo())
            {
                letterLabel = "LetterLabelAffair".Translate();
                letterDef = LetterDefOf.NegativeEvent;
                flag = true;
            }
            else
            {
                letterLabel = "LetterLabelNewLovers".Translate();
                letterDef = LetterDefOf.PositiveEvent;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (BedUtility.WillingToShareBed(initiator, recipient))
            {
                stringBuilder.AppendLineTagged("LetterNewLovers".Translate(initiator.Named("PAWN1"), recipient.Named("PAWN2")));
            }
            if (flag)
            {
                Pawn firstSpouse = initiator.GetFirstSpouse();
                if (firstSpouse != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLineTagged("LetterAffair".Translate(initiator.LabelShort, firstSpouse.LabelShort, recipient.LabelShort, initiator.Named("PAWN1"), recipient.Named("PAWN2"), firstSpouse.Named("SPOUSE")));
                }
                Pawn firstSpouse2 = recipient.GetFirstSpouse();
                if (firstSpouse2 != null)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLineTagged("LetterAffair".Translate(recipient.LabelShort, firstSpouse2.LabelShort, initiator.LabelShort, recipient.Named("PAWN1"), firstSpouse2.Named("SPOUSE"), initiator.Named("PAWN2")));
                }
            }
            for (int i = 0; i < initiatorOldLoversAndFiances.Count; i++)
            {
                if (!initiatorOldLoversAndFiances[i].Dead)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLineTagged("LetterNoLongerLovers".Translate(initiator.LabelShort, initiatorOldLoversAndFiances[i].LabelShort, initiator.Named("PAWN1"), initiatorOldLoversAndFiances[i].Named("PAWN2")));
                }
            }
            for (int j = 0; j < recipientOldLoversAndFiances.Count; j++)
            {
                if (!recipientOldLoversAndFiances[j].Dead)
                {
                    stringBuilder.AppendLine();
                    stringBuilder.AppendLineTagged("LetterNoLongerLovers".Translate(recipient.LabelShort, recipientOldLoversAndFiances[j].LabelShort, recipient.Named("PAWN1"), recipientOldLoversAndFiances[j].Named("PAWN2")));
                }
            }
            if (createdBond)
            {
                Pawn pawn = ((initiator.genes.GetFirstGeneOfType<Gene_PsychicBonding>() != null) ? initiator : recipient);
                Pawn arg = ((pawn == initiator) ? recipient : initiator);
                stringBuilder.AppendLine();
                stringBuilder.AppendLineTagged("LetterPsychicBondCreated".Translate(pawn.Named("BONDPAWN"), arg.Named("OTHERPAWN")));
            }
            letterText = stringBuilder.ToString().TrimEndNewlines();
            lookTargets = new LookTargets(initiator, recipient);
        }
    }
}
