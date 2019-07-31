using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Androids
{
    /// <summary>
    /// Represents a upgrade for a Android that changes body type on top of applying hediffs.
    /// </summary>
    public class UpgradeCommand_Body : UpgradeCommand_Hediff
    {
        /// <summary>
        /// Original body type for the Android.
        /// </summary>
        public BodyTypeDef originalBodyType;

        public override void Apply(Pawn customTarget = null)
        {
            base.Apply(customTarget);

            Pawn targetPawn = null;
            if(customTarget != null)
            {
                targetPawn = customTarget;
            }
            else
            {
                targetPawn = customizationWindow.newAndroid;
            }

            originalBodyType = targetPawn.story.bodyType;

            bool canChangeBodyType = false;

            if (targetPawn.def is ThingDef_AlienRace alienDef)
            {
                //Check if the alien race can use this body.
                canChangeBodyType = alienDef.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(def.newBodyType);
            }
            else
            {
                canChangeBodyType = true;
            }

            if(canChangeBodyType)
                targetPawn.story.bodyType = def.newBodyType;

            if(customizationWindow != null)
            {
                customizationWindow.refreshAndroidPortrait = true;
            }
        }

        public override void Undo()
        {
            base.Undo();

            customizationWindow.newAndroid.story.bodyType = originalBodyType;

            customizationWindow.refreshAndroidPortrait = true;
        }
    }
}
