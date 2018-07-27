using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Androids
{
    /// <summary>
    /// Contains data about what to set the Droid skill as.
    /// </summary>
    public class DroidSkill
    {
        /// <summary>
        /// SkillDef to use.
        /// </summary>
        public SkillDef def;

        /// <summary>
        /// Skill level to set.
        /// </summary>
        public int level = 0;

        /// <summary>
        /// Skill passion to set.
        /// </summary>
        public Passion passion = Passion.None;
    }
}
