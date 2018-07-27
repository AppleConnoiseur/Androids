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
    /// Utility for helping in dealing with fueled energy sources.
    /// </summary>
    public static class FuelUtility
    {
        public static readonly float autoRefillThreshhold = 0.8f;

        public static Thing FindSuitableFuelForPawn(Pawn pawn, EnergySource_Fueled fuelEnergySourceComp)
        {
            Thing closestConsumablePowerSource =
                GenClosest.ClosestThingReachable(
                    pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.OnCell, TraverseParms.For(pawn), 9999f,
                    thing => FuelUtility.ValidFuelSource(fuelEnergySourceComp, thing));

            return closestConsumablePowerSource;
        }

        public static bool ValidFuelSource(EnergySource_Fueled fuelEnergySourceComp, Thing checkThing)
        {
            if (fuelEnergySourceComp.EnergyProps.fuels.Any(fuelType => fuelType.thingDef == checkThing.def))
                return true;

            return false;
        }

        public static Thing FueledEnergySourceNeedRefilling(Pawn pawn)
        {
            //Look through equipped apparel.
            if (pawn.apparel != null && pawn.apparel.WornApparel.FirstOrDefault(ap => ap.TryGetComp<EnergySource_Fueled>() is EnergySource_Fueled fueledComp && fueledComp.MissingFuelPercentage > autoRefillThreshhold) is Apparel apparel)
            {
                return apparel;
            }

            return null;
        }
    }
}
