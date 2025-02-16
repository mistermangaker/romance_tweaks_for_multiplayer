using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Jobs;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using static UnityEngine.GraphicsBuffer;

namespace RomanceTweaker
{
  

    public static class RomanceTweakerLovinUtility
    {
        public static bool SuitableBedForLoving(Building_Bed bed, Pawn pawn1, Pawn pawn2)
        {
            if (bed == null)
            {
                return false;
            }
            if (bed.SleepingSlotsCount < 2) 
            {
                return false;
            }
            if (bed.AnyOccupants)
            {
                return false;
            }
            if (!RestUtility.CanUseBedEver(pawn2, bed.def)|| !RestUtility.CanUseBedEver(pawn1, bed.def))
            {
                return false;
            }
            if (!BothPawnsCanReachBed(bed, pawn1, pawn2))
            {
                return false;
            }
            return true;
            
        }
        public static Building_Bed FindEitherLoversBed(Pawn initator, Pawn recipient)
        {
            Log.Message("starting to find beds");
            Building_Bed initatorBed = initator.ownership.OwnedBed;
            Building_Bed recipientBed = recipient.ownership.OwnedBed;
            if (initatorBed == recipientBed && (BothPawnsCanReachBed(initatorBed, initator, recipient)))
            {
                Log.Message("both beds are the same");
                return initatorBed;
            }
            if (SuitableBedForLoving(initatorBed, initator, recipient))
            {
                Log.Message("picked initators bed");
                return initatorBed;
            }
            if (SuitableBedForLoving(recipientBed, initator, recipient))
            {
                Log.Message("picked recipient bed");
                return recipientBed;
            }
            return null;


        }
            private static bool BothPawnsCanReachBed(Building_Bed bed, Pawn initator, Pawn recipient)
        {
            if (!initator.Map.reachability.CanReach(initator.Position, bed.Position, PathEndMode.OnCell, TraverseParms.For(initator)))
            {
                return false;
            }
            if (!recipient.Map.reachability.CanReach(recipient.Position, bed.Position, PathEndMode.OnCell, TraverseParms.For(recipient)))
            {
                return false;
            }
            if(bed.IsForbidden(initator) || bed.IsForbidden(recipient))
            {
                return false;
            }
            return true;
        }
    }
    //public class JoyGiver_DoLovin_Recreation : JoyGiver
    //{

   // }

    public class DoLovin_idle : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.Message("starting to give job");
            if (!LovePartnerRelationUtility.HasAnyLovePartner(pawn, false))
            {
                Log.Message("no lover: "+ pawn);
                return null;
            }
            
           
           
            Pawn loverPawn = LovePartnerRelationUtility.ExistingLovePartner(pawn, false);
            Log.Message("lover pawn is "+ loverPawn.Label);
            if (!pawn.CanReserve(loverPawn) || !loverPawn.CanReserve(pawn))
            {
                Log.Message("cant reserve eachother");
                return null;
            }
            if (PawnUtility.WillSoonHaveBasicNeed(pawn) || PawnUtility.WillSoonHaveBasicNeed(loverPawn))
            {
                Log.Message("will soon have basic need");
                return null;
            }
            Building_Bed bed = RomanceTweakerLovinUtility.FindEitherLoversBed(pawn, loverPawn);
            Log.Message("bed:"+ bed.def);
            if (bed == null)
            {
                Log.Message("nobed");
                return null;
            }
            //Job job = JobMaker.MakeJob(InternalDefof.MMM_DoLovinForIdle, loverPawn, bed);
            //job.count = 1;
            //return job;
            //return JobMaker.MakeJob(InternalDefof.MMM_DoLovinForIdle, bed, loverPawn);
            return null;

        }
    }

    public class JobDriver_DoLovin_generic : JobDriver
    {

        private int ticksLeft;
        private const int TicksBetweenHeartMotes = 100;

        private readonly TargetIndex PartnerInd = TargetIndex.A;

        private readonly TargetIndex BedInd = TargetIndex.B;

        private readonly TargetIndex SlotInd = TargetIndex.C;


        

        private static float PregnancyChance = 0.05f;


        private Building_Bed Bed => (Building_Bed)(Thing)job.GetTarget(BedInd);

        private Pawn Partner => (Pawn)(Thing)job.GetTarget(PartnerInd);

        private Pawn Actor => GetActor();

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            Log.Message(PartnerInd);
            Log.Message(BedInd);
            Log.Message(SlotInd);
            return pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed) && pawn.Reserve(Bed, job, Bed.SleepingSlotsCount, 0, null, errorOnFailed);
        }


        public override bool CanBeginNowWhileLyingDown()
        {
            return JobInBedUtility.InBedOrRestSpotNow(pawn, job.GetTarget(BedInd));
        }

        public static Toil GoToPartner(Pawn pawn, Pawn target)
        {
            Toil toil = ToilMaker.MakeToil("GoGetAffection");

            toil.initAction = delegate
            {
                pawn.pather.StartPath(target, PathEndMode.Touch);
            };
            toil.AddFailCondition(delegate
            {
                if (target.DestroyedOrNull())
                {
                    return true;
                }
                return false;
            });

            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }



        public static Toil TakeToBed(Pawn pawn, Thing target)
        {
            Toil toil = ToilMaker.MakeToil("GoGetAffection");
            Log.Message("going to bed");

            toil.initAction = delegate
            {
                Log.Message("starting path");
                pawn.pather.StartPath(target, PathEndMode.Touch);
            };
            toil.AddFailCondition(delegate
            {
                if (target.DestroyedOrNull())
                {
                    return true;
                }
                return false;
            });
            Log.Message("going to path path");
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.PatherArrival;
            return toil;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            // first go to the partner and interact with them
            // do a romance attempt?
            Log.Message(Partner.Label);
            Log.Message(BedInd);
            this.FailOnDespawnedOrNull(BedInd);
            this.FailOnDespawnedOrNull(PartnerInd);
            this.FailOn(() => !Partner.health.capacities.CanBeAwake);
            Log.Message("starting");
            Toil goToTakee = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnDespawnedNullOrForbidden(TargetIndex.B);
            yield return goToTakee;


        }

    }


}
