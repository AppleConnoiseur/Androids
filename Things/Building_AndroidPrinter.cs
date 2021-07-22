using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Androids
{
    /// <summary>
    /// Android printer building.
    /// </summary>
    public class Building_AndroidPrinter : Building, IThingHolder, IStoreSettingsParent, IPawnCrafter
    {
        //Static values
        /// <summary>
        /// Requested nutrition to print one Android.
        /// </summary>
        public static float requestNutrition = 20f;
        /// <summary>
        /// Requested Plasteel to print one Android.
        /// </summary>
        public static int requestPlasteel = 150;
        /// <summary>
        /// Requested Components to print one Android.
        /// </summary>
        public static int requestComponents = 20;

        //Variables
        /// <summary>
        /// Stored ingredients for use in producing one pawn.
        /// </summary>
        public ThingOwner<Thing> ingredients = new ThingOwner<Thing>();
        /// <summary>
        /// Final calculated cost from the customization GUI.
        /// </summary>
        //public List<ThingOrderRequest> finalPrintingCost = new List<ThingOrderRequest>();
        /// <summary>
        /// Printer state.
        /// </summary>
        public CrafterStatus printerStatus;
        /// <summary>
        /// Pawn to print.
        /// </summary>
        public Pawn pawnToPrint;
        /// <summary>
        /// Class used to store the state of the order processor.
        /// </summary>
        public ThingOrderProcessor orderProcessor;
        /// <summary>
        /// Extra time cost set by the upgrades.
        /// </summary>
        public int extraTimeCost = 0;
        /// <summary>
        /// Storage settings for what nutrition sources to use.
        /// </summary>
        public StorageSettings inputSettings;
        /// <summary>
        /// Sustained sound.
        /// </summary>
        Sustainer soundSustainer;

        //Convenience variables
        /// <summary>
        /// Power component.
        /// </summary>
        private CompPowerTrader powerComp;
        /// <summary>
        /// Flickable component.
        /// </summary>
        private CompFlickable flickableComp;
        
        /// <summary>
        /// XML properties for the printer.
        /// </summary>
        protected PawnCrafterProperties printerProperties;

        //Variables, Construction
        /// <summary>
        /// Ticks left until pawn is finished printing.
        /// </summary>
        public int printingTicksLeft = 0;
        /// <summary>
        /// Next resource drain trick-
        /// </summary>
        public int nextResourceTick = 0;

        /// <summary>
        /// Sets the Storage tab to be visible.
        /// </summary>
        public bool StorageTabVisible => true;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            //None
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return ingredients;
        }

        public PawnCrafterProperties PrinterProperties
        {
            get
            {
                return def.GetModExtension<PawnCrafterProperties>();
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            powerComp = GetComp<CompPowerTrader>();
            flickableComp = GetComp<CompFlickable>();

            if(inputSettings == null)
            {
                inputSettings = new StorageSettings(this);
                if (def.building.defaultStorageSettings != null)
                {
                    inputSettings.CopyFrom(def.building.defaultStorageSettings);
                }
            }

            printerProperties = def.GetModExtension<PawnCrafterProperties>();

            if(!respawningAfterLoad)
            {
                //Setup 'orderProcessor'
                if (printerProperties == null)
                {
                    orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);
                    orderProcessor.requestedItems.Add(
                        new ThingOrderRequest()
                        {
                            nutrition = true,
                            amount = requestNutrition
                        });

                    orderProcessor.requestedItems.Add(
                        new ThingOrderRequest()
                        {
                            thingDef = RimWorld.ThingDefOf.Plasteel,
                            amount = requestPlasteel
                        });

                    orderProcessor.requestedItems.Add(
                        new ThingOrderRequest()
                        {
                            thingDef = RimWorld.ThingDefOf.ComponentIndustrial,
                            amount = requestComponents
                        });
                }
                else
                {
                    orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);
                    /*if (printerProperties != null)
                    {
                        orderProcessor.requestedItems.AddRange(printerProperties.costList);
                    }*/
                }
            }
            else
            {
                //orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);
            }

            AdjustPowerNeed();
        }

        public override void PostMake()
        {
            base.PostMake();

            inputSettings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                inputSettings.CopyFrom(def.building.defaultStorageSettings);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            //Save \ Load
            Scribe_Deep.Look(ref ingredients, "ingredients");
            Scribe_Values.Look(ref printerStatus, "printerStatus");
            Scribe_Values.Look(ref printingTicksLeft, "printingTicksLeft");
            Scribe_Values.Look(ref nextResourceTick, "nextResourceTick");
            Scribe_Deep.Look(ref pawnToPrint, "androidToPrint"); 
            Scribe_Deep.Look(ref inputSettings, "inputSettings");
            Scribe_Deep.Look(ref orderProcessor, "orderProcessor", ingredients, inputSettings);
            Scribe_Values.Look(ref extraTimeCost, "extraTimeCost");
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            //Drop ingredients.
            if(mode != DestroyMode.Vanish)
                ingredients.TryDropAll(PositionHeld, MapHeld, ThingPlaceMode.Near);

            base.Destroy(mode);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            List<Gizmo> gizmos = new List<Gizmo>(base.GetGizmos());

            if(pawnToPrint != null)
                gizmos.Insert(0, new Gizmo_PrinterPawnInfo(this));

            if (printerStatus != CrafterStatus.Finished)
                gizmos.Insert(0, new Gizmo_TogglePrinting(this));

            if(DebugSettings.godMode && pawnToPrint != null)
            {
                gizmos.Insert(0, new Command_Action()
                {
                    defaultLabel = "DEBUG: Finish crafting.",
                    defaultDesc = "Finishes crafting the pawn.",
                    action = delegate ()
                    {
                        printerStatus = CrafterStatus.Finished;
                    }
                });
            }

            return gizmos;
        }

        public override string GetInspectString()
        {
            if (ParentHolder != null && !(ParentHolder is Map))
                return base.GetInspectString();

            StringBuilder builder = new StringBuilder(base.GetInspectString());

            builder.AppendLine();
            builder.AppendLine(printerProperties.crafterStatusText.Translate((printerProperties.crafterStatusEnumText + (int)printerStatus).Translate()));

            if (printerStatus == CrafterStatus.Crafting)
            {
                builder.AppendLine(printerProperties.crafterProgressText.Translate(((float)((float)(printerProperties.ticksToCraft + extraTimeCost) - printingTicksLeft) / (float)(printerProperties.ticksToCraft + extraTimeCost)).ToStringPercent()));
            }

            if (printerStatus == CrafterStatus.Filling)
            {
                bool needsFulfilled = true;

                builder.Append(FormatIngredientCosts(out needsFulfilled, orderProcessor.requestedItems));

                if (!needsFulfilled)
                    builder.AppendLine();
            }

            if (ingredients.Count > 0)
                builder.Append(printerProperties.crafterMaterialsText.Translate() + " ");
            foreach (Thing item in ingredients)
            {
                builder.Append(item.LabelCap + "; ");
            }

            return builder.ToString().TrimEndNewlines();
        }

        public string FormatIngredientCosts(out bool needsFulfilled, IEnumerable<ThingOrderRequest> requestedItems, bool deductCosts = true)
        {
            StringBuilder builder = new StringBuilder();
            needsFulfilled = true;

            foreach (ThingOrderRequest thingOrderRequest in requestedItems)
            {
                if (thingOrderRequest.nutrition)
                {
                    float totalNutrition = CountNutrition();

                    if(deductCosts)
                    {
                        float nutritionDifference = thingOrderRequest.amount - totalNutrition;
                        if (nutritionDifference > 0f)
                        {
                            builder.Append(printerProperties.crafterMaterialNeedText.Translate((nutritionDifference), printerProperties.crafterNutritionText.Translate()) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        builder.Append(printerProperties.crafterMaterialNeedText.Translate((thingOrderRequest.amount), printerProperties.crafterNutritionText.Translate()) + " ");
                    }
                }
                else
                {
                    int itemCount = ingredients.TotalStackCountOfDef(thingOrderRequest.thingDef);
                    if (deductCosts)
                    {
                        if (itemCount < thingOrderRequest.amount)
                        {
                            builder.Append(printerProperties.crafterMaterialNeedText.Translate((thingOrderRequest.amount - itemCount), thingOrderRequest.thingDef.LabelCap) + " ");
                            needsFulfilled = false;
                        }
                    }
                    else
                    {
                        builder.Append(printerProperties.crafterMaterialNeedText.Translate((thingOrderRequest.amount), thingOrderRequest.thingDef.LabelCap) + " ");
                    }
                }
            }
            
            return builder.ToString();
        }

        public bool ReadyToPrint()
        {
            return printerStatus == CrafterStatus.Filling && orderProcessor.PendingRequests() == null;
        }

        public void InitiatePawnCrafting()
        {
            //Open Android Customization window.
            Find.WindowStack.Add(new CustomizeAndroidWindow(this));
        }

        public void StartPrinting()
        {
            //Setup printing procedure
            if(printerProperties == null)
            {
                printingTicksLeft = GenDate.TicksPerDay;
                nextResourceTick = GenDate.TicksPerHour;
            }
            else
            {
                printingTicksLeft = printerProperties.ticksToCraft + extraTimeCost;
                nextResourceTick = printerProperties.resourceTick;
            }

            printerStatus = CrafterStatus.Crafting;
        }

        public void StopPawnCrafting()
        {
            //Reset printer status.
            printerStatus = CrafterStatus.Idle;

            if(pawnToPrint != null)
                pawnToPrint.Destroy();
            pawnToPrint = null;

            //Eject unused materials.
            ingredients.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
        }

        public override void Tick()
        {
            base.Tick();

            AdjustPowerNeed();

            if (!powerComp.PowerOn && soundSustainer != null && !soundSustainer.Ended)
                soundSustainer.End();

            if (flickableComp == null || (flickableComp != null && flickableComp.SwitchIsOn))
            {
                //State machine
                switch (printerStatus)
                {
                    case CrafterStatus.Filling:
                        {
                            //Emit smoke
                            if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 300 == 0)
                            {
                                FleckMaker.ThrowSmoke(Position.ToVector3(), Map, 1f);
                            }

                            //If we aren't being filled, then start.
                            var pendingRequests = orderProcessor.PendingRequests();
                            bool startPrinting = pendingRequests == null;
                            if (pendingRequests != null && pendingRequests.Count() == 0)
                                startPrinting = true;

                            if (startPrinting)
                            {
                                //Initiate printing phase.
                                StartPrinting();
                            }
                        }
                        break;

                    case CrafterStatus.Crafting:
                        {
                            if (powerComp.PowerOn)
                            {
                                //Emit smoke
                                if (Current.Game.tickManager.TicksGame % 100 == 0)
                                {
                                    FleckMaker.ThrowSmoke(Position.ToVector3(), Map, 1.33f);
                                }

                                //Visual effects
                                if(Current.Game.tickManager.TicksGame % 250 == 0)
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        FleckMaker.ThrowMicroSparks(Position.ToVector3() + new Vector3(Rand.Range(-1, 1), 0f, Rand.Range(-1, 1)), Map);
                                    }
                                }

                                //Sound effect
                                if (soundSustainer == null || soundSustainer.Ended)
                                {
                                    SoundDef soundDef = printerProperties.craftingSound;
                                    if (soundDef != null && soundDef.sustain)
                                    {
                                        SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                                        soundSustainer = soundDef.TrySpawnSustainer(info);
                                    }
                                }

                                if (soundSustainer != null && !soundSustainer.Ended)
                                    soundSustainer.Maintain();

                                //Periodically use resources.
                                nextResourceTick--;

                                if (nextResourceTick <= 0)
                                {
                                    nextResourceTick = printerProperties.resourceTick;

                                    //Deduct resources from each category.
                                    foreach (ThingOrderRequest thingOrderRequest in orderProcessor.requestedItems)
                                    {
                                        if (thingOrderRequest.nutrition)
                                        {
                                            //Food
                                            if (CountNutrition() > 0f)
                                            {
                                                //Grab first stack of Nutrition.
                                                Thing item = ingredients.First(thing => thing.def.IsIngestible);

                                                if (item != null)
                                                {
                                                    int resourceTickAmount = (int)Math.Ceiling((thingOrderRequest.amount / ((double)(printerProperties.ticksToCraft + extraTimeCost) / printerProperties.resourceTick)));

                                                    int amount = Math.Min(resourceTickAmount, item.stackCount);
                                                    Thing outThing = null;

                                                    Corpse outCorpse = item as Corpse;
                                                    if (outCorpse != null)
                                                    {
                                                        if (outCorpse.IsDessicated())
                                                        {
                                                            //If rotten, just drop it.
                                                            ingredients.TryDrop(outCorpse, InteractionCell, Map, ThingPlaceMode.Near, 1, out outThing);
                                                        }
                                                        else
                                                        {
                                                            //Not rotten, dump all equipment.
                                                            ingredients.TryDrop(outCorpse, InteractionCell, Map, ThingPlaceMode.Near, 1, out outThing);
                                                            outCorpse.InnerPawn?.equipment?.DropAllEquipment(InteractionCell, false);
                                                            outCorpse.InnerPawn?.apparel?.DropAll(InteractionCell, false);

                                                            item.Destroy();
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Thing takenItem = ingredients.Take(item, amount);
                                                        takenItem.Destroy();
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            //Item
                                            if (ingredients.Any(thing => thing.def == thingOrderRequest.thingDef))
                                            {
                                                //Grab first stack of Plasteel.
                                                Thing item = ingredients.First(thing => thing.def == thingOrderRequest.thingDef);

                                                if (item != null)
                                                {
                                                    int resourceTickAmount = (int)Math.Ceiling((thingOrderRequest.amount / ((float)(printerProperties.ticksToCraft + extraTimeCost) / printerProperties.resourceTick)));

                                                    int amount = Math.Min(resourceTickAmount, item.stackCount);
                                                    Thing takenItem = ingredients.Take(item, amount);

                                                    takenItem.Destroy();
                                                }
                                            }
                                        }
                                    }
                                }

                                //Are we done yet?
                                if (printingTicksLeft > 0)
                                    printingTicksLeft--;
                                else
                                    printerStatus = CrafterStatus.Finished;
                            }
                        }
                        break;

                    case CrafterStatus.Finished:
                        {
                            if (pawnToPrint != null)
                            {
                                //Clear remaining materials.
                                ingredients.ClearAndDestroyContents();

                                //Add effects
                                FilthMaker.TryMakeFilth(InteractionCell, Map, RimWorld.ThingDefOf.Filth_Slime, 5);

                                //Spawn
                                GenSpawn.Spawn(pawnToPrint, InteractionCell, Map);
                                pawnToPrint.health.AddHediff(RimWorld.HediffDefOf.CryptosleepSickness);
                                pawnToPrint.needs.mood.thoughts.memories.TryGainMemory(NeedsDefOf.ChJAndroidSpawned);

                                //Make and send letter.
                                ChoiceLetter letter = LetterMaker.MakeLetter("AndroidPrintedLetterLabel".Translate(pawnToPrint.Name.ToStringShort), "AndroidPrintedLetterDescription".Translate(pawnToPrint.Name.ToStringFull), LetterDefOf.PositiveEvent, pawnToPrint);
                                Find.LetterStack.ReceiveLetter(letter);

                                //Reset
                                pawnToPrint = null;
                                printerStatus = CrafterStatus.Idle;
                                extraTimeCost = 0;
                                orderProcessor.requestedItems.Clear();
                            }
                        }
                        break;

                    default:
                        {
                            if (soundSustainer != null && !soundSustainer.Ended)
                                soundSustainer.End();
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adjusts the required power depending on the state of the printer.
        /// </summary>
        public void AdjustPowerNeed()
        {
            if(flickableComp == null || (flickableComp != null && flickableComp.SwitchIsOn))
            {
                if (printerStatus == CrafterStatus.Crafting)
                {
                    powerComp.PowerOutput = -powerComp.Props.basePowerConsumption;
                }
                else
                {
                    powerComp.PowerOutput = -powerComp.Props.basePowerConsumption * 0.1f;
                }
            }
            else
            {
                powerComp.PowerOutput = 0f;
            }
        }

        /// <summary>
        /// Counts all available nutrition in the printer.
        /// </summary>
        /// <returns>Total nutrition.</returns>
        public float CountNutrition()
        {
            float totalNutrition = 0f;

            //Count nutrition.
            foreach (Thing item in ingredients)
            {
                Corpse corpse = item as Corpse;
                if (corpse != null)
                {
                    if(!corpse.IsDessicated())
                        totalNutrition += FoodUtility.GetBodyPartNutrition(corpse, corpse.InnerPawn.RaceProps.body.corePart);
                }
                else
                {
                    if (item.def.IsIngestible)
                        totalNutrition += (item.def?.ingestible.CachedNutrition ?? 0.05f) * item.stackCount;
}
            }

            return totalNutrition;
        }
        /*public float CountNutrition()
        {
            float totalNutrition = 0f;

            //Count nutrition.
            foreach (Thing item in ingredients)
            {
                Corpse corpse = item as Corpse;
                if(corpse != null)
                {
                    totalNutrition += FoodUtility.GetBodyPartNutrition(corpse.InnerPawn, corpse.InnerPawn.RaceProps.body.corePart);
                }
                else
                {
                    if (item.def.IsIngestible)
                        totalNutrition += (item.def?.ingestible.nutrition ?? 0.05f) * item.stackCount;
                }
            }

            return totalNutrition;
        }*/

        public StorageSettings GetStoreSettings()
        {
            return inputSettings;
        }

        public StorageSettings GetParentStoreSettings()
        {
            return def.building.fixedStorageSettings;
        }

        public Pawn PawnBeingCrafted()
        {
            return pawnToPrint;
        }

        public CrafterStatus PawnCrafterStatus()
        {
            return printerStatus;
        }
    }
}
