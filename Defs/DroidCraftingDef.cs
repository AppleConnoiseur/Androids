using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Defines 
    /// </summary>
    public class DroidCraftingDef : Def
    {
        /// <summary>
        /// The cost to manufacture one Droid.
        /// </summary>
        public List<ThingOrderRequest> costList = new List<ThingOrderRequest>();

        /// <summary>
        /// The time it takes to manufacture one Droid.
        /// </summary>
        public int timeCost = 0;

        /// <summary>
        /// The Droid kind to spawn upon construction.
        /// </summary>
        public PawnKindDef pawnKind;

        /// <summary>
        /// Whether to use the Utility way of creating the Droid or not.
        /// </summary>
        public bool useDroidCreator = true;

        /// <summary>
        /// In what order to display this in menus.
        /// </summary>
        public int orderID = 0;

        /// <summary>
        /// Research required in order for it to be craftable.
        /// </summary>
        public ResearchProjectDef requiredResearch;
    }
}
