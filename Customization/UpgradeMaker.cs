using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Androids
{
    /// <summary>
    /// Utility for creating UpgradeCommands.
    /// </summary>
    public static class UpgradeMaker
    {
        /// <summary>
        /// Creates a UpgradeCommand from a Def.
        /// </summary>
        /// <param name="def">Def to create it from.</param>
        /// <param name="customizationWindow">Optional window to set.</param>
        /// <returns>New UpgradeCommand.</returns>
        public static UpgradeCommand Make(AndroidUpgradeDef def, CustomizeAndroidWindow customizationWindow = null)
        {
            UpgradeCommand result = (UpgradeCommand)Activator.CreateInstance(def.commandType);
            if(result != null)
            {
                result.def = def;
                result.customizationWindow = customizationWindow;
            }

            return result;
        }
    }
}
