using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    /// <summary>
    /// Represents a upgrade to a Android that applies one or more Hediffs.
    /// </summary>
    public class UpgradeCommand_Hediffs : UpgradeCommand
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

            if (def.hediffs.Count > 0)
            {
                foreach (var hediffApplication in def.hediffs)
                {
                    Hediff hediff = HediffMaker.MakeHediff(hediffApplication.def, targetPawn);
                    hediff.Severity = hediffApplication.severity;

                    appliedHediffs.Add(hediff);
                    BodyPartRecord record = null;
                    if(hediffApplication.part != null)
                    {
                        record = targetPawn.def.race.body.GetPartsWithDef(hediffApplication.part).FirstOrDefault();
                    }
                    targetPawn.health.AddHediff(hediff, record);
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
