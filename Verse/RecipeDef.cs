using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public class RecipeDef : Def
	{
		public Type workerClass = typeof(RecipeWorker);

		public Type workerCounterClass = typeof(RecipeWorkerCounter);

		[MustTranslate]
		public string jobString = "Doing an unknown recipe.";

		public WorkTypeDef requiredGiverWorkType;

		public float workAmount = -1f;

		public StatDef workSpeedStat;

		public StatDef efficiencyStat;

		public StatDef workTableEfficiencyStat;

		public StatDef workTableSpeedStat;

		public List<IngredientCount> ingredients = new List<IngredientCount>();

		public ThingFilter fixedIngredientFilter = new ThingFilter();

		public ThingFilter defaultIngredientFilter;

		public bool allowMixingIngredients;

		private Type ingredientValueGetterClass = typeof(IngredientValueGetter_Volume);

		public List<SpecialThingFilterDef> forceHiddenSpecialFilters;

		public bool autoStripCorpses = true;

		public List<ThingDefCountClass> products = new List<ThingDefCountClass>();

		public List<SpecialProductType> specialProducts;

		public bool productHasIngredientStuff;

		public int targetCountAdjustment = 1;

		public ThingDef unfinishedThingDef;

		public List<SkillRequirement> skillRequirements;

		public SkillDef workSkill;

		public float workSkillLearnFactor = 1f;

		public EffecterDef effectWorking;

		public SoundDef soundWorking;

		public List<ThingDef> recipeUsers;

		public List<BodyPartDef> appliedOnFixedBodyParts = new List<BodyPartDef>();

		public HediffDef addsHediff;

		public HediffDef removesHediff;

		public bool hideBodyPartNames;

		public bool isViolation;

		[MustTranslate]
		public string successfullyRemovedHediffMessage;

		public float surgerySuccessChanceFactor = 1f;

		public float deathOnFailedSurgeryChance;

		public bool targetsBodyPart = true;

		public bool anesthetize = true;

		public ResearchProjectDef researchPrerequisite;

		[NoTranslate]
		public List<string> factionPrerequisiteTags;

		public ConceptDef conceptLearned;

		public bool dontShowIfAnyIngredientMissing;

		[Unsaved]
		private RecipeWorker workerInt;

		[Unsaved]
		private RecipeWorkerCounter workerCounterInt;

		[Unsaved]
		private IngredientValueGetter ingredientValueGetterInt;

		[Unsaved]
		private List<ThingDef> premultipliedSmallIngredients;

		public RecipeWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (RecipeWorker)Activator.CreateInstance(workerClass);
					workerInt.recipe = this;
				}
				return workerInt;
			}
		}

		public RecipeWorkerCounter WorkerCounter
		{
			get
			{
				if (workerCounterInt == null)
				{
					workerCounterInt = (RecipeWorkerCounter)Activator.CreateInstance(workerCounterClass);
					workerCounterInt.recipe = this;
				}
				return workerCounterInt;
			}
		}

		public IngredientValueGetter IngredientValueGetter
		{
			get
			{
				if (ingredientValueGetterInt == null)
				{
					ingredientValueGetterInt = (IngredientValueGetter)Activator.CreateInstance(ingredientValueGetterClass);
				}
				return ingredientValueGetterInt;
			}
		}

		public bool AvailableNow
		{
			get
			{
				if (researchPrerequisite != null && !researchPrerequisite.IsFinished)
				{
					return false;
				}
				if (factionPrerequisiteTags != null && factionPrerequisiteTags.Any((string tag) => Faction.OfPlayer.def.recipePrerequisiteTags == null || !Faction.OfPlayer.def.recipePrerequisiteTags.Contains(tag)))
				{
					return false;
				}
				return true;
			}
		}

		public string MinSkillString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = false;
				if (skillRequirements != null)
				{
					for (int i = 0; i < skillRequirements.Count; i++)
					{
						SkillRequirement skillRequirement = skillRequirements[i];
						stringBuilder.AppendLine("   " + skillRequirement.skill.skillLabel.CapitalizeFirst() + ": " + skillRequirement.minLevel);
						flag = true;
					}
				}
				if (!flag)
				{
					stringBuilder.AppendLine("   (" + "NoneLower".Translate() + ")");
				}
				return stringBuilder.ToString();
			}
		}

		public IEnumerable<ThingDef> AllRecipeUsers
		{
			get
			{
				if (recipeUsers != null)
				{
					int j = 0;
					if (j < recipeUsers.Count)
					{
						yield return recipeUsers[j];
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				List<ThingDef> thingDefs = DefDatabase<ThingDef>.AllDefsListForReading;
				int i = 0;
				while (true)
				{
					if (i >= thingDefs.Count)
					{
						yield break;
					}
					if (thingDefs[i].recipes != null && thingDefs[i].recipes.Contains(this))
					{
						break;
					}
					i++;
				}
				yield return thingDefs[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool UsesUnfinishedThing => unfinishedThingDef != null;

		public bool IsSurgery
		{
			get
			{
				foreach (ThingDef allRecipeUser in AllRecipeUsers)
				{
					if (allRecipeUser.category == ThingCategory.Pawn)
					{
						return true;
					}
				}
				return false;
			}
		}

		public ThingDef ProducedThingDef
		{
			get
			{
				if (specialProducts != null)
				{
					return null;
				}
				if (products == null || products.Count != 1)
				{
					return null;
				}
				return products[0].thingDef;
			}
		}

		public float WorkAmountTotal(ThingDef stuffDef)
		{
			if (workAmount >= 0f)
			{
				return workAmount;
			}
			return products[0].thingDef.GetStatValueAbstract(StatDefOf.WorkToMake, stuffDef);
		}

		public IEnumerable<ThingDef> PotentiallyMissingIngredients(Pawn billDoer, Map map)
		{
			int i = 0;
			ThingDef def;
			while (true)
			{
				if (i >= ingredients.Count)
				{
					yield break;
				}
				IngredientCount ing = ingredients[i];
				bool foundIng = false;
				List<Thing> thingList = map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableEver);
				for (int j = 0; j < thingList.Count; j++)
				{
					Thing thing = thingList[j];
					if ((billDoer == null || !thing.IsForbidden(billDoer)) && !thing.Position.Fogged(map) && (ing.IsFixedIngredient || fixedIngredientFilter.Allows(thing)) && ing.filter.Allows(thing))
					{
						foundIng = true;
						break;
					}
				}
				if (!foundIng)
				{
					if (ing.IsFixedIngredient)
					{
						yield return ing.filter.AllowedThingDefs.First();
						/*Error: Unable to find new state assignment for yield return*/;
					}
					def = (from x in ing.filter.AllowedThingDefs
					orderby x.BaseMarketValue
					select x).FirstOrDefault((ThingDef x) => ((_003CPotentiallyMissingIngredients_003Ec__Iterator1)/*Error near IL_0190: stateMachine*/)._0024this.fixedIngredientFilter.Allows(x));
					if (def != null)
					{
						break;
					}
				}
				i++;
			}
			yield return def;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public bool IsIngredient(ThingDef th)
		{
			for (int i = 0; i < ingredients.Count; i++)
			{
				if (ingredients[i].filter.Allows(th) && (ingredients[i].IsFixedIngredient || fixedIngredientFilter.Allows(th)))
				{
					return true;
				}
			}
			return false;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (workerClass == null)
			{
				yield return "workerClass is null.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_00ec:
			/*Error near IL_00ed: Unexpected return in MoveNext()*/;
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (workTableSpeedStat == null)
			{
				workTableSpeedStat = StatDefOf.WorkTableWorkSpeedFactor;
			}
			if (workTableEfficiencyStat == null)
			{
				workTableEfficiencyStat = StatDefOf.WorkTableEfficiencyFactor;
			}
			for (int i = 0; i < ingredients.Count; i++)
			{
				ingredients[i].ResolveReferences();
			}
			if (fixedIngredientFilter != null)
			{
				fixedIngredientFilter.ResolveReferences();
			}
			if (defaultIngredientFilter == null)
			{
				defaultIngredientFilter = new ThingFilter();
				if (fixedIngredientFilter != null)
				{
					defaultIngredientFilter.CopyAllowancesFrom(fixedIngredientFilter);
				}
			}
			defaultIngredientFilter.ResolveReferences();
		}

		public bool PawnSatisfiesSkillRequirements(Pawn pawn)
		{
			return FirstSkillRequirementPawnDoesntSatisfy(pawn) == null;
		}

		public SkillRequirement FirstSkillRequirementPawnDoesntSatisfy(Pawn pawn)
		{
			if (skillRequirements == null)
			{
				return null;
			}
			for (int i = 0; i < skillRequirements.Count; i++)
			{
				if (!skillRequirements[i].PawnSatisfies(pawn))
				{
					return skillRequirements[i];
				}
			}
			return null;
		}

		public List<ThingDef> GetPremultipliedSmallIngredients()
		{
			if (premultipliedSmallIngredients != null)
			{
				return premultipliedSmallIngredients;
			}
			premultipliedSmallIngredients = (from td in ingredients.SelectMany((IngredientCount ingredient) => ingredient.filter.AllowedThingDefs)
			where td.smallVolume
			select td).Distinct().ToList();
			bool flag = true;
			while (flag)
			{
				flag = false;
				for (int i = 0; i < ingredients.Count; i++)
				{
					if (ingredients[i].filter.AllowedThingDefs.Any((ThingDef td) => !premultipliedSmallIngredients.Contains(td)))
					{
						foreach (ThingDef allowedThingDef in ingredients[i].filter.AllowedThingDefs)
						{
							flag |= premultipliedSmallIngredients.Remove(allowedThingDef);
						}
					}
				}
			}
			return premultipliedSmallIngredients;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			if (workSkill != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Skill".Translate(), workSkill.LabelCap, 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (ingredients != null && ingredients.Count > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Ingredients".Translate(), (from ic in ingredients
				select ic.Summary).ToCommaList(useAnd: true), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (skillRequirements != null && skillRequirements.Count > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "SkillRequirements".Translate(), (from sr in skillRequirements
				select sr.Summary).ToCommaList(useAnd: true), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (products != null && products.Count > 0)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "Products".Translate(), (from pr in products
				select pr.Summary).ToCommaList(useAnd: true), 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (workSpeedStat != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "WorkSpeedStat".Translate(), workSpeedStat.LabelCap, 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (efficiencyStat != null)
			{
				yield return new StatDrawEntry(StatCategoryDefOf.Basics, "EfficiencyStat".Translate(), efficiencyStat.LabelCap, 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (IsSurgery)
			{
				if (!(surgerySuccessChanceFactor >= 99999f))
				{
					yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgerySuccessChanceFactor".Translate(), surgerySuccessChanceFactor.ToStringPercent(), 0, string.Empty);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return new StatDrawEntry(StatCategoryDefOf.Surgery, "SurgerySuccessChanceFactor".Translate(), "100%", 0, string.Empty);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
