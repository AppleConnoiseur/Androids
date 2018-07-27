using Androids.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Makes the Pawn explode with varied degree on death.
    /// </summary>
    public class DeathActionWorker_Android : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse)
        {
            //Log.Message("Inside DeathActionWorker");

            if (!AndroidsModSettings.Instance.androidExplodesOnDeath)
                return;

            //Pawn
            Pawn pawn = corpse.InnerPawn;

            //Try get energy tracker.
            EnergyTrackerComp energy = pawn.TryGetComp<EnergyTrackerComp>();
            
            if (energy != null)
            {
                //Log.Message("EnergyTracker");

                //Make sure we did not die from natural causes. As natural as an Android can be.
                bool shouldBeDeadByNaturalCauses = pawn.health.hediffSet.hediffs.Any(hediff => hediff.CauseDeathNow());

                Hediff overheatingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ChjOverheating);
                //bool deadFromOverheating = overheatingHediff != null ? overheatingHediff.Severity >= 1f : false;

                if (overheatingHediff == null)
                    return;

                //Overheating death is excepted.
                if(overheatingHediff != null || !shouldBeDeadByNaturalCauses)
                {
                    float explosionRadius = overheatingHediff.Severity * AndroidsModSettings.Instance.androidExplosionRadius * energy.energy;

                    //if (deadFromOverheating)
                    //    explosionRadius *= 2;

                    //Scale explosion strength from how much remaining energy we got.
                    if (overheatingHediff != null && explosionRadius >= 1f)
                    {
                        GenExplosion.DoExplosion(corpse.Position, corpse.Map, explosionRadius, RimWorld.DamageDefOf.Bomb, corpse.InnerPawn);
                    }
                }
            }
            else
            {
                //Log.Message("Android Like");

                //Attempt to get the energy need directly.
                if (pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ChjAndroidLike) is Hediff androidLike && androidLike is AndroidLikeHediff androidLikeForReal)
                {
                    //Make sure we did not die from natural causes. As natural as an Android can be.
                    bool shouldBeDeadByNaturalCauses = pawn.health.hediffSet.hediffs.Any(hediff => hediff.CauseDeathNow());

                    Hediff overheatingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ChjOverheating);
                    //bool deadFromOverheating = overheatingHediff != null ? overheatingHediff.Severity >= 1f : false;

                    if (overheatingHediff == null)
                        return;

                    //Overheating death is excepted.
                    if (overheatingHediff != null || !shouldBeDeadByNaturalCauses)
                    {
                        float explosionRadius = overheatingHediff.Severity * AndroidsModSettings.Instance.androidExplosionRadius * androidLikeForReal.energyTracked;

                        //if (deadFromOverheating)
                        //    explosionRadius *= 2;

                        //Scale explosion strength from how much remaining energy we got.
                        if (overheatingHediff != null && explosionRadius >= 1f)
                            GenExplosion.DoExplosion(corpse.Position, corpse.Map, explosionRadius, RimWorld.DamageDefOf.Bomb, corpse.InnerPawn);
                    }
                    return;
                }
                else
                {
                    Log.Warning("Androids.DeathActionWorker_Android: EnergyTrackerComp is null at or is not Android Like either: " + corpse.ThingID);
                    return;
                }
            }
        }
    }
}
