using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_FloorFill : SymbolResolver
	{
		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			TerrainGrid terrainGrid = map.terrainGrid;
			TerrainDef terrainDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			bool? floorOnlyIfTerrainSupports = rp.floorOnlyIfTerrainSupports;
			bool flag = floorOnlyIfTerrainSupports.HasValue && floorOnlyIfTerrainSupports.Value;
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				if ((!rp.chanceToSkipFloor.HasValue || !Rand.Chance(rp.chanceToSkipFloor.Value)) && (!flag || GenConstruct.CanBuildOnTerrain(terrainDef, iterator.Current, map, Rot4.North)))
				{
					terrainGrid.SetTerrain(iterator.Current, terrainDef);
					if (rp.filthDef != null)
					{
						FilthMaker.MakeFilth(iterator.Current, map, rp.filthDef, (!rp.filthDensity.HasValue) ? 1 : Mathf.RoundToInt(rp.filthDensity.Value.RandomInRange));
					}
				}
				iterator.MoveNext();
			}
		}
	}
}
