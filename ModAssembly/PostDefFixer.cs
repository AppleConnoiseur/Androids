using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            //Fix Droid recipes.
            foreach (RecipeDef recipe in DefDatabase<RecipeDef>.AllDefs)
            {
                if(recipe.defName.StartsWith("Administer_"))
                    recipe.recipeUsers.RemoveAll(thingDef => thingDef.HasModExtension<MechanicalPawnProperties>());
            }
        }
    }
}
