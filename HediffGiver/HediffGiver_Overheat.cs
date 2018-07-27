using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Makes the Android overheat if the Coolant Loss Hediff is over a certain level.
    /// </summary>
    public class HediffGiver_Overheat : HediffGiver
    {
        public HediffDef contributingHediff;
        public float startToOverheatAt = 0.5f;

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            HediffSet hediffSet = pawn.health.hediffSet;

            Hediff coolantHediff = hediffSet.GetFirstHediffOfDef(contributingHediff);

            bool isOverheating = coolantHediff != null && coolantHediff.Severity >= startToOverheatAt;

            if (isOverheating)
            {
                HealthUtility.AdjustSeverity(pawn, this.hediff, hediffSet.BleedRateTotal * 0.005f);
            }
            else
            {
                HealthUtility.AdjustSeverity(pawn, this.hediff, -0.0125f);
            }
        }
    }
}
