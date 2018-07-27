using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// The tab for the Android Printer to pick what Nutrition sources to use,
    /// </summary>
    public class ITab_AndroidPrinter : ITab
    {
        private const float TopAreaHeight = 35f;

        private Vector2 scrollPosition = default(Vector2);

        private static readonly Vector2 WinSize = new Vector2(300f, 480f);

        private IStoreSettingsParent SelStoreSettingsParent
        {
            get
            {
                return (IStoreSettingsParent)base.SelObject;
            }
        }

        public override bool IsVisible
        {
            get
            {
                return this.SelStoreSettingsParent.StorageTabVisible;
            }
        }

        public ITab_AndroidPrinter()
        {
            size = WinSize;
            labelKey = "AndroidTab";
        }

        protected override void FillTab()
        {
            IStoreSettingsParent selStoreSettingsParent = this.SelStoreSettingsParent;
            StorageSettings settings = selStoreSettingsParent.GetStoreSettings();
            Rect position = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
            GUI.BeginGroup(position);

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(position);
            titleRect.height = 32f;
            Widgets.Label(titleRect, "AndroidTabTitle".Translate());

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.UpperLeft;

            ThingFilter parentFilter = null;
            if (selStoreSettingsParent.GetParentStoreSettings() != null)
            {
                parentFilter = selStoreSettingsParent.GetParentStoreSettings().filter;
            }
            Rect rect2 = new Rect(0f, 40f, position.width, position.height - 40f);
            ThingFilterUI.DoThingFilterConfigWindow(rect2, ref this.scrollPosition, settings.filter, parentFilter, 8, null, null, null);
            PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
            GUI.EndGroup();
        }
    }
}
