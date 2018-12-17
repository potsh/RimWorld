using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public static class GenRecipe
	{
		public static IEnumerable<Thing> MakeRecipeProducts(RecipeDef recipeDef, Pawn worker, List<Thing> ingredients, Thing dominantIngredient, IBillGiver billGiver)
		{
			float efficiency = (recipeDef.efficiencyStat != null) ? worker.GetStatValue(recipeDef.efficiencyStat) : 1f;
			if (recipeDef.workTableEfficiencyStat != null)
			{
				Building_WorkTable building_WorkTable = billGiver as Building_WorkTable;
				if (building_WorkTable != null)
				{
					efficiency *= building_WorkTable.GetStatValue(recipeDef.workTableEfficiencyStat);
				}
			}
			if (recipeDef.products != null)
			{
				int k = 0;
				if (k < recipeDef.products.Count)
				{
					ThingDefCountClass prod = recipeDef.products[k];
					Thing product3 = ThingMaker.MakeThing(stuff: (!prod.thingDef.MadeFromStuff) ? null : dominantIngredient.def, def: prod.thingDef);
					product3.stackCount = Mathf.CeilToInt((float)prod.count * efficiency);
					if (dominantIngredient != null)
					{
						product3.SetColor(dominantIngredient.DrawColor, reportFailure: false);
					}
					CompIngredients ingredientsComp = product3.TryGetComp<CompIngredients>();
					if (ingredientsComp != null)
					{
						for (int l = 0; l < ingredients.Count; l++)
						{
							ingredientsComp.RegisterIngredient(ingredients[l].def);
						}
					}
					CompFoodPoisonable foodPoisonable = product3.TryGetComp<CompFoodPoisonable>();
					if (foodPoisonable != null)
					{
						float chance = worker.GetRoom()?.GetStat(RoomStatDefOf.FoodPoisonChance) ?? RoomStatDefOf.FoodPoisonChance.roomlessScore;
						if (Rand.Chance(chance))
						{
							foodPoisonable.SetPoisoned(FoodPoisonCause.FilthyKitchen);
						}
						else
						{
							float statValue = worker.GetStatValue(StatDefOf.FoodPoisonChance);
							if (Rand.Chance(statValue))
							{
								foodPoisonable.SetPoisoned(FoodPoisonCause.IncompetentCook);
							}
						}
					}
					yield return PostProcessProduct(product3, recipeDef, worker);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (recipeDef.specialProducts != null)
			{
				for (int j = 0; j < recipeDef.specialProducts.Count; j++)
				{
					for (int i = 0; i < ingredients.Count; i++)
					{
						Thing ing = ingredients[i];
						switch (recipeDef.specialProducts[j])
						{
						case SpecialProductType.Butchery:
							using (IEnumerator<Thing> enumerator2 = ing.ButcherProducts(worker, efficiency).GetEnumerator())
							{
								if (enumerator2.MoveNext())
								{
									Thing product = enumerator2.Current;
									yield return PostProcessProduct(product, recipeDef, worker);
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
							break;
						case SpecialProductType.Smelted:
							using (IEnumerator<Thing> enumerator = ing.SmeltProducts(efficiency).GetEnumerator())
							{
								if (enumerator.MoveNext())
								{
									Thing product2 = enumerator.Current;
									yield return PostProcessProduct(product2, recipeDef, worker);
									/*Error: Unable to find new state assignment for yield return*/;
								}
							}
							break;
						}
					}
				}
			}
			yield break;
			IL_04dd:
			/*Error near IL_04de: Unexpected return in MoveNext()*/;
		}

		private static Thing PostProcessProduct(Thing product, RecipeDef recipeDef, Pawn worker)
		{
			CompQuality compQuality = product.TryGetComp<CompQuality>();
			if (compQuality != null)
			{
				if (recipeDef.workSkill == null)
				{
					Log.Error(recipeDef + " needs workSkill because it creates a product with a quality.");
				}
				QualityCategory q = QualityUtility.GenerateQualityCreatedByPawn(worker, recipeDef.workSkill);
				compQuality.SetQuality(q, ArtGenerationContext.Colony);
				QualityUtility.SendCraftNotification(product, worker);
			}
			CompArt compArt = product.TryGetComp<CompArt>();
			if (compArt != null)
			{
				compArt.JustCreatedBy(worker);
				if ((int)compQuality.Quality >= 4)
				{
					TaleRecorder.RecordTale(TaleDefOf.CraftedArt, worker, product);
				}
			}
			if (product.def.Minifiable)
			{
				product = product.MakeMinified();
			}
			return product;
		}
	}
}
