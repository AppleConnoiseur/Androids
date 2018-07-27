using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Periodically heals bleeding injuries.
    /// </summary>
    public class Hediff_MechaniteHive : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();

            if(pawn.IsHashIntervalTick(2000))
            {
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury injury)
                    {
                        if (injury.Bleeding)
                        {
                            injury.Tended(1f);
                        }
                    }
                }
            }
        }

        public override string TipStringExtra => "AndroidMechaniteHive".Translate();
    }
}
