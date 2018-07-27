using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Helps in processing multiplie Thing Order Requests.
    /// </summary>
    public class ThingOrderProcessor : IExposable
    {
        /// <summary>
        /// Inventory to check for.
        /// </summary>
        public ThingOwner thingHolder;

        /// <summary>
        /// Storage settings to take in account for Nutrition.
        /// </summary>
        public StorageSettings storageSettings;

        /// <summary>
        /// List of requested. And ideal.
        /// </summary>
        public List<ThingOrderRequest> requestedItems = new List<ThingOrderRequest>();

        public ThingOrderProcessor()
        {

        }

        public ThingOrderProcessor(ThingOwner thingHolder, StorageSettings storageSettings)
        {
            this.thingHolder = thingHolder;
            this.storageSettings = storageSettings;
        }

        /// <summary>
        /// Gets all pending requests that need to be processed using ideal requests as a base.
        /// </summary>
        /// <returns>Pending requests or none.</returns>
        public IEnumerable<ThingOrderRequest> PendingRequests()
        {
            foreach(ThingOrderRequest idealRequest in requestedItems)
            {
                if(idealRequest.nutrition)
                {
                    //Food
                    float totalNutrition = CountNutrition();
                    if(totalNutrition < idealRequest.amount)
                    {
                        ThingOrderRequest request = new ThingOrderRequest();
                        request.nutrition = true;
                        request.amount = idealRequest.amount - totalNutrition;
                        request.thingFilter = storageSettings.filter;

                        yield return request;
                    }
                }
                else
                {
                    //Item
                    float totalItemCount = thingHolder.TotalStackCountOfDef(idealRequest.thingDef);
                    if(totalItemCount < idealRequest.amount)
                    {
                        ThingOrderRequest request = new ThingOrderRequest();
                        request.thingDef = idealRequest.thingDef;
                        request.amount = idealRequest.amount - totalItemCount;

                        yield return request;
                    }
                }
            }
        }

        /// <summary>
        /// Counts all available nutrition in the printer.
        /// </summary>
        /// <returns>Total nutrition.</returns>
        public float CountNutrition()
        {
            float totalNutrition = 0f;

            //Count nutrition.
            foreach (Thing item in thingHolder)
            {
                Corpse corpse = item as Corpse;
                if (corpse != null)
                {
                    totalNutrition += FoodUtility.GetBodyPartNutrition(corpse, corpse.InnerPawn.RaceProps.body.corePart);
                }
                else
                {
                    if (item.def.IsIngestible)
                        totalNutrition += (item.def?.ingestible.CachedNutrition ?? 0.05f) * item.stackCount;
                }
            }

            return totalNutrition;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref requestedItems, "requestedItems", LookMode.Deep);
        }
    }
}
