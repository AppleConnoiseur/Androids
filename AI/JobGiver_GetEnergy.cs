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
    /// Selfishly tries to recharge their Energy need through either a PowerNet or a consumable EnergySource.
    /// </summary>
    public class JobGiver_GetEnergy : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            JobGiver_GetEnergy jobGiver_GetEnergy = (JobGiver_GetEnergy)base.DeepCopy(resolve);
            return jobGiver_GetEnergy;
        }

        public override float GetPriority(Pawn pawn)
        {
            Need_Energy energy = pawn.needs.TryGetNeed<Need_Energy>();
            if (energy == null)
                return 0f;

            if (energy.CurLevelPercentage < Need_Energy.rechargePercentage)
                return 11.5f;

            return 0f;
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            if (pawn.Downed)
                return null;

            Need_Energy energy = pawn.needs.TryGetNeed<Need_Energy>();
            if (energy == null)
                return null;

            if (energy.CurLevelPercentage >= Need_Energy.rechargePercentage)
                return null;

            //See if we got a nearby powernet to tap into.
            Thing closestPowerSource = EnergyNeedUtility.ClosestPowerSource(pawn);

            if(closestPowerSource != null)
            {
                Building building = closestPowerSource as Building;
                if (closestPowerSource != null && building != null && building.PowerComp != null && building.PowerComp.PowerNet.CurrentStoredEnergy() > 50f)
                {
                    //Find a suitable spot to drain from.
                    IntVec3 drainSpot = closestPowerSource.Position;

                    //Give out job to go out and tap it.
                    if (drainSpot.Walkable(pawn.Map) && drainSpot.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(drainSpot)) && pawn.CanReach(drainSpot, PathEndMode.OnCell, Danger.Deadly))
                        return new Job(JobDefOf.ChJAndroidRecharge, closestPowerSource);

                    //Check surrounding cells.
                    foreach(IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(building).OrderByDescending(selector => selector.DistanceTo(pawn.Position)))
                    {
                        if (adjCell.Walkable(pawn.Map) && adjCell.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(adjCell)) && pawn.CanReach(adjCell, PathEndMode.OnCell, Danger.Deadly))
                            return new Job(JobDefOf.ChJAndroidRecharge, closestPowerSource, adjCell);
                    }
                }
            }

            //No power source? Try looking for a consumable resource.

            //In the inventory. (Or being carried)
            if(pawn.carryTracker is Pawn_CarryTracker carryTracker && carryTracker.CarriedThing is Thing carriedThing && carriedThing.TryGetComp<EnergySourceComp>() is EnergySourceComp carriedThingComp && carriedThingComp.EnergyProps.isConsumable)
            {
                if (carriedThing.stackCount > 0)
                {
                    return new Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(carriedThing))
                    {
                        count = carriedThing.stackCount
                    };
                }
            }
            if(pawn.inventory is Pawn_InventoryTracker inventory && inventory.innerContainer.Any(thing => thing.TryGetComp<EnergySourceComp>() is EnergySourceComp comp && comp.EnergyProps.isConsumable))
            {
                Thing validEnergySource =
                        inventory.innerContainer.FirstOrDefault(
                            thing =>
                            thing.TryGetComp<EnergySourceComp>() is EnergySourceComp energySource &&
                            energySource.EnergyProps.isConsumable
                            );
                if (validEnergySource != null)
                {
                    //Use enough to get satisfied.
                    EnergySourceComp energySourceComp = validEnergySource.TryGetComp<EnergySourceComp>();

                    int thingCount = (int)Math.Ceiling((energy.MaxLevel - energy.CurLevel) / energySourceComp.EnergyProps.energyWhenConsumed);
                    thingCount = Math.Min(thingCount, validEnergySource.stackCount);

                    if (thingCount > 0)
                    {
                        return new Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(validEnergySource))
                        {
                            count = thingCount
                        };
                    }
                }
            }

            //On the map.
            Thing closestConsumablePowerSource = 
                GenClosest.ClosestThingReachable(
                    pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f,
                    thing => thing.TryGetComp<EnergySourceComp>() != null && !thing.IsForbidden(pawn) && pawn.CanReserve(new LocalTargetInfo(thing)) && thing.Position.InAllowedArea(pawn) && pawn.CanReach(new LocalTargetInfo(thing), PathEndMode.OnCell, Danger.Deadly));
            if(closestConsumablePowerSource != null)
            {
                EnergySourceComp energySourceComp = closestConsumablePowerSource.TryGetComp<EnergySourceComp>();
                if(energySourceComp != null)
                {
                    int thingCount = (int)Math.Ceiling((energy.MaxLevel - energy.CurLevel) / energySourceComp.EnergyProps.energyWhenConsumed);
                    if (thingCount > 0)
                    {
                        return new Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(closestConsumablePowerSource))
                        {
                            count = thingCount
                        };
                    }
                }
            }

            return null;
        }
    }
}
