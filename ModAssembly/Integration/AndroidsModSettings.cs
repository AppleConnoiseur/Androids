using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids.Integration
{
    /// <summary>
    /// Settings for this mod.
    /// </summary>
    public class AndroidsModSettings : ModSettings
    {
        /// <summary>
        /// Singleton instance for our mod settings.
        /// </summary>
        public static AndroidsModSettings Instance;

        /// <summary>
        /// If true Androids have their characteristic eyeglow.
        /// </summary>
        public bool androidEyeGlow = true;

        /// <summary>
        /// If true Androids will explode on death.
        /// </summary>
        public bool androidExplodesOnDeath = true;

        /// <summary>
        /// Explosion radius on dying Android.
        /// </summary>
        public float androidExplosionRadius = 3.5f;

        /// <summary>
        /// Turns on all Needs that would be expected for Droids.
        /// </summary>
        public bool droidCompatibilityMode = false;

        /// <summary>
        /// If true the game will ALWAYS ask for confirmation before attempting to detonate a Droid.
        /// </summary>
        public bool droidDetonationConfirmation = true;

        /// <summary>
        /// If true the HediffGiver_MachineWearAndTear will be active.
        /// </summary>
        public bool droidWearDown = true;

        /// <summary>
        /// If true checks are done every quadrum instead of every day.
        /// </summary>
        public bool droidWearDownQuadrum = true;

        public AndroidsModSettings()
        {
            AndroidsModSettings.Instance = this;
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref androidEyeGlow, "androidEyeGlow", true);
            Scribe_Values.Look(ref androidExplodesOnDeath, "androidExplodesOnDeath", true);
            Scribe_Values.Look(ref androidExplosionRadius, "androidExplosionRadius", 3.5f);
            Scribe_Values.Look(ref droidCompatibilityMode, "droidCompatibilityMode", false);
            Scribe_Values.Look(ref droidDetonationConfirmation, "droidDetonationConfirmation", true);
            Scribe_Values.Look(ref droidWearDown, "droidWearDown", true);
            Scribe_Values.Look(ref droidWearDownQuadrum, "droidWearDownQuadrum", true);
        }
    }
}
