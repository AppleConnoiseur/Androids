using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Represents a group in which a upgrade can belong to.
    /// </summary>
    public class AndroidUpgradeGroupDef : Def
    {
        /// <summary>
        /// In what order the upgrade group should be rendered.
        /// </summary>
        public int orderID = 0;

        /// <summary>
        /// Internal cache of all upgrades belonging to this group.
        /// </summary>
        [Unsaved]
        private List<AndroidUpgradeDef> intCachedUpgrades;

        public IEnumerable<AndroidUpgradeDef> Upgrades
        {
            get
            {
                //Cache upgrades.
                if (intCachedUpgrades == null)
                {
                    intCachedUpgrades = new List<AndroidUpgradeDef>();

                    foreach(AndroidUpgradeDef def in DefDatabase<AndroidUpgradeDef>.AllDefs)
                    {
                        if (def.upgradeGroup == this)
                            intCachedUpgrades.Add(def);
                    }
                }

                return intCachedUpgrades;
            }
        }

        public float calculateNeededHeight(Rect upgradeSize, float rowWidth)
        {
            //float x = 0f;
            int itemsPerRow = (int)Math.Floor(rowWidth / upgradeSize.width);
            float neededHeight = upgradeSize.height * (float)Math.Ceiling((double)Upgrades.Count() / (double)itemsPerRow);

            return neededHeight;
        }
    }
}
