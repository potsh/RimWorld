using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class ThingDef : BuildableDef
	{
		public Type thingClass;

		public ThingCategory category;

		public TickerType tickerType;

		public int stackLimit = 1;

		public IntVec2 size = new IntVec2(1, 1);

		public bool destroyable = true;

		public bool rotatable = true;

		public bool smallVolume;

		public bool useHitPoints = true;

		public bool receivesSignals;

		public List<CompProperties> comps = new List<CompProperties>();

		public List<ThingDefCountClass> killedLeavings;

		public List<ThingDefCountClass> butcherProducts;

		public List<ThingDefCountClass> smeltProducts;

		public bool smeltable;

		public bool randomizeRotationOnSpawn;

		public List<DamageMultiplier> damageMultipliers;

		public bool isTechHediff;

		public RecipeMakerProperties recipeMaker;

		public ThingDef minifiedDef;

		public bool isUnfinishedThing;

		public bool leaveResourcesWhenKilled;

		public ThingDef slagDef;

		public bool isFrameInt;

		public IntVec3 interactionCellOffset = IntVec3.Zero;

		public bool hasInteractionCell;

		public ThingDef interactionCellIcon;

		public bool interactionCellIconReverse;

		public ThingDef filthLeaving;

		public bool forceDebugSpawnable;

		public bool intricate;

		public bool scatterableOnMapGen = true;

		public float deepCommonality;

		public int deepCountPerCell = 300;

		public int deepCountPerPortion = -1;

		public IntRange deepLumpSizeRange = IntRange.zero;

		public float generateCommonality = 1f;

		public float generateAllowChance = 1f;

		private bool canOverlapZones = true;

		public FloatRange startingHpRange = FloatRange.One;

		[NoTranslate]
		public List<string> thingSetMakerTags;

		public bool alwaysFlee;

		public List<RecipeDef> recipes;

		public GraphicData graphicData;

		public DrawerType drawerType = DrawerType.RealtimeOnly;

		public bool drawOffscreen;

		public ColorGenerator colorGenerator;

		public float hideAtSnowDepth = 99999f;

		public bool drawDamagedOverlay = true;

		public bool castEdgeShadows;

		public float staticSunShadowHeight;

		public bool selectable;

		public bool neverMultiSelect;

		public bool isAutoAttackableMapObject;

		public bool hasTooltip;

		public List<Type> inspectorTabs;

		[Unsaved]
		public List<InspectTabBase> inspectorTabsResolved;

		public bool seeThroughFog;

		public bool drawGUIOverlay;

		public ResourceCountPriority resourceReadoutPriority;

		public bool resourceReadoutAlwaysShow;

		public bool drawPlaceWorkersWhileSelected;

		public ConceptDef storedConceptLearnOpportunity;

		public float uiIconScale = 1f;

		public bool alwaysHaulable;

		public bool designateHaulable;

		public List<ThingCategoryDef> thingCategories;

		public bool mineable;

		public bool socialPropernessMatters;

		public bool stealable = true;

		public SoundDef soundDrop;

		public SoundDef soundPickup;

		public SoundDef soundInteract;

		public SoundDef soundImpactDefault;

		public bool saveCompressible;

		public bool isSaveable = true;

		public bool holdsRoof;

		public float fillPercent;

		public bool coversFloor;

		public bool neverOverlapFloors;

		public SurfaceType surfaceType;

		public bool blockPlants;

		public bool blockLight;

		public bool blockWind;

		public Tradeability tradeability = Tradeability.All;

		[NoTranslate]
		public List<string> tradeTags;

		public bool tradeNeverStack;

		public ColorGenerator colorGeneratorInTraderStock;

		private List<VerbProperties> verbs;

		public List<Tool> tools;

		public float equippedAngleOffset;

		public EquipmentType equipmentType;

		public TechLevel techLevel;

		[NoTranslate]
		public List<string> weaponTags;

		[NoTranslate]
		public List<string> techHediffsTags;

		public bool destroyOnDrop;

		public List<StatModifier> equippedStatOffsets;

		public BuildableDef entityDefToBuild;

		public ThingDef projectileWhenLoaded;

		public IngestibleProperties ingestible;

		public FilthProperties filth;

		public GasProperties gas;

		public BuildingProperties building;

		public RaceProperties race;

		public ApparelProperties apparel;

		public MoteProperties mote;

		public PlantProperties plant;

		public ProjectileProperties projectile;

		public StuffProperties stuffProps;

		public SkyfallerProperties skyfaller;

		[Unsaved]
		private string descriptionDetailedCached;

		[Unsaved]
		public Graphic interactionCellGraphic;

		public const int SmallUnitPerVolume = 10;

		public const float SmallVolumePerUnit = 0.1f;

		private List<RecipeDef> allRecipesCached;

		private static List<VerbProperties> EmptyVerbPropertiesList = new List<VerbProperties>();

		private Dictionary<ThingDef, Thing> concreteExamplesInt;

		public bool EverHaulable => alwaysHaulable || designateHaulable;

		public float VolumePerUnit => smallVolume ? 0.1f : 1f;

		public override IntVec2 Size => size;

		public bool DiscardOnDestroyed => race == null;

		public int BaseMaxHitPoints => Mathf.RoundToInt(this.GetStatValueAbstract(StatDefOf.MaxHitPoints));

		public float BaseFlammability => this.GetStatValueAbstract(StatDefOf.Flammability);

		public float BaseMarketValue
		{
			get
			{
				return this.GetStatValueAbstract(StatDefOf.MarketValue);
			}
			set
			{
				this.SetStatBaseValue(StatDefOf.MarketValue, value);
			}
		}

		public float BaseMass => this.GetStatValueAbstract(StatDefOf.Mass);

		public bool PlayerAcquirable => !destroyOnDrop;

		public bool EverTransmitsPower
		{
			get
			{
				for (int i = 0; i < comps.Count; i++)
				{
					CompProperties_Power compProperties_Power = comps[i] as CompProperties_Power;
					if (compProperties_Power != null && compProperties_Power.transmitsPower)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool Minifiable => minifiedDef != null;

		public bool HasThingIDNumber => category != ThingCategory.Mote;

		public List<RecipeDef> AllRecipes
		{
			get
			{
				if (allRecipesCached == null)
				{
					allRecipesCached = new List<RecipeDef>();
					if (recipes != null)
					{
						for (int i = 0; i < recipes.Count; i++)
						{
							allRecipesCached.Add(recipes[i]);
						}
					}
					List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
					for (int j = 0; j < allDefsListForReading.Count; j++)
					{
						if (allDefsListForReading[j].recipeUsers != null && allDefsListForReading[j].recipeUsers.Contains(this))
						{
							allRecipesCached.Add(allDefsListForReading[j]);
						}
					}
				}
				return allRecipesCached;
			}
		}

		public bool ConnectToPower
		{
			get
			{
				if (EverTransmitsPower)
				{
					return false;
				}
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == typeof(CompPowerBattery))
					{
						return true;
					}
					if (comps[i].compClass == typeof(CompPowerTrader))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CoexistsWithFloors => !neverOverlapFloors && !coversFloor;

		public FillCategory Fillage
		{
			get
			{
				if (fillPercent < 0.01f)
				{
					return FillCategory.None;
				}
				if (fillPercent > 0.99f)
				{
					return FillCategory.Full;
				}
				return FillCategory.Partial;
			}
		}

		public bool MakeFog => Fillage == FillCategory.Full;

		public bool CanOverlapZones
		{
			get
			{
				if (building != null && building.SupportsPlants)
				{
					return false;
				}
				if (passability == Traversability.Impassable && category != ThingCategory.Plant)
				{
					return false;
				}
				if ((int)surfaceType >= 1)
				{
					return false;
				}
				if (typeof(ISlotGroupParent).IsAssignableFrom(thingClass))
				{
					return false;
				}
				if (!canOverlapZones)
				{
					return false;
				}
				if (IsBlueprint || IsFrame)
				{
					ThingDef thingDef = entityDefToBuild as ThingDef;
					if (thingDef != null)
					{
						return thingDef.CanOverlapZones;
					}
				}
				return true;
			}
		}

		public bool CountAsResource => resourceReadoutPriority != ResourceCountPriority.Uncounted;

		public bool BlockPlanting
		{
			get
			{
				if (building != null && building.SupportsPlants)
				{
					return false;
				}
				if (blockPlants)
				{
					return true;
				}
				if (category == ThingCategory.Plant)
				{
					return true;
				}
				if ((int)Fillage > 0)
				{
					return true;
				}
				if (this.IsEdifice())
				{
					return true;
				}
				return false;
			}
		}

		public List<VerbProperties> Verbs
		{
			get
			{
				if (verbs != null)
				{
					return verbs;
				}
				return EmptyVerbPropertiesList;
			}
		}

		public bool CanHaveFaction
		{
			get
			{
				if (!IsBlueprint && !IsFrame)
				{
					switch (category)
					{
					case ThingCategory.Pawn:
						return true;
					case ThingCategory.Building:
						return true;
					default:
						return false;
					}
				}
				return true;
			}
		}

		public bool Claimable => building != null && building.claimable && !building.isNaturalRock;

		public ThingCategoryDef FirstThingCategory
		{
			get
			{
				if (thingCategories.NullOrEmpty())
				{
					return null;
				}
				return thingCategories[0];
			}
		}

		public float MedicineTendXpGainFactor => Mathf.Clamp(this.GetStatValueAbstract(StatDefOf.MedicalPotency) * 0.7f, 0.5f, 1f);

		public bool CanEverDeteriorate
		{
			get
			{
				if (!useHitPoints)
				{
					return false;
				}
				return category == ThingCategory.Item || this == ThingDefOf.BurnedTree;
			}
		}

		public bool CanInteractThroughCorners
		{
			get
			{
				if (category != ThingCategory.Building)
				{
					return false;
				}
				if (!holdsRoof)
				{
					return false;
				}
				if (building != null && building.isNaturalRock && !IsSmoothed)
				{
					return false;
				}
				return true;
			}
		}

		public bool AffectsRegions => passability == Traversability.Impassable || IsDoor;

		public bool AffectsReachability
		{
			get
			{
				if (AffectsRegions)
				{
					return true;
				}
				if (passability == Traversability.Impassable || IsDoor)
				{
					return true;
				}
				if (TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(this))
				{
					return true;
				}
				return false;
			}
		}

		public string DescriptionDetailed
		{
			get
			{
				if (descriptionDetailedCached == null)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(description);
					if (IsApparel)
					{
						stringBuilder.AppendLine();
						stringBuilder.AppendLine(string.Format("{0}: {1}", "Layer".Translate(), apparel.GetLayersString()));
						stringBuilder.AppendLine(string.Format("{0}: {1}", "Covers".Translate(), apparel.GetCoveredOuterPartsString(BodyDefOf.Human)));
						if (equippedStatOffsets != null && equippedStatOffsets.Count > 0)
						{
							stringBuilder.AppendLine();
							foreach (StatModifier equippedStatOffset in equippedStatOffsets)
							{
								stringBuilder.AppendLine($"{equippedStatOffset.stat.LabelCap}: {equippedStatOffset.ValueToStringAsOffset}");
							}
						}
					}
					descriptionDetailedCached = stringBuilder.ToString();
				}
				return descriptionDetailedCached;
			}
		}

		public bool IsApparel => apparel != null;

		public bool IsBed => typeof(Building_Bed).IsAssignableFrom(thingClass);

		public bool IsCorpse => typeof(Corpse).IsAssignableFrom(thingClass);

		public bool IsFrame => isFrameInt;

		public bool IsBlueprint => entityDefToBuild != null && category == ThingCategory.Ethereal;

		public bool IsStuff => stuffProps != null;

		public bool IsMedicine => statBases.StatListContains(StatDefOf.MedicalPotency);

		public bool IsDoor => typeof(Building_Door).IsAssignableFrom(thingClass);

		public bool IsFilth => filth != null;

		public bool IsIngestible => ingestible != null;

		public bool IsNutritionGivingIngestible => IsIngestible && ingestible.CachedNutrition > 0f;

		public bool IsWeapon => category == ThingCategory.Item && (!verbs.NullOrEmpty() || !tools.NullOrEmpty());

		public bool IsCommsConsole => typeof(Building_CommsConsole).IsAssignableFrom(thingClass);

		public bool IsOrbitalTradeBeacon => typeof(Building_OrbitalTradeBeacon).IsAssignableFrom(thingClass);

		public bool IsFoodDispenser => typeof(Building_NutrientPasteDispenser).IsAssignableFrom(thingClass);

		public bool IsDrug => ingestible != null && ingestible.drugCategory != DrugCategory.None;

		public bool IsPleasureDrug => IsDrug && ingestible.joy > 0f;

		public bool IsNonMedicalDrug => IsDrug && ingestible.drugCategory != DrugCategory.Medical;

		public bool IsTable => surfaceType == SurfaceType.Eat && HasComp(typeof(CompGatherSpot));

		public bool IsWorkTable => typeof(Building_WorkTable).IsAssignableFrom(thingClass);

		public bool IsShell => projectileWhenLoaded != null;

		public bool IsArt => IsWithinCategory(ThingCategoryDefOf.BuildingsArt);

		public bool IsSmoothable => building != null && building.smoothedThing != null;

		public bool IsSmoothed => building != null && building.unsmoothedThing != null;

		public bool IsMetal => stuffProps != null && stuffProps.categories.Contains(StuffCategoryDefOf.Metallic);

		public bool IsAddictiveDrug
		{
			get
			{
				CompProperties_Drug compProperties = GetCompProperties<CompProperties_Drug>();
				return compProperties != null && compProperties.addictiveness > 0f;
			}
		}

		public bool IsMeat => category == ThingCategory.Item && thingCategories != null && thingCategories.Contains(ThingCategoryDefOf.MeatRaw);

		public bool IsLeather => category == ThingCategory.Item && thingCategories != null && thingCategories.Contains(ThingCategoryDefOf.Leathers);

		public bool IsRangedWeapon
		{
			get
			{
				if (!IsWeapon)
				{
					return false;
				}
				if (!verbs.NullOrEmpty())
				{
					for (int i = 0; i < verbs.Count; i++)
					{
						if (!verbs[i].IsMeleeAttack)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsMeleeWeapon => IsWeapon && !IsRangedWeapon;

		public bool IsWeaponUsingProjectiles
		{
			get
			{
				if (!IsWeapon)
				{
					return false;
				}
				if (!verbs.NullOrEmpty())
				{
					for (int i = 0; i < verbs.Count; i++)
					{
						if (verbs[i].LaunchesProjectile)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsBuildingArtificial => (category == ThingCategory.Building || IsFrame) && (building == null || (!building.isNaturalRock && !building.isResourceRock));

		public string LabelAsStuff
		{
			get
			{
				if (!stuffProps.stuffAdjective.NullOrEmpty())
				{
					return stuffProps.stuffAdjective;
				}
				return label;
			}
		}

		public bool EverStorable(bool willMinifyIfPossible)
		{
			if (typeof(MinifiedThing).IsAssignableFrom(thingClass))
			{
				return true;
			}
			if (!thingCategories.NullOrEmpty())
			{
				if (category == ThingCategory.Item)
				{
					return true;
				}
				if (willMinifyIfPossible && Minifiable)
				{
					return true;
				}
			}
			return false;
		}

		public Thing GetConcreteExample(ThingDef stuff = null)
		{
			if (concreteExamplesInt == null)
			{
				concreteExamplesInt = new Dictionary<ThingDef, Thing>();
			}
			if (stuff == null)
			{
				stuff = ThingDefOf.Steel;
			}
			if (!concreteExamplesInt.ContainsKey(stuff))
			{
				if (race == null)
				{
					concreteExamplesInt[stuff] = ThingMaker.MakeThing(this, (!base.MadeFromStuff) ? null : stuff);
				}
				else
				{
					concreteExamplesInt[stuff] = PawnGenerator.GeneratePawn((from pkd in DefDatabase<PawnKindDef>.AllDefsListForReading
					where pkd.race == this
					select pkd).FirstOrDefault());
				}
			}
			return concreteExamplesInt[stuff];
		}

		public CompProperties CompDefFor<T>() where T : ThingComp
		{
			return comps.FirstOrDefault((CompProperties c) => c.compClass == typeof(T));
		}

		public CompProperties CompDefForAssignableFrom<T>() where T : ThingComp
		{
			return comps.FirstOrDefault((CompProperties c) => typeof(T).IsAssignableFrom(c.compClass));
		}

		public bool HasComp(Type compType)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				if (comps[i].compClass == compType)
				{
					return true;
				}
			}
			return false;
		}

		public T GetCompProperties<T>() where T : CompProperties
		{
			for (int i = 0; i < comps.Count; i++)
			{
				T val = comps[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			return (T)null;
		}

		public override void PostLoad()
		{
			if (graphicData != null)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					if (graphicData.shaderType == null)
					{
						graphicData.shaderType = ShaderTypeDefOf.Cutout;
					}
					graphic = graphicData.Graphic;
				});
			}
			if (tools != null)
			{
				for (int i = 0; i < tools.Count; i++)
				{
					tools[i].id = i.ToString();
				}
			}
			if (verbs != null && verbs.Count == 1)
			{
				verbs[0].label = label;
			}
			base.PostLoad();
			if (category == ThingCategory.Building && building == null)
			{
				building = new BuildingProperties();
			}
			if (building != null)
			{
				building.PostLoadSpecial(this);
			}
			if (plant != null)
			{
				plant.PostLoadSpecial(this);
			}
		}

		protected override void ResolveIcon()
		{
			base.ResolveIcon();
			if (category == ThingCategory.Pawn)
			{
				if (!race.Humanlike)
				{
					PawnKindDef anyPawnKind = race.AnyPawnKind;
					if (anyPawnKind != null)
					{
						Material material = anyPawnKind.lifeStages.Last().bodyGraphicData.Graphic.MatAt(Rot4.East);
						uiIcon = (Texture2D)material.mainTexture;
						uiIconColor = material.color;
					}
				}
			}
			else
			{
				ThingDef thingDef = GenStuff.DefaultStuffFor(this);
				if (colorGenerator != null && (thingDef == null || thingDef.stuffProps.allowColorGenerators))
				{
					uiIconColor = colorGenerator.ExemplaryColor;
				}
				else if (thingDef != null)
				{
					uiIconColor = thingDef.stuffProps.color;
				}
				else if (graphicData != null)
				{
					uiIconColor = graphicData.color;
				}
				if (rotatable && graphic != null && graphic != BaseContent.BadGraphic && graphic.ShouldDrawRotated && defaultPlacingRot == Rot4.South)
				{
					uiIconAngle = 180f + graphic.DrawRotatedExtraAngleOffset;
				}
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (ingestible != null)
			{
				ingestible.parent = this;
			}
			if (building != null)
			{
				building.ResolveReferencesSpecial();
			}
			if (graphicData != null)
			{
				graphicData.ResolveReferencesSpecial();
			}
			if (race != null)
			{
				race.ResolveReferencesSpecial();
			}
			if (stuffProps != null)
			{
				stuffProps.ResolveReferencesSpecial();
			}
			if (soundImpactDefault == null)
			{
				soundImpactDefault = SoundDefOf.BulletImpact_Ground;
			}
			if (soundDrop == null)
			{
				soundDrop = SoundDefOf.Standard_Drop;
			}
			if (soundPickup == null)
			{
				soundPickup = SoundDefOf.Standard_Pickup;
			}
			if (soundInteract == null)
			{
				soundInteract = SoundDefOf.Standard_Pickup;
			}
			if (inspectorTabs != null && inspectorTabs.Any())
			{
				inspectorTabsResolved = new List<InspectTabBase>();
				for (int i = 0; i < inspectorTabs.Count; i++)
				{
					try
					{
						inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(inspectorTabs[i]));
					}
					catch (Exception ex)
					{
						Log.Error("Could not instantiate inspector tab of type " + inspectorTabs[i] + ": " + ex);
					}
				}
			}
			if (comps != null)
			{
				for (int j = 0; j < comps.Count; j++)
				{
					comps[j].ResolveReferences(this);
				}
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string str = enumerator.Current;
					yield return str;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (label.NullOrEmpty())
			{
				yield return "no label";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (graphicData != null)
			{
				using (IEnumerator<string> enumerator2 = graphicData.ConfigErrors(this).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string err6 = enumerator2.Current;
						yield return err6;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (projectile != null)
			{
				using (IEnumerator<string> enumerator3 = projectile.ConfigErrors(this).GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						string err5 = enumerator3.Current;
						yield return err5;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (statBases != null)
			{
				using (List<StatModifier>.Enumerator enumerator4 = statBases.GetEnumerator())
				{
					while (enumerator4.MoveNext())
					{
						_003CConfigErrors_003Ec__Iterator0 _003CConfigErrors_003Ec__Iterator = (_003CConfigErrors_003Ec__Iterator0)/*Error near IL_033b: stateMachine*/;
						StatModifier statBase = enumerator4.Current;
						if ((from st in statBases
						where st.stat == statBase.stat
						select st).Count() > 1)
						{
							yield return "defines the stat base " + statBase.stat + " more than once.";
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (!BeautyUtility.BeautyRelevant(category) && this.StatBaseDefined(StatDefOf.Beauty))
			{
				yield return "Beauty stat base is defined, but Things of category " + category + " cannot have beauty.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (char.IsNumber(defName[defName.Length - 1]))
			{
				yield return "ends with a numerical digit, which is not allowed on ThingDefs.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (thingClass == null)
			{
				yield return "has null thingClass.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (comps.Count > 0 && !typeof(ThingWithComps).IsAssignableFrom(thingClass))
			{
				yield return "has components but it's thingClass is not a ThingWithComps";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (ConnectToPower && drawerType == DrawerType.RealtimeOnly && IsFrame)
			{
				yield return "connects to power but does not add to map mesh. Will not create wire meshes.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (costList != null)
			{
				foreach (ThingDefCountClass cost in costList)
				{
					if (cost.count == 0)
					{
						yield return "cost in " + cost.thingDef + " is zero.";
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (thingCategories != null)
			{
				ThingCategoryDef doubleCat = thingCategories.FirstOrDefault(delegate(ThingCategoryDef cat)
				{
					ThingDef _0024this2 = ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_0646: stateMachine*/)._0024this;
					return ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_0646: stateMachine*/)._0024this.thingCategories.Count((ThingCategoryDef c) => c == cat) > 1;
				});
				if (doubleCat != null)
				{
					yield return "has duplicate thingCategory " + doubleCat + ".";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Fillage == FillCategory.Full && category != ThingCategory.Building)
			{
				yield return "gives full cover but is not a building.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (comps.Any((CompProperties c) => c.compClass == typeof(CompPowerTrader)) && drawerType == DrawerType.MapMeshOnly)
			{
				yield return "has PowerTrader comp but does not draw real time. It won't draw a needs-power overlay.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (equipmentType != 0)
			{
				if (techLevel == TechLevel.Undefined)
				{
					yield return "is equipment but has no tech level.";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (!comps.Any((CompProperties c) => c.compClass == typeof(CompEquippable)))
				{
					yield return "is equipment but has no CompEquippable";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (thingClass == typeof(Bullet) && projectile.damageDef == null)
			{
				yield return " is a bullet but has no damageDef.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (destroyOnDrop)
			{
				if (!menuHidden)
				{
					yield return "destroyOnDrop but not menuHidden.";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (tradeability != 0)
				{
					yield return "destroyOnDrop but tradeability is " + tradeability;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (stackLimit > 1 && !drawGUIOverlay)
			{
				yield return "has stackLimit > 1 but also has drawGUIOverlay = false.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (damageMultipliers != null)
			{
				using (List<DamageMultiplier>.Enumerator enumerator6 = damageMultipliers.GetEnumerator())
				{
					while (enumerator6.MoveNext())
					{
						_003CConfigErrors_003Ec__Iterator0 _003CConfigErrors_003Ec__Iterator2 = (_003CConfigErrors_003Ec__Iterator0)/*Error near IL_092f: stateMachine*/;
						DamageMultiplier mult = enumerator6.Current;
						if ((from m in damageMultipliers
						where m.damageDef == mult.damageDef
						select m).Count() > 1)
						{
							yield return "has multiple damage multipliers for damageDef " + mult.damageDef;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (Fillage == FillCategory.Full && !this.IsEdifice())
			{
				yield return "fillPercent is 1.00 but is not edifice";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (base.MadeFromStuff && constructEffect != null)
			{
				yield return "madeFromStuff but has a defined constructEffect (which will always be overridden by stuff's construct animation).";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (base.MadeFromStuff && stuffCategories.NullOrEmpty())
			{
				yield return "madeFromStuff but has no stuffCategories.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (costList.NullOrEmpty() && costStuffCount <= 0 && recipeMaker != null)
			{
				yield return "has a recipeMaker but no costList or costStuffCount.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (this.GetStatValueAbstract(StatDefOf.DeteriorationRate) > 1E-05f && !CanEverDeteriorate)
			{
				yield return "has >0 DeteriorationRate but can't deteriorate.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (drawerType == DrawerType.MapMeshOnly && comps.Any((CompProperties c) => c.compClass == typeof(CompForbiddable)))
			{
				yield return "drawerType=MapMeshOnly but has a CompForbiddable, which must draw in real time.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (smeltProducts != null && smeltable)
			{
				yield return "has smeltProducts but has smeltable=false";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (equipmentType != 0 && verbs.NullOrEmpty() && tools.NullOrEmpty())
			{
				yield return "is equipment but has no verbs or tools";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Minifiable && thingCategories.NullOrEmpty())
			{
				yield return "is minifiable but not in any thing category";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (category == ThingCategory.Building && !Minifiable && !thingCategories.NullOrEmpty())
			{
				yield return "is not minifiable yet has thing categories (could be confusing in thing filters because it can't be moved/stored anyway)";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (this != ThingDefOf.MinifiedThing && (EverHaulable || Minifiable) && (statBases.NullOrEmpty() || !statBases.Any((StatModifier s) => s.stat == StatDefOf.Mass)))
			{
				yield return "is haulable, but does not have an authored mass value";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (ingestible == null && this.GetStatValueAbstract(StatDefOf.Nutrition) != 0f)
			{
				yield return "has nutrition but ingestible properties are null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (BaseFlammability != 0f && !useHitPoints && category != ThingCategory.Pawn)
			{
				yield return "flammable but has no hitpoints (will burn indefinitely)";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (graphicData != null && graphicData.shadowData != null && staticSunShadowHeight > 0f)
			{
				yield return "graphicData defines a shadowInfo but staticSunShadowHeight > 0";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (saveCompressible && Claimable)
			{
				yield return "claimable item is compressible; faction will be unset after load";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (deepCommonality > 0f != deepLumpSizeRange.TrueMax > 0)
			{
				yield return "if deepCommonality or deepLumpSizeRange is set, the other also must be set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (deepCommonality > 0f && deepCountPerPortion <= 0)
			{
				yield return "deepCommonality > 0 but deepCountPerPortion is not set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (verbs != null)
			{
				for (int k = 0; k < verbs.Count; k++)
				{
					using (IEnumerator<string> enumerator7 = verbs[k].ConfigErrors(this).GetEnumerator())
					{
						if (enumerator7.MoveNext())
						{
							yield return string.Format(arg1: enumerator7.Current, format: "verb {0}: {1}", arg0: k);
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (race != null && tools != null)
			{
				_003CConfigErrors_003Ec__Iterator0 _003CConfigErrors_003Ec__Iterator3 = (_003CConfigErrors_003Ec__Iterator0)/*Error near IL_106f: stateMachine*/;
				int i;
				for (i = 0; i < tools.Count; i++)
				{
					if (tools[i].linkedBodyPartsGroup != null && !race.body.AllParts.Any((BodyPartRecord part) => part.groups.Contains(_003CConfigErrors_003Ec__Iterator3._0024this.tools[i].linkedBodyPartsGroup)))
					{
						yield return "has tool with linkedBodyPartsGroup " + tools[i].linkedBodyPartsGroup + " but body " + race.body + " has no parts with that group.";
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (building != null)
			{
				using (IEnumerator<string> enumerator8 = building.ConfigErrors(this).GetEnumerator())
				{
					if (enumerator8.MoveNext())
					{
						string err3 = enumerator8.Current;
						yield return err3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (apparel != null)
			{
				using (IEnumerator<string> enumerator9 = apparel.ConfigErrors(this).GetEnumerator())
				{
					if (enumerator9.MoveNext())
					{
						string err2 = enumerator9.Current;
						yield return err2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (comps != null)
			{
				for (int j = 0; j < comps.Count; j++)
				{
					using (IEnumerator<string> enumerator10 = comps[j].ConfigErrors(this).GetEnumerator())
					{
						if (enumerator10.MoveNext())
						{
							string err = enumerator10.Current;
							yield return err;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (race != null)
			{
				using (IEnumerator<string> enumerator11 = race.ConfigErrors().GetEnumerator())
				{
					if (enumerator11.MoveNext())
					{
						string e4 = enumerator11.Current;
						yield return e4;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (ingestible != null)
			{
				using (IEnumerator<string> enumerator12 = ingestible.ConfigErrors().GetEnumerator())
				{
					if (enumerator12.MoveNext())
					{
						string e3 = enumerator12.Current;
						yield return e3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (plant != null)
			{
				using (IEnumerator<string> enumerator13 = plant.ConfigErrors().GetEnumerator())
				{
					if (enumerator13.MoveNext())
					{
						string e2 = enumerator13.Current;
						yield return e2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (tools != null)
			{
				Tool dupeTool = tools.SelectMany(delegate(Tool lhs)
				{
					ThingDef _0024this = ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_15cd: stateMachine*/)._0024this;
					return from rhs in ((_003CConfigErrors_003Ec__Iterator0)/*Error near IL_15cd: stateMachine*/)._0024this.tools
					where lhs != rhs && lhs.id == rhs.id
					select rhs;
				}).FirstOrDefault();
				if (dupeTool != null)
				{
					yield return $"duplicate thingdef tool id {dupeTool.id}";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				foreach (Tool tool in tools)
				{
					using (IEnumerator<string> enumerator15 = tool.ConfigErrors().GetEnumerator())
					{
						if (enumerator15.MoveNext())
						{
							string e = enumerator15.Current;
							yield return e;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_1722:
			/*Error near IL_1723: Unexpected return in MoveNext()*/;
		}

		public static ThingDef Named(string defName)
		{
			return DefDatabase<ThingDef>.GetNamed(defName);
		}

		public bool IsWithinCategory(ThingCategoryDef category)
		{
			if (thingCategories == null)
			{
				return false;
			}
			for (int i = 0; i < thingCategories.Count; i++)
			{
				for (ThingCategoryDef thingCategoryDef = thingCategories[i]; thingCategoryDef != null; thingCategoryDef = thingCategoryDef.parent)
				{
					if (thingCategoryDef == category)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			using (IEnumerator<StatDrawEntry> enumerator = base.SpecialDisplayStats(req).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					StatDrawEntry stat = enumerator.Current;
					yield return stat;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (apparel != null)
			{
				yield return new StatDrawEntry(valueString: apparel.GetCoveredOuterPartsString(BodyDefOf.Human), category: StatCategoryDefOf.Apparel, label: "Covers".Translate(), displayPriorityWithinCategory: 100, overrideReportText: string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (IsMedicine && MedicineTendXpGainFactor != 1f)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MedicineXpGainFactor".Translate(), MedicineTendXpGainFactor.ToStringPercent(), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (fillPercent > 0f && fillPercent < 1f && (category == ThingCategory.Item || category == ThingCategory.Building || category == ThingCategory.Plant))
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CoverEffectiveness".Translate(), this.BaseBlockChance().ToStringPercent(), 0, string.Empty)
				{
					overrideReportText = "CoverEffectivenessExplanation".Translate()
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (constructionSkillPrerequisite > 0)
			{
				StatCategoryDef basics = StatCategoryDefOf.Basics;
				string label = "ConstructionSkillRequired".Translate();
				string valueString = constructionSkillPrerequisite.ToString();
				string overrideReportText = "ConstructionSkillRequiredExplanation".Translate();
				yield return new StatDrawEntry(basics, label, valueString, 0, overrideReportText);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!verbs.NullOrEmpty())
			{
				VerbProperties verb2 = verbs.First((VerbProperties x) => x.isPrimary);
				object statCategoryDef;
				if (category == ThingCategory.Pawn)
				{
					StatCategoryDef basics = StatCategoryDefOf.PawnCombat;
					statCategoryDef = basics;
				}
				else
				{
					StatCategoryDef basics = StatCategoryDefOf.Weapon;
					statCategoryDef = basics;
				}
				StatCategoryDef verbStatCategory = (StatCategoryDef)statCategoryDef;
				float warmup = verb2.warmupTime;
				if (warmup > 0f)
				{
					string warmupLabel = (category != ThingCategory.Pawn) ? "WarmupTime".Translate() : "MeleeWarmupTime".Translate();
					yield return new StatDrawEntry(verbStatCategory, warmupLabel, warmup.ToString("0.##") + " s", 40, string.Empty);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (verb2.defaultProjectile != null)
				{
					StringBuilder damageAmountExplanation = new StringBuilder();
					float dam = (float)verb2.defaultProjectile.projectile.GetDamageAmount(req.Thing, damageAmountExplanation);
					yield return new StatDrawEntry(verbStatCategory, "Damage".Translate(), dam.ToString(), 50, damageAmountExplanation.ToString());
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (verb2.LaunchesProjectile)
				{
					int burstShotCount = verb2.burstShotCount;
					float num = 60f / verb2.ticksBetweenBurstShots.TicksToSeconds();
					float range = verb2.range;
					if (burstShotCount <= 1)
					{
						yield return new StatDrawEntry(verbStatCategory, "Range".Translate(), range.ToString("F0"), 10, string.Empty);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					yield return new StatDrawEntry(verbStatCategory, "BurstShotCount".Translate(), burstShotCount.ToString(), 20, string.Empty);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (verb2.forcedMissRadius > 0f)
				{
					yield return new StatDrawEntry(verbStatCategory, "MissRadius".Translate(), verb2.forcedMissRadius.ToString("0.#"), 30, string.Empty);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (plant != null)
			{
				using (IEnumerator<StatDrawEntry> enumerator2 = plant.SpecialDisplayStats().GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						StatDrawEntry s6 = enumerator2.Current;
						yield return s6;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (ingestible != null)
			{
				using (IEnumerator<StatDrawEntry> enumerator3 = ingestible.SpecialDisplayStats().GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						StatDrawEntry s5 = enumerator3.Current;
						yield return s5;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (race != null)
			{
				using (IEnumerator<StatDrawEntry> enumerator4 = race.SpecialDisplayStats(this).GetEnumerator())
				{
					if (enumerator4.MoveNext())
					{
						StatDrawEntry s4 = enumerator4.Current;
						yield return s4;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (building != null)
			{
				using (IEnumerator<StatDrawEntry> enumerator5 = building.SpecialDisplayStats(this, req).GetEnumerator())
				{
					if (enumerator5.MoveNext())
					{
						StatDrawEntry s3 = enumerator5.Current;
						yield return s3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (isTechHediff)
			{
				foreach (RecipeDef item in from x in DefDatabase<RecipeDef>.AllDefs
				where x.IsIngredient(((_003CSpecialDisplayStats_003Ec__Iterator1)/*Error near IL_0b37: stateMachine*/)._0024this)
				select x)
				{
					HediffDef diff = item.addsHediff;
					if (diff != null)
					{
						if (diff.addedPartProps != null)
						{
							yield return new StatDrawEntry(StatCategoryDefOf.Basics, "BodyPartEfficiency".Translate(), diff.addedPartProps.partEfficiency.ToStringByStyle(ToStringStyle.PercentZero), 0, string.Empty);
							/*Error: Unable to find new state assignment for yield return*/;
						}
						using (IEnumerator<StatDrawEntry> enumerator7 = diff.SpecialDisplayStats(StatRequest.ForEmpty()).GetEnumerator())
						{
							if (enumerator7.MoveNext())
							{
								StatDrawEntry s2 = enumerator7.Current;
								yield return s2;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
						HediffCompProperties_VerbGiver vg = diff.CompProps<HediffCompProperties_VerbGiver>();
						if (vg != null)
						{
							if (!vg.verbs.NullOrEmpty())
							{
								VerbProperties verb = vg.verbs[0];
								if (!verb.IsMeleeAttack)
								{
									if (verb.defaultProjectile != null)
									{
										int projDamage = verb.defaultProjectile.projectile.GetDamageAmount(null);
										yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Damage".Translate(), projDamage.ToString(), 0, string.Empty);
										/*Error: Unable to find new state assignment for yield return*/;
									}
								}
								else
								{
									int meleeDamage = verb.meleeDamageBaseAmount;
									if (verb.meleeDamageDef.armorCategory != null)
									{
										float armorPenetration2 = verb.meleeArmorPenetrationBase;
										if (armorPenetration2 < 0f)
										{
											armorPenetration2 = (float)meleeDamage * 0.015f;
										}
										StatCategoryDef basics = StatCategoryDefOf.Weapon;
										string overrideReportText = "ArmorPenetration".Translate();
										string valueString = armorPenetration2.ToStringPercent();
										string label = "ArmorPenetrationExplanation".Translate();
										yield return new StatDrawEntry(basics, overrideReportText, valueString, 0, label);
										/*Error: Unable to find new state assignment for yield return*/;
									}
								}
							}
							else if (!vg.tools.NullOrEmpty())
							{
								Tool tool = vg.tools[0];
								if (ThingUtility.PrimaryMeleeWeaponDamageType(vg.tools).armorCategory != null)
								{
									float armorPenetration = tool.armorPenetration;
									if (armorPenetration < 0f)
									{
										armorPenetration = tool.power * 0.015f;
									}
									StatCategoryDef basics = StatCategoryDefOf.Weapon;
									string label = "ArmorPenetration".Translate();
									string valueString = armorPenetration.ToStringPercent();
									string overrideReportText = "ArmorPenetrationExplanation".Translate();
									yield return new StatDrawEntry(basics, label, valueString, 0, overrideReportText);
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
						}
						ThoughtDef thought = DefDatabase<ThoughtDef>.AllDefs.FirstOrDefault((ThoughtDef x) => x.hediff == diff);
						if (thought != null && thought.stages != null && thought.stages.Any())
						{
							yield return new StatDrawEntry(StatCategoryDefOf.Basics, "MoodChange".Translate(), thought.stages.First().baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset), 0, string.Empty);
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			for (int i = 0; i < comps.Count; i++)
			{
				using (IEnumerator<StatDrawEntry> enumerator8 = comps[i].SpecialDisplayStats(req).GetEnumerator())
				{
					if (enumerator8.MoveNext())
					{
						StatDrawEntry s = enumerator8.Current;
						yield return s;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_1162:
			/*Error near IL_1163: Unexpected return in MoveNext()*/;
		}
	}
}
