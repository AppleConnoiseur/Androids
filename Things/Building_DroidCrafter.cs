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
    /// Specialized building for crafting Droids.
    /// </summary>
    public class Building_DroidCrafter : Building_PawnCrafter
    {
        /// <summary>
        /// Sustained sound.
        /// </summary>
        Sustainer soundSustainer;

        //Repeat crafting stuff.
        public DroidCraftingDef lastDef;
        public bool repeatLastPawn = false;

        public override void InitiatePawnCrafting()
        {
            //Bring up Float Menu
            //FloatMenuUtility.
            List<FloatMenuOption> floatMenuOptions = new List<FloatMenuOption>();
            foreach(DroidCraftingDef def in DefDatabase<DroidCraftingDef>.AllDefs.OrderBy(def => def.orderID))
            {
                bool disabled = false;
                string labelText = "";
                if (def.requiredResearch != null && !def.requiredResearch.IsFinished)
                {
                    disabled = true;
                }

                if(disabled)
                {
                    labelText = "AndroidDroidCrafterPawnNeedResearch".Translate(def.label, def.requiredResearch.LabelCap);
                }
                else
                {
                    labelText = "AndroidDroidCrafterPawnMake".Translate(def.label);
                }

                FloatMenuOption option = new FloatMenuOption(labelText, 
                delegate() 
                {
                    //Stuff
                    if(!disabled)
                    {
                        lastDef = def;
                        MakePawnAndInitCrafting(def);
                    }
                }
                );

                option.Disabled = disabled;
                floatMenuOptions.Add(option);
            }

            if(floatMenuOptions.Count > 0)
            {
                FloatMenu floatMenu = new FloatMenu(floatMenuOptions);
                Find.WindowStack.Add(floatMenu);
            }

            /*pawnBeingCrafted = 
                DroidUtility.MakeDroidTemplate(
                    printerProperties.pawnKind.race, 
                    printerProperties.pawnKind, Faction, 
                    Map, 
                    printerProperties.skills, 
                    printerProperties.defaultSkillLevel);

            crafterStatus = CrafterStatus.Filling;*/
        }

        public void MakePawnAndInitCrafting(DroidCraftingDef def)
        {
            //Update costs.
            orderProcessor.requestedItems.Clear();

            foreach (ThingOrderRequest cost in def.costList)
            {
                ThingOrderRequest costCopy = new ThingOrderRequest();
                costCopy.nutrition = cost.nutrition;
                costCopy.thingDef = cost.thingDef;
                costCopy.amount = cost.amount;

                orderProcessor.requestedItems.Add(costCopy);
            }

            craftingTime = def.timeCost;

            //Apply template.
            if (def.useDroidCreator)
            {
                pawnBeingCrafted = DroidUtility.MakeDroidTemplate(def.pawnKind.race, def.pawnKind, Faction, Map);
            }
            else
            {
                pawnBeingCrafted = PawnGenerator.GeneratePawn(def.pawnKind, Faction);
            }

            crafterStatus = CrafterStatus.Filling;
        }

        public override void ExtraCrafterTickAction()
        {
            if(!powerComp.PowerOn && soundSustainer != null && !soundSustainer.Ended)
                soundSustainer.End();

            //Make construction effects
            switch (crafterStatus)
            {
                case CrafterStatus.Filling:
                    //Emit smoke
                    if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 300 == 0)
                    {
                        MoteMaker.ThrowSmoke(Position.ToVector3(), Map, 1f);
                    }
                    break;

                case CrafterStatus.Crafting:
                    //Emit smoke
                    if (powerComp.PowerOn && Current.Game.tickManager.TicksGame % 100 == 0)
                    {
                        for(int i = 0; i < 5; i++)
                            MoteMaker.ThrowMicroSparks(Position.ToVector3() + new Vector3(Rand.Range(-1, 1), 0f, Rand.Range(-1, 1)), Map);
                        for (int i = 0; i < 3; i++)
                            MoteMaker.ThrowSmoke(Position.ToVector3() + new Vector3(Rand.Range(-1f, 1f), 0f, Rand.Range(-1f, 1f)), Map, Rand.Range(0.5f, 0.75f));
                        MoteMaker.ThrowHeatGlow(Position, Map, 1f);

                        if (soundSustainer == null || soundSustainer.Ended)
                        {
                            SoundDef soundDef = printerProperties.craftingSound;
                            if (soundDef != null && soundDef.sustain)
                            {
                                SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
                                soundSustainer = soundDef.TrySpawnSustainer(info);
                            }
                        }
                    }
                    if (soundSustainer != null && !soundSustainer.Ended)
                        soundSustainer.Maintain();
                    break;

                default:
                    {
                        if (soundSustainer != null && !soundSustainer.Ended)
                            soundSustainer.End();
                    }
                    break;
            }
        }

        public override void FinishAction()
        {
            orderProcessor.requestedItems.Clear();

            if(repeatLastPawn && lastDef != null)
            {
                MakePawnAndInitCrafting(lastDef);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Deep.Look(ref orderProcessor, "orderProcessor", ingredients, inputSettings);
            Scribe_Defs.Look(ref lastDef, "lastDef");
            Scribe_Values.Look(ref repeatLastPawn, "repeatLastPawn");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);

            if(!respawningAfterLoad)
            {
                orderProcessor = new ThingOrderProcessor(ingredients, inputSettings);
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
                yield return gizmo;

            yield return new Command_Toggle()
            {
                defaultLabel = "AndroidGizmoRepeatPawnCraftingLabel".Translate(),
                defaultDesc = "AndroidGizmoRepeatPawnCraftingDescription".Translate(),
                icon = ContentFinder<Texture2D>.Get("ui/designators/PlanOn", true),
                isActive = () => repeatLastPawn,
                toggleAction = delegate()
                {
                    repeatLastPawn = !repeatLastPawn;
                }
            };
        }
    }
}
