using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Androids
{
    /// <summary>
    /// Changes the skin color of the Android on top of applying Hediffs.
    /// </summary>
    public class UpgradeCommand_Skin : UpgradeCommand_Hediff
    {
        /// <summary>
        /// Primary original skin color.
        /// </summary>
        public Color originalSkinColor;

        /// <summary>
        /// Secondary original skin color.
        /// </summary>
        public Color originalSkinColorTwo;

        public override void Apply(Pawn customTarget = null)
        {
            base.Apply(customTarget);

            Pawn targetPawn = null;
            if (customTarget != null)
            {
                targetPawn = customTarget;
            }
            else
            {
                targetPawn = customizationWindow.newAndroid;
            }

            AlienComp alienComp = targetPawn.TryGetComp<AlienComp>();
            if (alienComp != null)
            {
                originalSkinColor = alienComp.ColorChannels["skin"].first;
                originalSkinColorTwo = alienComp.ColorChannels["skin"].second;

                alienComp.ColorChannels["skin"].first = def.newSkinColor;
                alienComp.ColorChannels["skin"].second = def.newSkinColor;

                if(customizationWindow != null)
                {
                    customizationWindow.refreshAndroidPortrait = true;
                }
                else
                {
                    PortraitsCache.SetDirty(targetPawn);
                    PortraitsCache.PortraitsCacheUpdate();
                }
            }
            else
            {
                Log.Error("alienComp is null! Impossible to alter skin color without it.");
            }
        }

        public override void Undo()
        {
            base.Undo();

            AlienComp alienComp = customizationWindow.newAndroid.TryGetComp<AlienComp>();
            if (alienComp != null)
            {
                alienComp.ColorChannels["skin"].first = originalSkinColor;
                alienComp.ColorChannels["skin"].second = originalSkinColorTwo;

                customizationWindow.refreshAndroidPortrait = true;
            }
            else
            {
                Log.Error("alienComp is null! Impossible to alter skin color without it.");
            }
        }
    }
}
