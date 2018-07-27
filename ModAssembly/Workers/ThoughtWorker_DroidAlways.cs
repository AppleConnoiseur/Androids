using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// This thought is ALWAYS turned on for Droids.
    /// </summary>
    public class ThoughtWorker_DroidAlways : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if(p.def.HasModExtension<MechanicalPawnProperties>())
                return ThoughtState.ActiveAtStage(0);

            return ThoughtState.Inactive;
        }
    }
}
