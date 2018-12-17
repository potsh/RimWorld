using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class FactionDef : Def
	{
		public bool isPlayer;

		public RulePackDef factionNameMaker;

		public RulePackDef settlementNameMaker;

		public RulePackDef playerInitialSettlementNameMaker;

		[MustTranslate]
		public string fixedName;

		public bool humanlikeFaction = true;

		public bool hidden;

		public float listOrderPriority;

		public List<PawnGroupMaker> pawnGroupMakers;

		public SimpleCurve raidCommonalityFromPointsCurve;

		public bool autoFlee = true;

		public bool canSiege;

		public bool canStageAttacks;

		public bool canUseAvoidGrid = true;

		public float earliestRaidDays;

		public FloatRange allowedArrivalTemperatureRange = new FloatRange(-1000f, 1000f);

		public PawnKindDef basicMemberKind;

		public List<ResearchProjectTagDef> startingResearchTags;

		[NoTranslate]
		public List<string> recipePrerequisiteTags;

		public bool rescueesCanJoin;

		[MustTranslate]
		public string pawnSingular = "member";

		[MustTranslate]
		public string pawnsPlural = "members";

		public string leaderTitle = "leader";

		public float forageabilityFactor = 1f;

		public SimpleCurve maxPawnCostPerTotalPointsCurve;

		public int requiredCountAtGameStart;

		public int maxCountAtGameStart = 9999;

		public bool canMakeRandomly;

		public float settlementGenerationWeight;

		public RulePackDef pawnNameMaker;

		public TechLevel techLevel;

		[NoTranslate]
		public List<string> backstoryCategories;

		[NoTranslate]
		public List<string> hairTags = new List<string>();

		public ThingFilter apparelStuffFilter;

		public List<TraderKindDef> caravanTraderKinds = new List<TraderKindDef>();

		public List<TraderKindDef> visitorTraderKinds = new List<TraderKindDef>();

		public List<TraderKindDef> baseTraderKinds = new List<TraderKindDef>();

		public float geneticVariance = 1f;

		public IntRange startingGoodwill = IntRange.zero;

		public bool mustStartOneEnemy;

		public IntRange naturalColonyGoodwill = IntRange.zero;

		public float goodwillDailyGain;

		public float goodwillDailyFall;

		public bool permanentEnemy;

		[NoTranslate]
		public string homeIconPath;

		[NoTranslate]
		public string expandingIconTexture;

		public List<Color> colorSpectrum;

		[Unsaved]
		private Texture2D expandingIconTextureInt;

		public bool CanEverBeNonHostile => !permanentEnemy;

		public Texture2D ExpandingIconTexture
		{
			get
			{
				if (expandingIconTextureInt == null)
				{
					if (!expandingIconTexture.NullOrEmpty())
					{
						expandingIconTextureInt = ContentFinder<Texture2D>.Get(expandingIconTexture);
					}
					else
					{
						expandingIconTextureInt = BaseContent.BadTex;
					}
				}
				return expandingIconTextureInt;
			}
		}

		public float MinPointsToGeneratePawnGroup(PawnGroupKindDef groupKind)
		{
			if (pawnGroupMakers == null)
			{
				return 0f;
			}
			IEnumerable<PawnGroupMaker> source = from x in pawnGroupMakers
			where x.kindDef == groupKind
			select x;
			if (!source.Any())
			{
				return 0f;
			}
			return source.Min((PawnGroupMaker pgm) => pgm.MinPointsToGenerateAnything);
		}

		public bool CanUseStuffForApparel(ThingDef stuffDef)
		{
			if (apparelStuffFilter == null)
			{
				return true;
			}
			return apparelStuffFilter.Allows(stuffDef);
		}

		public float RaidCommonalityFromPoints(float points)
		{
			if (points < 0f || raidCommonalityFromPointsCurve == null)
			{
				return 1f;
			}
			return raidCommonalityFromPointsCurve.Evaluate(points);
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (apparelStuffFilter != null)
			{
				apparelStuffFilter.ResolveReferences();
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string error = enumerator.Current;
					yield return error;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (pawnGroupMakers != null && maxPawnCostPerTotalPointsCurve == null)
			{
				yield return "has pawnGroupMakers but missing maxPawnCostPerTotalPointsCurve";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!isPlayer && factionNameMaker == null && fixedName == null)
			{
				yield return "FactionTypeDef " + defName + " lacks a factionNameMaker and a fixedName.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (techLevel == TechLevel.Undefined)
			{
				yield return defName + " has no tech level.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (humanlikeFaction)
			{
				if (backstoryCategories.NullOrEmpty())
				{
					yield return defName + " is humanlikeFaction but has no backstory categories.";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (hairTags.Count == 0)
				{
					yield return defName + " is humanlikeFaction but has no hairTags.";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (isPlayer)
			{
				if (settlementNameMaker == null)
				{
					yield return "isPlayer is true but settlementNameMaker is null";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (factionNameMaker == null)
				{
					yield return "isPlayer is true but factionNameMaker is null";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (playerInitialSettlementNameMaker == null)
				{
					yield return "isPlayer is true but playerInitialSettlementNameMaker is null";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (permanentEnemy)
			{
				if (mustStartOneEnemy)
				{
					yield return "permanentEnemy has mustStartOneEnemy = true, which is redundant";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (goodwillDailyFall != 0f || goodwillDailyGain != 0f)
				{
					yield return "permanentEnemy has a goodwillDailyFall or goodwillDailyGain";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (startingGoodwill != IntRange.zero)
				{
					yield return "permanentEnemy has a startingGoodwill defined";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (naturalColonyGoodwill != IntRange.zero)
				{
					yield return "permanentEnemy has a naturalColonyGoodwill defined";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_03ff:
			/*Error near IL_0400: Unexpected return in MoveNext()*/;
		}

		public static FactionDef Named(string defName)
		{
			return DefDatabase<FactionDef>.GetNamed(defName);
		}
	}
}
