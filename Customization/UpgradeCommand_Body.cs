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

        public override void Apply()
        {
            base.Apply();

            originalBodyType = customizationWindow.newAndroid.story.bodyType;

            bool canChangeBodyType = false;

            if (customizationWindow.newAndroid.def is ThingDef_AlienRace alienDef)
            {
                //Check if the alien race can use this body.
                canChangeBodyType = alienDef.alienRace.generalSettings.alienPartGenerator.alienbodytypes.Contains(def.newBodyType);
            }
            else
            {
                canChangeBodyType = true;
            }

            if(canChangeBodyType)
                customizationWindow.newAndroid.story.bodyType = def.newBodyType;

            customizationWindow.refreshAndroidPortrait = true;
        }

        public override void Undo()
        {
            base.Undo();

            customizationWindow.newAndroid.story.bodyType = originalBodyType;

            customizationWindow.refreshAndroidPortrait = true;
        }
    }
}
