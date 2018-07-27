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

        public override void Apply()
        {
            base.Apply();

            AlienComp alienComp = customizationWindow.newAndroid.TryGetComp<AlienComp>();
            if (alienComp != null)
            {
                originalSkinColor = alienComp.skinColor;
                originalSkinColorTwo = alienComp.skinColorSecond;

                alienComp.skinColor = def.newSkinColor;
                alienComp.skinColorSecond = def.newSkinColor;

                customizationWindow.refreshAndroidPortrait = true;
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
                alienComp.skinColor = originalSkinColor;
                alienComp.skinColorSecond = originalSkinColorTwo;

                customizationWindow.refreshAndroidPortrait = true;
            }
            else
            {
                Log.Error("alienComp is null! Impossible to alter skin color without it.");
            }
        }
    }
}
