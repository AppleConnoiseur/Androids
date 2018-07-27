using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// A fueled energy source. Can be anything from wood to AI Persona Cores.
    /// </summary>
    public class EnergySource_Fueled : EnergySourceComp, IExtraDisplayStats
    {
        /// <summary>
        /// Returns the amount fuel used per tick.
        /// </summary>
        public double FuelUsedPerInterval
        {
            get
            {
                return EnergyProps.fuelAmountUsedPerInterval;
            }
        }

        /// <summary>
        /// Returns the missing fuel amount.
        /// </summary>
        public float MissingFuel
        {
            get
            {
                return EnergyProps.maxFuelAmount - (float)fuelAmountLoaded;
            }
        }

        /// <summary>
        /// Returns missing fuel in percentage format.
        /// </summary>
        public float MissingFuelPercentage
        {
            get
            {
                return MissingFuel / EnergyProps.maxFuelAmount;
            }
        }

        /// <summary>
        /// Current fuel amount loaded.
        /// </summary>
        public double fuelAmountLoaded = 0f;

        /// <summary>
        /// Will the pawn with this automatically try to refuel?
        /// </summary>
        public bool autoRefuel = true;

        /// <summary>
        /// Loads fuel into the comp.
        /// </summary>
        /// <param name="fuel">Fuel we try to load.</param>
        /// <returns>True if fuel is accepted.</returns>
        public bool LoadFuel(Thing fuel, bool doNotDestroy = false)
        {
            if (fuel.stackCount <= 0)
                return false;

            //If the fuel is right, we accept it.
            if (EnergyProps.fuels.FirstOrDefault(req => req.thingDef == fuel.def) is ThingOrderRequest fuelRequest)
            {
                //Deduct as much as we need.
                //int unitsToLoad = Math.Min((int)Math.Ceiling(MissingFuel / fuelRequest.amount), Math.Min((int)Math.Ceiling((float)fuel.stackCount / fuelRequest.amount), fuel.stackCount));
                int desiredAmount = (int)Math.Ceiling(MissingFuel / fuelRequest.amount);
                //Log.Message("L´desiredAmount´1=" + desiredAmount);
                desiredAmount = Math.Min(desiredAmount, fuel.stackCount);
                //Log.Message("L´desiredAmount`2=" + desiredAmount);
                int unitsToLoad = desiredAmount;
                //Log.Message("L´unitsToLoad=" + unitsToLoad);

                //Log.Message("unitsToLoad=" + unitsToLoad);

                //If we somehow have 0 units to load we abort with a failure to accept.
                if (unitsToLoad <= 0)   
                    return false;

                //Refuel
                fuelAmountLoaded += (int)Math.Ceiling(unitsToLoad * fuelRequest.amount);
                if (fuelAmountLoaded > EnergyProps.maxFuelAmount)
                    fuelAmountLoaded = EnergyProps.maxFuelAmount;

                fuel.stackCount -= unitsToLoad;
                if (!doNotDestroy && fuel.stackCount <= 0)
                    fuel.Destroy();

                return true;
            }

            return false;
        }

        public int CalculateFuelNeededToRefill(Thing fuel)
        {
            if (EnergyProps.fuels.FirstOrDefault(req => req.thingDef == fuel.def) is ThingOrderRequest fuelRequest)
            {
                //Deduct as much as we need.
                //int unitsToLoad = Math.Min((int)Math.Ceiling(MissingFuel / fuelRequest.amount), fuel.stackCount);
                //int unitsToLoad = Math.Min((int)Math.Ceiling(MissingFuel / fuelRequest.amount), (int)Math.Ceiling((float)fuel.stackCount / fuelRequest.amount));
                int desiredAmount = (int)Math.Ceiling(MissingFuel / fuelRequest.amount);
                //Log.Message("desiredAmount´1=" + desiredAmount);
                desiredAmount = Math.Min(desiredAmount, fuel.stackCount);
                //Log.Message("desiredAmount`2=" + desiredAmount);
                int unitsToLoad = desiredAmount;
                //Log.Message("unitsToLoad=" + unitsToLoad);

                return unitsToLoad;
            }

            return -1;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref fuelAmountLoaded, "fuelAmountLoaded");
            Scribe_Values.Look(ref autoRefuel, "autoRefuel");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(Find.Selector.NumSelected <= 1)
            {
                yield return new Gizmo_EnergySourceFueled()
                {
                    apparel = parent as Apparel,
                    fueledEnergySource = this
                };
            }

            yield return new Command_Toggle()
            {
                defaultLabel = "AndroidGizmoAutoRefuelLabel".Translate(),
                defaultDesc =  "AndroidGizmoAutoRefuelDescription".Translate(),
                isActive = () => autoRefuel,
                order = -99,
                toggleAction = delegate()
                {
                    autoRefuel = !autoRefuel;
                },
                icon = ContentFinder<Texture2D>.Get("UI/Commands/SetTargetFuelLevel", true)
            };

            yield return new Command_Action()
            {
                defaultLabel = "AndroidGizmoRefuelNowLabel".Translate(),
                defaultDesc =  "AndroidGizmoRefuelNowDescription".Translate(),
                icon = RimWorld.ThingDefOf.Chemfuel.uiIcon,
                order = -99,
                action = delegate()
                {
                    Apparel apparel = parent as Apparel;
                    if(apparel != null)
                    {
                        Thing closestConsumablePowerSource = FuelUtility.FindSuitableFuelForPawn(apparel.Wearer, this);
                        if(closestConsumablePowerSource != null)
                        {
                            int refuelCount = CalculateFuelNeededToRefill(closestConsumablePowerSource);
                            if(refuelCount > 0)
                            {
                                Job refuelJob = new Job(EnergyProps.refillJob, parent, closestConsumablePowerSource);
                                refuelJob.count = refuelCount;

                                apparel.Wearer.jobs.TryTakeOrderedJob(refuelJob, JobTag.Misc);
                            }
                        }
                    }
                }
            };
        }

        public double FuelUsageModifier()
        {
            double finalModifier = 1f;

            double qualityModifier = 1d;
            if (parent.TryGetComp<CompQuality>() is CompQuality quality)
            {
                switch (quality.Quality)
                {
                    case QualityCategory.Awful:
                        qualityModifier = 2d;
                        break;
                    case QualityCategory.Poor:
                        qualityModifier = 1.5d;
                        break;
                    case QualityCategory.Normal:
                        qualityModifier = 1d;
                        break;
                    case QualityCategory.Good:
                        qualityModifier = 0.9d;
                        break;
                    case QualityCategory.Excellent:
                        qualityModifier = 0.7d;
                        break;
                    case QualityCategory.Masterwork:
                        qualityModifier = 0.5d;
                        break;
                    case QualityCategory.Legendary:
                        qualityModifier = 0.25d;
                        break;
                }
            }

            finalModifier = finalModifier * qualityModifier;

            return finalModifier;
        }

        public override void RechargeEnergyNeed(Pawn targetPawn)
        {
            base.RechargeEnergyNeed(targetPawn);

            //Deduct fuel.
            if (fuelAmountLoaded > 0d)
            {
                fuelAmountLoaded -= FuelUsedPerInterval * FuelUsageModifier();

                //Recharge.
                Need_Energy energyNeed = targetPawn.needs.TryGetNeed<Need_Energy>();

                if (energyNeed != null)
                    energyNeed.CurLevel += EnergyProps.activeEnergyGeneration;
            }

            //Cap fuel amount.
            if (fuelAmountLoaded < 0d)
                fuelAmountLoaded = 0d;

            //Attempt refilling in caravan if possible.
            if(targetPawn.IsCaravanMember() && MissingFuelPercentage > 0.8f)
            {
                Caravan caravan = targetPawn.GetCaravan();
                Thing validFuelSource =
                        caravan.Goods.FirstOrDefault(
                            fuelThing => EnergyProps.fuels.Any(req => req.thingDef == fuelThing.def));
                if(validFuelSource != null)
                {
                    int refillAmount = CalculateFuelNeededToRefill(validFuelSource);
                    if(refillAmount > 0)
                    {
                        Thing takenFuel = validFuelSource.SplitOff(refillAmount);
                        if(takenFuel != null)
                        {
                            LoadFuel(takenFuel);
                        }
                    }
                }
            }
        }

        public IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            StatDrawEntry fuelEfficencyEntry = new StatDrawEntry(StatCategoryDefOf.EquippedStatOffsets, "AndroidFuelEfficencyStatPartLabel".Translate(), FuelUsageModifier().ToString("F2"), 0, "AndroidFuelEfficencyStatPartReport".Translate());
            yield return fuelEfficencyEntry;
        }
    }
}
