using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace Androids
{
    /// <summary>
    /// Order request for a specific Thing.
    /// </summary>
    public class ThingOrderRequest : IExposable
    {
        /// <summary>
        /// ThingDef to request.
        /// </summary>
        public ThingDef thingDef;

        /// <summary>
        /// Is this request about ordering nutrition?
        /// </summary>
        public bool nutrition = false;

        /// <summary>
        /// Used ThingFilter for the nutrition request.
        /// </summary>
        public ThingFilter thingFilter = null;

        /// <summary>
        /// Amount the requester want.
        /// </summary>
        public float amount;

        /// <summary>
        /// Constructs the ThingRequest for the orderer.
        /// </summary>
        /// <returns>Thing request</returns>
        public ThingRequest Request()
        {
            if (thingDef != null)
                return ThingRequest.ForDef(thingDef);

            if (nutrition)
                return ThingRequest.ForGroup(ThingRequestGroup.FoodSourceNotPlantOrTree);

            return ThingRequest.ForUndefined();
        }

        /// <summary>
        /// Extra predicate used when searching for possible ingredients.
        /// </summary>
        /// <returns>New predicate</returns>
        public Predicate<Thing> ExtraPredicate()
        {
            if (nutrition)
            {
                if(thingFilter == null)
                    return thing => (!thing.def?.ingestible.IsMeal ?? false) && thing.def.IsNutritionGivingIngestible;
                else
                    return delegate(Thing thing)
                    {
                        if(thingFilter.Allows(thing) && thing.def.IsNutritionGivingIngestible)
                        {
                            Corpse corpse = thing as Corpse;
                            if(corpse != null && corpse.IsDessicated())
                                return false;
                            return true;
                        }

                        return false;
                    };
                //return thing => thingFilter.Allows(thing) && thing.def.IsNutritionGivingIngestible
            }

            return thing => true;
        }

        /// <summary>
        /// Configures this from XML data.
        /// </summary>
        /// <param name="xmlRoot">Root XML node.</param>
        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.ChildNodes.Count != 1)
            {
                Log.Error("Misconfigured ThingOrderRequest: " + xmlRoot.OuterXml);
                return;
            }

            if(xmlRoot.Name.ToLower() == "nutrition")
            {
                //Log.Message("Nutrition=" + (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float)));
                nutrition = true;
            }
            else
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            }

            amount = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
        }

        public void ExposeData()
        {
            Scribe_Defs.Look(ref thingDef, "thingDef");
            Scribe_Values.Look(ref nutrition, "nutrition");
            Scribe_Values.Look(ref amount, "amount");
        }
    }
}
