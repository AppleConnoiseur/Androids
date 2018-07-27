using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Tops up the Food and Energy needs if available.
    /// </summary>
    public class Hediff_VanometricCell : HediffWithComps
    {
        public override void Tick()
        {
            base.Tick();

            if(pawn?.needs?.food is Need_Food food)
            {
                food.CurLevel = food.MaxLevel;
            }

            if(pawn?.needs?.TryGetNeed<Need_Energy>() is Need_Energy energy)
            {
                energy.CurLevel = energy.MaxLevel;
            }
        }

        public override string TipStringExtra => "AndroidHediffVanometricCell".Translate();
    }
}
