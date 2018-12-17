using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EnsureCanHoldRoof : SymbolResolver
	{
		private static HashSet<IntVec3> roofsAboutToCollapse = new HashSet<IntVec3>();

		private static List<IntVec3> edgeRoofs = new List<IntVec3>();

		private static HashSet<IntVec3> visited = new HashSet<IntVec3>();

		public override void Resolve(ResolveParams rp)
		{
			ThingDef wallStuff = rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction);
			do
			{
				CalculateRoofsAboutToCollapse(rp.rect);
				CalculateEdgeRoofs(rp.rect);
			}
			while (TrySpawnPillar(rp.faction, wallStuff));
		}

		private void CalculateRoofsAboutToCollapse(CellRect rect)
		{
			Map map = BaseGen.globalSettings.map;
			roofsAboutToCollapse.Clear();
			visited.Clear();
			CellRect.CellRectIterator iterator = rect.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				if (current.Roofed(map) && !RoofCollapseCellsFinder.ConnectsToRoofHolder(current, map, visited))
				{
					map.floodFiller.FloodFill(current, (IntVec3 x) => x.Roofed(map), delegate(IntVec3 x)
					{
						roofsAboutToCollapse.Add(x);
					});
				}
				iterator.MoveNext();
			}
			CellRect.CellRectIterator iterator2 = rect.GetIterator();
			while (!iterator2.Done())
			{
				IntVec3 current2 = iterator2.Current;
				if (current2.Roofed(map) && !roofsAboutToCollapse.Contains(current2) && !RoofCollapseUtility.WithinRangeOfRoofHolder(current2, map))
				{
					roofsAboutToCollapse.Add(current2);
				}
				iterator2.MoveNext();
			}
		}

		private void CalculateEdgeRoofs(CellRect rect)
		{
			edgeRoofs.Clear();
			foreach (IntVec3 item2 in roofsAboutToCollapse)
			{
				for (int i = 0; i < 4; i++)
				{
					IntVec3 item = item2 + GenAdj.CardinalDirections[i];
					if (!roofsAboutToCollapse.Contains(item))
					{
						edgeRoofs.Add(item2);
						break;
					}
				}
			}
		}

		private bool TrySpawnPillar(Faction faction, ThingDef wallStuff)
		{
			if (!roofsAboutToCollapse.Any())
			{
				return false;
			}
			Map map = BaseGen.globalSettings.map;
			IntVec3 bestCell = IntVec3.Invalid;
			float bestScore = 0f;
			FloodFiller floodFiller = map.floodFiller;
			IntVec3 invalid = IntVec3.Invalid;
			Predicate<IntVec3> passCheck = (IntVec3 x) => roofsAboutToCollapse.Contains(x);
			Action<IntVec3> processor = delegate(IntVec3 x)
			{
				float pillarSpawnScore = GetPillarSpawnScore(x);
				if (pillarSpawnScore > 0f && (!bestCell.IsValid || pillarSpawnScore >= bestScore))
				{
					bestCell = x;
					bestScore = pillarSpawnScore;
				}
			};
			List<IntVec3> extraRoots = edgeRoofs;
			floodFiller.FloodFill(invalid, passCheck, processor, 2147483647, rememberParents: false, extraRoots);
			if (bestCell.IsValid)
			{
				Thing thing = ThingMaker.MakeThing(ThingDefOf.Wall, wallStuff);
				thing.SetFaction(faction);
				GenSpawn.Spawn(thing, bestCell, map);
				return true;
			}
			return false;
		}

		private float GetPillarSpawnScore(IntVec3 c)
		{
			Map map = BaseGen.globalSettings.map;
			if (c.Impassable(map) || c.GetFirstBuilding(map) != null || c.GetFirstItem(map) != null || c.GetFirstPawn(map) != null)
			{
				return 0f;
			}
			bool flag = true;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 c2 = c + GenAdj.AdjacentCells[i];
				if (!c2.InBounds(map) || !c2.Walkable(map))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return 2f;
			}
			return 1f;
		}
	}
}
