using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Convenience class for getting HediffDefs.
    /// </summary>
    [DefOf]
    public static class HediffDefOf
    {
        //Health related Hediffs
        public static HediffDef ChjPowerFailure;
        public static HediffDef ChjOverheating;
        public static HediffDef ChjCoolantLoss;
        public static HediffDef ChjPowerShortage;
        public static HediffDef ChjAndroidUpgrade_DroneCore;

        //For Android-like things.
        public static HediffDef ChjAndroidLike;
    }
}
