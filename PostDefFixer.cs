using AlienRace;
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
    /// Fixes so recipes that administer to Pawns can't be done on mechanical Pawns.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class PostDefFixer
    {
        static PostDefFixer()
        {
            Log.Message("Androids: Fixing surgery recipes for Droids.");
            //Fix Droid recipes.
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                if(recipe.defName.StartsWith("Administer_"))
                {
                    int removedNum = recipe.recipeUsers.RemoveAll(thingDef => thingDef.HasModExtension<MechanicalPawnProperties>());
                    if (Prefs.LogVerbose && removedNum > 0)
                    {
                        Log.Message("Androids: Removed '" + removedNum + "' recipes for Droids.");
                    }
                }
            }

            Log.Message("Androids: Fixing belts whitelist for AlienRace.ThingDef_AlienRace with defName='ChjBattleDroid'.");
            //Fix Battle droid belts.
            ThingDef_AlienRace ChjBattleDroid = (ThingDef_AlienRace)ThingDef.Named("ChjBattleDroid");
            {
                List<ThingDef> whitelist = ChjBattleDroid.alienRace.raceRestriction.whiteApparelList;
                foreach(ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    if(thingDef.IsApparel && 
                        (thingDef.apparel.bodyPartGroups != null && thingDef.apparel.bodyPartGroups.Count == 1 && thingDef.apparel.bodyPartGroups.First().defName == "Waist") &&
                        (thingDef.apparel.layers != null && thingDef.apparel.layers.Count == 1 && thingDef.apparel.layers.First().defName == "Belt") &&
                        !whitelist.Any(item => item.defName == thingDef.defName))
                    {
                        if(Prefs.LogVerbose)
                        {
                            Log.Message("Androids: Belt found and added: " + thingDef.defName);
                        }
                        whitelist.Add(thingDef);
                    }
                }
            }
        }
    }
}
