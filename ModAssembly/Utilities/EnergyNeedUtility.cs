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
    /// Helps dealing with the energy need.
    /// </summary>
    public static class EnergyNeedUtility
    {
        public static Thing ClosestPowerSource(Pawn pawn)
        {
            Thing closestPowerSource =
                GenClosest.ClosestThingReachable(
                    pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f,
                    thing => EnergyNeedUtility.BestClosestPowerSource(pawn, thing));

            return closestPowerSource;
        }

        public static bool BestClosestPowerSource(Pawn pawn, Thing thing)
        {
            //Predicate which checks all relevant things first.
            bool predicate = thing.Faction == pawn.Faction && thing.TryGetComp<CompPower>() is CompPower compPower && compPower.PowerNet != null && compPower.PowerNet.CurrentStoredEnergy() > 50f && !thing.IsForbidden(pawn) && pawn.CanReserve(new LocalTargetInfo(thing)) && thing.Position.InAllowedArea(pawn) && pawn.CanReach(new LocalTargetInfo(thing), PathEndMode.OnCell, Danger.Deadly);
            if (!predicate)
                return false;

            Building building = thing as Building;

            //Now check if it is a valid target.
            IntVec3 drainSpot = thing.Position;

            //Give out job to go out and tap it.
            if (drainSpot.Walkable(pawn.Map) && drainSpot.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(drainSpot)) && pawn.CanReach(drainSpot, PathEndMode.OnCell, Danger.Deadly))
                return true;

            //Check surrounding cells.
            foreach (IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(building).OrderByDescending(selector => selector.DistanceTo(pawn.Position)))
            {
                if (adjCell.Walkable(pawn.Map) && adjCell.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(adjCell)) && pawn.CanReach(adjCell, PathEndMode.ClosestTouch, Danger.Deadly))
                    return true;
            }

            return false;
        }
    }
}
