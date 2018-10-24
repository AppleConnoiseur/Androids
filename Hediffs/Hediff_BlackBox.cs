using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    /// <summary>
    /// Creates a nuclear esque explosion upon death.
    /// </summary>
    public class Hediff_BlackBox : HediffWithComps, IExtraGizmos
    {
        public override void Notify_PawnDied()
        {
            base.Notify_PawnDied();

            if (pawn.Corpse != null)
            {
                GenExplosion.DoExplosion(pawn.Corpse.Position, pawn.Corpse.Map, 50f, RimWorld.DamageDefOf.Bomb, null, 500, 15);
            }
        }

        public IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "AndroidGizmoDetonateBlackBoxLabel".Translate(),
                defaultDesc =  "AndroidGizmoDetonateBlackBoxDescription".Translate(),
                icon = ContentFinder<Texture2D>.Get("Icons/Upgrades/BlackBoxIcon", true),
                order = -97,
                action = delegate ()
                {
                    Dialog_MessageBox dialog = 
                        Dialog_MessageBox.CreateConfirmation(
                            "AndroidSelfDetonationConfirmationDialogText".Translate(pawn.Name.ToStringFull), 
                            () => pawn.Kill(null), 
                            true, 
                            "AndroidGizmoSelfDetonationLabel".Translate());
                    Find.WindowStack.Add(dialog);
                }
            };
        }

        public override string TipStringExtra => "AndroidHediffBlackBox".Translate();
    }
}
