using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Represents a upgrade that a Android can get upon printing.
    /// </summary>
    public class AndroidUpgradeDef : Def
    {
        /// <summary>
        /// Android upgrade graphic.
        /// </summary>
        /*public Graphic Graphic
        {
            get
            {
                return icon.Graphic;
            }
        }*/

        /// <summary>
        /// In what order this upgrade should be rendered in the GUI.
        /// </summary>
        public int orderID = 0;

        /// <summary>
        /// In what group this upgrade should be in.
        /// </summary>
        public AndroidUpgradeGroupDef upgradeGroup;

        /// <summary>
        /// Represents what Type the UpgradeCommand for this use.
        /// </summary>
        public Type commandType = typeof(UpgradeCommand_Hediff);

        /*/// <summary>
        /// Graphic used to represent the upgrade in GUI.
        /// </summary>*/
        //public GraphicData icon = new GraphicData();
        
        /// <summary>
        /// Path for texture representation of the upgrade.
        /// </summary>
        public string iconTexturePath;

        /*// <summary>
        /// Icon texture.
        /// </summary>
        [Unsaved]
        public Texture2D iconTexture;*/

        /// <summary>
        /// Added cost on pawn manufacturing.
        /// </summary>
        public List<ThingOrderRequest> costList = new List<ThingOrderRequest>();

        /// <summary>
        /// Any ThingDefs added here will not be added to the final cost calculation. Meant for rare items like AI Persona Cores.
        /// </summary>
        public List<ThingDef> costsNotAffectedByBodySize = new List<ThingDef>();

        /// <summary>
        /// Added printing time when manufacturing pawn.
        /// </summary>
        public int extraPrintingTime = 0;

        /// <summary>
        /// Hediff to apply upon printing.
        /// </summary>
        public HediffDef hediffToApply;

        /// <summary>
        /// Hediff severity.
        /// </summary>
        public float hediffSeverity = 1f;

        /// <summary>
        /// Parts to apply the Hediff to.
        /// </summary>
        public List<BodyPartGroupDef> partsToApplyTo;

        /// <summary>
        /// At what depth to apply the depth.
        /// </summary>
        public BodyPartDepth partsDepth = BodyPartDepth.Undefined;

        /// <summary>
        /// This upgrade can't be used with these.
        /// </summary>
        //public List<AndroidUpgradeDef> cantBeUsedWith = new List<AndroidUpgradeDef>();

        /// <summary>
        /// If the upgrade is part of any of these groups only one can be picked out of them.
        /// </summary>
        public List<string> exclusivityGroups = new List<string>();

        /// <summary>
        /// Body type applied on the pawn. If null none is applied.
        /// </summary>
        public BodyTypeDef newBodyType;

        /// <summary>
        /// If true skin color will be changed.
        /// </summary>
        public bool changeSkinColor = false;

        /// <summary>
        /// Skin color to apply on Android.
        /// </summary>
        public Color newSkinColor = new Color(1f, 1f, 1f);

        /// <summary>
        /// Research required in order to use this upgrade.
        /// </summary>
        public ResearchProjectDef requiredResearch;

        /*public override void ResolveReferences()
        {
            iconTexture = ContentFinder<Texture2D>.Get(iconTexturePath);

            base.ResolveReferences();
        }*/
    }
}
