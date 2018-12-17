using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Corpses
	{
		private const float DaysToStartRot = 2.5f;

		private const float DaysToDessicate = 5f;

		private const float RotDamagePerDay = 2f;

		private const float DessicatedDamagePerDay = 0.7f;

		private const float ButcherProductsMarketValueFactor = 0.6f;

		public static IEnumerable<ThingDef> ImpliedCorpseDefs()
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.ToList())
			{
				if (item.category == ThingCategory.Pawn)
				{
					ThingDef d = new ThingDef
					{
						category = ThingCategory.Item,
						thingClass = typeof(Corpse),
						selectable = true,
						tickerType = TickerType.Rare,
						altitudeLayer = AltitudeLayer.ItemImportant,
						scatterableOnMapGen = false
					};
					d.SetStatBaseValue(StatDefOf.Beauty, -50f);
					d.SetStatBaseValue(StatDefOf.DeteriorationRate, 1f);
					d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.05f);
					d.alwaysHaulable = true;
					d.soundDrop = SoundDefOf.Corpse_Drop;
					d.pathCost = 15;
					d.socialPropernessMatters = false;
					d.tradeability = Tradeability.None;
					d.inspectorTabs = new List<Type>();
					d.inspectorTabs.Add(typeof(ITab_Pawn_Health));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Character));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Gear));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Social));
					d.inspectorTabs.Add(typeof(ITab_Pawn_Log));
					d.comps.Add(new CompProperties_Forbiddable());
					d.recipes = new List<RecipeDef>();
					if (!item.race.IsMechanoid)
					{
						d.recipes.Add(RecipeDefOf.RemoveBodyPart);
					}
					d.defName = "Corpse_" + item.defName;
					d.label = "CorpseLabel".Translate(item.label);
					d.description = "CorpseDesc".Translate(item.label);
					d.soundImpactDefault = item.soundImpactDefault;
					d.SetStatBaseValue(StatDefOf.MarketValue, CalculateMarketValue(item));
					d.SetStatBaseValue(StatDefOf.Flammability, item.GetStatValueAbstract(StatDefOf.Flammability));
					d.SetStatBaseValue(StatDefOf.MaxHitPoints, (float)item.BaseMaxHitPoints);
					d.SetStatBaseValue(StatDefOf.Mass, item.statBases.GetStatOffsetFromList(StatDefOf.Mass));
					d.SetStatBaseValue(StatDefOf.Nutrition, 5.2f);
					d.modContentPack = item.modContentPack;
					d.ingestible = new IngestibleProperties();
					d.ingestible.parent = d;
					IngestibleProperties ing = d.ingestible;
					ing.foodType = FoodTypeFlags.Corpse;
					ing.sourceDef = item;
					ing.preferability = ((!item.race.IsFlesh) ? FoodPreferability.NeverForNutrition : FoodPreferability.DesperateOnly);
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(ing, "tasteThought", ThoughtDefOf.AteCorpse.defName);
					ing.maxNumToIngestAtOnce = 1;
					ing.ingestEffect = EffecterDefOf.EatMeat;
					ing.ingestSound = SoundDefOf.RawMeat_Eat;
					ing.specialThoughtDirect = item.race.FleshType.ateDirect;
					if (item.race.IsFlesh)
					{
						CompProperties_Rottable compProperties_Rottable = new CompProperties_Rottable();
						compProperties_Rottable.daysToRotStart = 2.5f;
						compProperties_Rottable.daysToDessicated = 5f;
						compProperties_Rottable.rotDamagePerDay = 2f;
						compProperties_Rottable.dessicatedDamagePerDay = 0.7f;
						d.comps.Add(compProperties_Rottable);
						CompProperties_SpawnerFilth compProperties_SpawnerFilth = new CompProperties_SpawnerFilth();
						compProperties_SpawnerFilth.filthDef = ThingDefOf.Filth_CorpseBile;
						compProperties_SpawnerFilth.spawnCountOnSpawn = 0;
						compProperties_SpawnerFilth.spawnMtbHours = 0f;
						compProperties_SpawnerFilth.spawnRadius = 0.1f;
						compProperties_SpawnerFilth.spawnEveryDays = 1f;
						compProperties_SpawnerFilth.requiredRotStage = RotStage.Rotting;
						d.comps.Add(compProperties_SpawnerFilth);
					}
					if (d.thingCategories == null)
					{
						d.thingCategories = new List<ThingCategoryDef>();
					}
					if (item.race.Humanlike)
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(d.thingCategories, ThingCategoryDefOf.CorpsesHumanlike.defName, d);
					}
					else
					{
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(d.thingCategories, item.race.FleshType.corpseCategory.defName, d);
					}
					item.race.corpseDef = d;
					yield return d;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_05de:
			/*Error near IL_05df: Unexpected return in MoveNext()*/;
		}

		private static float CalculateMarketValue(ThingDef raceDef)
		{
			float num = 0f;
			if (raceDef.race.meatDef != null)
			{
				int num2 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.MeatAmount));
				num += (float)num2 * raceDef.race.meatDef.GetStatValueAbstract(StatDefOf.MarketValue);
			}
			if (raceDef.race.leatherDef != null)
			{
				int num3 = Mathf.RoundToInt(raceDef.GetStatValueAbstract(StatDefOf.LeatherAmount));
				num += (float)num3 * raceDef.race.leatherDef.GetStatValueAbstract(StatDefOf.MarketValue);
			}
			if (raceDef.butcherProducts != null)
			{
				for (int i = 0; i < raceDef.butcherProducts.Count; i++)
				{
					num += raceDef.butcherProducts[i].thingDef.BaseMarketValue * (float)raceDef.butcherProducts[i].count;
				}
			}
			return num * 0.6f;
		}
	}
}
