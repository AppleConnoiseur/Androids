using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// Generic variant of the Android Printer WorkGiver which make pawns attempt to fill up the crafter.
    /// </summary>
    public class WorkGiver_PawnCrafter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(WorkGiverProperties.defToScan);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        private PawnCrafterWorkgiverProperties intWorkGiverProperties = null;

        public PawnCrafterWorkgiverProperties WorkGiverProperties
        {
            get
            {
                if(intWorkGiverProperties == null)
                {
                    intWorkGiverProperties = def.GetModExtension<PawnCrafterWorkgiverProperties>();
                }

                return intWorkGiverProperties;
            }
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_PawnCrafter pawnCrafter = t as Building_PawnCrafter;

            if (pawnCrafter == null || pawnCrafter.crafterStatus != CrafterStatus.Filling)
                return false;

            if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
            {
                return false;
            }

            if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
            {
                return false;
            }

            //Check if there is anything to fill.
            IEnumerable<ThingOrderRequest> potentionalRequests = pawnCrafter.orderProcessor.PendingRequests();
            bool validRequest = false;
            if (potentionalRequests != null)
            {
                foreach (ThingOrderRequest request in potentionalRequests)
                {
                    Thing ingredientThing = FindIngredient(pawn, pawnCrafter, request);
                    if(ingredientThing != null)
                    {
                        validRequest = true;
                        break;
                    }
                }
            }

            return validRequest;
        }

        public override Job JobOnThing(Pawn pawn, Thing crafterThing, bool forced = false)
        {
            Building_PawnCrafter pawnCrafter = crafterThing as Building_PawnCrafter;

            IEnumerable<ThingOrderRequest> potentionalRequests = pawnCrafter.orderProcessor.PendingRequests();

            if (potentionalRequests != null)
            {
                foreach (ThingOrderRequest request in potentionalRequests)
                {
                    Thing ingredientThing = FindIngredient(pawn, pawnCrafter, request);

                    if (ingredientThing != null)
                    {
                        if (request.nutrition)
                        {
                            int nutritionCount = (int)(Math.Ceiling(request.amount / (ingredientThing.def.ingestible.CachedNutrition)));

                            if (nutritionCount > 0)
                                return new Job(WorkGiverProperties.fillJob, ingredientThing, crafterThing)
                                {
                                    count = nutritionCount
                                };
                        }
                        else
                            return new Job(WorkGiverProperties.fillJob, ingredientThing, crafterThing)
                            {
                                count = (int)request.amount
                            };
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to find a appropiate ingredient.
        /// </summary>
        /// <param name="pawn">Pawn to search for.</param>
        /// <param name="androidPrinter">Printer to fill.</param>
        /// <param name="request">Thing order request to fulfill.</param>
        /// <returns>Valid thing if found, otherwise null.</returns>
        private Thing FindIngredient(Pawn pawn, Building_PawnCrafter androidPrinter, ThingOrderRequest request)
        {
            if (request != null)
            {
                Predicate<Thing> extraPredicate = request.ExtraPredicate();
                Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && extraPredicate(x);
                Predicate<Thing> validator = predicate;

                return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, request.Request(), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator, null, 0, -1, false, RegionType.Set_Passable, false);
            }

            return null;
        }
    }
}
