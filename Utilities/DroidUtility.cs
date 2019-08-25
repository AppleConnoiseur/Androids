using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;
using static AlienRace.AlienPartGenerator;

namespace Androids
{
    /// <summary>
    /// Utility creating and modifying Droids.
    /// </summary>
    public static class DroidUtility
    {
        private static List<Hediff> tmpHediffsToTend = new List<Hediff>();
        private static List<Hediff> tmpHediffs = new List<Hediff>();

        /// <summary>
        /// Creates a Droid template.
        /// </summary>
        /// <param name="raceDef">ThingDef to use as race.</param>
        /// <param name="pawnKindDef">PawnKindDef to use as kind.</param>
        /// <param name="faction">Faction that owns this Droid.</param>
        /// <param name="map">Map to spawn in.</param>
        /// <returns>New Pawn if successful. Null if not.</returns>
        public static Pawn MakeDroidTemplate(PawnKindDef pawnKindDef, Faction faction, int tile, List<SkillRequirement> skills = null, int defaultSkillLevel = 6)
        {
            Map map = null;
            if(tile > -1)
            {
                map = Current.Game?.FindMap(tile);
            }

            //Log.Message("Map: " + map);

            //Manually craft a Droid Pawn.
            Pawn pawnBeingCrafted = (Pawn)ThingMaker.MakeThing(pawnKindDef.race);
            if (pawnBeingCrafted == null)
                return null;

            //Kind, Faction and initial Components.
            pawnBeingCrafted.kindDef = pawnKindDef;
            if(faction != null)
            {
                pawnBeingCrafted.SetFactionDirect(faction);
            }
            PawnComponentsUtility.CreateInitialComponents(pawnBeingCrafted);

            //Gender
            pawnBeingCrafted.gender = Gender.Male;

            //Set Needs at initial levels.
            pawnBeingCrafted.needs.SetInitialLevels();

            //Set age
            pawnBeingCrafted.ageTracker.AgeBiologicalTicks = 0;
            pawnBeingCrafted.ageTracker.AgeChronologicalTicks = 0;

            //Set Story
            if (pawnBeingCrafted.RaceProps.Humanlike)
            {
                DroidSpawnProperties spawnProperties = pawnKindDef.race.GetModExtension<DroidSpawnProperties>();

                if (spawnProperties != null)
                {
                    pawnBeingCrafted.gender = spawnProperties.gender;
                    pawnBeingCrafted.playerSettings.hostilityResponse = spawnProperties.hostileResponse;
                }

                //Appearance
                pawnBeingCrafted.story.melanin = 1f;
                pawnBeingCrafted.story.crownType = CrownType.Average;

                if(spawnProperties != null && spawnProperties.generateHair)
                {
                    IEnumerable<HairDef> source = from hair in DefDatabase<HairDef>.AllDefs
                                                  where hair.hairTags.SharesElementWith(spawnProperties.hairTags)
                                                  select hair;
                    HairDef resultHair = source.RandomElementByWeightWithFallback((hair) => HairChoiceLikelihoodFor(hair, pawnBeingCrafted), DefDatabase<HairDef>.GetNamed("Shaved"));

                    pawnBeingCrafted.story.hairDef = resultHair;

                    if(pawnBeingCrafted.def is ThingDef_AlienRace alienRaceDef)
                    {
                        pawnBeingCrafted.story.hairColor = alienRaceDef.alienRace?.generalSettings?.alienPartGenerator?.alienhaircolorgen?.NewRandomizedColor() ?? new UnityEngine.Color(1f, 1f, 1f, 1f);
                    }
                }
                else
                {
                    pawnBeingCrafted.story.hairColor = new UnityEngine.Color(1f, 1f, 1f, 1f);
                    pawnBeingCrafted.story.hairDef = DefDatabase<HairDef>.GetNamed("Shaved");
                }
                

                if (spawnProperties != null && spawnProperties.bodyType != null)
                {
                    pawnBeingCrafted.story.bodyType = spawnProperties.bodyType;
                }
                else
                {
                    pawnBeingCrafted.story.bodyType = BodyTypeDefOf.Thin;
                }

                PortraitsCache.SetDirty(pawnBeingCrafted);
                
                //Backstory
                Backstory backstory = null;
                if (spawnProperties != null && spawnProperties.backstory != null)
                {
                    BackstoryDatabase.TryGetWithIdentifier(spawnProperties.backstory.defName, out backstory);
                }
                else
                {
                    BackstoryDatabase.TryGetWithIdentifier("ChJAndroid_Droid", out backstory);
                }
                
                pawnBeingCrafted.story.childhood = backstory;

                //Skills
                if(skills == null || skills.Count <= 0)
                {
                    if(spawnProperties != null)
                    {
                        //Set all skills to default first.
                        foreach(SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
                        {
                            SkillRecord skill = pawnBeingCrafted.skills.GetSkill(skillDef);
                            skill.Level = spawnProperties.defaultSkillLevel;
                        }

                        //Set skills and passions.
                        foreach (DroidSkill droidSkill in spawnProperties.skills)
                        {
                            SkillRecord skill = pawnBeingCrafted.skills.GetSkill(droidSkill.def);
                            if(skill != null)
                            {
                                skill.Level = droidSkill.level;
                                skill.passion = droidSkill.passion;
                            }
                        }
                    }
                    else
                    {
                        List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                        for (int i = 0; i < allDefsListForReading.Count; i++)
                        {
                            SkillDef skillDef = allDefsListForReading[i];
                            SkillRecord skill = pawnBeingCrafted.skills.GetSkill(skillDef);

                            if (skillDef == SkillDefOf.Shooting || skillDef == SkillDefOf.Melee || skillDef == SkillDefOf.Mining || skillDef == SkillDefOf.Plants)
                                skill.Level = 8;
                            else
                                if (skillDef == SkillDefOf.Medicine || skillDef == SkillDefOf.Crafting || skillDef == SkillDefOf.Cooking)
                                skill.Level = 4;
                            else
                                skill.Level = 6;
                            skill.passion = Passion.None;
                        }
                    }
                }
                else
                {
                    List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
                    for (int i = 0; i < allDefsListForReading.Count; i++)
                    {
                        SkillDef skillDef = allDefsListForReading[i];
                        SkillRecord skill = pawnBeingCrafted.skills.GetSkill(skillDef);

                        SkillRequirement skillRequirement = skills.First(sr => sr.skill == skillDef);
                        if (skillRequirement != null)
                        {
                            skill.Level = skillRequirement.minLevel;
                        }
                        else
                        {
                            skill.Level = defaultSkillLevel;
                        }
                        
                        skill.passion = Passion.None;
                    }
                }
            }

            //Work settings
            if (pawnBeingCrafted.workSettings != null)
            {
                pawnBeingCrafted.workSettings.EnableAndInitialize();
            }

            //Name
            if(map != null && faction.IsPlayer)
            {
                var names = from pawn in map.mapPawns.FreeColonists
                            select pawn.Name;

                if (names != null)
                {
                    int droidNameCount = names.Count(name => name.ToStringShort.ToLower().StartsWith(pawnKindDef.race.label.ToLower()));
                    string finalShortName = pawnKindDef.race.LabelCap + " " + droidNameCount;
                    pawnBeingCrafted.Name = MakeDroidName(finalShortName);
                }
                else
                {
                    pawnBeingCrafted.Name = MakeDroidName(null);
                }
            }
            else
            {
                pawnBeingCrafted.Name = MakeDroidName(null);
            }

            return pawnBeingCrafted;
        }

        public static float HairChoiceLikelihoodFor(HairDef hair, Pawn pawn)
        {
            float result;
            if (pawn.gender == Gender.None)
            {
                result = 100f;
            }
            else
            {
                if (pawn.gender == Gender.Male)
                {
                    switch (hair.hairGender)
                    {
                        case HairGender.Male:
                            return 70f;
                        case HairGender.MaleUsually:
                            return 30f;
                        case HairGender.Any:
                            return 60f;
                        case HairGender.FemaleUsually:
                            return 5f;
                        case HairGender.Female:
                            return 1f;
                    }
                }
                if (pawn.gender == Gender.Female)
                {
                    switch (hair.hairGender)
                    {
                        case HairGender.Male:
                            return 1f;
                        case HairGender.MaleUsually:
                            return 5f;
                        case HairGender.Any:
                            return 60f;
                        case HairGender.FemaleUsually:
                            return 30f;
                        case HairGender.Female:
                            return 70f;
                    }
                }
                Log.Error(string.Concat(new object[]
                {
                    "Unknown hair likelihood for ",
                    hair,
                    " with ",
                    pawn
                }), false);
                result = 0f;
            }
            return result;
        }

        public static Pawn MakeCustomDroid(PawnKindDef pawnKind, Faction faction)
        {
            Pawn pawnBeingCrafted =
                PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                    pawnKind,
                    faction,
                    fixedBiologicalAge: 0,
                    fixedChronologicalAge: 0
                    ));

            return pawnBeingCrafted;
        }

        public static NameTriple MakeDroidName(string nickName)
        {
            string finalLongName = String.Format("D-{0:X}-{1:X}", Rand.Range(0, 256), Rand.Range(0, 256));

            if(nickName == null)
                return new NameTriple(finalLongName, finalLongName, "");
            else
                return new NameTriple(finalLongName, nickName, "");
        }

        public static void DoTend(Pawn doctor, Pawn patient, Thing medicine)
        {
            if (patient.health.HasHediffsNeedingTend(false))
            {
                if (medicine != null && medicine.Destroyed)
                {
                    Log.Warning("Tried to use destroyed repair kit.", false);
                    medicine = null;
                }

                //float quality = 1f; //CalculateBaseTendQuality
                GetOptimalHediffsToTendWithSingleTreatment(patient, medicine != null, tmpHediffsToTend, null);

                for (int i = 0; i < tmpHediffsToTend.Count; i++)
                {
                    //tmpHediffsToTend[i].Tended(quality, i);
                    if(medicine == null)
                    {
                        tmpHediffsToTend[i].Tended(0.1f, i);
                    }
                    else
                    {
                        patient.health.RemoveHediff(tmpHediffsToTend[i]);
                    }
                }
                if (doctor != null && doctor.Faction == Faction.OfPlayer && patient.Faction != doctor.Faction && !patient.IsPrisoner && patient.Faction != null)
                {
                    patient.mindState.timesGuestTendedToByPlayer++;
                }
                if (doctor != null && doctor.IsColonistPlayerControlled)
                {
                    patient.records.AccumulateStoryEvent(StoryEventDefOf.TendedByPlayer);
                }
                if (doctor != null && doctor.RaceProps.Humanlike && patient.RaceProps.Animal)
                {
                    if (RelationsUtility.TryDevelopBondRelation(doctor, patient, 0.004f))
                    {
                        if (doctor.Faction != null && doctor.Faction != patient.Faction)
                        {
                            InteractionWorker_RecruitAttempt.DoRecruit(doctor, patient, 1f, false);
                        }
                    }
                }
                patient.records.Increment(RecordDefOf.TimesTendedTo);
                if (doctor != null)
                {
                    doctor.records.Increment(RecordDefOf.TimesTendedOther);
                }
                if (doctor == patient && !doctor.Dead)
                {
                    doctor.mindState.Notify_SelfTended();
                }
                if (medicine != null)
                {
                    if (patient.Spawned || (doctor != null && doctor.Spawned))
                    {
                        //if (medicine != null && medicine.GetStatValue(StatDefOf.MedicalPotency, true) > RimWorld.ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency, null))
                        {
                            SoundDefOf.Building_Complete.PlayOneShot(new TargetInfo(patient.Position, patient.Map, false));
                        }
                    }
                    if (medicine.stackCount > 1)
                    {
                        medicine.stackCount--;
                    }
                    else if (!medicine.Destroyed)
                    {
                        medicine.Destroy(DestroyMode.Vanish);
                    }
                }
            }
        }

        public static void GetOptimalHediffsToTendWithSingleTreatment(Pawn patient, bool usingMedicine, List<Hediff> outHediffsToTend, List<Hediff> tendableHediffsInTendPriorityOrder = null)
        {
            outHediffsToTend.Clear();
            tmpHediffs.Clear();
            if (tendableHediffsInTendPriorityOrder != null)
            {
                tmpHediffs.AddRange(tendableHediffsInTendPriorityOrder);
            }
            else
            {
                List<Hediff> hediffs = patient.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    if (hediffs[i].TendableNow(false))
                    {
                        tmpHediffs.Add(hediffs[i]);
                    }
                }
                TendUtility.SortByTendPriority(tmpHediffs);
            }
            if (tmpHediffs.Any<Hediff>())
            {
                Hediff hediff = tmpHediffs[0];
                outHediffsToTend.Add(hediff);
                HediffCompProperties_TendDuration hediffCompProperties_TendDuration = hediff.def.CompProps<HediffCompProperties_TendDuration>();
                if (hediffCompProperties_TendDuration != null && hediffCompProperties_TendDuration.tendAllAtOnce)
                {
                    for (int j = 0; j < tmpHediffs.Count; j++)
                    {
                        if (tmpHediffs[j] != hediff && tmpHediffs[j].def == hediff.def)
                        {
                            outHediffsToTend.Add(tmpHediffs[j]);
                        }
                    }
                }
                else if (hediff is Hediff_Injury && usingMedicine)
                {
                    float num = hediff.Severity;
                    for (int k = 0; k < tmpHediffs.Count; k++)
                    {
                        if (tmpHediffs[k] != hediff)
                        {
                            Hediff_Injury hediff_Injury = tmpHediffs[k] as Hediff_Injury;
                            if (hediff_Injury != null)
                            {
                                float severity = hediff_Injury.Severity;
                                if (num + severity <= 20f)
                                {
                                    num += severity;
                                    outHediffsToTend.Add(hediff_Injury);
                                }
                            }
                        }
                    }
                }
                tmpHediffs.Clear();
            }
        }
    }
}
