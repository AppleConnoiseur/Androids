using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class CompUseEffect_SpawnCustomDroid : CompUseEffect_SpawnPawn
    {
        public override void DoSpawn(Pawn usedBy)
        {
            Faction newFaction = GetFaction();

            Pawn spawnPawn = DroidUtility.MakeCustomDroid(SpawnerProps.pawnKind, usedBy.Faction);

            if (spawnPawn != null)
            {
                GenPlace.TryPlaceThing(spawnPawn, parent.Position, parent.Map, ThingPlaceMode.Near);

                if(SpawnerProps.sendMessage)
                    Messages.Message(SpawnerProps.pawnSpawnedStringKey.Translate(spawnPawn.Name.ToStringFull, usedBy.Name.ToStringFull), new GlobalTargetInfo(spawnPawn), MessageTypeDefOf.NeutralEvent);
            }
        }
    }
}
