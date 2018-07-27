using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Tries to remove all diseases.
    /// </summary>
    public class HediffGiver_Machine : HediffGiver
    {
        public override bool OnHediffAdded(Pawn pawn, Hediff hediff)
        {
            //Remove any disease from affecting.
            if(hediff.def.makesSickThought && !hediff.Bleeding)
            {
                pawn.health.RemoveHediff(hediff);
                return false;
            }

            return true;
        }
    }
}
