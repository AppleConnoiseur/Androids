using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using static AlienRace.AlienPartGenerator;

namespace Androids
{
    /// <summary>
    /// The customization Window for Androids.
    /// </summary>
    public class CustomizeAndroidWindow : Window
    {
        //Variables
        public Building_AndroidPrinter androidPrinter;
        public Pawn newAndroid;
        public List<ThingOrderRequest> finalCalculatedPrintingCost = new List<ThingOrderRequest>();
        public int finalExtraPrintingTimeCost = 0;
        public bool refreshAndroidPortrait = false;
        public Vector2 upgradesScrollPosition = new Vector2();
        public Vector2 traitsScrollPosition = new Vector2();
        List<Trait> allTraits = new List<Trait>();

        //Customization
        public PawnKindDef currentPawnKindDef;
        public Backstory newChildhoodBackstory;
        public Backstory newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;
        public List<UpgradeCommand> appliedUpgradeCommands = new List<UpgradeCommand>();

        //Original android values
        public List<Trait> originalTraits = new List<Trait>();

        //Static Values
        public override Vector2 InitialSize => new Vector2(898f, 608f);
        public static readonly float upgradesOffset = 640f;
        private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);
        private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 0f),
                true
            },
            {
                new CurvePoint(0.5f, 150f),
                true
            },
            {
                new CurvePoint(4f, 150f),
                true
            },
            {
                new CurvePoint(5f, 25f),
                true
            },
            {
                new CurvePoint(10f, 5f),
                true
            },
            {
                new CurvePoint(15f, 0f),
                true
            }
        };
        private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 0f),
                true
            },
            {
                new CurvePoint(10f, 10f),
                true
            },
            {
                new CurvePoint(20f, 16f),
                true
            },
            {
                new CurvePoint(27f, 20f),
                true
            }
        };

        public static List<Color> DefaultHairColors = new List<Color>(new Color[] {
            //Mundane
            new Color(0.17f, 0.17f, 0.17f, 1),
            new Color(0.02f, 0.02f, 0.02f, 1f),
            new Color(0.90f, 0.90f, 0.90f, 1f),
            new Color(0.51f, 0.25f, 0.25f, 1f),
            new Color(1.00f, 0.66f, 0.32f, 1f),

            //Exotic
            new Color(0.0f, 0.5f, 1.0f, 1f),
            new Color(1.0f, 0.00f, 0.5f, 1f),
            new Color(1.00f, 0.00f, 0.00f, 1f),
            new Color(0.00f, 1.00f, 0.00f, 1f),
            new Color(0.00f, 1.00f, 1.00f, 1f),
            new Color(0.78f, 0.78f, 0.78f, 1f),
            new Color(0.92f, 0.92f, 0.29f, 1f),
            new Color(0.63f, 0.28f, 0.64f, 1f)
            });

        public IEnumerable<Color> HairColors
        {
            get
            {
                ThingDef_AlienRace alien = ThingDefOf.ChjAndroid as ThingDef_AlienRace;
                if(alien == null)
                {
                    foreach (Color color in DefaultHairColors)
                        yield return color;
                }
                else
                {
                    if(alien.alienRace.generalSettings.alienPartGenerator.alienhaircolorgen is ColorGenerator_Options colorOptions)
                    {
                        foreach(ColorOption colorOption in colorOptions.options)
                            yield return colorOption.only;
                    }
                    else
                    {
                        foreach (Color color in DefaultHairColors)
                            yield return color;
                    }
                }

                yield break;
            }
        }

        public IEnumerable<Color> SkinColors
        {
            get
            {
                ThingDef_AlienRace alien = ThingDefOf.ChjAndroid as ThingDef_AlienRace;
                if (alien != null && alien.alienRace.generalSettings.alienPartGenerator.alienskincolorgen is ColorGenerator_Options colorOptions)
                {
                    foreach (ColorOption colorOption in colorOptions.options)
                        yield return colorOption.only;
                }

                yield break;
            }
        }

        public CustomizeAndroidWindow(Building_AndroidPrinter androidPrinter)
        {
            this.androidPrinter = androidPrinter;
            currentPawnKindDef = PawnKindDef.Named("ChjAndroidColonist");
            newAndroid = GetNewPawn();
            RefreshCosts();
        }

        public override void DoWindowContents(Rect inRect)
        {
            //Detect changes
            if (refreshAndroidPortrait)
            {
                newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(newAndroid);
                PortraitsCache.PortraitsCacheUpdate();

                refreshAndroidPortrait = false;
            }

            if (newChildhoodBackstory != null)
            {
                newAndroid.story.childhood = newChildhoodBackstory;
                newChildhoodBackstory = null;
                RefreshPawn();
            }

            if (newAdulthoodBackstory != null)
            {
                newAndroid.story.adulthood = newAdulthoodBackstory;
                newAdulthoodBackstory = null;
                RefreshPawn();
            }

            if(newTrait != null)
            {
                if(replacedTrait != null)
                {
                    newAndroid.story.traits.allTraits.Remove(replacedTrait);
                    replacedTrait = null;
                }

                Trait gainedTrait = new Trait(newTrait.def, newTrait.Degree);

                //newAndroid.story.traits.GainTrait(gainedTrait);

                newAndroid.story.traits.allTraits.Add(gainedTrait);
                if (newAndroid.workSettings != null)
                {
                    newAndroid.workSettings.Notify_GainedTrait();
                }
                if (newAndroid.skills != null)
                {
                    newAndroid.skills.Notify_SkillDisablesChanged();
                }
                if (newAndroid.RaceProps.Humanlike)
                {
                    newAndroid.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                }

                RefreshPawn();

                //Log.Message("DoWindowContents: gainedTrait: " + gainedTrait?.LabelCap ?? "NO TRAIT WTF!?");
                newTrait = null;
            }

            Rect pawnRect = new Rect(inRect);
            pawnRect.width = PawnPortraitSize.x + 16f;
            pawnRect.height = PawnPortraitSize.y + 16f;
            pawnRect = pawnRect.CenteredOnXIn(inRect);
            pawnRect = pawnRect.CenteredOnYIn(inRect);
            pawnRect.x += 16f;
            pawnRect.y += 16f;

            //Draw Pawn stuff.
            if (newAndroid != null)
            {
                //Pawn
                Rect pawnRenderRect = new Rect(pawnRect.xMin + (pawnRect.width - PawnPortraitSize.x) / 2f - 10f, pawnRect.yMin + 20f, PawnPortraitSize.x, PawnPortraitSize.y);
                GUI.DrawTexture(pawnRenderRect, PortraitsCache.Get(newAndroid, PawnPortraitSize, default(Vector3), 1f));

                Widgets.InfoCardButton(pawnRenderRect.xMax - 16f, pawnRenderRect.y, newAndroid);

                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AndroidCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;

                //Name
                float row = 32f;
                {
                    Rect rowRect = new Rect(32, row, 256f - 16f, 24f);
                    NameTriple nameTriple = newAndroid.Name as NameTriple;
                    if (nameTriple != null)
                    {
                        Rect rect3 = new Rect(rowRect);
                        rect3.width *= 0.333f;
                        Rect rect4 = new Rect(rowRect);
                        rect4.width *= 0.333f;
                        rect4.x += rect4.width;
                        Rect rect5 = new Rect(rowRect);
                        rect5.width *= 0.333f;
                        rect5.x += rect4.width * 2f;
                        string first = nameTriple.First;
                        string nick = nameTriple.Nick;
                        string last = nameTriple.Last;
                        CharacterCardUtility.DoNameInputRect(rect3, ref first, 12);
                        if (nameTriple.Nick == nameTriple.First || nameTriple.Nick == nameTriple.Last)
                        {
                            GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        }
                        CharacterCardUtility.DoNameInputRect(rect4, ref nick, 9);
                        GUI.color = Color.white;
                        CharacterCardUtility.DoNameInputRect(rect5, ref last, 12);
                        if (nameTriple.First != first || nameTriple.Nick != nick || nameTriple.Last != last)
                        {
                            newAndroid.Name = new NameTriple(first, nick, last);
                        }
                        TooltipHandler.TipRegion(rect3, "FirstNameDesc".Translate());
                        TooltipHandler.TipRegion(rect4, "ShortIdentifierDesc".Translate());
                        TooltipHandler.TipRegion(rect5, "LastNameDesc".Translate());
                    }
                    else
                    {
                        rowRect.width = 999f;
                        Text.Font = GameFont.Medium;
                        Widgets.Label(rowRect, newAndroid.Name.ToStringFull);
                        Text.Font = GameFont.Small;
                    }
                }

                //Hair customization
                float finalPawnCustomizationWidthOffset = (pawnRect.x + pawnRect.width + 16f + (inRect.width - upgradesOffset));

                {
                    Rect rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y, inRect.width - finalPawnCustomizationWidthOffset, 24f);

                    //Color
                    //newAndroid.story.hairColor
                    Rect hairColorRect = new Rect(rowRect);
                    hairColorRect.width = hairColorRect.height;

                    Widgets.DrawBoxSolid(hairColorRect, newAndroid.story.hairColor);
                    Widgets.DrawBox(hairColorRect);
                    Widgets.DrawHighlightIfMouseover(hairColorRect);

                    if (Widgets.ButtonInvisible(hairColorRect))
                    {
                        //Change color
                        Func<Color, Action> setColorAction = (Color color) => delegate {
                            newAndroid.story.hairColor = color;
                            newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                            PortraitsCache.SetDirty(newAndroid);
                            PortraitsCache.PortraitsCacheUpdate();
                        };

                        List<FloatMenuOption> list = new List<FloatMenuOption>();
                        foreach(Color hairColor in HairColors)
                        {
                            list.Add(new FloatMenuOption("AndroidCustomizationChangeColor".Translate(), setColorAction(hairColor), MenuOptionPriority.Default, null, null, 24f, delegate (Rect rect)
                            {
                                Rect colorRect = new Rect(rect);
                                colorRect.x += 8f;
                                Widgets.DrawBoxSolid(colorRect, hairColor);
                                Widgets.DrawBox(colorRect);
                                return false;
                            }, null));
                        }
                        Find.WindowStack.Add(new FloatMenu(list));
                    }

                    //Type
                    //newAndroid.story.hairDef
                    Rect hairTypeRect = new Rect(rowRect);
                    hairTypeRect.width -= hairColorRect.width;
                    hairTypeRect.width -= 8f;
                    hairTypeRect.x = hairColorRect.x + hairColorRect.width + 8f;

                    if (Widgets.ButtonText(hairTypeRect, newAndroid?.story?.hairDef?.LabelCap ?? "Bald"))
                    {
                        //Change hairstyle
                        //FloatMenuUtility.

                        IEnumerable<HairDef> hairs = 
                            from hairdef in DefDatabase<HairDef>.AllDefs
                            where (newAndroid.gender == Gender.Female && (hairdef.hairGender == HairGender.Any || hairdef.hairGender == HairGender.Female || hairdef.hairGender == HairGender.FemaleUsually)) || (newAndroid.gender == Gender.Male && (hairdef.hairGender == HairGender.Any || hairdef.hairGender == HairGender.Male || hairdef.hairGender == HairGender.MaleUsually))
                            select hairdef;

                        if(hairs != null)
                        {
                            FloatMenuUtility.MakeMenu<HairDef>(hairs, hairDef => hairDef.LabelCap, (HairDef hairDef) => delegate
                            {
                                newAndroid.story.hairDef = hairDef;
                                newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                                PortraitsCache.SetDirty(newAndroid);
                                PortraitsCache.PortraitsCacheUpdate();
                            });
                        }
                    }
                }

                //Print button
                {
                    Rect rowRect = new Rect(pawnRect.x + pawnRect.width + 16f, pawnRect.y + 32f, inRect.width - finalPawnCustomizationWidthOffset, 32f);
                    Text.Font = GameFont.Medium;
                    if (Widgets.ButtonText(rowRect, "AndroidCustomizationPrint".Translate()))
                    {
                        androidPrinter.orderProcessor.requestedItems = finalCalculatedPrintingCost;
                        androidPrinter.extraTimeCost = finalExtraPrintingTimeCost;
                        androidPrinter.pawnToPrint = newAndroid;
                        androidPrinter.printerStatus = CrafterStatus.Filling;
                        Close();
                    }
                    Text.Font = GameFont.Small;
                }

                //Race selector (If possible)
                if(RaceUtility.AlienRaceKinds.Count() > 1)
                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 256f - 16f, 24f);

                    if (Widgets.ButtonText(rowRect, currentPawnKindDef.race.LabelCap))
                    {
                        FloatMenuUtility.MakeMenu<PawnKindDef>(RaceUtility.AlienRaceKinds, raceKind => raceKind.race.LabelCap, (PawnKindDef raceKind) => delegate
                        {
                            currentPawnKindDef = raceKind;

                            //Figure out default gender.
                            Gender defaultGender = Gender.Female;

                            ThingDef_AlienRace alienRaceDef = currentPawnKindDef.race as ThingDef_AlienRace;
                            if(alienRaceDef != null)
                            {
                                if (alienRaceDef.alienRace.generalSettings.maleGenderProbability >= 1f)
                                {
                                    defaultGender = Gender.Male;
                                }
                            }

                            newAndroid = GetNewPawn(defaultGender);
                            RefreshUpgrades();
                            RefreshCosts();
                        });
                    }

                    row += 26f;
                }

                //Generate new pawn
                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 128f - 8f, 24f);

                    //Only allow to roll females if the gender probability is less than 1.0
                    ThingDef_AlienRace alienRaceDef = currentPawnKindDef.race as ThingDef_AlienRace;
                    if (alienRaceDef != null)
                    {
                        if (alienRaceDef.alienRace.generalSettings.maleGenderProbability < 1f)
                        {
                            if (Widgets.ButtonText(rowRect, "AndroidCustomizationRollFemale".Translate()))
                            {
                                newAndroid.Destroy();
                                newAndroid = GetNewPawn(Gender.Female);
                                RefreshUpgrades();
                                RefreshCosts();
                            }
                        }
                    }

                    rowRect = new Rect(32 + 16f + 256f + 128f - 8f, row, 128f - 8f, 24f);

                    if (Widgets.ButtonText(rowRect, "AndroidCustomizationRollMale".Translate()))
                    {
                        newAndroid.Destroy();
                        newAndroid = GetNewPawn(Gender.Male);
                        RefreshUpgrades();
                        RefreshCosts();
                    }
                }

                //Backstories
                row += 26f;
                {
                    Rect rowRect = new Rect(32f, row, 256f - 16f, 24f);

                    Widgets.DrawBox(rowRect);
                    Widgets.DrawHighlightIfMouseover(rowRect);

                    string label = "";

                    if (newAndroid.story.childhood != null)
                        label = "AndroidCustomizationFirstIdentity".Translate() + " " + newAndroid.story.childhood.TitleCapFor(newAndroid.gender);
                    else
                        label = "AndroidCustomizationFirstIdentity".Translate() + " " + "AndroidNone".Translate();

                    if (Widgets.ButtonText(rowRect, label))
                    {
                        IEnumerable<Backstory> backstories = from backstory in (from backstoryPair in BackstoryDatabase.allBackstories
                                                              select backstoryPair.Value)
                                                             where (backstory.spawnCategories.Any(category => currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category)) || backstory.spawnCategories.Contains("ChjAndroid")) && backstory.slot == BackstorySlot.Childhood
                                                             select backstory;
                        FloatMenuUtility.MakeMenu<Backstory>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (Backstory backstory) => delegate
                        {
                            newChildhoodBackstory = backstory;
                        });
                    }

                    if (newAndroid.story.childhood != null)
                        TooltipHandler.TipRegion(rowRect, newAndroid.story.childhood.FullDescriptionFor(newAndroid));
                }
                    
                {
                    Rect rowRect = new Rect(32 + 16f + 256f, row, 256f - 16f, 24f);

                    Widgets.DrawBox(rowRect);
                    Widgets.DrawHighlightIfMouseover(rowRect);

                    string label = "";

                    if (newAndroid.story.adulthood != null)
                        label = "AndroidCustomizationSecondIdentity".Translate() + " " + newAndroid.story.adulthood.TitleCapFor(newAndroid.gender);
                    else
                        label = "AndroidCustomizationSecondIdentity".Translate() + " " + "AndroidNone".Translate();

                    if (Widgets.ButtonText(rowRect, label))
                    {
                        IEnumerable<Backstory> backstories = from backstory in (from backstoryPair in BackstoryDatabase.allBackstories
                                                                                select backstoryPair.Value)
                                                             where (backstory.spawnCategories.Any(category => currentPawnKindDef.backstoryCategories.Any(subCategory => subCategory == category)) || backstory.spawnCategories.Contains("ChjAndroid")) && backstory.slot == BackstorySlot.Adulthood
                                                             select backstory;
                        FloatMenuUtility.MakeMenu<Backstory>(backstories, backstory => backstory.TitleCapFor(newAndroid.gender), (Backstory backstory) => delegate
                        {
                            newAdulthoodBackstory = backstory;
                        });
                    }

                    if(newAndroid.story.adulthood != null)
                        TooltipHandler.TipRegion(rowRect, newAndroid.story.adulthood.FullDescriptionFor(newAndroid));
                }

                //Skills
                row += 32f;

                Rect skillRerollRect = new Rect(32f, row, 256f, 27f);

                {
                    if (Widgets.ButtonText(skillRerollRect, "AndroidCustomizationRerollSkills".Translate()))
                    {
                        RefreshSkills();
                    }
                }

                row += 27f;

                Vector2 skillsVector = new Vector2(32f, row);

                SkillUI.DrawSkillsOf(newAndroid, skillsVector, SkillUI.SkillDrawMode.Gameplay);

                //Costs
                row = pawnRect.y + pawnRect.height;
                float column = skillRerollRect.xMax;

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Medium;

                Rect costLabelRect = new Rect(column, row, 256f, 26f);
                Widgets.DrawTitleBG(costLabelRect);
                Widgets.Label(costLabelRect.ContractedBy(2f), "AndroidCustomizationCostLabel".Translate());
                row += 26f;

                int currentCostItem = 0;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.LowerLeft;

                //Time
                {
                    Rect costRect = new Rect(column + 3f + currentCostItem * 32f, row, 26f, 26f);
                    //Widgets.ThingIcon(costRect, RimWorld.ThingDefOf.AncientCryptosleepCasket);
                    Widgets.DrawTextureFitted(costRect, ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast"), 1f);
                    TooltipHandler.TipRegion(costRect, "AndroidCustomizationTimeCost".Translate() + ": "+ (androidPrinter.PrinterProperties.ticksToCraft + finalExtraPrintingTimeCost).ToStringTicksToPeriodVerbose());
                    Widgets.DrawHighlightIfMouseover(costRect);

                    Widgets.Label(costRect.ExpandedBy(8), "" + (androidPrinter.PrinterProperties.ticksToCraft + finalExtraPrintingTimeCost).ToStringTicksToPeriodVerbose());
                }
                currentCostItem++;

                Text.Anchor = TextAnchor.LowerRight;

                //Nutrition and items.
                foreach (ThingOrderRequest cost in finalCalculatedPrintingCost)
                {
                    Rect costRect = new Rect(column + 3f + currentCostItem * 32f, row, 26f, 26f);

                    if (cost.nutrition)
                    {
                        Widgets.ThingIcon(costRect, RimWorld.ThingDefOf.Meat_Human);
                        TooltipHandler.TipRegion(costRect, "AndroidNutrition".Translate());
                    }
                    else
                    {
                        Widgets.ThingIcon(costRect, cost.thingDef);
                        TooltipHandler.TipRegion(costRect, cost.thingDef.LabelCap);
                    }

                    Widgets.DrawHighlightIfMouseover(costRect);

                    Widgets.Label(costRect, "" + cost.amount);

                    currentCostItem++;
                }

                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Small;

                //Traits
                row += 32f;

                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Medium;

                Rect traitsLabelRect = new Rect(column, row, 256f, 26f);
                Widgets.DrawTitleBG(traitsLabelRect);
                Widgets.Label(traitsLabelRect.ContractedBy(2f), "AndroidCustomizationTraitsLabel".Translate());

                Text.Font = GameFont.Small;

                row += 26f;

                Text.Anchor = TextAnchor.MiddleCenter;

                //traitsScrollPosition

                Trait traitToBeRemoved = null;
                float traitRowWidth = 256f;
                float traitRowHeight = 24f;

                float innerTraitsRectHeight = (newAndroid.story.traits.allTraits.Count + 1) * traitRowHeight;

                Rect outerTraitsRect = new Rect(traitsLabelRect);
                outerTraitsRect.y += 26f;
                outerTraitsRect.height = inRect.height - outerTraitsRect.y;
                outerTraitsRect.width += 12f;

                Rect innerTraitsRect = new Rect(outerTraitsRect);
                innerTraitsRect.height = innerTraitsRectHeight + 8f;
                //innerTraitsRect.width -= 8f;

                Widgets.BeginScrollView(outerTraitsRect, ref traitsScrollPosition, innerTraitsRect);

                foreach (Trait trait in newAndroid.story.traits.allTraits)
                {
                    Rect rowRect = new Rect(skillRerollRect.xMax, row, traitRowWidth, traitRowHeight);
                    Widgets.DrawBox(rowRect);
                    Widgets.DrawHighlightIfMouseover(rowRect);

                    Rect traitLabelRect = new Rect(rowRect);
                    traitLabelRect.width -= traitLabelRect.height;

                    Rect removeButtonRect = new Rect(rowRect);
                    removeButtonRect.width = removeButtonRect.height;
                    removeButtonRect.x = traitLabelRect.xMax;

                    if (originalTraits.Any(otherTrait => otherTrait.def == trait.def && otherTrait.Degree == trait.Degree))
                    {
                        Widgets.Label(traitLabelRect, "<" + trait.LabelCap + ">");
                    }
                    else
                    {
                        Widgets.Label(traitLabelRect, trait.LabelCap);
                    }
                    
                    TooltipHandler.TipRegion(traitLabelRect, trait.TipString(newAndroid));

                    //Bring up trait selection menu.
                    if(Widgets.ButtonInvisible(traitLabelRect))
                    {
                        PickTraitMenu(trait);
                    }

                    //Removes this trait.
                    if(Widgets.ButtonImage(removeButtonRect, TexCommand.ForbidOn))
                    {
                        traitToBeRemoved = trait;
                    }

                    row += 26f;
                }

                Text.Anchor = TextAnchor.MiddleRight;

                //Add traits. Until 7 by default.
                {
                    Rect rowRect = new Rect(skillRerollRect.xMax, row, traitRowWidth, traitRowHeight);

                    Rect traitLabelRect = new Rect(rowRect);
                    traitLabelRect.width -= traitLabelRect.height;

                    Rect addButtonRect = new Rect(rowRect);
                    addButtonRect.width = addButtonRect.height;
                    addButtonRect.x = traitLabelRect.xMax;

                    Widgets.Label(traitLabelRect, "AndroidCustomizationAddTraitLabel".Translate(newAndroid.story.traits.allTraits.Count, AndroidCustomizationTweaks.maxTraitsToPick));

                    if (Widgets.ButtonImage(addButtonRect, TexCommand.Install) && newAndroid.story.traits.allTraits.Count < AndroidCustomizationTweaks.maxTraitsToPick)
                    {
                        PickTraitMenu(null);
                    }
                }

                Widgets.EndScrollView();

                Text.Anchor = TextAnchor.UpperLeft;

                if (traitToBeRemoved != null)
                {
                    //Remove all associated bonuses and reroll skills.
                    //TraitDef traitDef = traitToBeRemoved.def;

                    newAndroid.story.traits.allTraits.Remove(traitToBeRemoved);

                    RefreshPawn();
                    traitToBeRemoved = null;
                }

                //Upgrades
                {
                    row = 32f;
                    float rowWidth = inRect.width - upgradesOffset;
                    float rowHeight = 32f;

                    Rect upgradesRowRect = new Rect(upgradesOffset, row, rowWidth, rowHeight);
                    Text.Font = GameFont.Medium;
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(upgradesRowRect, "AndroidCustomizationUpgrades".Translate());
                    Widgets.DrawLineHorizontal(upgradesRowRect.x, upgradesRowRect.y + 32f, upgradesRowRect.width);
                    Text.Font = GameFont.Small;
                    Text.Anchor = TextAnchor.UpperLeft;

                    row += 35f;

                    Rect upgradeSizeBase = new Rect(0f, 0f, AndroidCustomizationTweaks.upgradeBaseSize, AndroidCustomizationTweaks.upgradeBaseSize);
                    int itemsPerRow = (int)Math.Floor(rowWidth / upgradeSizeBase.width);

                    //Make it inside a internal frame if needed.
                    Rect outerUpgradesFrameRect = new Rect(upgradesOffset, row, rowWidth, inRect.height - rowHeight);
                    float innerUpgradesHeight = 0f;

                    foreach (AndroidUpgradeGroupDef groupDef in DefDatabase<AndroidUpgradeGroupDef>.AllDefs)
                    {
                        innerUpgradesHeight += groupDef.calculateNeededHeight(upgradeSizeBase, rowWidth);
                        innerUpgradesHeight += 52f;
                    }

                    Rect innerUpgradesFrameRect = new Rect(outerUpgradesFrameRect);
                    innerUpgradesFrameRect.height = innerUpgradesHeight;

                    //upgradesScrollPosition
                    Widgets.BeginScrollView(outerUpgradesFrameRect, ref upgradesScrollPosition, innerUpgradesFrameRect);

                    foreach (AndroidUpgradeGroupDef groupDef in DefDatabase<AndroidUpgradeGroupDef>.AllDefs.OrderBy(upgradeGroup => upgradeGroup.orderID))
                    {
                        Rect groupTitleRect = new Rect(upgradesRowRect);
                        groupTitleRect.y = row;
                        groupTitleRect.height = 22f;
                        row += 30f;

                        Text.Anchor = TextAnchor.MiddleCenter;
                        Widgets.DrawTitleBG(groupTitleRect);
                        Widgets.Label(groupTitleRect, groupDef.label);
                        Widgets.DrawLineHorizontal(groupTitleRect.x, groupTitleRect.y + 22f, groupTitleRect.width);
                        Text.Anchor = TextAnchor.UpperLeft;

                        float neededHeight = groupDef.calculateNeededHeight(upgradeSizeBase, rowWidth);
                        int upgradeItem = 0;
                        float upgradeItemRow = 0f;

                        foreach(AndroidUpgradeDef upgrade in groupDef.Upgrades.OrderBy(upgradeSubGroup => upgradeSubGroup.orderID))
                        {
                            if(upgradeItem >= itemsPerRow)
                            {
                                upgradeItem = 0;
                                upgradeItemRow += upgradeSizeBase.height;
                            }

                            Rect upgradeItemRect = new Rect(upgradesRowRect.x + (upgradeSizeBase.width * upgradeItem), row + upgradeItemRow, upgradeSizeBase.width, upgradeSizeBase.height);

                            //Button
                            bool needsFulfilled = false;

                            if(Mouse.IsOver(upgradeItemRect))
                            {
                                StringBuilder tooltip = new StringBuilder();
                                tooltip.AppendLine(upgrade.label);
                                tooltip.AppendLine();
                                tooltip.AppendLine(upgrade.description);
                                tooltip.AppendLine();
                                if (upgrade.hediffToApply != null && upgrade.hediffToApply.ConcreteExample != null)
                                {
                                    tooltip.AppendLine(upgrade.hediffToApply.ConcreteExample.TipStringExtra.TrimEndNewlines());
                                    tooltip.AppendLine();
                                }
                                if (upgrade.newBodyType != null)
                                {
                                    tooltip.AppendLine("AndroidCustomizationChangeBodyType".Translate());
                                    tooltip.AppendLine();
                                }
                                if (upgrade.changeSkinColor)
                                {
                                    tooltip.AppendLine("AndroidCustomizationChangeSkinColor".Translate());
                                    tooltip.AppendLine();
                                }
                                tooltip.AppendLine(androidPrinter.FormatIngredientCosts(out needsFulfilled, upgrade.costList, false));
                                tooltip.AppendLine("AndroidCustomizationTimeCost".Translate() + ": " + upgrade.extraPrintingTime.ToStringTicksToPeriodVerbose());
                                if (upgrade.requiredResearch != null && !upgrade.requiredResearch.IsFinished)
                                {
                                    tooltip.AppendLine();
                                    tooltip.AppendLine("AndroidCustomizationRequiredResearch".Translate() + ": " + upgrade.requiredResearch.LabelCap);
                                }

                                TooltipHandler.TipRegion(upgradeItemRect, tooltip.ToString());
                            }

                            //(upgrade.requiredResearch != null && upgrade.requiredResearch.IsFinished)
                            bool disabledUpgrade = false;

                            if (upgrade.requiredResearch != null)
                            {
                                disabledUpgrade =
                                !upgrade.requiredResearch.IsFinished ||
                                appliedUpgradeCommands.Any(appUpgrade =>
                                appUpgrade.def != upgrade && appUpgrade.def.exclusivityGroups.Any(group => upgrade.exclusivityGroups.Contains(group)));
                            }
                            else
                            {
                                disabledUpgrade =
                                appliedUpgradeCommands.Any(appUpgrade =>
                                appUpgrade.def != upgrade && appUpgrade.def.exclusivityGroups.Any(group => upgrade.exclusivityGroups.Contains(group)));
                            }

                            if(disabledUpgrade)
                            {
                                Widgets.DrawRectFast(upgradeItemRect, Color.red);
                            }
                            else
                            {
                                if (appliedUpgradeCommands.Any(upgradeCommand => upgradeCommand.def == upgrade))
                                    Widgets.DrawRectFast(upgradeItemRect, Color.white);
                            }

                            if(upgrade.iconTexturePath != null)
                            {
                                Widgets.DrawTextureFitted(upgradeItemRect.ContractedBy(3f), ContentFinder<Texture2D>.Get(upgrade.iconTexturePath), 1f);
                            }
                            Widgets.DrawHighlightIfMouseover(upgradeItemRect);
                            UpgradeCommand existingCommand = appliedUpgradeCommands.FirstOrDefault(upgradeCommand => upgradeCommand.def == upgrade);

                            if (!disabledUpgrade && Widgets.ButtonInvisible(upgradeItemRect))
                            {
                                if (existingCommand != null)
                                {
                                    //Undo upgrade.
                                    existingCommand.Undo();

                                    appliedUpgradeCommands.Remove(existingCommand);
                                }
                                else
                                {
                                    //Apply upgrade.
                                    UpgradeCommand command = UpgradeMaker.Make(upgrade, this);
                                    command.Apply();
                                    command.Notify_UpgradeAdded();

                                    appliedUpgradeCommands.Add(command);
                                }

                                //RefreshUpgrades();
                                RefreshCosts();
                            }

                            if (existingCommand != null)
                            {
                                existingCommand.ExtraOnGUI(upgradeItemRect);
                            }

                            upgradeItem++;
                        }

                        row += neededHeight + 22f;
                    }

                    Widgets.EndScrollView();
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        public void PickTraitMenu(Trait oldTrait)
        {
            //Populate available traits.
            allTraits.Clear();

            foreach (TraitDef def in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                foreach(TraitDegreeData degree in def.degreeDatas)
                {
                    Trait trait = new Trait(def, degree.degree, false);
                    allTraits.Add(trait);
                }
            }

            //Filter out traits the race can NEVER get.
            //AlienComp alienComp = newAndroid.TryGetComp<AlienComp>();
            if(newAndroid.def is ThingDef_AlienRace alienRaceDef)
            {
                List<string> disallowedTraits = alienRaceDef?.alienRace?.generalSettings?.disallowedTraits;
                if(disallowedTraits != null)
                {
                    foreach (string traitDefName in disallowedTraits)
                    {
                        allTraits.RemoveAll(trait => trait.def.defName == traitDefName);
                    }
                }
            }

            //Filter out traits we already got.
            //Filter out conflicting traits.
            foreach (Trait trait in newAndroid.story.traits.allTraits)
            {
                //Same traits.
                //allTraits.RemoveAll(aTrait => aTrait.def == trait.def && aTrait.Degree == trait.Degree);
                allTraits.RemoveAll(aTrait => aTrait.def == trait.def);

                //Conflicting traits.
                allTraits.RemoveAll(aTrait => trait.def.conflictingTraits.Contains(aTrait.def));
            }

            FloatMenuUtility.MakeMenu<Trait>(allTraits, 
                delegate(Trait labelTrait)
                    {
                        if(originalTraits.Any(originalTrait => originalTrait.def == labelTrait.def && originalTrait.Degree == labelTrait.Degree))
                        {
                            return "AndroidCustomizationOriginalTraitFloatMenu".Translate(labelTrait.LabelCap);
                        }
                        else
                        {
                            return labelTrait.LabelCap;
                        }
                    }, 
                (Trait theTrait) => 
                    delegate() 
                    {
                        Trait oldOldTrait = oldTrait;
                        replacedTrait = oldOldTrait;
                        newTrait = theTrait;
                        //Log.Message("theTrait: " + theTrait?.LabelCap ?? "No trait!!");
                    });
        }

        public void RefreshUpgrades()
        {
            foreach(UpgradeCommand command in appliedUpgradeCommands)
            {
                command.Apply();
            }

            refreshAndroidPortrait = true;
        }

        public void RefreshCosts()
        {
            //Reset costs
            finalCalculatedPrintingCost.Clear();
            finalExtraPrintingTimeCost = 0;

            PawnCrafterProperties printerProperties = androidPrinter.def.GetModExtension<PawnCrafterProperties>();

            //Copy base costs.
            foreach(ThingOrderRequest baseCost in printerProperties.costList)
            {
                ThingOrderRequest baseCostCopy = new ThingOrderRequest();
                baseCostCopy.amount = baseCost.amount;
                baseCostCopy.nutrition = baseCost.nutrition;
                baseCostCopy.thingDef = baseCost.thingDef;

                finalCalculatedPrintingCost.Add(baseCostCopy);
            }
            //finalCalculatedPrintingCost.AddRange(printerProperties.costList);

            //Add costs from upgrades.
            List<ThingDef> thingsExemptedFromBodySize = new List<ThingDef>();

            foreach(UpgradeCommand upgrade in appliedUpgradeCommands)
            {
                foreach(ThingOrderRequest upgradeCost in upgrade.def.costList)
                {
                    //Attempt to merge costs if possible.
                    if(finalCalculatedPrintingCost.FirstOrDefault(finalCost => finalCost.thingDef == upgradeCost.thingDef || finalCost.nutrition && upgradeCost.nutrition) is ThingOrderRequest finalRequest)
                    {
                        finalRequest.amount += upgradeCost.amount;
                    }
                    else
                    {
                        ThingOrderRequest upgradeCostCopy = new ThingOrderRequest();
                        upgradeCostCopy.amount = upgradeCost.amount;
                        upgradeCostCopy.nutrition = upgradeCost.nutrition;
                        upgradeCostCopy.thingDef = upgradeCost.thingDef;

                        finalCalculatedPrintingCost.Add(upgradeCostCopy);
                    }
                }

                thingsExemptedFromBodySize.AddRange(upgrade.def.costsNotAffectedByBodySize);

                finalExtraPrintingTimeCost += upgrade.def.extraPrintingTime;
            }

            //Only get a list of all distinct defs.
            if(thingsExemptedFromBodySize.Count > 0)
                thingsExemptedFromBodySize = new List<ThingDef>(thingsExemptedFromBodySize.Distinct());

            //Add costs from traits.
            //For each added non-original trait.
            //Deduct costs from original traits.
            int traitTimePenaltyCost = 45000;
            int traitsTimeCost = 0;
            foreach(Trait trait in newAndroid.story.traits.allTraits)
            {
                traitsTimeCost += traitTimePenaltyCost;

                if(originalTraits.Any(originalTrait => originalTrait.def == trait.def && originalTrait.Degree == trait.Degree))
                {
                    traitsTimeCost -= traitTimePenaltyCost;
                }
            }
            //Add cost for each missing original trait.
            foreach(Trait originalTrait in originalTraits)
            {
                if(!newAndroid.story.traits.allTraits.Any(trait => originalTrait.def == trait.def && originalTrait.Degree == trait.Degree))
                {
                    traitsTimeCost += traitTimePenaltyCost;
                }
            }

            finalExtraPrintingTimeCost += traitsTimeCost;

            //Deduct costs from body size.
            foreach (ThingOrderRequest cost in finalCalculatedPrintingCost)
            {
                if(!thingsExemptedFromBodySize.Contains(cost.thingDef))
                    cost.amount = (float)Math.Ceiling(cost.amount * newAndroid.def.race.baseBodySize);
            }
        }

        public void RefreshPawn()
        {
            //Reflection hackery.
            Type storyTrackerType = typeof(Pawn_StoryTracker);
            FieldInfo cachedDisabledWorkTypesField = storyTrackerType.GetField("cachedDisabledWorkTypes", BindingFlags.Instance | BindingFlags.NonPublic);
            
            //Set to null.
            cachedDisabledWorkTypesField.SetValue(newAndroid.story, null);

            //Refresh disabled skills.
            newAndroid.skills.Notify_SkillDisablesChanged();

            //Refresh skills.
            RefreshSkills();
            RefreshUpgrades();
            RefreshCosts();
        }

        public void RefreshSkills(bool addBackstoryBonuses = false)
        {
            List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                SkillDef skillDef = allDefsListForReading[i];
                int num = FinalLevelOfSkill(newAndroid, skillDef);
                SkillRecord skill = newAndroid.skills.GetSkill(skillDef);
                skill.Level = num;
                skill.passion = Passion.None;
                if (!skill.TotallyDisabled)
                {
                    float num2 = (float)num * 0.11f;
                    float value = Rand.Value;
                    if (value < num2)
                    {
                        if (value < num2 * 0.2f)
                        {
                            skill.passion = Passion.Major;
                        }
                        else
                        {
                            skill.passion = Passion.Minor;
                        }
                    }
                    skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
                }
            }
        }

        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num;
            if (sk.usuallyDefinedInBackstories)
            {
                num = (float)Rand.RangeInclusive(0, 4);
            }
            else
            {
                num = Rand.ByCurve(LevelRandomCurve);
            }
            foreach (Backstory current in from bs in pawn.story.AllBackstories
                                          where bs != null
                                          select bs)
            {
                foreach (KeyValuePair<SkillDef, int> current2 in current.skillGainsResolved)
                {
                    if (current2.Key == sk)
                    {
                        num += (float)current2.Value * Rand.Range(1f, 1.4f);
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                int num2 = 0;
                if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out num2))
                {
                    num += (float)num2;
                }
            }
            //float num3 = 1f;
            //num *= num3;
            num = LevelFinalAdjustmentCurve.Evaluate(num);

            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }

        public Pawn GetNewPawn(Gender gender = Gender.Female)
        {
            //Make base pawn.
            Pawn pawn;

            //Make Android-like if not a Android.
            if(currentPawnKindDef.race != ThingDefOf.ChjAndroid)
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, androidPrinter.Faction, RimWorld.PawnGenerationContext.NonPlayer,
                -1, true, false, false, false, false, false, 0f, false, true, true, false, false, false, true, genPawn => genPawn.gender == gender));
                HarmonyPatches.bypassGenerationOfUpgrades = false;

                //Give random skin and hair color.
                AndroidUtility.Androidify(pawn);

                //Post process age to adulthood. Two methods.
                LifeStageAge adultLifestage = pawn.RaceProps.lifeStageAges.Last();
                if(adultLifestage != null)
                {
                    long ageInTicks = (long)Math.Ceiling(adultLifestage.minAge) * (long)GenDate.TicksPerYear;

                    pawn.ageTracker.AgeBiologicalTicks = ageInTicks;
                    pawn.ageTracker.AgeChronologicalTicks = ageInTicks;
                }
                else
                {
                    //Max age
                    long ageInTicks = (long)(pawn.RaceProps.lifeExpectancy * (long)GenDate.TicksPerYear * 0.2f);

                    pawn.ageTracker.AgeBiologicalTicks = ageInTicks;
                    pawn.ageTracker.AgeChronologicalTicks = ageInTicks;
                }
            }
            else
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, androidPrinter.Faction, RimWorld.PawnGenerationContext.NonPlayer,
                -1, true, false, false, false, false, false, 0f, false, true, true, false, false, false, true, genPawn => genPawn.gender == gender, fixedBiologicalAge: 20, fixedChronologicalAge: 20));
                HarmonyPatches.bypassGenerationOfUpgrades = false;
            }

            //Destroy all equipment and items in inventory.
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();

            //Strip off clothes and replace with bandages.
            pawn.apparel.DestroyAll();
            if(pawn.apparel.CanWearWithoutDroppingAnything(ThingDefOf.ChJAndroidThermalBandages))
                pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(ThingDefOf.ChJAndroidThermalBandages, ThingDef.Named("Synthread")));

            //Refresh disabled skills and work.
            if (pawn.workSettings != null)
            {
                pawn.workSettings.Notify_GainedTrait();
            }
            //newAndroid.story.Notify_TraitChanged();
            if (pawn.skills != null)
            {
                pawn.skills.Notify_SkillDisablesChanged();
            }
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
            {
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }

            //Set original values
            {
                //Traits
                originalTraits.Clear();
                foreach (Trait trait in pawn.story.traits.allTraits)
                {
                    Trait cloneTrait = new Trait(trait.def, trait.Degree, trait.ScenForced);
                    originalTraits.Add(cloneTrait);
                }
            }

            return pawn;
        }
    }
}
