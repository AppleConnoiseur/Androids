
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class CompUseEffect_SpawnPawn : CompUseEffect
    {
        public override float OrderPriority => 1000;

        public CompProperties_SpawnPawn SpawnerProps
        {
            get
            {
                return props as CompProperties_SpawnPawn;
            }
        }

        public virtual void DoSpawn(Pawn usedBy)
        {
            Pawn spawnPawn = PawnGenerator.GeneratePawn(SpawnerProps.pawnKind);
            if(spawnPawn != null)
            {
                spawnPawn.SetFaction(GetFaction());

                GenPlace.TryPlaceThing(spawnPawn, parent.Position, parent.Map, ThingPlaceMode.Near);

                if (SpawnerProps.sendMessage)
                    Messages.Message("AndroidSpawnedPawnMessageText".Translate(spawnPawn.Name), new GlobalTargetInfo(spawnPawn), MessageTypeDefOf.NeutralEvent);
            }
        }

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            for(int i = 0; i < SpawnerProps.amount; i++)
                DoSpawn(usedBy);
        }

        public Faction GetFaction()
        {
            if(!SpawnerProps.usePlayerFaction)
            {
                if (SpawnerProps.forcedFaction == null)
                    return parent.Faction;
                else
                    return FactionUtility.DefaultFactionFrom(SpawnerProps.forcedFaction);
            }

            return Faction.OfPlayer;
        }
    }
}
