using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Basically tags a ThingDef as a mechanical pawn.
    /// </summary>
    public class MechanicalPawnProperties : DefModExtension
    {
        /// <summary>
        /// If true the pawn will not lose any skill due to decay.
        /// </summary>
        public bool noSkillLoss = true;

        /// <summary>
        /// Can this Droid be social?
        /// </summary>
        public bool canSocialize = false;

        /// <summary>
        /// Do the colony care if they die?
        /// </summary>
        public bool colonyCaresIfDead = false;
    }
}
