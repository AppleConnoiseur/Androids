using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Works as a Energy source for Androids and Droids in caravans and on the map.
    /// </summary>
    public class EnergySourceComp : ThingComp
    {
        /// <summary>
        /// Properties for this Comp.
        /// </summary>
        public CompProperties_EnergySource EnergyProps
        {
            get
            {
                return props as CompProperties_EnergySource;
            }
        }

        public virtual void RechargeEnergyNeed(Pawn targetPawn)
        {
            Need_Energy energyNeed = targetPawn.needs.TryGetNeed<Need_Energy>();
            if(energyNeed != null)
            {
                if (EnergyProps.isConsumable)
                {
                    float finalEnergyGain = parent.stackCount * EnergyProps.energyWhenConsumed;
                    energyNeed.CurLevel += finalEnergyGain;
                }
                else
                {
                    energyNeed.CurLevel += EnergyProps.passiveEnergyGeneration;
                }
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            Need_Energy energyNeed = selPawn.needs.TryGetNeed<Need_Energy>();
            if (EnergyProps.isConsumable)
            {
                if(energyNeed != null)
                {
                    //Consume for self.
                    int thingCount = (int)Math.Ceiling((energyNeed.MaxLevel - energyNeed.CurLevel) / EnergyProps.energyWhenConsumed);

                    if (thingCount > 0)
                    {
                        FloatMenuOption floatMenuOption = new FloatMenuOption("AndroidConsumeEnergySource".Translate(parent.LabelCap),
                        () => selPawn.jobs.TryTakeOrderedJob(
                            new Verse.AI.Job(JobDefOf.ChJAndroidRechargeEnergyComp, new LocalTargetInfo(parent))
                            {
                                count = thingCount
                            }),
                        MenuOptionPriority.Default,
                        null,
                        parent);

                        yield return floatMenuOption;
                    }
                }
            }
        }
    }
}
