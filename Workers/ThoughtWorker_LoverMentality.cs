using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class ThoughtWorker_LoverMentality : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn p, Pawn otherPawn)
        {
            if(p.health.hediffSet.HasHediff(def.hediff) && p.health.hediffSet.GetFirstHediffOfDef(def.hediff) is Hediff_LoverMentality mentality && mentality.loverToChase == otherPawn)
            {
                return true;
            }

            return false;
        }
    }
}
