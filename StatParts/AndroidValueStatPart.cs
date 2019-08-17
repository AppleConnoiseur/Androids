using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class AndroidValueStatPart : StatPart
    {
        public override string ExplanationPart(StatRequest req)
        {
            if(req.Thing is Pawn pawn)
            {
                IEnumerable<Hediff> hediffs = GetRelevantHediffs(pawn);
                if(hediffs == null)
                {
                    return null;
                }

                List<Hediff> hediffList = new List<Hediff>(hediffs);
                if(hediffList.Count == 0)
                {
                    return null;
                }

                StringBuilder builder = new StringBuilder();
                builder.AppendLine("AndroidMarketValueStatPartLabel".Translate());

                foreach(Hediff hediff in hediffList)
                {
                    builder.AppendLine("    " + hediff.LabelCap + ": +" + string.Format(parentStat.formatString, (float)Math.Ceiling(PriceUtility.PawnQualityPriceFactor(pawn) * CalculateMarketValueFromHediff(hediff, pawn.RaceProps.baseBodySize))));
                }

                return builder.ToString();
            }

            return null;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (req.Thing is Pawn pawn)
            {
                IEnumerable<Hediff> hediffs = GetRelevantHediffs(pawn);
                if (hediffs == null)
                {
                    return;
                }

                List<Hediff> hediffList = new List<Hediff>(hediffs);

                foreach (Hediff hediff in hediffList)
                {
                    val += (float)Math.Ceiling(PriceUtility.PawnQualityPriceFactor(pawn) * CalculateMarketValueFromHediff(hediff, pawn.RaceProps.baseBodySize));
                }
            }
        }

        private float CalculateMarketValueFromHediff(Hediff hediff, float bodySize = 1f)
        {
            if (hediff == null)
            {
                Log.Error($"Hediff is 'null'. This should not happen!");
                return 0f;
            }

            if (hediff.def.GetModExtension<AndroidUpgradeHediffProperties>() is AndroidUpgradeHediffProperties properties)
            {
                float totalMarketValue = 0f;

                if(properties.def == null)
                {
                    Log.Error($"Hediff '{hediff.LabelCap}' got 'null' properties despite having the 'AndroidUpgradeHediffProperties' DefModExtension!");
                    return 0f;
                }

                foreach (ThingOrderRequest pair in properties.def.costList)
                {
                    if(!pair.nutrition)
                    {
                        if (properties.def.costsNotAffectedByBodySize.Contains(pair.thingDef))
                        {
                            totalMarketValue += pair.thingDef.BaseMarketValue * pair.amount;
                        }
                        else
                        {
                            totalMarketValue += (pair.thingDef.BaseMarketValue * pair.amount) * bodySize;
                        }
                    }
                }

                return (float)Math.Ceiling(totalMarketValue);
            }

            return 0f;
        }

        private IEnumerable<Hediff> GetRelevantHediffs(Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.Where(hediff => hediff.def.GetModExtension<AndroidUpgradeHediffProperties>() is AndroidUpgradeHediffProperties extension && extension.def.costList.Count > 0 && !(extension.def.costList.Count == 1 && extension.def.costList[0].nutrition));
        }
    }
}
