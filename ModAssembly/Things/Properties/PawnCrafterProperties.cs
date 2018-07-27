using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Properties for pawn crafters.
    /// </summary>
    public class PawnCrafterProperties : DefModExtension
    {
        /// <summary>
        /// Pawn kind to craft.
        /// </summary>
        public PawnKindDef pawnKind;

        /// <summary>
        /// The cost to manufacture one Pawn.
        /// </summary>
        public List<ThingOrderRequest> costList = new List<ThingOrderRequest>();

        /// <summary>
        /// Races that are not possible to print in this printer.
        /// </summary>
        public List<ThingDef> disabledRaces = new List<ThingDef>();

        /// <summary>
        /// Label on the letter when finished crafting.
        /// </summary>
        public string pawnCraftedLetterLabel = "AndroidPrintedLetterLabel";

        /// <summary>
        /// Text on the letter when finished crafting.
        /// </summary>
        public string pawnCraftedLetterText = "AndroidPrintedLetterDescription";

        /// <summary>
        /// Status text on the crafter.
        /// </summary>
        public string crafterStatusText = "AndroidPrinterStatus";

        /// <summary>
        /// Enum prefix text.
        /// </summary>
        public string crafterStatusEnumText = "AndroidPrinterStatusEnum";

        /// <summary>
        /// Crafter progress text.
        /// </summary>
        public string crafterProgressText = "AndroidPrinterProgress";

        /// <summary>
        /// Crafter materials text.
        /// </summary>
        public string crafterMaterialsText = "AndroidPrinterMaterials";

        /// <summary>
        /// Crafter material need.
        /// </summary>
        public string crafterMaterialNeedText = "AndroidPrinterNeed";

        /// <summary>
        /// Crafter nutrition text.
        /// </summary>
        public string crafterNutritionText = "AndroidNutrition";

        /// <summary>
        /// How many ticks are required to craft the pawn.
        /// </summary>
        public int ticksToCraft = 60000;

        /// <summary>
        /// How often a "resource" tick happen in which resources are deducted from internal storage.
        /// </summary>
        public int resourceTick = 2500;

        /// <summary>
        /// Optional Hediff to apply on newly crafted pawn.
        /// </summary>
        public HediffDef hediffOnPawnCrafted;

        /// <summary>
        /// Optional thought to apply on newly crafted pawn.
        /// </summary>
        public ThoughtDef thoughtOnPawnCrafted;

        /// <summary>
        /// The factor 0.0 - 1.0 in which power is consumed when not crafting.
        /// </summary>
        public float powerConsumptionFactorIdle = 0.1f;

        /// <summary>
        /// Optional set of skills to give to newly created pawns.
        /// </summary>
        public List<SkillRequirement> skills = new List<SkillRequirement>();

        /// <summary>
        /// If not skill is named this will be the default skill level.
        /// </summary>
        public int defaultSkillLevel = 6;

        /// <summary>
        /// Sound played during crafting.
        /// </summary>
        public SoundDef craftingSound;

        /// <summary>
        /// If true the derived class will handle it.
        /// </summary>
        public bool customOrderProcessor = false;

        /// <summary>
        /// If true the derived class will handle it.
        /// </summary>
        public bool customCraftingTime = false;

        /// <summary>
        /// How many ticks will happen in the crafting period.
        /// </summary>
        /// <returns>Resource ticks.</returns>
        public float ResourceTicks()
        {
            return (float)Math.Ceiling((double)ticksToCraft / resourceTick);
        }
    }
}
