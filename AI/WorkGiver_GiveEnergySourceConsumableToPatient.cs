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
    /// This workgiver will try to make Doctors give power to patients through consumable energy sources.
    /// </summary>
    public class WorkGiver_GiveEnergySourceConsumableToPatient : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if(pawn.Downed)
                return false;

            if (thing.IsForbidden(pawn) || !thing.Position.InAllowedArea(pawn))

            if (!pawn.CanReach(new LocalTargetInfo(thing), PathEndMode.ClosestTouch, Danger.Deadly))
                return false;

            if (HealthAIUtility.ShouldSeekMedicalRest(pawn))
                return false;

            Pawn targetPawn = thing as Pawn;

            if (targetPawn == null)
                return false;

            if (!pawn.CanReserve(new LocalTargetInfo(targetPawn)))
                return false;

            if (!targetPawn?.Faction?.IsPlayer ?? true)
                return false;

            if (/*!targetPawn.InBed() || */!targetPawn.Downed)
                return false;

            if (!HealthAIUtility.ShouldSeekMedicalRest(targetPawn))
                return false;

            Need_Energy needEnergy = targetPawn.needs.TryGetNeed<Need_Energy>();
            if (needEnergy == null)
                return false;

            if (!forced && needEnergy.CurLevelPercentage > 0.5f)
                return false;

            Thing closestEnergySource = TryFindBestEnergySource(pawn);

            if (closestEnergySource == null)
                return false;

            if (closestEnergySource.Spawned && !pawn.CanReserve(new LocalTargetInfo(closestEnergySource)))
                return false;

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn targetPawn = thing as Pawn;

            //Get closest consumable source we can find.
            Thing closestEnergySource = TryFindBestEnergySource(pawn);

            if(closestEnergySource != null)
            {
                Need_Energy needEnergy = targetPawn.needs.TryGetNeed<Need_Energy>();

                EnergySourceComp energySourceComp = closestEnergySource.TryGetComp<EnergySourceComp>();

                //Consume for pawn.
                int thingCount = (int)Math.Ceiling((needEnergy.MaxLevel - needEnergy.CurLevel) / energySourceComp.EnergyProps.energyWhenConsumed);
                thingCount = Math.Min(thingCount, closestEnergySource.stackCount);

                if (thingCount > 0)
                    return new Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(closestEnergySource), new LocalTargetInfo(targetPawn))
                    {
                        count = thingCount
                    };
            }

            return null;
        }

        /// <summary>
        /// Tries to get a suitable EnergySource to use.
        /// </summary>
        /// <param name="pawn">Pawn to look for.</param>
        /// <returns>Thing if found any, null if not.</returns>
        public Thing TryFindBestEnergySource(Pawn pawn)
        {
            //In inventory. (Or carried)
            if (pawn.carryTracker is Pawn_CarryTracker carryTracker && carryTracker.CarriedThing is Thing carriedThing && carriedThing.TryGetComp<EnergySourceComp>() is EnergySourceComp carriedThingComp && carriedThingComp.EnergyProps.isConsumable)
            {
                if (carriedThing.stackCount > 0)
                {
                    return carryTracker.CarriedThing;
                }
            }
            if (pawn.inventory is Pawn_InventoryTracker inventory)
            {
                Thing validInternalEnergySource =
                        inventory.innerContainer.FirstOrDefault(
                            thing =>
                            thing.TryGetComp<EnergySourceComp>() is EnergySourceComp energySource &&
                            energySource.EnergyProps.isConsumable
                            );
                if (validInternalEnergySource != null)
                {
                    return validInternalEnergySource;
                }
            }

            //On map.
            Thing closestEnergySource = GenClosest.ClosestThingReachable(
            pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f,
            searchThing => searchThing.TryGetComp<EnergySourceComp>() != null && !searchThing.IsForbidden(pawn) && pawn.CanReserve(searchThing) && searchThing.Position.InAllowedArea(pawn) && pawn.CanReach(new LocalTargetInfo(searchThing), PathEndMode.OnCell, Danger.Deadly));

            return closestEnergySource;
        }
    }
}
