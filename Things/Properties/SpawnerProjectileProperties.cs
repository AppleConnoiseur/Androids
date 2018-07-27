using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Androids
{
    public class SpawnerProjectileProperties : DefModExtension
    {
        public PawnKindDef pawnKind;
        public ThingDef pawnThingDef;
        public int amount = 1;

        public FactionDef forcedFaction;
        public bool usePlayerFaction = true;

        public bool forceAgeToZero = false;

        public MentalStateDef mentalStateUponSpawn;

        public bool joinLordOnSpawn;
        public Type lordJob = typeof(LordJob_DefendPoint);
        public float lordJoinRadius = 99999f;
        public bool joinSameLordFromProjectile = true;

        public LordJob CreateJobForLord(IntVec3 point)
        {
            LordJob job = null;

            if (lordJob.GetConstructors().Any(constructor => constructor.GetParameters() is ParameterInfo[]args && args.Count() > 0 && args[0].ParameterType == typeof(IntVec3)))
            {
                job = (LordJob)Activator.CreateInstance(lordJob, new object[] { point });
            }
            else
            {
                job = (LordJob)Activator.CreateInstance(lordJob);
            }
            
            return job;
        }

        public Faction GetFaction(Thing launcher)
        {
            if (!usePlayerFaction)
            {
                if (forcedFaction == null)
                    return launcher.Faction;
                else
                    return FactionUtility.DefaultFactionFrom(forcedFaction);
            }

            return Faction.OfPlayer;
        }
    }
}
