using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Helps in dealing with races.
    /// </summary>
    public static class RaceUtility
    {
        private static List<PawnKindDef> alienRaceKindsint = new List<PawnKindDef>();
        private static bool alienRaceKindSearchDoneint = false;
        private static bool alienRacesFoundint = false;

        public static bool AlienRacesExist
        {
            get
            {
                return alienRacesFoundint;
            }
        }

        public static IEnumerable<PawnKindDef> AlienRaceKinds
        {
            get
            {
                if(!alienRaceKindSearchDoneint)
                {
                    //Log.Message("AlienRaceKinds: Setting up");
                    foreach (ThingDef_AlienRace alienDef in DefDatabase<ThingDef_AlienRace>.AllDefs)
                    {
                        //Log.Message("AlienRaceKinds: Picking best PawnkindDef for: " + alienDef.defName);
                        //Cross reference with pawnkinds and pick the first one.
                        PawnKindDef bestKindDef = DefDatabase<PawnKindDef>.AllDefs.FirstOrDefault(def => def.race == alienDef);
                        if (bestKindDef != null)
                        {
                            //Log.Message("AlienRaceKinds: Found '"+ bestKindDef.defName + "' for: " + alienDef.defName);
                            alienRaceKindsint.Add(bestKindDef);
                        }
                        /*else
                        {
                            Log.Message("AlienRaceKinds: Found no PawnkindDef for: " + alienDef.defName);
                        }*/
                    }

                    //Remove Human from the list. (Not intended to be printed)
                    alienRaceKindsint.RemoveAll(def => def.race.defName == "Human");

                    //Remove Droids from the list.
                    alienRaceKindsint.RemoveAll(def => def.race.HasModExtension<MechanicalPawnProperties>());

                    //Log.Message("AlienRaceKinds: Removing disallowed pawns.");
                    //Look through all ThingDefs for alien races not to allow.
                    foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                    {
                        if(thingDef.GetModExtension<PawnCrafterProperties>() is PawnCrafterProperties properties)
                        {
                            //Log.Message("AlienRaceKinds: Removing Pawnkinds from: " + thingDef.defName);
                            foreach (ThingDef raceDef in properties.disabledRaces)
                            {
                                alienRaceKindsint.RemoveAll(def => def.race == raceDef);
                            }
                        }
                    }

                    //If we got more than just the Android left then we found other alien races.
                    if (alienRaceKindsint.Count > 1)
                        alienRacesFoundint = true;

                    alienRaceKindSearchDoneint = true;
                }

                return alienRaceKindsint;
            }
        }

        public static bool IsAndroid(this Pawn pawn)
        {
            return pawn.def == Androids.ThingDefOf.ChjAndroid || pawn.health.hediffSet.HasHediff(HediffDefOf.ChjAndroidLike);
        }
    }
}
