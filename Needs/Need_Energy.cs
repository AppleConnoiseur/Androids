using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using UnityEngine;
using Androids.Integration;

namespace Androids
{
    /// <summary>
    /// Energy need for Androids.
    /// </summary>
    public class Need_Energy : Need
    {
        /// <summary>
        /// The percentage level at which we should attempt recharging. 50.5%
        /// </summary>
        public static float rechargePercentage = 0.505f;

        public EnergyTrackerComp EnergyTracker
        {
            get
            {
                return pawn.TryGetComp<EnergyTrackerComp>();
            }
        }

        public Need_Energy(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public override float MaxLevel => pawn.TryGetComp<EnergyTrackerComp>() is EnergyTrackerComp tracker ? tracker.EnergyProperties.maxEnergy : 1f;

        /// <summary>
        /// Start with Energy maxed.
        /// </summary>
        public override void SetInitialLevel()
        {
            CurLevel = MaxLevel;
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = int.MaxValue, float customMargin = -1F, bool drawArrows = true, bool doTooltip = true)
        {
            if (threshPercents == null)
            {
                threshPercents = new List<float>();
            }
            threshPercents.Clear();
            
            //Add one for each full percent.
            if(MaxLevel > 1.0f)
            {
                float fullPip = 1f / MaxLevel;
                threshPercents.Add(fullPip * 0.5f);
                threshPercents.Add(fullPip * 0.2f);
                for (int i = 0; i < (int)Math.Floor(MaxLevel); i++)
                {
                    threshPercents.Add(fullPip + (fullPip * i));
                }
            }
            else
            {
                threshPercents.Add(0.5f);
                threshPercents.Add(0.2f);
            }

            //Widgets.Label(rect, $"MaxLevel={MaxLevel}");

            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip);
        }

        public override void NeedInterval()
        {
            //150 ticks

            //Compatibility Mode
            if(AndroidsModSettings.Instance.droidCompatibilityMode && pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                //Top up all needs except Mood and Energy.
                foreach(Need need in pawn.needs.AllNeeds)
                {
                    if(need.def != NeedsDefOf.ChJEnergy)
                    {
                        need.CurLevelPercentage = 1f;
                    }
                }
            }

            float drainModifier = 1f;
            if(pawn.TryGetComp<EnergyTrackerComp>() is EnergyTrackerComp energyTracker)
            {
                if(!pawn.IsCaravanMember() && energyTracker.EnergyProperties.canHibernate && pawn.CurJobDef == energyTracker.EnergyProperties.hibernationJob)
                {
                    drainModifier = -0.1f;
                }
                else
                {
                    drainModifier *= energyTracker.EnergyProperties.drainRateModifier;
                }
            }

            //Top up our Energy levels as long as we are fed.
            if(pawn.needs != null && !IsFrozen)
            {
                //Normal drain
                CurLevel -= drainModifier * (1f / 1200f);

                //Energy gain from having a food need
                if (pawn.needs.food != null && pawn.needs.food.CurLevelPercentage > 0.0f)
                {
                    CurLevel += 1f / 75f;
                }

                //Energy gain from apparel
                if (pawn.apparel != null)
                {
                    foreach(Apparel apparel in pawn.apparel.WornApparel)
                    {
                        EnergySourceComp energySourceComp = apparel.TryGetComp<EnergySourceComp>();
                        if(energySourceComp != null && !energySourceComp.EnergyProps.isConsumable)
                        {
                            energySourceComp.RechargeEnergyNeed(pawn);
                        }
                    }
                }

                //Energy gain in caravan.
                if(pawn.IsCaravanMember() && pawn.IsHashIntervalTick(250) && CurLevelPercentage < rechargePercentage)
                {
                    Caravan caravan = pawn.GetCaravan();
                    Thing validEnergySource = 
                        caravan.Goods.FirstOrDefault(
                            thing => 
                            thing.TryGetComp<EnergySourceComp>() is EnergySourceComp energySource && 
                            energySource.EnergyProps.isConsumable
                            );
                    if(validEnergySource != null)
                    {
                        //Use enough to get satisfied.
                        EnergySourceComp energySourceComp = validEnergySource.TryGetComp<EnergySourceComp>();

                        int thingCount = (int)Math.Ceiling((MaxLevel - CurLevel) / energySourceComp.EnergyProps.energyWhenConsumed);
                        thingCount = Math.Min(thingCount, validEnergySource.stackCount);

                        Thing splitOff = validEnergySource.SplitOff(thingCount);
                        EnergySourceComp energySourceCompSplit = splitOff.TryGetComp<EnergySourceComp>();
                        energySourceCompSplit.RechargeEnergyNeed(pawn);
                        splitOff.Destroy(DestroyMode.Vanish);
                    }
                }

                //Slow down.
                if (CurLevel < 0.2f)
                {
                    if(!pawn.health.hediffSet.HasHediff(HediffDefOf.ChjPowerShortage))
                        pawn.health.AddHediff(HediffDefOf.ChjPowerShortage);
                }
                else
                {
                    if (pawn.health.hediffSet.HasHediff(HediffDefOf.ChjPowerShortage))
                        pawn.health.RemoveHediff(pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ChjPowerShortage)); ;
                }

                //Point beyond no return.
                if (CurLevel <= 0f)
                {
                    //Die
                    Hediff exactCulprit = HediffMaker.MakeHediff(HediffDefOf.ChjPowerFailure, pawn);
                    pawn.health.AddHediff(exactCulprit);
                    pawn.Kill(null, exactCulprit);
                }
            }
        }
    }
}
