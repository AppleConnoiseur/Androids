using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Lets the player to turn on and off printing.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_TogglePrinting : Command
    {
        public static Texture2D startIcon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
        public static Texture2D stopIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel", true);

        public IPawnCrafter printer;

        public string labelStart = "AndroidGizmoTogglePrintingStartLabel";
        public string descriptionStart = "AndroidGizmoTogglePrintingStartDescription";

        public string labelStop = "AndroidGizmoTogglePrintingStopLabel";
        public string descriptionStop = "AndroidGizmoTogglePrintingStopDescription";

        static Gizmo_TogglePrinting()
        {

        }

        public Gizmo_TogglePrinting(IPawnCrafter printer)
        {
            this.printer = printer;

            if (printer.PawnCrafterStatus() == CrafterStatus.Idle)
            {
                //Start
                defaultLabel = labelStart.Translate();
                defaultDesc = descriptionStart.Translate();
                icon = startIcon;
            }
            else if (printer.PawnCrafterStatus() == CrafterStatus.Crafting || printer.PawnCrafterStatus() == CrafterStatus.Filling)
            {
                //Stop
                defaultLabel = labelStop.Translate();
                defaultDesc = descriptionStop.Translate();
                icon = stopIcon;
            }
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            if (printer.PawnCrafterStatus() == CrafterStatus.Idle)
            {
                //Start
                printer.InitiatePawnCrafting();
            }
            else
            {
                //Stop
                printer.StopPawnCrafting();
            }
        }
    }
}
