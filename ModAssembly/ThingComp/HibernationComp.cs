using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// Marks a Thing as a location to hibernate at for MachineLikes.
    /// </summary>
    public class HibernationComp : ThingComp
    {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if(selPawn.TryGetComp<EnergyTrackerComp>() is EnergyTrackerComp energyTracker && energyTracker.EnergyProperties is CompProperties_EnergyTracker props && props.canHibernate)
            {
                if(selPawn.CanReserveAndReach(parent, PathEndMode.OnCell, Danger.Deadly))
                {
                    if(parent.TryGetComp<CompPowerTrader>() is CompPowerTrader power)
                    {
                        if(power.PowerOn)
                        {
                            FloatMenuOption option = new FloatMenuOption("AndroidMachinelikeHibernate".Translate(selPawn.Name.ToStringShort),
                            delegate ()
                            {
                                //Give hibernation Job.
                                selPawn.jobs.TryTakeOrderedJob(new Job(props.hibernationJob, parent), JobTag.Misc);
                            });
                            yield return option;
                        }
                        else
                        {
                            FloatMenuOption option = new FloatMenuOption("AndroidMachinelikeHibernateFailNoPower".Translate(selPawn.Name.ToStringShort, parent.LabelCap), null);
                            option.Disabled = true;
                            yield return option;
                        }
                    }
                    else
                    {
                        FloatMenuOption option = new FloatMenuOption("AndroidMachinelikeHibernate".Translate(selPawn.Name.ToStringShort),
                        delegate ()
                        {
                            //Give hibernation Job.
                            selPawn.jobs.TryTakeOrderedJob(new Job(props.hibernationJob, parent), JobTag.Misc);
                        });
                        yield return option;
                    }
                }
                else
                {
                    FloatMenuOption option = new FloatMenuOption("AndroidMachinelikeHibernateFailReserveOrReach".Translate(selPawn.Name.ToStringShort, parent.LabelCap), null);
                    option.Disabled = true;
                    yield return option;
                }
            }
            else
            {
                FloatMenuOption option = new FloatMenuOption("AndroidMachinelikeHibernateFail".Translate(selPawn.Name.ToStringShort), null);
                option.Disabled = true;
                yield return option;
            }
        }
    }
}
