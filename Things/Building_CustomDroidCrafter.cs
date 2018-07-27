using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Uses the default pawn generator and do a few alterations.
    /// </summary>
    public class Building_CustomDroidCrafter : Building_DroidCrafter
    {
        public override void InitiatePawnCrafting()
        {
            pawnBeingCrafted = DroidUtility.MakeCustomDroid(printerProperties.pawnKind, Faction);

            crafterStatus = CrafterStatus.Filling;
        }
    }
}
