using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ThingDefGenerator_Meat
	{
		public static IEnumerable<ThingDef> ImpliedMeatDefs()
		{
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.ToList())
			{
				if (item.category == ThingCategory.Pawn && item.race.useMeatFrom == null)
				{
					if (item.race.IsFlesh)
					{
						ThingDef d = new ThingDef();
						d.resourceReadoutPriority = ResourceCountPriority.Middle;
						d.category = ThingCategory.Item;
						d.thingClass = typeof(ThingWithComps);
						d.graphicData = new GraphicData();
						d.graphicData.graphicClass = typeof(Graphic_StackCount);
						d.useHitPoints = true;
						d.selectable = true;
						d.SetStatBaseValue(StatDefOf.MaxHitPoints, 100f);
						d.altitudeLayer = AltitudeLayer.Item;
						d.stackLimit = 75;
						d.comps.Add(new CompProperties_Forbiddable());
						CompProperties_Rottable rotProps = new CompProperties_Rottable
						{
							daysToRotStart = 2f,
							rotDestroys = true
						};
						d.comps.Add(rotProps);
						d.tickerType = TickerType.Rare;
						d.SetStatBaseValue(StatDefOf.Beauty, -4f);
						d.alwaysHaulable = true;
						d.rotatable = false;
						d.pathCost = 15;
						d.drawGUIOverlay = true;
						d.socialPropernessMatters = true;
						d.modContentPack = item.modContentPack;
						d.category = ThingCategory.Item;
						if (item.race.Humanlike)
						{
							d.description = "MeatHumanDesc".Translate(item.label);
						}
						else if (item.race.FleshType == FleshTypeDefOf.Insectoid)
						{
							d.description = "MeatInsectDesc".Translate(item.label);
						}
						else
						{
							d.description = "MeatDesc".Translate(item.label);
						}
						d.useHitPoints = true;
						d.SetStatBaseValue(StatDefOf.MaxHitPoints, 60f);
						d.SetStatBaseValue(StatDefOf.DeteriorationRate, 6f);
						d.SetStatBaseValue(StatDefOf.Mass, 0.03f);
						d.SetStatBaseValue(StatDefOf.Flammability, 0.5f);
						d.SetStatBaseValue(StatDefOf.Nutrition, 0.05f);
						d.SetStatBaseValue(StatDefOf.FoodPoisonChanceFixedHuman, 0.02f);
						d.BaseMarketValue = item.race.meatMarketValue;
						if (d.thingCategories == null)
						{
							d.thingCategories = new List<ThingCategoryDef>();
						}
						DirectXmlCrossRefLoader.RegisterListWantsCrossRef(d.thingCategories, "MeatRaw", d);
						d.ingestible = new IngestibleProperties();
						d.ingestible.parent = d;
						d.ingestible.foodType = FoodTypeFlags.Meat;
						d.ingestible.preferability = FoodPreferability.RawBad;
						DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(d.ingestible, "tasteThought", ThoughtDefOf.AteRawFood.defName);
						d.ingestible.ingestEffect = EffecterDefOf.EatMeat;
						d.ingestible.ingestSound = SoundDefOf.RawMeat_Eat;
						d.ingestible.specialThoughtDirect = item.race.FleshType.ateDirect;
						d.ingestible.specialThoughtAsIngredient = item.race.FleshType.ateAsIngredient;
						d.graphicData.color = item.race.meatColor;
						if (item.race.Humanlike)
						{
							d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Human";
						}
						else if (item.race.FleshType == FleshTypeDefOf.Insectoid)
						{
							d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Insect";
						}
						else if (item.race.baseBodySize < 0.7f)
						{
							d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Small";
						}
						else
						{
							d.graphicData.texPath = "Things/Item/Resource/MeatFoodRaw/Meat_Big";
						}
						d.defName = "Meat_" + item.defName;
						if (item.race.meatLabel.NullOrEmpty())
						{
							d.label = "MeatLabel".Translate(item.label);
						}
						else
						{
							d.label = item.race.meatLabel;
						}
						d.ingestible.sourceDef = item;
						item.race.meatDef = d;
						yield return d;
						/*Error: Unable to find new state assignment for yield return*/;
					}
					DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(item.race, "meatDef", "Steel");
				}
			}
			yield break;
			IL_066c:
			/*Error near IL_066d: Unexpected return in MoveNext()*/;
		}
	}
}
