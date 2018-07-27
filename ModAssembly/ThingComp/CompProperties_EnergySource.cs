using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Properties for: EnergySourceComp
    /// </summary>
    public class CompProperties_EnergySource : CompProperties
    {
        /// <summary>
        /// If true then this item is consumed from the stack to refill their energy reserves.
        /// </summary>
        public bool isConsumable = false;

        /// <summary>
        /// Energy gained when consumed.
        /// </summary>
        public float energyWhenConsumed = 0f;

        /// <summary>
        /// Energy gained passively when equipped.
        /// </summary>
        public float passiveEnergyGeneration = 0f;

        /// <summary>
        /// The fuels this energy source can use, and how much each refill.
        /// </summary>
        public List<ThingOrderRequest> fuels = new List<ThingOrderRequest>();

        /// <summary>
        /// Maximum fuel amount when fully filled.
        /// </summary>
        public float maxFuelAmount = 75f;

        /// <summary>
        /// How much fuel is used per day.
        /// </summary>
        public double fuelAmountUsedPerInterval = 0.001d;

        /// <summary>
        /// How much energy is given when actively charging.
        /// </summary>
        public float activeEnergyGeneration = 0f;

        /// <summary>
        /// Job to use when refilling.
        /// </summary>
        public JobDef refillJob;

        public CompProperties_EnergySource()
        {
            compClass = typeof(EnergySourceComp);
        }
    }
}
