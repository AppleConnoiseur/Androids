using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// The pawn goes to valid fuel source to refill their energy producing equipment.
    /// </summary>
    public class JobDriver_RefillFuelEnergySource : JobDriver
    {
        public TargetIndex FuelIndex => TargetIndex.B;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (TargetB.IsValid)
            {
                if (!pawn.CanReserve(TargetB))
                {
                    return false;
                }
                else
                {
                    pawn.Reserve(TargetB, job, errorOnFailed: errorOnFailed);
                    return true;
                }
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedNullOrForbidden(FuelIndex);

            yield return Toils_Reserve.Reserve(FuelIndex);
            yield return Toils_Goto.GotoThing(FuelIndex, PathEndMode.OnCell).FailOnSomeonePhysicallyInteracting(FuelIndex);
            yield return Toils_Reserve.Release(FuelIndex);
            yield return Toils_Haul.StartCarryThing(FuelIndex, subtractNumTakenFromJobCount: true);
            yield return Toils_General.Wait(100).WithProgressBarToilDelay(FuelIndex, false);
            Toil refuelToil = new Toil();
            refuelToil.AddFinishAction(delegate ()
            {
                //Use up the carried stack 
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null)
                {
                    Thing targetThing = TargetThingA;

                    EnergySource_Fueled fuelEnergySourceComp = targetThing.TryGetComp<EnergySource_Fueled>();
                    if(fuelEnergySourceComp != null)
                    {
                        fuelEnergySourceComp.LoadFuel(carriedThing);
                    }

                    pawn.carryTracker.DestroyCarriedThing();
                }
            });

            yield return refuelToil;
        }
    }
}
