using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class RaceProperties
	{
		public Intelligence intelligence;

		private FleshTypeDef fleshType;

		private ThingDef bloodDef;

		public bool hasGenders = true;

		public bool needsRest = true;

		public ThinkTreeDef thinkTreeMain;

		public ThinkTreeDef thinkTreeConstant;

		public PawnNameCategory nameCategory;

		public FoodTypeFlags foodType;

		public BodyDef body;

		public Type deathActionWorkerClass;

		public List<AnimalBiomeRecord> wildBiomes;

		public SimpleCurve ageGenerationCurve;

		public bool makesFootprints;

		public int executionRange = 2;

		public float lifeExpectancy = 10f;

		public List<HediffGiverSetDef> hediffGiverSets;

		public bool herdAnimal;

		public bool packAnimal;

		public bool predator;

		public float maxPreyBodySize = 99999f;

		public float wildness;

		public float petness;

		public float nuzzleMtbHours = -1f;

		public float manhunterOnDamageChance;

		public float manhunterOnTameFailChance;

		public bool canBePredatorPrey = true;

		public bool herdMigrationAllowed = true;

		public float gestationPeriodDays = 10f;

		public SimpleCurve litterSizeCurve;

		public float mateMtbHours = 12f;

		[NoTranslate]
		public List<string> untrainableTags;

		[NoTranslate]
		public List<string> trainableTags;

		public TrainabilityDef trainability;

		private RulePackDef nameGenerator;

		private RulePackDef nameGeneratorFemale;

		public float nameOnTameChance;

		public float nameOnNuzzleChance;

		public float baseBodySize = 1f;

		public float baseHealthScale = 1f;

		public float baseHungerRate = 1f;

		public List<LifeStageAge> lifeStageAges = new List<LifeStageAge>();

		[MustTranslate]
		public string meatLabel;

		public Color meatColor = Color.white;

		public float meatMarketValue = 2f;

		public ThingDef useMeatFrom;

		public ThingDef useLeatherFrom;

		public ThingDef leatherDef;

		public ShadowData specialShadowData;

		public IntRange soundCallIntervalRange = new IntRange(2000, 4000);

		public SoundDef soundMeleeHitPawn;

		public SoundDef soundMeleeHitBuilding;

		public SoundDef soundMeleeMiss;

		[Unsaved]
		private DeathActionWorker deathActionWorkerInt;

		[Unsaved]
		public ThingDef meatDef;

		[Unsaved]
		public ThingDef corpseDef;

		[Unsaved]
		private PawnKindDef cachedAnyPawnKind;

		public bool Humanlike => (int)intelligence >= 2;

		public bool ToolUser => (int)intelligence >= 1;

		public bool Animal => !ToolUser && IsFlesh;

		public bool EatsFood => foodType != FoodTypeFlags.None;

		public float FoodLevelPercentageWantEat
		{
			get
			{
				switch (ResolvedDietCategory)
				{
				case DietCategory.NeverEats:
					return 0.3f;
				case DietCategory.Omnivorous:
					return 0.3f;
				case DietCategory.Carnivorous:
					return 0.3f;
				case DietCategory.Ovivorous:
					return 0.4f;
				case DietCategory.Herbivorous:
					return 0.45f;
				case DietCategory.Dendrovorous:
					return 0.45f;
				default:
					throw new InvalidOperationException();
				}
			}
		}

		public DietCategory ResolvedDietCategory
		{
			get
			{
				if (!EatsFood)
				{
					return DietCategory.NeverEats;
				}
				if (Eats(FoodTypeFlags.Tree))
				{
					return DietCategory.Dendrovorous;
				}
				if (Eats(FoodTypeFlags.Meat))
				{
					if (Eats(FoodTypeFlags.VegetableOrFruit) || Eats(FoodTypeFlags.Plant))
					{
						return DietCategory.Omnivorous;
					}
					return DietCategory.Carnivorous;
				}
				if (Eats(FoodTypeFlags.AnimalProduct))
				{
					return DietCategory.Ovivorous;
				}
				return DietCategory.Herbivorous;
			}
		}

		public DeathActionWorker DeathActionWorker
		{
			get
			{
				if (deathActionWorkerInt == null)
				{
					if (deathActionWorkerClass != null)
					{
						deathActionWorkerInt = (DeathActionWorker)Activator.CreateInstance(deathActionWorkerClass);
					}
					else
					{
						deathActionWorkerInt = new DeathActionWorker_Simple();
					}
				}
				return deathActionWorkerInt;
			}
		}

		public FleshTypeDef FleshType
		{
			get
			{
				if (fleshType != null)
				{
					return fleshType;
				}
				return FleshTypeDefOf.Normal;
			}
		}

		public bool IsMechanoid => FleshType == FleshTypeDefOf.Mechanoid;

		public bool IsFlesh => FleshType != FleshTypeDefOf.Mechanoid;

		public ThingDef BloodDef
		{
			get
			{
				if (bloodDef != null)
				{
					return bloodDef;
				}
				if (IsFlesh)
				{
					return ThingDefOf.Filth_Blood;
				}
				return null;
			}
		}

		public bool CanDoHerdMigration => Animal && herdMigrationAllowed;

		public PawnKindDef AnyPawnKind
		{
			get
			{
				if (cachedAnyPawnKind == null)
				{
					List<PawnKindDef> allDefsListForReading = DefDatabase<PawnKindDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].race.race == this)
						{
							cachedAnyPawnKind = allDefsListForReading[i];
							break;
						}
					}
				}
				return cachedAnyPawnKind;
			}
		}

		public RulePackDef GetNameGenerator(Gender gender)
		{
			if (gender == Gender.Female && nameGeneratorFemale != null)
			{
				return nameGeneratorFemale;
			}
			return nameGenerator;
		}

		public bool CanEverEat(Thing t)
		{
			return CanEverEat(t.def);
		}

		public bool CanEverEat(ThingDef t)
		{
			if (!EatsFood)
			{
				return false;
			}
			if (t.ingestible == null)
			{
				return false;
			}
			if (t.ingestible.preferability == FoodPreferability.Undefined)
			{
				return false;
			}
			return Eats(t.ingestible.foodType);
		}

		public bool Eats(FoodTypeFlags food)
		{
			if (!EatsFood)
			{
				return false;
			}
			return (foodType & food) != FoodTypeFlags.None;
		}

		public void ResolveReferencesSpecial()
		{
			if (useMeatFrom != null)
			{
				meatDef = useMeatFrom.race.meatDef;
			}
			if (useLeatherFrom != null)
			{
				leatherDef = useLeatherFrom.race.leatherDef;
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (soundMeleeHitPawn == null)
			{
				yield return "soundMeleeHitPawn is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (soundMeleeHitBuilding == null)
			{
				yield return "soundMeleeHitBuilding is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (soundMeleeMiss == null)
			{
				yield return "soundMeleeMiss is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (predator && !Eats(FoodTypeFlags.Meat))
			{
				yield return "predator but doesn't eat meat";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			for (int j = 0; j < lifeStageAges.Count; j++)
			{
				for (int i = 0; i < j; i++)
				{
					if (lifeStageAges[i].minAge > lifeStageAges[j].minAge)
					{
						yield return "lifeStages minAges are not in ascending order";
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (litterSizeCurve != null)
			{
				using (IEnumerator<string> enumerator = litterSizeCurve.ConfigErrors("litterSizeCurve").GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						string e = enumerator.Current;
						yield return e;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (nameOnTameChance > 0f && nameGenerator == null)
			{
				yield return "can be named, but has no nameGenerator";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Animal && wildness < 0f)
			{
				yield return "is animal but wildness is not defined";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (useMeatFrom != null && useMeatFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use meat from non-pawn " + useMeatFrom;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (useMeatFrom != null && useMeatFrom.race.useMeatFrom != null)
			{
				yield return "tries to use meat from " + useMeatFrom + " which uses meat from " + useMeatFrom.race.useMeatFrom;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (useLeatherFrom != null && useLeatherFrom.category != ThingCategory.Pawn)
			{
				yield return "tries to use leather from non-pawn " + useLeatherFrom;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (useLeatherFrom != null && useLeatherFrom.race.useLeatherFrom != null)
			{
				yield return "tries to use leather from " + useLeatherFrom + " which uses leather from " + useLeatherFrom.race.useLeatherFrom;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Animal && trainability == null)
			{
				yield return "animal has trainability = null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_050a:
			/*Error near IL_050b: Unexpected return in MoveNext()*/;
		}

		public IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
		{
			yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Race".Translate(), parentDef.LabelCap, 2000, string.Empty);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
