using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// Will attempt giving a fuel refilling Job if a fuel source need refilling.
    /// </summary>
    public class JobGiver_RefillFuelEnergySource : ThinkNode_JobGiver
    {
        public JobDef refillJob;

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_RefillFuelEnergySource jobGiver_RefillEnergySource = (JobGiver_RefillFuelEnergySource)base.DeepCopy(resolve);
            jobGiver_RefillEnergySource.refillJob = refillJob;
            return jobGiver_RefillEnergySource;
        }

        public override float GetPriority(Pawn pawn)
        {
            if (FuelUtility.FueledEnergySourceNeedRefilling(pawn) != null)
                return 10f;

            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed)
                return null;

            if (pawn.InBed())
                return null;

            Thing fueledEnergySource = FuelUtility.FueledEnergySourceNeedRefilling(pawn);

            //Nothing to refill.
            if (fueledEnergySource == null)
                return null;

            //Is there anything to refill it witH?
            EnergySource_Fueled fuelEnergySourceComp = fueledEnergySource.TryGetComp<EnergySource_Fueled>();
            if(!fuelEnergySourceComp.autoRefuel)
                return null;

            Thing closestConsumablePowerSource = FuelUtility.FindSuitableFuelForPawn(pawn, fuelEnergySourceComp);

            //No compatible fuel source found.
            if (closestConsumablePowerSource == null)
                return null;

            //Refill the fueled energy source with this.
            Job refuelJob = new Job(refillJob, fueledEnergySource, closestConsumablePowerSource);
            int refuelCount = fuelEnergySourceComp.CalculateFuelNeededToRefill(closestConsumablePowerSource);
            refuelJob.count = refuelCount;
            return refuelJob;
        }
    }
}
