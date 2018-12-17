using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnWeaponGenerator
	{
		private static List<ThingStuffPair> allWeaponPairs;

		private static List<ThingStuffPair> workingWeapons = new List<ThingStuffPair>();

		public static void Reset()
		{
			Predicate<ThingDef> isWeapon = (ThingDef td) => td.equipmentType == EquipmentType.Primary && !td.weaponTags.NullOrEmpty();
			allWeaponPairs = ThingStuffPair.AllWith(isWeapon);
			foreach (ThingDef item in from td in DefDatabase<ThingDef>.AllDefs
			where isWeapon(td)
			select td)
			{
				float num = allWeaponPairs.Where((ThingStuffPair pa) => pa.thing == item).Sum((ThingStuffPair pa) => pa.Commonality);
				float num2 = item.generateCommonality / num;
				if (num2 != 1f)
				{
					for (int i = 0; i < allWeaponPairs.Count; i++)
					{
						ThingStuffPair thingStuffPair = allWeaponPairs[i];
						if (thingStuffPair.thing == item)
						{
							allWeaponPairs[i] = new ThingStuffPair(thingStuffPair.thing, thingStuffPair.stuff, thingStuffPair.commonalityMultiplier * num2);
						}
					}
				}
			}
		}

		public static void TryGenerateWeaponFor(Pawn pawn)
		{
			workingWeapons.Clear();
			if (pawn.kindDef.weaponTags != null && pawn.kindDef.weaponTags.Count != 0 && pawn.RaceProps.ToolUser && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && (pawn.story == null || !pawn.story.WorkTagIsDisabled(WorkTags.Violent)))
			{
				float randomInRange = pawn.kindDef.weaponMoney.RandomInRange;
				for (int i = 0; i < allWeaponPairs.Count; i++)
				{
					ThingStuffPair w2 = allWeaponPairs[i];
					if (!(w2.Price > randomInRange) && (pawn.kindDef.weaponTags == null || pawn.kindDef.weaponTags.Any((string tag) => w2.thing.weaponTags.Contains(tag))) && (!(w2.thing.generateAllowChance < 1f) || Rand.ChanceSeeded(w2.thing.generateAllowChance, pawn.thingIDNumber ^ w2.thing.shortHash ^ 0x1B3B648)))
					{
						workingWeapons.Add(w2);
					}
				}
				if (workingWeapons.Count != 0)
				{
					pawn.equipment.DestroyAllEquipment();
					if (workingWeapons.TryRandomElementByWeight((ThingStuffPair w) => w.Commonality * w.Price, out ThingStuffPair result))
					{
						ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(result.thing, result.stuff);
						PawnGenerator.PostProcessGeneratedGear(thingWithComps, pawn);
						pawn.equipment.AddEquipment(thingWithComps);
					}
					workingWeapons.Clear();
				}
			}
		}

		public static bool IsDerpWeapon(ThingDef thing, ThingDef stuff)
		{
			if (stuff == null)
			{
				return false;
			}
			if (thing.IsMeleeWeapon)
			{
				if (thing.tools.NullOrEmpty())
				{
					return false;
				}
				DamageDef damageDef = ThingUtility.PrimaryMeleeWeaponDamageType(thing);
				if (damageDef == null)
				{
					return false;
				}
				DamageArmorCategoryDef armorCategory = damageDef.armorCategory;
				if (armorCategory != null && armorCategory.multStat != null && stuff.GetStatValueAbstract(armorCategory.multStat) < 0.7f)
				{
					return true;
				}
			}
			return false;
		}

		public static float CheapestNonDerpPriceFor(ThingDef weaponDef)
		{
			float num = 9999999f;
			for (int i = 0; i < allWeaponPairs.Count; i++)
			{
				ThingStuffPair thingStuffPair = allWeaponPairs[i];
				if (thingStuffPair.thing == weaponDef && !IsDerpWeapon(thingStuffPair.thing, thingStuffPair.stuff) && thingStuffPair.Price < num)
				{
					num = thingStuffPair.Price;
				}
			}
			return num;
		}

		[DebugOutput]
		private static void WeaponPairs()
		{
			DebugTables.MakeTablesDialog(from p in allWeaponPairs
			orderby p.thing.defName descending
			select p, new TableDataGetter<ThingStuffPair>("thing", (ThingStuffPair p) => p.thing.defName), new TableDataGetter<ThingStuffPair>("stuff", (ThingStuffPair p) => (p.stuff == null) ? string.Empty : p.stuff.defName), new TableDataGetter<ThingStuffPair>("price", (ThingStuffPair p) => p.Price.ToString()), new TableDataGetter<ThingStuffPair>("commonality", (ThingStuffPair p) => p.Commonality.ToString("F5")), new TableDataGetter<ThingStuffPair>("commMult", (ThingStuffPair p) => p.commonalityMultiplier.ToString("F5")), new TableDataGetter<ThingStuffPair>("generateCommonality", (ThingStuffPair p) => p.thing.generateCommonality.ToString("F2")), new TableDataGetter<ThingStuffPair>("derp", (ThingStuffPair p) => (!IsDerpWeapon(p.thing, p.stuff)) ? string.Empty : "D"));
		}

		[DebugOutput]
		private static void WeaponPairsByThing()
		{
			DebugOutputsGeneral.MakeTablePairsByThing(allWeaponPairs);
		}
	}
}
