using Androids.Integration;
using Harmony;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Androids
{
    /// <summary>
    /// Dog bless Harmony.
    /// </summary>
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        /// <summary>
        /// For internal use: Field for getting the pawn.
        /// </summary>
        public static FieldInfo int_Pawn_NeedsTracker_GetPawn;
        public static FieldInfo int_PawnRenderer_GetPawn;
        public static FieldInfo int_Need_Food_Starving_GetPawn;
        public static FieldInfo int_ConditionalPercentageNeed_need;
        public static FieldInfo int_Pawn_HealthTracker_GetPawn;
        public static FieldInfo int_Pawn_InteractionsTracker_GetPawn;

        public static NeedDef Need_Bladder;
        public static NeedDef Need_Hygiene;

        public static bool bypassGenerationOfUpgrades = false;

        static HarmonyPatches()
        {
            //Try get needs.
            Need_Bladder = DefDatabase<NeedDef>.GetNamedSilentFail("Bladder");
            Need_Hygiene = DefDatabase<NeedDef>.GetNamedSilentFail("Hygiene");

            HarmonyInstance harmony = HarmonyInstance.Create("chjees.androids");

            //Patch, Method: Pawn_NeedsTracker
            {
                Type type = typeof(Pawn_NeedsTracker);

                //Get private variable 'pawn' from 'Pawn_NeedsTracker'.
                int_Pawn_NeedsTracker_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                //Patch: Pawn_NeedsTracker.ShouldHaveNeed as Postfix
                harmony.Patch(
                    type.GetMethod("ShouldHaveNeed", BindingFlags.NonPublic | BindingFlags.Instance), 
                    null, 
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Pawn_NeedsTracker_ShouldHaveNeed))));
            }

            //Patch, Method: PawnRenderer
            {
                Type type = typeof(PawnRenderer);

                //Get private variable 'pawn' from 'PawnRenderer'.
                int_PawnRenderer_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                //Patch: PawnRenderer.RenderPawnInternal as Postfix
                harmony.Patch(type.GetMethod("RenderPawnInternal", BindingFlags.NonPublic | BindingFlags.Instance,
                    Type.DefaultBinder, CallingConventions.Any, 
                    new Type[] { typeof(Vector3), typeof(float), typeof(bool), typeof(Rot4), typeof(Rot4), typeof(RotDrawMode), typeof(bool), typeof(bool) }, null),
                    null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PawnRenderer_RenderPawnInternal))));
            }

            //Patch, Property: Need_Food.Starving
            {
                Type type = typeof(Need_Food);

                //Get protected variable 'pawn' from 'PawnRenderer'.
                int_Need_Food_Starving_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                //Get, get method.
                MethodInfo getMethod = AccessTools.Property(type, "Starving")?.GetGetMethod();

                //Patch: Pawn_NeedsTracker.ShouldHaveNeed as Postfix
                harmony.Patch(getMethod, null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Need_Food_Starving_Get))));
            }

            //Patch, Method: HealthUtility
            {
                Type type = typeof(HealthUtility);

                //Patch: HealthUtility.AdjustSeverity as Prefix
                harmony.Patch(
                    type.GetMethod("AdjustSeverity"), 
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_HealthUtility_AdjustSeverity))), null);
            }

            //Patch, Method: ThinkNode_ConditionalNeedPercentageAbove
            {
                Type type = typeof(ThinkNode_ConditionalNeedPercentageAbove);

                int_ConditionalPercentageNeed_need = type.GetField("need", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                //Patch: ThinkNode_ConditionalNeedPercentageAbove.Satisfied as Prefix
                harmony.Patch(
                    type.GetMethod("Satisfied", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance), 
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_ThinkNode_ConditionalNeedPercentageAbove_Satisfied))), 
                    null);
            }

            //Patch, Method: Pawn_HealthTracker.DropBloodFilth
            {
                //Pawn_HealthTracker
                Type type = typeof(Pawn_HealthTracker);

                //Patch: Pawn_HealthTracker.DropBloodFilth as Prefix
                harmony.Patch(
                    type.GetMethod("DropBloodFilth"), 
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Pawn_HealthTracker_DropBloodFilth))), 
                    null);
            }

            {
                //Pawn_HealthTracker
                Type type = typeof(Pawn_HealthTracker);

                //Patch: Pawn_HealthTracker.HealthTick as Prefix
                harmony.Patch(
                    type.GetMethod("HealthTick"),
                    null,
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Pawn_HealthTracker_HealthTick))));
            }

            {
                //Pawn_HealthTracker
                Type type = typeof(Pawn_HealthTracker);

                //Patch: Pawn_HealthTracker.AddHediff as Postfix
                harmony.Patch(
                    type.GetMethod("AddHediff", new Type[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo), typeof(DamageWorker.DamageResult) }),
                    null,
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Pawn_HealthTracker_AddHediff))));
            }

            {
                //SkillRecord
                Type type = typeof(SkillRecord);

                //Patch: SkillRecord.Interval as Prefix
                harmony.Patch(
                    type.GetMethod("Interval"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_SkillRecord_Interval))),
                    null);
            }

            {
                //Pawn
                Type type = typeof(Pawn);

                //Patch: Pawn.GetGizmos as Postfix
                harmony.Patch(
                    type.GetMethod("GetGizmos"),
                    null,
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Pawn_GetGizmos))));
            }

            {
                //HealthAIUtility
                Type type = typeof(HealthAIUtility);

                //Patch: HealthAIUtility.FindBestMedicine as Prefix
                harmony.Patch(
                    type.GetMethod("FindBestMedicine"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_HealthAIUtility_FindBestMedicine))),
                    null);
            }

            {
                //Toils_Tend
                Type type = typeof(Toils_Tend);

                //Patch: Toils_Tend.FinalizeTend as Prefix
                harmony.Patch(
                    type.GetMethod("FinalizeTend"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_Toils_Tend_FinalizeTend))),
                    null);
            }

            {
                //DaysWorthOfFoodCalculator
                Type type = typeof(DaysWorthOfFoodCalculator);

                //Patch: DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood as Prefix
                Type[] types = new Type[] {
                        typeof(List<Pawn>), typeof(List<ThingDefCount>), typeof(int), typeof(IgnorePawnsInventoryMode),
                        typeof(Faction), typeof(WorldPath), typeof(float), typeof(int), typeof(bool)};
                harmony.Patch(
                    type.GetMethod("ApproxDaysWorthOfFood", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Static, Type.DefaultBinder, types, null),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_DaysWorthOfFoodCalculator_ApproxDaysWorthOfFood))),
                    null);
            }

            {
                //PartyUtility
                Type type = typeof(PartyUtility);

                //Patch: PartyUtility.ShouldPawnKeepPartying as Prefix
                harmony.Patch(
                    type.GetMethod("ShouldPawnKeepPartying"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PartyUtility_ShouldPawnKeepPartying))),
                    null);
            }

            {
                //PartyUtility
                Type type = typeof(PartyUtility);

                //Patch: PartyUtility.EnoughPotentialGuestsToStartParty as Prefix
                harmony.Patch(
                    type.GetMethod("EnoughPotentialGuestsToStartParty"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PartyUtility_EnoughPotentialGuestsToStartParty))),
                    null);
            }

            {
                //PartyUtility
                Type type = typeof(ThoughtWorker_NeedFood);

                //Patch: ThoughtWorker_NeedFood.CurrentStateInternal as Prefix
                harmony.Patch(
                    type.GetMethod("CurrentStateInternal", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_ThoughtWorker_NeedFood_CurrentStateInternal))),
                    null);
            }

            {
                //PawnUtility
                Type type = typeof(PawnUtility);

                //Patch: Toils_Tend.FinalizeTend as Prefix
                harmony.Patch(
                    type.GetMethod("HumanFilthChancePerCell"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PawnUtility_HumanFilthChancePerCell))),
                    null);
            }

            {
                //PawnGenerator
                Type type = typeof(PawnGenerator);

                harmony.Patch(
                    AccessTools.Method(type, "TryGenerateNewPawnInternal"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PawnGenerator_TryGenerateNewPawnInternal))),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(Patch_PawnGenerator_TryGenerateNewPawnInternal_Post))));
            }

            //Droid
            //Compatibility Patches
            {
                Type type = typeof(FoodUtility);

                harmony.Patch(type.GetMethod("WillIngestStackCountOf"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_WillIngestStackCountOf))), null);
            }

            {
                Type type = typeof(RecordWorker_TimeInBedForMedicalReasons);

                harmony.Patch(type.GetMethod("ShouldMeasureTimeNow"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_ShouldMeasureTimeNow))), null);
            }

            {
                Type type = typeof(InteractionUtility);

                harmony.Patch(type.GetMethod("CanInitiateInteraction"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_CanInitiateInteraction))), null);
            }

            {
                Type type = typeof(Pawn_HealthTracker);

                int_Pawn_HealthTracker_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                harmony.Patch(type.GetMethod("ShouldBeDeadFromRequiredCapacity"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_ShouldBeDeadFromRequiredCapacity))), null);
            }

            {
                Type type = typeof(HediffSet);

                harmony.Patch(type.GetMethod("CalculatePain", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_CalculatePain))), null);
            }

            {
                Type type = typeof(RestUtility);

                harmony.Patch(type.GetMethod("TimetablePreventsLayDown"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_TimetablePreventsLayDown))), null);
            }

            {
                Type type = typeof(GatheringsUtility);

                harmony.Patch(type.GetMethod("ShouldGuestKeepAttendingGathering"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_ShouldGuestKeepAttendingGathering))), null);
            }

            {
                Type type = typeof(JobGiver_EatInPartyArea);

                harmony.Patch(type.GetMethod("TryGiveJob", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_EatInPartyAreaTryGiveJob))), null);
            }

            {
                Type type = typeof(JobGiver_GetJoy);

                harmony.Patch(type.GetMethod("TryGiveJob", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_GetJoyTryGiveJob))), null);
            }

            {
                Type type = typeof(Pawn_InteractionsTracker);

                int_Pawn_InteractionsTracker_GetPawn = type.GetField("pawn", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance);

                harmony.Patch(type.GetMethod("SocialFightChance"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_SocialFightChance))), null);
                harmony.Patch(type.GetMethod("InteractionsTrackerTick"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_InteractionsTrackerTick))), null);
                harmony.Patch(type.GetMethod("CanInteractNowWith"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_CanInteractNowWith))), null);
            }

            {
                Type type = typeof(InteractionUtility);

                harmony.Patch(type.GetMethod("CanInitiateInteraction"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_CanDoInteraction))), null);
                harmony.Patch(type.GetMethod("CanReceiveInteraction"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_CanDoInteraction))), null);
            }

            {
                Type type = typeof(PawnDiedOrDownedThoughtsUtility);

                harmony.Patch(type.GetMethod("AppendThoughts_ForHumanlike", BindingFlags.NonPublic |  BindingFlags.Static), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_AppendThoughts_ForHumanlike))), null);
            }

            {
                Type type = typeof(InspirationHandler);

                harmony.Patch(type.GetMethod("InspirationHandlerTick"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_InspirationHandlerTick))), null);
            }

            {
                Type type = typeof(JobDriver_Vomit);

                harmony.Patch(
                    type.GetMethod("MakeNewToils", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod), 
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_VomitJob))), 
                    null);
            }

            {
                Type type = typeof(Alert_Boredom);

                //For some reason this did not work.
                /*harmony.Patch(
                    AccessTools.Method(type, "BoredPawns"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_BoredPawns))),
                    null);

                Log.Message("Patched Alert_Boredom.BoredPawns");*/

                //But this did.
                harmony.Patch(
                    AccessTools.Method(type, "GetReport"),
                    new HarmonyMethod(typeof(HarmonyPatches).GetMethod(nameof(CompatPatch_Boredom_GetReport))),
                    null);

                //Log.Message("Patched Alert_Boredom.BoredPawns");
            }

            /*{
                //Patches courtesy of erdelf!
                harmony.Patch(
                    typeof(SymbolResolver_RandomMechanoidGroup).GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                    .First(
                        mi => 
                        mi.HasAttribute<CompilerGeneratedAttribute>() && 
                        mi.ReturnType == typeof(bool) && 
                        mi.GetParameters().Count() == 1 && 
                        mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)), 
                    null, new HarmonyMethod(typeof(HarmonyPatches), nameof(MechanoidsFixerAncient)));

                harmony.Patch(
                    typeof(CompSpawnerMechanoidsOnDamaged).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                    .First(
                        mi => 
                        mi.HasAttribute<CompilerGeneratedAttribute>() && 
                        mi.ReturnType == typeof(bool) && 
                        mi.GetParameters().Count() == 1 && 
                        mi.GetParameters()[0].ParameterType == typeof(PawnKindDef)), 
                    null, new HarmonyMethod(typeof(HarmonyPatches), nameof(MechanoidsFixer)));
            }*/

            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static bool Patch_PawnGenerator_TryGenerateNewPawnInternal(ref Pawn __result, ref PawnGenerationRequest request, out string error)
        {
            error = null;

            //Hijack the process if a Droid is detected.
            if (request.KindDef.race.GetModExtension<DroidSpawnProperties>() is DroidSpawnProperties props)
            {
                __result = DroidUtility.MakeDroidTemplate(request.KindDef, request.Faction, request.Tile);
                return false;
            }

            //Let original pawn generator run.
            return true;
        }

        public static void Patch_PawnGenerator_TryGenerateNewPawnInternal_Post(ref Pawn __result)
        {
            if(__result == null)
            {
                return;
            }

            //Post process with upgrades for naturally generated pawns.
            if(!bypassGenerationOfUpgrades)
            {
                if(__result.IsAndroid())
                {
                    bool alreadyGotUpgrade = false;

                    if (__result.story != null)
                    {
                        foreach (AndroidUpgradeDef def in DefDatabase<AndroidUpgradeDef>.AllDefs)
                        {
                            if (__result.story.childhood != null && def.spawnInBackstories.Contains(__result.story.childhood.untranslatedTitle) ||
                                __result.story.adulthood != null && def.spawnInBackstories.Contains(__result.story.adulthood.untranslatedTitle))
                            {
                                UpgradeCommand command = UpgradeMaker.Make(def);
                                command.Apply(__result);
                                alreadyGotUpgrade = true;
                            }
                        }
                    }

                    //1 in 10 Androids get a free upgrade.
                    if(!alreadyGotUpgrade && Rand.Chance(0.1f))
                    {
                        AndroidUpgradeDef def = DefDatabase<AndroidUpgradeDef>.AllDefs.RandomElement();
                        UpgradeCommand command = UpgradeMaker.Make(def);
                        command.Apply(__result);
                    }
                }
            }
        }

        public static bool Patch_PawnUtility_HumanFilthChancePerCell(float __result, ThingDef def)
        {
            if (def.HasModExtension<MechanicalPawnProperties>())
            {
                __result = 0f;
                return false;
            }

            return true;
        }

        //Patch_ThoughtWorker_NeedFood_CurrentStateInternal
        public static bool Patch_ThoughtWorker_NeedFood_CurrentStateInternal(ref ThoughtState __result, Pawn p)
        {
            if (p.IsAndroid())
            {
                __result = ThoughtState.Inactive;
                return false;
            }

            return true;
        }

        public static bool Patch_PartyUtility_EnoughPotentialGuestsToStartParty(ref bool __result, Map map, ref IntVec3? partySpot)
        {
            if(map.mapPawns.FreeColonistsSpawned.Any(p => p.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize))
            {
                int num = Mathf.RoundToInt((float)map.mapPawns.FreeColonistsSpawned.Count(p => !(p.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)) * 0.65f);
                num = Mathf.Clamp(num, 2, 10);
                int num2 = 0;
                foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
                {
                    if (PartyUtility.ShouldPawnKeepPartying(pawn))
                    {
                        if (partySpot == null || !partySpot.Value.IsForbidden(pawn))
                        {
                            if (partySpot == null || pawn.CanReach(partySpot.Value, PathEndMode.Touch, Danger.Some, false, TraverseMode.ByPawn))
                            {
                                num2++;
                            }
                        }
                    }
                }
                __result = num2 >= num;
                return false;
            }

            return true;
        }

        public static bool Patch_PartyUtility_ShouldPawnKeepPartying(ref bool __result, Pawn p)
        {
            if (p.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                //Log.Message("Droid: " + p.Name);
                __result = false;
                return false;
            }

            //Log.Message("Not Droid: " + p.Name);
            return true;
        }

        public static bool Patch_DaysWorthOfFoodCalculator_ApproxDaysWorthOfFood(
            ref List<Pawn> pawns, List<ThingDefCount> extraFood, int tile, IgnorePawnsInventoryMode ignoreInventory, Faction faction, 
            WorldPath path, float nextTileCostLeft, int caravanTicksPerMove, bool assumeCaravanMoving)
        {
            List<Pawn> modifiedPawnsList = new List<Pawn>(pawns);
            modifiedPawnsList.RemoveAll(pawn => pawn.def.HasModExtension<MechanicalPawnProperties>());

            pawns = modifiedPawnsList;
            return true;
        }

        public static bool Patch_HealthAIUtility_FindBestMedicine(ref Thing __result, Pawn healer, Pawn patient)
        {
            if (patient.def.HasModExtension<MechanicalPawnProperties>())
            {
                Thing result;
                if (patient.playerSettings == null || patient.playerSettings.medCare <= MedicalCareCategory.NoMeds)
                {
                    result = null;
                }
                else if (Medicine.GetMedicineCountToFullyHeal(patient) <= 0)
                {
                    result = null;
                }
                else
                {
                    Predicate<Thing> predicate = (Thing m) => !m.IsForbidden(healer) && patient.playerSettings.medCare.AllowsMedicine(m.def) && healer.CanReserve(m, 10, 1, null, false) && m.def.GetModExtension<DroidRepairProperties>() != null;
                    Func<Thing, float> priorityGetter = delegate(Thing t)
                    {
                        DroidRepairProperties repairParts = t.def.GetModExtension<DroidRepairProperties>();
                        if (repairParts == null)
                            return 0f;

                        return repairParts.repairPotency;
                    };
                    IntVec3 position = patient.Position;
                    Map map = patient.Map;
                    List<Thing> searchSet = patient.Map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
                    PathEndMode peMode = PathEndMode.ClosestTouch;
                    TraverseParms traverseParams = TraverseParms.For(healer, Danger.Deadly, TraverseMode.ByPawn, false);
                    Predicate<Thing> validator = predicate;
                    result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator, priorityGetter);
                }

                __result = result;
                return false;
            }

            return true;
        }

        public static bool Patch_Toils_Tend_FinalizeTend(ref Toil __result, Pawn patient)
        {
            if (patient.def.HasModExtension<MechanicalPawnProperties>())
            {
                Toil toil = new Toil();
                toil.initAction = delegate
                {
                    Pawn actor = toil.actor;

                    Thing repairParts = (Thing)actor.CurJob.targetB.Thing;

                    //Experience
                    float num = (!patient.RaceProps.Animal) ? 500f : 175f;
                    float num2 = RimWorld.ThingDefOf.MedicineIndustrial.MedicineTendXpGainFactor;
                    actor.skills.Learn(SkillDefOf.Crafting, num * num2, false);

                    //Tending
                    //TendUtility.DoTend(actor, patient, medicine);
                    DroidUtility.DoTend(actor, patient, repairParts);

                    if (repairParts != null && repairParts.Destroyed)
                    {
                        actor.CurJob.SetTarget(TargetIndex.B, LocalTargetInfo.Invalid);
                    }
                    if (toil.actor.CurJob.endAfterTendedOnce)
                    {
                        actor.jobs.EndCurrentJob(JobCondition.Succeeded, true);
                    }
                };
                toil.defaultCompleteMode = ToilCompleteMode.Instant;
                __result = toil;
                return false;
            }

            return true;
        }

        public static bool Patch_SkillRecord_Interval(SkillRecord __instance)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(SkillRecord), "pawn").GetValue(__instance);
            if(pawn.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && properties.noSkillLoss)
            {
                //No skill loss
                return false;
            }

            return true;
        }

        public static void Patch_Pawn_GetGizmos(Pawn __instance, ref IEnumerable<Gizmo> __result)
        {
            Pawn pawn = __instance;

            if (pawn.IsColonistPlayerControlled)
            {
                List<Gizmo> gizmos = new List<Gizmo>(__result);

                if(pawn.needs.TryGetNeed<Need_Energy>() is Need_Energy energyNeed)
                {
                    gizmos.Add(
                    new Command_Action()
                    {
                        defaultLabel = "AndroidGizmoRechargeNowLabel".Translate(),
                        defaultDesc = "AndroidGizmoRechargeNowDescription".Translate(),
                        icon = ContentFinder<Texture2D>.Get("UI/Commands/TryReconnect", true),
                        order = -98,
                        action = delegate()
                        {
                            //Find suitable power source.
                            Thing closestPowerSource = EnergyNeedUtility.ClosestPowerSource(pawn);
                            if(closestPowerSource != null)
                            {
                                Job jobToUse = null;

                                Building building = closestPowerSource as Building;
                                if (closestPowerSource != null && building != null && building.PowerComp != null && building.PowerComp.PowerNet.CurrentStoredEnergy() > 50f)
                                {
                                    //Find a suitable spot to drain from.
                                    IntVec3 drainSpot = closestPowerSource.Position;

                                    //Give out job to go out and tap it.
                                    if (drainSpot.Walkable(pawn.Map) && drainSpot.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(drainSpot)) && pawn.CanReach(drainSpot, PathEndMode.OnCell, Danger.Deadly))
                                        jobToUse = new Job(JobDefOf.ChJAndroidRecharge, closestPowerSource);

                                    //Check surrounding cells.
                                    if(jobToUse == null)
                                    {
                                        foreach (IntVec3 adjCell in GenAdj.CellsAdjacentCardinal(building).OrderByDescending(selector => selector.DistanceTo(pawn.Position)))
                                        {
                                            if (adjCell.Walkable(pawn.Map) && adjCell.InAllowedArea(pawn) && pawn.CanReserve(new LocalTargetInfo(adjCell)) && pawn.CanReach(adjCell, PathEndMode.OnCell, Danger.Deadly))
                                                jobToUse = new Job(JobDefOf.ChJAndroidRecharge, closestPowerSource, adjCell);
                                        }
                                    }
                                }

                                if(jobToUse != null)
                                {
                                    pawn.jobs.TryTakeOrderedJob(jobToUse, JobTag.SatisfyingNeeds);
                                }
                            }
                        }
                    });
                }

                //Hediffs
                foreach(Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if(hediff is IExtraGizmos extraGizmos)
                    {
                        foreach (Gizmo gizmo in extraGizmos.GetGizmosExtra())
                        {
                            gizmos.Add(gizmo);
                        }
                    }
                }

                __result = gizmos;
            }
        }

        public static void Patch_Pawn_HealthTracker_AddHediff(Pawn_HealthTracker __instance, Hediff hediff, BodyPartRecord part, ref DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            Pawn pawn = Pawn_HealthTracker_GetPawn(__instance);
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.ChjAndroidLike) && !pawn.Dead)
            {
                if (ThingDefOf.ChjAndroid.race.hediffGiverSets != null)
                {
                    for (int i = 0; i < ThingDefOf.ChjAndroid.race.hediffGiverSets.Count; i++)
                    {
                        HediffGiverSetDef hediffGiverSetDef = ThingDefOf.ChjAndroid.race.hediffGiverSets[i];
                        for (int j = 0; j < hediffGiverSetDef.hediffGivers.Count; j++)
                        {
                            hediffGiverSetDef.hediffGivers[j].OnHediffAdded(pawn, hediff);
                        }
                    }
                }
            }
        }

        public static void Patch_Pawn_HealthTracker_HealthTick(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Pawn_HealthTracker_GetPawn(__instance);
            if(pawn.health.hediffSet.HasHediff(HediffDefOf.ChjAndroidLike) && !pawn.Dead)
            {
                //Tick Android HediffGivers and remove bleeding effects.
                List<HediffGiverSetDef> hediffGiverSets = ThingDefOf.ChjAndroid.race.hediffGiverSets;
                if (hediffGiverSets != null && pawn.IsHashIntervalTick(60))
                {
                    for (int k = 0; k < hediffGiverSets.Count; k++)
                    {
                        List<HediffGiver> hediffGivers = hediffGiverSets[k].hediffGivers;
                        for (int l = 0; l < hediffGivers.Count; l++)
                        {
                            hediffGivers[l].OnIntervalPassed(pawn, null);
                            if (pawn.Dead)
                            {
                                return;
                            }
                        }
                    }
                }

                //Remove bleeding.
                pawn.health.hediffSet.hediffs.RemoveAll(hediff => hediff.def == RimWorld.HediffDefOf.BloodLoss);
            }
        }

        public static bool Patch_Pawn_HealthTracker_DropBloodFilth(Pawn_HealthTracker __instance)
        {
            Pawn pawn = Pawn_HealthTracker_GetPawn(__instance);
            if (pawn.health.hediffSet.HasHediff(HediffDefOf.ChjAndroidLike) && (pawn.Spawned || pawn.ParentHolder is Pawn_CarryTracker) && pawn.SpawnedOrAnyParentSpawned && pawn.RaceProps.BloodDef != null)
            {
                //Drop Android blood instead.
                FilthMaker.MakeFilth(pawn.PositionHeld, pawn.MapHeld, ThingDefOf.ChjAndroid.race.BloodDef, pawn.LabelIndefinite(), 1);
                return false;
            }

            return true;
        }

        public static bool CompatPatch_Boredom_GetReport(ref Alert_Boredom __instance, ref AlertReport __result)
        {
            IEnumerable<Pawn> culprits = null;
            CompatPatch_BoredPawns(ref culprits);

            __result = AlertReport.CulpritsAre(culprits);
            return false;
        }

        public static bool CompatPatch_BoredPawns(ref IEnumerable<Pawn> __result)
        {
            //Log.Message("CompatPatch_BoredPawns Alert");

            List<Pawn> legiblePawns = new List<Pawn>();

            foreach (Pawn p in PawnsFinder.AllMaps_FreeColonistsSpawned)
            {
                //Log.Message("Pawn=" + p.Label);
                if (p.needs.joy != null && (p.needs.joy.CurLevelPercentage < 0.24000001f || p.GetTimeAssignment() == TimeAssignmentDefOf.Joy))
                {
                    if (p.needs.joy.tolerances.BoredOfAllAvailableJoyKinds(p))
                    {
                        legiblePawns.Add(p);
                    }
                }
            }

            /*if(legiblePawns.Count > 0)
            {
                __result = legiblePawns;
            }
            else
            {
                __result = null;
            }*/

            __result = legiblePawns;

            //NO Original method
            return false;
        }

        public static bool CompatPatch_VomitJob(ref JobDriver_Vomit __instance, ref IEnumerable<Toil> __result)
        {
            Pawn pawn = __instance.pawn;

            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                JobDriver_Vomit instance = __instance;

                List <Toil> toils = new List<Toil>();
                toils.Add(new Toil()
                {
                    initAction = delegate()
                    {
                        instance.pawn.jobs.StopAll();
                    }
                });

                __result = toils;
                return false;
            }

            return true;
        }

        /*public static bool CompatPatch_VomitJob(ref JobDriver_Vomit __instance)
        {
            Pawn pawn = __instance.pawn;

            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                //__instance.ended = true;
                return false;
            }

            return true;
        }*/

        public static bool CompatPatch_CanDoInteraction(ref bool __result, ref Pawn pawn)
        {
            if (pawn.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                __result = false;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_InspirationHandlerTick(ref InspirationHandler __instance)
        {
            if (__instance.pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                return false;
            }

            return true;
        }

        public static bool CompatPatch_AppendThoughts_ForHumanlike(ref Pawn victim)
        {
            if (victim.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.colonyCaresIfDead)
            {
                return false;
            }

            return true;
        }

        public static bool CompatPatch_InteractionsTrackerTick(ref Pawn_InteractionsTracker __instance)
        {
            Pawn pawn = Pawn_InteractionsTracker_GetPawn(__instance);

            if (pawn.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                return false;
            }

            return true;
        }

        public static bool CompatPatch_CanInteractNowWith(ref Pawn_InteractionsTracker __instance, ref bool __result, ref Pawn recipient)
        {
            //Pawn pawn = Pawn_InteractionsTracker_GetPawn(__instance);

            if (recipient.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                __result = false;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_SocialFightChance(ref Pawn_InteractionsTracker __instance, ref float __result, ref InteractionDef interaction, ref Pawn initiator)
        {
            Pawn pawn = Pawn_InteractionsTracker_GetPawn(__instance);

            if ((pawn.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize) || (initiator.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties propertiesTwo && !propertiesTwo.canSocialize))
            {
                __result = 0f;
                return false;
            }

            return true;
        }

        //erdelf: No special mechanoids in ancient dangers.
        public static void MechanoidsFixerAncient(ref bool __result, PawnKindDef kind)
        {
            if (kind.race.HasModExtension<MechanicalPawnProperties>()) __result = false;
        }

        //erdelf:  No special mechanoids in crashed ships.
        public static void MechanoidsFixer(ref bool __result, PawnKindDef def)
        {
            if (def.race.HasModExtension<MechanicalPawnProperties>()) __result = false;
        }

        public static bool CompatPatch_GetJoyTryGiveJob(ref JobGiver_EatInPartyArea __instance, ref Job __result, ref Pawn pawn)
        {
            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                __result = null;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_EatInPartyAreaTryGiveJob(ref JobGiver_EatInPartyArea __instance, ref Job __result, ref Pawn pawn)
        {
            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                __result = null;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_ShouldGuestKeepAttendingGathering(ref bool __result, ref Pawn p)
        {
            //Log.Message("Guest p=" + p?.ToString() ?? "null");
            if (p.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                //Log.Message("Guest (Mechanical) p=" + p.ToString());
                __result = !p.Downed && 
                    p.health.hediffSet.BleedRateTotal <= 0f && 
                    !p.health.hediffSet.HasTendableNonInjuryNonMissingPartHediff(false) && 
                    !p.InAggroMentalState && !p.IsPrisoner;
                return false;
            }

            //Log.Message("Guest NOT (Mechanical) p=" + p?.ToString() ?? "null");
            return true;
        }

        public static bool CompatPatch_TimetablePreventsLayDown(ref bool __result, ref Pawn pawn)
        {
            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                __result = pawn.timetable != null && !pawn.timetable.CurrentAssignment.allowRest;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_CalculatePain(ref HediffSet __instance, ref float __result)
        {
            if (__instance.pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                __result = 0f;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_ShouldBeDeadFromRequiredCapacity(ref Pawn_HealthTracker __instance, ref PawnCapacityDef __result)
        {
            Pawn pawn = Pawn_HealthTracker_GetPawn(__instance);

            if (pawn.def.HasModExtension<MechanicalPawnProperties>())
            {
                List<PawnCapacityDef> allDefsListForReading = DefDatabase<PawnCapacityDef>.AllDefsListForReading;
                for (int i = 0; i < allDefsListForReading.Count; i++)
                {
                    PawnCapacityDef pawnCapacityDef = allDefsListForReading[i];
                    if (allDefsListForReading[i] == PawnCapacityDefOf.Consciousness && !__instance.capacities.CapableOf(pawnCapacityDef))
                    {
                        __result = pawnCapacityDef;
                        return false;
                    }
                }

                __result = null;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_WillIngestStackCountOf(int __result, ref Pawn ingester, ref ThingDef def)
        {
            if (ingester == null)
                return true;

            bool haveNeed = ingester?.needs.TryGetNeed(NeedDefOf.Food) != null;

            if (!haveNeed)
            {
                __result = 0;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_ShouldMeasureTimeNow(bool __result, ref Pawn pawn)
        {
            if (pawn == null)
                return true;

            bool haveNeed = pawn?.needs.TryGetNeed(NeedDefOf.Rest) != null;

            if (!haveNeed)
            {
                __result = pawn.InBed() && (HealthAIUtility.ShouldSeekMedicalRestUrgent(pawn) || (HealthAIUtility.ShouldSeekMedicalRest(pawn) && pawn.CurJob.restUntilHealed)); ;
                return false;
            }

            return true;
        }

        public static bool CompatPatch_CanInitiateInteraction(bool __result, ref Pawn pawn)
        {
            if (pawn.def.GetModExtension<MechanicalPawnProperties>() is MechanicalPawnProperties properties && !properties.canSocialize)
            {
                __result = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the Pawn_NeedsTracker class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static Pawn Pawn_HealthTracker_GetPawn(Pawn_HealthTracker instance)
        {
            return (Pawn)int_Pawn_HealthTracker_GetPawn.GetValue(instance);
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the Pawn_NeedsTracker class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static Pawn Pawn_InteractionsTracker_GetPawn(Pawn_InteractionsTracker instance)
        {
            return (Pawn)int_Pawn_InteractionsTracker_GetPawn.GetValue(instance);
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the Pawn_NeedsTracker class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static Pawn Pawn_NeedsTracker_GetPawn(Pawn_NeedsTracker instance)
        {
            return (Pawn)int_Pawn_NeedsTracker_GetPawn.GetValue(instance);
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the Need_Food class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static Pawn Need_Food_Starving_GetPawn(Need_Food instance)
        {
            return (Pawn)int_Need_Food_Starving_GetPawn.GetValue(instance);
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the PawnRenderer class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static Pawn PawnRenderer_GetPawn_GetPawn(PawnRenderer instance)
        {
            return (Pawn)int_PawnRenderer_GetPawn.GetValue(instance);
        }

        /// <summary>
        /// Accesses the private (For whatever reason) pawn field in the PawnRenderer class.
        /// </summary>
        /// <param name="instance">Instance where we should access the value.</param>
        /// <returns>Pawn if it got a pawn, null if it got no pawn.</returns>
        public static NeedDef ThinkNode_ConditionalNeedPercentageAbove_GetNeed(ThinkNode_ConditionalNeedPercentageAbove instance)
        {
            return (NeedDef)int_ConditionalPercentageNeed_need.GetValue(instance);
        }

        /// <summary>
        /// Adds a null check.
        /// </summary>
        public static bool Patch_ThinkNode_ConditionalNeedPercentageAbove_Satisfied(ref ThinkNode_ConditionalNeedPercentageAbove __instance, ref bool __result, ref Pawn pawn)
        {
            NeedDef need = ThinkNode_ConditionalNeedPercentageAbove_GetNeed(__instance);
            bool haveNeed = pawn.needs.TryGetNeed(need) != null;

            if(!haveNeed)
            {
                __result = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Essentially makes it so Androids can't starve.
        /// </summary>
        public static bool Patch_HealthUtility_AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
        {
            if (pawn.IsAndroid() && hdDef == RimWorld.HediffDefOf.Malnutrition)
            {
                //Override the method. (Do nothing in this case)
                return false;
            }
            else
            {
                //Let original method run.
                return true;
            }
        }

        /// <summary>
        /// Adds an additional check for our custom needs.
        /// </summary>
        public static void Patch_Pawn_NeedsTracker_ShouldHaveNeed(ref Pawn_NeedsTracker __instance, ref bool __result, ref NeedDef nd)
        {
            //Do not bother checking if our need do not exist.
            Pawn pawn = Pawn_NeedsTracker_GetPawn(__instance);

            if (NeedsDefOf.ChJEnergy != null)
            {
                //Is the need our Energy need?
                if(nd == NeedsDefOf.ChJEnergy)
                {
                    if (pawn.IsAndroid() || pawn.def.HasModExtension<MechanicalPawnProperties>())
                    {
                        __result = true;
                    }
                    else
                    {
                        __result = false;
                    }
                }
            }

            if(!AndroidsModSettings.Instance.droidCompatibilityMode)
            {
                if (
                nd == NeedDefOf.Food || nd == NeedDefOf.Rest || nd == NeedDefOf.Joy ||
                nd == NeedsDefOf.Beauty || nd == NeedsDefOf.Comfort || nd == NeedsDefOf.RoomSize ||
                nd == NeedsDefOf.Outdoors ||
                (Need_Bladder != null && nd == Need_Bladder) || (Need_Hygiene != null && nd == Need_Hygiene))
                {
                    if (pawn.def.HasModExtension<MechanicalPawnProperties>())
                    {
                        __result = false;
                    }
                }
            }
        }

        /// <summary>
        /// Adds glowing eyes to anything mechanical.
        /// </summary>
        public static void Patch_PawnRenderer_RenderPawnInternal(ref PawnRenderer __instance, Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
        {
            if(__instance != null && AndroidsModSettings.Instance.androidEyeGlow)
            {
                Pawn pawn = PawnRenderer_GetPawn_GetPawn(__instance);

                //Draw glowing eyes.                                                                                Null check galore!
                if (pawn != null && pawn.IsAndroid() && !pawn.Dead && !headStump &&  ((!portrait && pawn?.jobs?.curDriver != null ? !pawn.jobs.curDriver.asleep : portrait) || portrait))
                {
                    Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);

                    //Get base offset.
                    Vector3 baseHeadOffset = rootLoc;
                    if (bodyFacing != Rot4.North)
                    {
                        baseHeadOffset.y += 0.0281250011f;
                        rootLoc.y += 0.0234375f;
                    }
                    else
                    {
                        baseHeadOffset.y += 0.0234375f;
                        rootLoc.y += 0.0281250011f;
                    }

                    Vector3 headOffset = quat * __instance.BaseHeadOffsetAt(headFacing);

                    //Finalize offset.
                    Vector3 eyeOffset = baseHeadOffset + headOffset + new Vector3(0f, 0.01f, 0f);

                    //Render eyes.
                    if (headFacing != Rot4.North)
                    {
                        //Is not the back.
                        Mesh headMesh = MeshPool.humanlikeHeadSet.MeshAt(headFacing);

                        if (headFacing.IsHorizontal)
                        {
                            //Side
                            GenDraw.DrawMeshNowOrLater(headMesh, eyeOffset, quat, EffectTextures.GetEyeGraphic(false, pawn.story.hairColor.SaturationChanged(0.6f)).MatSingle, portrait);
                        }
                        else
                        {
                            //Front
                            GenDraw.DrawMeshNowOrLater(headMesh, eyeOffset, quat, EffectTextures.GetEyeGraphic(true, pawn.story.hairColor.SaturationChanged(0.6f)).MatSingle, portrait);
                        }
                    }
                }
            }
        }

        public static void Patch_Need_Food_Starving_Get(ref Need_Food __instance, ref bool __result)
        {
            Pawn pawn = Need_Food_Starving_GetPawn(__instance);

            if (pawn != null && pawn.IsAndroid())
                __result = false;
        }
    }
}
