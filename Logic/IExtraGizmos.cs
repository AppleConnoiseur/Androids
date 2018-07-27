using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Interface for getting extra Gizmos from a object.
    /// </summary>
    public interface IExtraGizmos
    {
        /// <summary>
        /// Gets all extra gizmos the object may have.
        /// </summary>
        /// <returns>Gizmos</returns>
        IEnumerable<Gizmo> GetGizmosExtra();
    }
}
