using Androids.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Tracks stored energy for use in case of death.
    /// </summary>
    public class EnergyTrackerComp : ThingComp
    {
        /// <summary>
        /// Last known energy amount.
        /// </summary>
        public float energy;

        /// <summary>
        /// Convenience access.
        /// </summary>
        Pawn pawn;

        /// <summary>
        /// Energy need for the Pawn.
        /// </summary>
        Need_Energy energyNeed;

        public CompProperties_EnergyTracker EnergyProperties
        {
            get
            {
                return props as CompProperties_EnergyTracker;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            pawn = parent as Pawn;

            if(pawn != null)
            {
                energyNeed = pawn.needs.TryGetNeed<Need_Energy>();
            }
        }

        public override void CompTick()
        {
            //Keep track of our Need.
            if (energyNeed != null)
                energy = energyNeed.CurLevel;
        }

        public override void PostExposeData()
        {
            //Log.Message("EnergyTrackerComp is being exposed! energy=" + energy);
            Scribe_Values.Look(ref energy, "energy");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            //Debug stuff
            if (Prefs.DevMode && DebugSettings.godMode)
            {
                {
                    Command_Action gizmo = new Command_Action();
                    gizmo.defaultLabel = "DEBUG: Set Energy to 100%";
                    gizmo.action = () => energyNeed.CurLevelPercentage = 1.0f;
                    yield return gizmo;
                }

                {
                    Command_Action gizmo = new Command_Action();
                    gizmo.defaultLabel = "DEBUG: Set Energy to 50%";
                    gizmo.action = () => energyNeed.CurLevelPercentage = 0.5f;
                    yield return gizmo;
                }

                {
                    Command_Action gizmo = new Command_Action();
                    gizmo.defaultLabel = "DEBUG: Set Energy to 20%";
                    gizmo.action = () => energyNeed.CurLevelPercentage = 0.2f;
                    yield return gizmo;
                }
            }

            //Add self detonation button to mechanical pawns.
            Pawn pawn = parent as Pawn;
            if(AndroidsModSettings.Instance.androidExplodesOnDeath && pawn != null && pawn.IsColonistPlayerControlled && pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                {
                    Command_Action gizmo = new Command_Action();
                    gizmo.defaultLabel = "AndroidGizmoSelfDetonationLabel".Translate();
                    gizmo.defaultDesc = "AndroidGizmoSelfDetonationDescription".Translate();
                    gizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/Detonate", true);
                    gizmo.action = delegate()
                    {
                        if(AndroidsModSettings.Instance.droidDetonationConfirmation)
                        {
                            Dialog_MessageBox dialog = Dialog_MessageBox.CreateConfirmation("AndroidSelfDetonationConfirmationDialogText".Translate(pawn.Name), () => HealthUtility.AdjustSeverity(pawn, HediffDefOf.ChjOverheating, 1.1f), true, "AndroidGizmoSelfDetonationLabel".Translate());
                            Find.WindowStack.Add(dialog);
                        }
                        else
                        {
                            HealthUtility.AdjustSeverity(pawn, HediffDefOf.ChjOverheating, 1.1f);
                        }
                    };
                    yield return gizmo;
                }
            }

            yield break;
        }
    }
}
