using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class GenStuff
	{
		public static ThingDef DefaultStuffFor(BuildableDef bd)
		{
			if (!bd.MadeFromStuff)
			{
				return null;
			}
			ThingDef thingDef = bd as ThingDef;
			if (thingDef != null)
			{
				if (thingDef.IsMeleeWeapon)
				{
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
					if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Plasteel;
					}
				}
				if (thingDef.IsApparel)
				{
					if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Cloth;
					}
					if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Leather_Plain;
					}
					if (ThingDefOf.Steel.stuffProps.CanMake(bd))
					{
						return ThingDefOf.Steel;
					}
				}
			}
			if (ThingDefOf.WoodLog.stuffProps.CanMake(bd))
			{
				return ThingDefOf.WoodLog;
			}
			if (ThingDefOf.Steel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Steel;
			}
			if (ThingDefOf.Plasteel.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Plasteel;
			}
			if (ThingDefOf.BlocksGranite.stuffProps.CanMake(bd))
			{
				return ThingDefOf.BlocksGranite;
			}
			if (ThingDefOf.Cloth.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Cloth;
			}
			if (ThingDefOf.Leather_Plain.stuffProps.CanMake(bd))
			{
				return ThingDefOf.Leather_Plain;
			}
			return AllowedStuffsFor(bd).First();
		}

		public static ThingDef RandomStuffFor(ThingDef td)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			return AllowedStuffsFor(td).RandomElement();
		}

		public static ThingDef RandomStuffByCommonalityFor(ThingDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				return null;
			}
			if (!TryRandomStuffByCommonalityFor(td, out ThingDef stuff, maxTechLevel))
			{
				return DefaultStuffFor(td);
			}
			return stuff;
		}

		public static IEnumerable<ThingDef> AllowedStuffsFor(BuildableDef td, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (td.MadeFromStuff)
			{
				List<ThingDef> allDefs = DefDatabase<ThingDef>.AllDefsListForReading;
				int i = 0;
				ThingDef d;
				while (true)
				{
					if (i >= allDefs.Count)
					{
						yield break;
					}
					d = allDefs[i];
					if (d.IsStuff && (maxTechLevel == TechLevel.Undefined || (int)d.techLevel <= (int)maxTechLevel) && d.stuffProps.CanMake(td))
					{
						break;
					}
					i++;
				}
				yield return d;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public static bool TryRandomStuffByCommonalityFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			IEnumerable<ThingDef> source = AllowedStuffsFor(td, maxTechLevel);
			return source.TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out stuff);
		}

		public static bool TryRandomStuffFor(ThingDef td, out ThingDef stuff, TechLevel maxTechLevel = TechLevel.Undefined)
		{
			if (!td.MadeFromStuff)
			{
				stuff = null;
				return true;
			}
			IEnumerable<ThingDef> source = AllowedStuffsFor(td, maxTechLevel);
			return source.TryRandomElement(out stuff);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, Faction faction)
		{
			return RandomStuffInexpensiveFor(thingDef, faction?.def.techLevel ?? TechLevel.Undefined);
		}

		public static ThingDef RandomStuffInexpensiveFor(ThingDef thingDef, TechLevel maxTechLevel)
		{
			if (!thingDef.MadeFromStuff)
			{
				return null;
			}
			IEnumerable<ThingDef> enumerable = AllowedStuffsFor(thingDef, maxTechLevel);
			float cheapestPrice = -1f;
			foreach (ThingDef item in enumerable)
			{
				float num = item.BaseMarketValue / item.VolumePerUnit;
				if (cheapestPrice == -1f || num < cheapestPrice)
				{
					cheapestPrice = num;
				}
			}
			enumerable = from x in enumerable
			where x.BaseMarketValue / x.VolumePerUnit <= cheapestPrice * 4f
			select x;
			if (enumerable.TryRandomElementByWeight((ThingDef x) => x.stuffProps.commonality, out ThingDef result))
			{
				return result;
			}
			return null;
		}
	}
}
