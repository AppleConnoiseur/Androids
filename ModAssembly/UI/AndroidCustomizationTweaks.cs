using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Tweaks for the mod.
    /// </summary>
    public static class AndroidCustomizationTweaks
    {
        [TweakValue("AndroidCustomizationWindow", 16f, 128f)]
        public static int upgradeBaseSize = 48;
        [TweakValue("AndroidCustomizationWindow", 3, 100)]
        public static int maxTraitsToPick = 7;
    }
}
