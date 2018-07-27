using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Properties for WorkGivers related to Pawn Crafters.
    /// </summary>
    public class PawnCrafterWorkgiverProperties : DefModExtension
    {
        /// <summary>
        /// ThingDef to scan for.
        /// </summary>
        public ThingDef defToScan;

        /// <summary>
        /// Fill Job to give.
        /// </summary>
        public JobDef fillJob;
    }
}
