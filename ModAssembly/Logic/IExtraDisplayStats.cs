using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Androids
{
    /// <summary>
    /// Interface for showing extra display stats on stuff.
    /// </summary>
    public interface IExtraDisplayStats
    {
        IEnumerable<StatDrawEntry> SpecialDisplayStats();
    }
}
