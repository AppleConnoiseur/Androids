using Androids.Integration;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class HediffGiver_MachineWearAndTear : HediffGiver
    {
        public float potencyToIncreasePerDay = 1.0f;
        public float chanceToContract = 0.5f;

        public float PotencyPerTick
        {
            get
            {
                return potencyToIncreasePerDay / (float)GenDate.TicksPerDay;
            }
        }

        public int CheckInterval
        {
            get
            {
                if (AndroidsModSettings.Instance.droidWearDownQuadrum)
                    return GenDate.TicksPerQuadrum;
                return GenDate.TicksPerDay;
            }
        }

        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if(AndroidsModSettings.Instance.droidWearDown && cause == null && partsToAffect != null)
            {
                foreach(BodyPartDef def in partsToAffect)
                {
                    BodyPartRecord bodyPart = pawn.RaceProps.body.AllParts.FirstOrDefault(part => part.def == def);
                    if(bodyPart != null)
                    {
                        Hediff bodyPartHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(partHediff => partHediff.Part == bodyPart && partHediff.def == hediff);
                        if (bodyPartHediff == null)
                        {
                            //Give a chance to give it every day.
                            if(pawn.IsHashIntervalTick(CheckInterval) && Rand.Chance(chanceToContract))
                            {
                                Hediff newHediff = HediffMaker.MakeHediff(hediff, pawn, bodyPart);
                                pawn.health.AddHediff(newHediff);
                            }
                        }
                        else
                        {
                            //bodyPartHediff.Severity += Rand.Range(0f, PotencyPerTick);
                            bodyPartHediff.Severity += PotencyPerTick;
                        }
                    }
                }
            }
        }
    }
}
