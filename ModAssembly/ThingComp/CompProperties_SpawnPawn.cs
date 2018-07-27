using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class CompProperties_SpawnPawn : CompProperties_UseEffect
    {
        public PawnKindDef pawnKind;
        public int amount = 1;
        public FactionDef forcedFaction;
        public bool usePlayerFaction = true;
        public string pawnSpawnedStringKey = "AndroidSpawnedDroidMessageText";
        public bool sendMessage = true;

        public CompProperties_SpawnPawn()
        {
            compClass = typeof(CompUseEffect_SpawnPawn);
        }
    }
}
