using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Basically let Apparel show off gizmos from components. And other stuff.
    /// </summary>
    public class ApparelWithGizmos : Apparel
    {
        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach(ThingComp comp in AllComps)
            {
                foreach(Gizmo gizmo in comp.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            foreach (StatDrawEntry entry in base.SpecialDisplayStats())
                yield return entry;

            foreach (ThingComp comp in AllComps)
            {
                if(comp is IExtraDisplayStats displayStat)
                {
                    foreach (StatDrawEntry entry in displayStat.SpecialDisplayStats())
                        yield return entry;
                }
            }
        }
    }
}
