using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Merely cosmetical class.
    /// </summary>
    public class Hediff_Percentage : HediffWithComps
    {
        public override string SeverityLabel => Math.Abs(Severity / def.lethalSeverity).ToStringPercent();
    }
}
