using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace Androids
{
    public class WorkGiver_AndroidPrinter : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(ThingDefOf.ChJAndroidPrinter);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Building_AndroidPrinter androidPrinter = t as Building_AndroidPrinter;

            if (androidPrinter == null || androidPrinter.printerStatus != CrafterStatus.Filling)
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
            IEnumerable<ThingOrderRequest> potentionalRequests = androidPrinter.orderProcessor.PendingRequests();
            bool validRequest = false;
            if (potentionalRequests != null)
            {
                foreach (ThingOrderRequest request in potentionalRequests)
                {
                    Thing ingredientThing = FindIngredient(pawn, androidPrinter, request);
                    if(ingredientThing != null)
                    {
                        validRequest = true;
                        break;
                    }
                }
            }

            return validRequest;
        }

        public override Job JobOnThing(Pawn pawn, Thing printerThing, bool forced = false)
        {
            Building_AndroidPrinter androidPrinter = printerThing as Building_AndroidPrinter;

            IEnumerable<ThingOrderRequest> potentionalRequests = androidPrinter.orderProcessor.PendingRequests();

            if(potentionalRequests != null)
            {
                foreach (ThingOrderRequest request in potentionalRequests)
                {
                    Thing ingredientThing = FindIngredient(pawn, androidPrinter, request);

                    if (ingredientThing != null)
                    {
                        if (request.nutrition)
                        {
                            int nutritionCount = (int)(Math.Ceiling(request.amount / (ingredientThing.def.ingestible.CachedNutrition)));

                            if (nutritionCount > 0)
                                return new Job(JobDefOf.ChJFillAndroidPrinter, ingredientThing, printerThing)
                                {
                                    count = nutritionCount
                                };
                        }
                        else
                            return new Job(JobDefOf.ChJFillAndroidPrinter, ingredientThing, printerThing)
                            {
                                count = (int)request.amount
                            };
                    }
                }
            }

            /*ThingOrderRequest request = androidPrinter.GetThingRequest();

            Thing ingredientThing = FindIngredient(pawn, androidPrinter, request);

            if(ingredientThing != null)
            {
                if(request.nutrition)
                {
                    int nutritionCount = (int)(Math.Ceiling(request.amount / (ingredientThing.def.ingestible.nutrition)));

                    if (nutritionCount > 0)
                        return new Job(JobDefOF.ChJFillAndroidPrinter, ingredientThing, printerThing)
                        {
                            count = nutritionCount
                        };
                }
                else
                    return new Job(JobDefOF.ChJFillAndroidPrinter, ingredientThing, printerThing)
                    {
                        count = (int)request.amount
                    };
            }*/

            return null;
        }

        /// <summary>
        /// Tries to find a appropiate ingredient.
        /// </summary>
        /// <param name="pawn">Pawn to search for.</param>
        /// <param name="androidPrinter">Printer to fill.</param>
        /// <param name="request">Thing order request to fulfill.</param>
        /// <returns>Valid thing if found, otherwise null.</returns>
        private Thing FindIngredient(Pawn pawn, Building_AndroidPrinter androidPrinter, ThingOrderRequest request)
        {
            if(request != null)
            {
                Predicate<Thing> extraPredicate = request.ExtraPredicate();
                Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x, 1, -1, null, false) && extraPredicate(x);
                Predicate<Thing> validator = predicate;

                return GenClosest.ClosestThingReachable(
                    pawn.Position, pawn.Map, request.Request(), PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, 
                    validator);
            }

            return null;
        }
    }
}
