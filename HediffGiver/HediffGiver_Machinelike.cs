using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Modifies incoming injuries to match the machinelike being of the pawn.
    /// </summary>
    public class HediffGiver_Machinelike : HediffGiver
    {
        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            //Replace bleeding with coolant loss. (Vampires Fix)
            if (hediff.def == RimWorld.HediffDefOf.BloodLoss)
            {
                HealthUtility.AdjustSeverity(pawn, HediffDefOf.ChjCoolantLoss, hediff.Severity);
                hediff.Severity = 0f;
                return true;
            }

            //If it is not a injury, stop here.
            if (!(hediff is Hediff_Injury))
            {
                return false;
            }

            HediffWithComps hediffWithComps = hediff as HediffWithComps;
            if (hediffWithComps == null)
                return false;

            hediffWithComps.comps.RemoveAll(hediffComp => hediffComp is HediffComp_Infecter);

            return true;
        }
    }
}
