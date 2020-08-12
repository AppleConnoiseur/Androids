using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Androids
{
    public static class AndroidUtility
    {
        public static void Androidify(Pawn pawn)
        {
            ThingDef_AlienRace alien = ThingDefOf.ChjAndroid as ThingDef_AlienRace;
            pawn.story.hairColor = alien.alienRace.generalSettings.alienPartGenerator.colorChannels.FirstOrDefault(channel => channel.name == "hair").first.NewRandomizedColor();
            AlienComp alienComp = pawn.TryGetComp<AlienComp>();
            if (alienComp != null)
            {
                alienComp.ColorChannels["skin"].first = alien.alienRace.generalSettings.alienPartGenerator.colorChannels.FirstOrDefault(channel => channel.name == "skin").first.NewRandomizedColor();
            }
            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();

            //Add Android Hediff.
            pawn.health.AddHediff(HediffDefOf.ChjAndroidLike);

            //Remove old wounds and bad birthday related ones.
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs.ToList();
            foreach (Hediff hediff in hediffs)
            {
                if (hediff is Hediff_Injury injury && injury.IsPermanent())
                {
                    pawn.health.hediffSet.hediffs.Remove(injury);
                    injury.PostRemoved();
                    pawn.health.Notify_HediffChanged(null);
                }
                else if (
                   pawn.def.race.hediffGiverSets.Any(
                       setDef => setDef.hediffGivers.Any(
                           hediffGiver => hediffGiver is HediffGiver_Birthday birthday && birthday.hediff == hediff.def &&
                           (birthday.hediff.stages?.Any(
                               stage =>
                               (stage.capMods?.Any(cap => cap.offset < 0f) ?? false) ||
                               stage.lifeThreatening ||
                               stage.partEfficiencyOffset < 0f ||
                               (stage.statOffsets?.Any(stat => stat.value < 0f) ?? false) ||
                               stage.painOffset > 0f ||
                               stage.painFactor > 1f) ?? false))))
                {
                    //Log.Message("Bad Birthday Hediff Removed: " + hediff.LabelCap);
                    pawn.health.hediffSet.hediffs.Remove(hediff);
                    hediff.PostRemoved();
                    pawn.health.Notify_HediffChanged(null);
                }
            }
        }
    }
}
