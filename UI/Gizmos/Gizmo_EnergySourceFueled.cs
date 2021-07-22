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
    /// Gizmo showing off the status of a worn fueled energy source.
    /// </summary>
    [StaticConstructorOnStartup]
    public class Gizmo_EnergySourceFueled : Gizmo
    {
        public Apparel apparel;
        public EnergySource_Fueled fueledEnergySource;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(1.0f, 0.5f, 0.0f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        public Gizmo_EnergySourceFueled()
        {
            this.order = -100f;
        }

        // Token: 0x0600267C RID: 9852 RVA: 0x0014A874 File Offset: 0x00148C74
        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        // Token: 0x0600267D RID: 9853 RVA: 0x0014A890 File Offset: 0x00148C90
        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(984689, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect = overRect.AtZero().ContractedBy(6f);
                Rect rect2 = rect;
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, apparel.LabelCap);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = 1f - fueledEnergySource.MissingFuelPercentage;
                Widgets.FillableBar(rect3, fillPercent, FullShieldBarTex, EmptyShieldBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, fueledEnergySource.fuelAmountLoaded.ToString("F0") + " / " + fueledEnergySource.EnergyProps.maxFuelAmount.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
