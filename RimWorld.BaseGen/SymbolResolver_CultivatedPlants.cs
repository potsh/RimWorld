using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_CultivatedPlants : SymbolResolver
	{
		private const float MinPlantGrowth = 0.2f;

		private static List<Thing> tmpThings = new List<Thing>();

		public override bool CanResolve(ResolveParams rp)
		{
			return base.CanResolve(rp) && (rp.cultivatedPlantDef != null || DeterminePlantDef(rp.rect) != null);
		}

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			ThingDef thingDef = rp.cultivatedPlantDef ?? DeterminePlantDef(rp.rect);
			if (thingDef != null)
			{
				float growth = Rand.Range(0.2f, 1f);
				int age = thingDef.plant.LimitedLifespan ? Rand.Range(0, Mathf.Max(thingDef.plant.LifespanTicks - 2500, 0)) : 0;
				CellRect.CellRectIterator iterator = rp.rect.GetIterator();
				while (!iterator.Done())
				{
					float num = map.fertilityGrid.FertilityAt(iterator.Current);
					if (!(num < thingDef.plant.fertilityMin) && TryDestroyBlockingThingsAt(iterator.Current))
					{
						Plant plant = (Plant)GenSpawn.Spawn(thingDef, iterator.Current, map);
						plant.Growth = growth;
						if (plant.def.plant.LimitedLifespan)
						{
							plant.Age = age;
						}
					}
					iterator.MoveNext();
				}
			}
		}

		public static ThingDef DeterminePlantDef(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			if (map.mapTemperature.OutdoorTemp < 0f || map.mapTemperature.OutdoorTemp > 58f)
			{
				return null;
			}
			float minFertility = 3.40282347E+38f;
			bool flag = false;
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				float num = map.fertilityGrid.FertilityAt(iterator.Current);
				if (!(num <= 0f))
				{
					flag = true;
					minFertility = Mathf.Min(minFertility, num);
				}
				iterator.MoveNext();
			}
			if (!flag)
			{
				return null;
			}
			if ((from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.category == ThingCategory.Plant && x.plant.Sowable && !x.plant.IsTree && !x.plant.cavePlant && x.plant.fertilityMin <= minFertility && x.plant.Harvestable
			select x).TryRandomElement(out ThingDef result))
			{
				return result;
			}
			return null;
		}

		private bool TryDestroyBlockingThingsAt(IntVec3 c)
		{
			Map map = BaseGen.globalSettings.map;
			tmpThings.Clear();
			tmpThings.AddRange(c.GetThingList(map));
			for (int i = 0; i < tmpThings.Count; i++)
			{
				if (!(tmpThings[i] is Pawn) && !tmpThings[i].def.destroyable)
				{
					tmpThings.Clear();
					return false;
				}
			}
			for (int j = 0; j < tmpThings.Count; j++)
			{
				if (!(tmpThings[j] is Pawn))
				{
					tmpThings[j].Destroy();
				}
			}
			tmpThings.Clear();
			return true;
		}
	}
}
