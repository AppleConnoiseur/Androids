using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace Androids
{
    /// <summary>
    /// Safely disassembles Droids.
    /// </summary>
    public class Recipe_Disassemble : RecipeWorker
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {
            //If damaged, have option to apply.
            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                yield return null;
            }

            yield break;
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            //Set the power Need and EnergyTrackerComp to 0.
            Need_Energy needEnergy = pawn.needs.TryGetNeed<Need_Energy>();
            EnergyTrackerComp energyTrackerComp = pawn.TryGetComp<EnergyTrackerComp>();

            if(needEnergy != null)
            {
                needEnergy.CurLevelPercentage = 0f;
            }

            if(energyTrackerComp != null)
            {
                energyTrackerComp.energy = 0f;
            }

            //Spawn extra butcher products.
            ButcherUtility.SpawnDrops(pawn, pawn.Position, pawn.Map);

            pawn.Kill(null);
        }
    }
}
