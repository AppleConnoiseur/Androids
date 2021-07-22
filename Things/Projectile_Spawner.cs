using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace Androids
{
    public class Projectile_Spawner : Projectile
    {
        Lord lord = null;

        public SpawnerProjectileProperties SpawnerProps
        {
            get
            {
                return def.GetModExtension<SpawnerProjectileProperties>() as SpawnerProjectileProperties;
            }
        }

        public virtual void DoSpawn(Thing hitThing)
        {
            Pawn spawnPawn = null;

            if(SpawnerProps.pawnKind != null)
                spawnPawn = PawnGenerator.GeneratePawn(SpawnerProps.pawnKind);

            if (SpawnerProps.pawnThingDef != null)
                spawnPawn = (Pawn)ThingMaker.MakeThing(SpawnerProps.pawnThingDef);

            if (spawnPawn != null)
            {
                spawnPawn.SetFaction(SpawnerProps.GetFaction(launcher));
                if (SpawnerProps.forceAgeToZero)
                {
                    spawnPawn.ageTracker.AgeBiologicalTicks = 0;
                    spawnPawn.ageTracker.AgeChronologicalTicks = 0;
                }

                GenPlace.TryPlaceThing(spawnPawn, Position, Map, ThingPlaceMode.Near);

                if (SpawnerProps.mentalStateUponSpawn != null)
                {
                    spawnPawn.mindState.mentalStateHandler.TryStartMentalState(SpawnerProps.mentalStateUponSpawn, null, true);
                }

                if(SpawnerProps.joinLordOnSpawn)
                {
                    if (lord == null && !SpawnerProps.joinSameLordFromProjectile)
                    {
                        lord = GetLord(spawnPawn);
                    }

                    lord.AddPawn(spawnPawn);
                }

                FleckMaker.ThrowSmoke(spawnPawn.Position.ToVector3(), Map, Rand.Range(0.5f, 1.5f));
                FleckMaker.ThrowSmoke(spawnPawn.Position.ToVector3(), Map, Rand.Range(1.0f, 3.0f));
                FleckMaker.ThrowAirPuffUp(spawnPawn.Position.ToVector3(), Map);
            }
        }

        public Lord GetLord(Pawn forPawn)
        {
            Lord lord = null;
            Faction faction = forPawn.Faction;

            if (forPawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != forPawn))
            {
                Pawn p2 = (Pawn)GenClosest.ClosestThing_Global(forPawn.Position, forPawn.Map.mapPawns.SpawnedPawnsInFaction(faction), SpawnerProps.lordJoinRadius, (Thing p) => p != forPawn && ((Pawn)p).GetLord() != null, null);
                lord = p2.GetLord();
            }
            if (lord == null)
            {
                LordJob lordJob = SpawnerProps.CreateJobForLord(forPawn.Position);
                lord = LordMaker.MakeNewLord(faction, lordJob, Map, null);
            }

            return lord;
        }

        protected override void Impact(Thing hitThing)
        {
            SoundDef soundExplode = def.projectile.soundExplode;
            if (soundExplode != null)
                soundExplode.PlayOneShot(new TargetInfo(base.Position, base.Map, false));

            if(SpawnerProps.joinSameLordFromProjectile)
            {
                LordJob lordJob = SpawnerProps.CreateJobForLord(Position);
                lord = LordMaker.MakeNewLord(Faction, lordJob, Map, null);
            }

            //Spawn on impact point.
            for (int i = 0; i < SpawnerProps.amount; i++)
            {
                DoSpawn(hitThing);
            }

            Destroy(DestroyMode.Vanish);
        }
    }
}
