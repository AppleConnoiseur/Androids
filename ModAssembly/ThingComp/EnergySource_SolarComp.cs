using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Solar variant of the energy source component. Passively recharges energy as soon as it is sunny outside.
    /// </summary>
    public class EnergySource_SolarComp : EnergySourceComp
    {
        public override void RechargeEnergyNeed(Pawn targetPawn)
        {
            bool isNight = GenLocalDate.DayPercent(targetPawn) < 0.2f || GenLocalDate.DayPercent(targetPawn) > 0.7f;

            if (isNight)
                return;

            if (targetPawn.InContainerEnclosed)
                return;

            if (!targetPawn.IsCaravanMember() && targetPawn.Position.Roofed(targetPawn.Map))
                return;

            Need_Energy energyNeed = targetPawn.needs.TryGetNeed<Need_Energy>();

            if(energyNeed != null)
                energyNeed.CurLevel += EnergyProps.passiveEnergyGeneration;
        }
    }
}
