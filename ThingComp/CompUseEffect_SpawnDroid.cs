using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class CompUseEffect_SpawnDroid : CompUseEffect_SpawnPawn
    {
        public override void DoSpawn(Pawn usedBy)
        {
            Faction newFaction = GetFaction();

            Pawn spawnPawn = DroidUtility.MakeDroidTemplate(SpawnerProps.pawnKind, newFaction, parent.Map.Tile);
            if (spawnPawn != null)
            {
                GenPlace.TryPlaceThing(spawnPawn, parent.Position, parent.Map, ThingPlaceMode.Near);

                if (SpawnerProps.sendMessage)
                    Messages.Message("AndroidSpawnedDroidMessageText".Translate(spawnPawn.Name, usedBy.Name), new GlobalTargetInfo(spawnPawn), MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
