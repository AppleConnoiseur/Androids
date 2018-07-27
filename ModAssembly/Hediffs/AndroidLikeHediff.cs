using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Androids
{
    public class AndroidLikeHediff : Hediff
    {
        public float energyTracked;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref energyTracked, "energyTracked");
        }

        public override void Tick()
        {
            base.Tick();

            if(pawn.needs.TryGetNeed<Need_Energy>() is Need_Energy energy)
            {
                energyTracked = energy.CurLevel;
            }
        }

        public override void Notify_PawnDied()
        {
            //Log.Message("Pawn died: " + pawn);
            //Log.Message("Parent Holder: " + pawn.ParentHolder);

            if (pawn.health.hediffSet.HasHediff(HediffDefOf.ChjAndroidLike) && ThingDefOf.ChjAndroid.race.DeathActionWorker != null)
            {
                //Log.Message("Is Android");
                if (pawn.Corpse != null)
                {
                    //Log.Message("Pre: Death action worker");
                    ThingDefOf.ChjAndroid.race.DeathActionWorker.PawnDied(pawn.Corpse);
                    //Log.Message("Post: Death action worker");
                }
            }
        }
    }
}
