using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Represents a upgrade to a Android that applies a Hediff.
    /// </summary>
    public class UpgradeCommand_Hediff : UpgradeCommand
    {
        public List<Hediff> appliedHediffs = new List<Hediff>();

        public override void Apply(Pawn customTarget = null)
        {
            Pawn targetPawn = null;
            if (customTarget != null)
            {
                targetPawn = customTarget;
            }
            else
            {
                targetPawn = customizationWindow.newAndroid;
            }

            if (customTarget == null && customizationWindow == null)
            {
                Log.Error("customizationWindow is null! Impossible to add Hediffs without it.");
                return;
            }

            if (def.hediffToApply != null)
            {
                if (def.partsToApplyTo != null)
                {
                    foreach (BodyPartGroupDef bodyPartDef in def.partsToApplyTo)
                    {
                        IEnumerable<BodyPartRecord> notMissingParts = targetPawn.health.hediffSet.GetNotMissingParts(depth: BodyPartDepth.Outside);
                        foreach (BodyPartRecord part in notMissingParts)
                        {
                            if (part.IsInGroup(bodyPartDef) && (def.partsDepth == BodyPartDepth.Undefined || part.depth == def.partsDepth))
                            {
                                Hediff hediff = HediffMaker.MakeHediff(def.hediffToApply, targetPawn, part);
                                hediff.Severity = def.hediffSeverity;

                                appliedHediffs.Add(hediff);
                                targetPawn.health.AddHediff(hediff);
                            }
                        }
                    }
                }
                else
                {
                    Hediff hediff = HediffMaker.MakeHediff(def.hediffToApply, targetPawn);
                    hediff.Severity = def.hediffSeverity;

                    appliedHediffs.Add(hediff);
                    targetPawn.health.AddHediff(hediff);

                    //Log.Message("Applied Hediff: " + hediff.ToString());
                }
            }
        }

        public override string GetExplanation()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(def.hediffToApply.ConcreteExample.TipStringExtra);

            return builder.ToString();
        }

        public override void Undo()
        {
            if(customizationWindow == null)
            {
                Log.Error("customizationWindow is null! Impossible to remove Hediffs without it.");
                return;
            }

            foreach (Hediff hediff in appliedHediffs)
            {
                customizationWindow.newAndroid.health.RemoveHediff(hediff);
            }

            appliedHediffs.Clear();
        }
    }
}
