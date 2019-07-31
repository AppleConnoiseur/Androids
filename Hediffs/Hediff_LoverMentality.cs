using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    public class Hediff_LoverMentality : HediffWithComps, IExtraGizmos
    {
        public Pawn loverToChase;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_References.Look(ref loverToChase, "loverToChase");
        }

        public override void Tick()
        {
            base.Tick();

            //Reset lover if they are dead or destroyed.
            if(loverToChase != null)
            {
                if(loverToChase.Dead || loverToChase.Destroyed)
                {
                    SetNewLover(null);
                }
            }
        }

        public void SetNewLover(Pawn newLover)
        {
            if(loverToChase != newLover)
            {
                loverToChase = newLover;
            }
        }

        public IEnumerable<Gizmo> GetGizmosExtra()
        {
            yield return new Command_Action()
            {
                defaultLabel = "AndroidGizmoLoverMentalityLabel".Translate(),
                defaultDesc = "AndroidGizmoLoverMentalityDescription".Translate(),
                icon = ContentFinder<Texture2D>.Get("Icons/Upgrades/love-mystery", true),
                order = -97,
                action = delegate ()
                {
                    //Valid lovers are within the colony.
                    List<FloatMenuOption> options = new List<FloatMenuOption>();

                    foreach(Pawn targetPawn in pawn.Map.mapPawns.FreeColonistsAndPrisoners)
                    {
                        FloatMenuOption option = new FloatMenuOption(targetPawn.LabelCap, delegate()
                        {
                            SetNewLover(targetPawn);
                            MoteMaker.ThrowMetaIcon(pawn.Position, pawn.Map, RimWorld.ThingDefOf.Mote_Heart);
                        });

                        options.Add(option);
                    }

                    {
                        FloatMenuOption option = new FloatMenuOption("AndroidNone".Translate(), delegate ()
                        {
                            SetNewLover(null);
                        });

                        options.Add(option);

                        FloatMenu floatMenu = new FloatMenu(options);
                        Find.WindowStack.Add(floatMenu);
                    }
                }
            };
        }
    }
}
