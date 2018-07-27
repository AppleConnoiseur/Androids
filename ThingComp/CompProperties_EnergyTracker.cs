using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Properties for the EnergyTracker.
    /// </summary>
    public class CompProperties_EnergyTracker : CompProperties
    {
        public CompProperties_EnergyTracker()
        {
            compClass = typeof(EnergyTrackerComp);
        }

        /// <summary>
        /// Can the thing hibernate at specific points?
        /// </summary>
        public bool canHibernate = true;

        /// <summary>
        /// Job to give when hibernating.
        /// </summary>
        public JobDef hibernationJob;
    }
}
