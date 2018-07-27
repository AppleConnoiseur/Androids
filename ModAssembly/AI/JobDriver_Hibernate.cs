using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// Makes the pawn hibernate at a spot. Doing nothing but keeps their Energy need from draining.
    /// </summary>
    public class JobDriver_Hibernate : JobDriver
    {
        public Thing Target
        {
            get
            {
                return TargetA.Thing;
            }
        }

        public CompPowerTrader powerTrader;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if(pawn.CanReserveAndReach(Target, PathEndMode.OnCell, Danger.Deadly))
            {
                pawn.Reserve(Target, job, errorOnFailed: errorOnFailed);
                return true;
            }

            return false;
        }

        public override RandomSocialMode DesiredSocialMode()
        {
            return RandomSocialMode.Off;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            powerTrader = Target.TryGetComp<CompPowerTrader>();

            this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Reserve.Reserve(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);

            Toil hibernateToil = new Toil();
            hibernateToil.initAction = delegate ()
            {
                pawn.pather.StopDead();
            };
            hibernateToil.tickAction = delegate ()
            {
                //Check if we took damage recently. If that is the case, abort.
                if(pawn.mindState.lastHarmTick - Find.TickManager.TicksGame >= -20)
                {
                    EndJobWith(JobCondition.InterruptOptional);
                }

                //If there is fire nearby, abort.
                if(Find.TickManager.TicksGame % 200 == 0)
                {
                    foreach (IntVec3 vec in pawn.CellsAdjacent8WayAndInside())
                    {
                        if (vec.InBounds(pawn.Map) && vec.GetFirstThing(pawn.Map, RimWorld.ThingDefOf.Fire) is Thing fire)
                        {
                            EndJobWith(JobCondition.InterruptOptional);
                            break;
                        }
                    }
                }

                //If our target lost power its bad, abort.
                if(powerTrader != null && !powerTrader.PowerOn)
                {
                    EndJobWith(JobCondition.InterruptOptional);
                }
            };
            hibernateToil.defaultCompleteMode = ToilCompleteMode.Never;
            yield return hibernateToil;
        }
    }
}
