using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Street : SymbolResolver
	{
		private static List<bool> street = new List<bool>();

		public override void Resolve(ResolveParams rp)
		{
			bool? streetHorizontal = rp.streetHorizontal;
			bool flag = (!streetHorizontal.HasValue) ? (rp.rect.Width >= rp.rect.Height) : streetHorizontal.Value;
			int width = (!flag) ? rp.rect.Width : rp.rect.Height;
			TerrainDef floorDef = rp.floorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction);
			CalculateStreet(rp.rect, flag, floorDef);
			FillStreetGaps(flag, width);
			RemoveShortStreetParts(flag, width);
			SpawnFloor(rp.rect, flag, floorDef);
		}

		private void CalculateStreet(CellRect rect, bool horizontal, TerrainDef floorDef)
		{
			street.Clear();
			int num = (!horizontal) ? rect.Height : rect.Width;
			for (int i = 0; i < num; i++)
			{
				if (horizontal)
				{
					street.Add(CausesStreet(new IntVec3(rect.minX + i, 0, rect.minZ - 1), floorDef) && CausesStreet(new IntVec3(rect.minX + i, 0, rect.maxZ + 1), floorDef));
				}
				else
				{
					street.Add(CausesStreet(new IntVec3(rect.minX - 1, 0, rect.minZ + i), floorDef) && CausesStreet(new IntVec3(rect.maxX + 1, 0, rect.minZ + i), floorDef));
				}
			}
		}

		private void FillStreetGaps(bool horizontal, int width)
		{
			int num = -1;
			for (int i = 0; i < street.Count; i++)
			{
				if (street[i])
				{
					num = i;
				}
				else if (num != -1 && i - num <= width)
				{
					for (int j = i + 1; j < i + width + 1 && j < street.Count; j++)
					{
						if (street[j])
						{
							street[i] = true;
							break;
						}
					}
				}
			}
		}

		private void RemoveShortStreetParts(bool horizontal, int width)
		{
			for (int i = 0; i < street.Count; i++)
			{
				if (street[i])
				{
					int num = 0;
					for (int j = i; j < street.Count && street[j]; j++)
					{
						num++;
					}
					int num2 = 0;
					int num3 = i;
					while (num3 >= 0 && street[num3])
					{
						num2++;
						num3--;
					}
					int num4 = num2 + num - 1;
					if (num4 < width)
					{
						street[i] = false;
					}
				}
			}
		}

		private void SpawnFloor(CellRect rect, bool horizontal, TerrainDef floorDef)
		{
			Map map = BaseGen.globalSettings.map;
			TerrainGrid terrainGrid = map.terrainGrid;
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if ((horizontal && street[current.x - rect.minX]) || (!horizontal && street[current.z - rect.minZ]))
				{
					terrainGrid.SetTerrain(current, floorDef);
				}
				iterator.MoveNext();
			}
		}

		private bool CausesStreet(IntVec3 c, TerrainDef floorDef)
		{
			Map map = BaseGen.globalSettings.map;
			if (!c.InBounds(map))
			{
				return false;
			}
			Building edifice = c.GetEdifice(map);
			if (edifice != null && edifice.def == ThingDefOf.Wall)
			{
				return true;
			}
			if (c.GetDoor(map) != null)
			{
				return true;
			}
			if (c.GetTerrain(map) == floorDef)
			{
				return true;
			}
			return false;
		}
	}
}
