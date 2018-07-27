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
    /// When clicked shows the currently printed pawn.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_PrinterPawnInfo : Command
    {
        /// <summary>
        /// Linked printer.
        /// </summary>
        public IPawnCrafter printer;

        /// <summary>
        /// Placeholder icon for drawing nothing.
        /// </summary>
        public static Texture2D emptyIcon = ContentFinder<Texture2D>.Get("UI/Overlays/ThingLine", true);

        public string description = "AndroidGizmoPrinterAndroidInfoDescription";

        static Gizmo_PrinterPawnInfo()
        {

        }

        public Gizmo_PrinterPawnInfo(IPawnCrafter printer)
        {
            this.printer = printer;

            //Start
            defaultLabel = printer.PawnBeingCrafted().Name.ToStringFull;
            defaultDesc = description.Translate();
            icon = emptyIcon;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth)
        {
            GizmoResult result = base.GizmoOnGUI(topLeft, maxWidth);

            //Custom render.
            float width = GetWidth(maxWidth);
            Rect pawnRect = new Rect(topLeft.x + 10f, topLeft.y, width - 40f, width - 20f);
            Vector2 PawnPortraitSize = new Vector2(width - 20f, width);

            GUI.DrawTexture(new Rect(pawnRect.x, pawnRect.y, PawnPortraitSize.x, PawnPortraitSize.y), PortraitsCache.Get(printer.PawnBeingCrafted(), PawnPortraitSize, default(Vector3), 1f));
            return result;
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);

            Find.WindowStack.Add(new Dialog_InfoCard(printer.PawnBeingCrafted()));
        }
    }
}
