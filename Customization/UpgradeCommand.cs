using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Represents a upgrade on a Android.
    /// </summary>
    public abstract class UpgradeCommand
    {
        /// <summary>
        /// Def which this UpgradeCommand comes from.
        /// </summary>
        public AndroidUpgradeDef def;

        /// <summary>
        /// Customization window to do the work in.
        /// </summary>
        public CustomizeAndroidWindow customizationWindow;

        /// <summary>
        /// Applies the upgrade to the Android.
        /// </summary>
        public abstract void Apply(Pawn customTarget = null);

        /// <summary>
        /// Undoes the upgrade to the Android.
        /// </summary>
        public abstract void Undo();

        /// <summary>
        /// This is run the first time the upgrade is made and applied. Use for customization windows and what not.
        /// </summary>
        public virtual void Notify_UpgradeAdded()
        {

        }

        /// <summary>
        /// Extra stuff to draw on the GUI over the upgrade when active.
        /// </summary>
        /// <param name="inRect"></param>
        public virtual void ExtraOnGUI(Rect inRect)
        {

        }

        /// <summary>
        /// Gets a explanation of what the upgrade does.
        /// </summary>
        /// <returns>Explanation.</returns>
        public abstract string GetExplanation();
    }
}
