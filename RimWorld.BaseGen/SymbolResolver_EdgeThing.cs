using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_EdgeThing : SymbolResolver
	{
		private List<int> randomRotations = new List<int>
		{
			0,
			1,
			2,
			3
		};

		private int MaxTriesToAvoidOtherEdgeThings = 4;

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (rp.singleThingDef != null)
			{
				bool? edgeThingAvoidOtherEdgeThings = rp.edgeThingAvoidOtherEdgeThings;
				bool avoidOtherEdgeThings = edgeThingAvoidOtherEdgeThings.HasValue && edgeThingAvoidOtherEdgeThings.Value;
				IntVec3 spawnCell;
				if (rp.thingRot.HasValue)
				{
					if (!TryFindSpawnCell(rp.rect, rp.singleThingDef, rp.thingRot.Value, avoidOtherEdgeThings, out spawnCell))
					{
						return false;
					}
				}
				else if (!rp.singleThingDef.rotatable)
				{
					if (!TryFindSpawnCell(rp.rect, rp.singleThingDef, Rot4.North, avoidOtherEdgeThings, out spawnCell))
					{
						return false;
					}
				}
				else
				{
					bool flag = false;
					for (int i = 0; i < 4; i++)
					{
						if (TryFindSpawnCell(rp.rect, rp.singleThingDef, new Rot4(i), avoidOtherEdgeThings, out spawnCell))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			ThingDef thingDef = rp.singleThingDef ?? (from x in DefDatabase<ThingDef>.AllDefsListForReading
			where (x.IsWeapon || x.IsMedicine || x.IsDrug) && x.graphicData != null && !x.destroyOnDrop && x.size.x <= rp.rect.Width && x.size.z <= rp.rect.Width && x.size.x <= rp.rect.Height && x.size.z <= rp.rect.Height
			select x).RandomElement();
			IntVec3 spawnCell = IntVec3.Invalid;
			Rot4 value = Rot4.North;
			bool? edgeThingAvoidOtherEdgeThings = rp.edgeThingAvoidOtherEdgeThings;
			bool avoidOtherEdgeThings = edgeThingAvoidOtherEdgeThings.HasValue && edgeThingAvoidOtherEdgeThings.Value;
			if (rp.thingRot.HasValue)
			{
				if (!TryFindSpawnCell(rp.rect, thingDef, rp.thingRot.Value, avoidOtherEdgeThings, out spawnCell))
				{
					return;
				}
				value = rp.thingRot.Value;
			}
			else if (!thingDef.rotatable)
			{
				if (!TryFindSpawnCell(rp.rect, thingDef, Rot4.North, avoidOtherEdgeThings, out spawnCell))
				{
					return;
				}
				value = Rot4.North;
			}
			else
			{
				randomRotations.Shuffle();
				bool flag = false;
				for (int i = 0; i < randomRotations.Count; i++)
				{
					if (TryFindSpawnCell(rp.rect, thingDef, new Rot4(randomRotations[i]), avoidOtherEdgeThings, out spawnCell))
					{
						value = new Rot4(randomRotations[i]);
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return;
				}
			}
			ResolveParams resolveParams = rp;
			resolveParams.rect = CellRect.SingleCell(spawnCell);
			resolveParams.thingRot = value;
			resolveParams.singleThingDef = thingDef;
			BaseGen.symbolStack.Push("thing", resolveParams);
		}

		private bool TryFindSpawnCell(CellRect rect, ThingDef thingDef, Rot4 rot, bool avoidOtherEdgeThings, out IntVec3 spawnCell)
		{
			if (avoidOtherEdgeThings)
			{
				spawnCell = IntVec3.Invalid;
				int num = -1;
				for (int i = 0; i < MaxTriesToAvoidOtherEdgeThings; i++)
				{
					if (TryFindSpawnCell(rect, thingDef, rot, out IntVec3 spawnCell2))
					{
						int distanceSquaredToExistingEdgeThing = GetDistanceSquaredToExistingEdgeThing(spawnCell2, rect, thingDef);
						if (!spawnCell.IsValid || distanceSquaredToExistingEdgeThing > num)
						{
							spawnCell = spawnCell2;
							num = distanceSquaredToExistingEdgeThing;
							if (num == 2147483647)
							{
								break;
							}
						}
					}
				}
				return spawnCell.IsValid;
			}
			return TryFindSpawnCell(rect, thingDef, rot, out spawnCell);
		}

		private bool TryFindSpawnCell(CellRect rect, ThingDef thingDef, Rot4 rot, out IntVec3 spawnCell)
		{
			Map map = BaseGen.globalSettings.map;
			IntVec3 center = IntVec3.Zero;
			IntVec2 size = thingDef.size;
			GenAdj.AdjustForRotation(ref center, ref size, rot);
			CellRect rect2 = CellRect.Empty;
			Predicate<CellRect> basePredicate = (CellRect x) => x.Cells.All((IntVec3 y) => y.Standable(map)) && !GenSpawn.WouldWipeAnythingWith(x, thingDef, map, (Thing z) => z.def.category == ThingCategory.Building) && (thingDef.category != ThingCategory.Item || x.CenterCell.GetFirstItem(map) == null);
			bool flag = false;
			if (thingDef.category == ThingCategory.Building)
			{
				flag = rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, (CellRect x) => basePredicate(x) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(x, map) && GenConstruct.TerrainCanSupport(x, map, thingDef));
				if (!flag)
				{
					flag = rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, (CellRect x) => basePredicate(x) && !BaseGenUtility.AnyDoorAdjacentCardinalTo(x, map));
				}
			}
			if (!flag && !rect.TryFindRandomInnerRectTouchingEdge(size, out rect2, basePredicate))
			{
				spawnCell = IntVec3.Invalid;
				return false;
			}
			CellRect.CellRectIterator iterator = rect2.GetIterator();
			while (!iterator.Done())
			{
				if (GenAdj.OccupiedRect(iterator.Current, rot, thingDef.size) == rect2)
				{
					spawnCell = iterator.Current;
					return true;
				}
				iterator.MoveNext();
			}
			Log.Error("We found a valid rect but we couldn't find the root position. This should never happen.");
			spawnCell = IntVec3.Invalid;
			return false;
		}

		private int GetDistanceSquaredToExistingEdgeThing(IntVec3 cell, CellRect rect, ThingDef thingDef)
		{
			Map map = BaseGen.globalSettings.map;
			int num = 2147483647;
			foreach (IntVec3 edgeCell in rect.EdgeCells)
			{
				List<Thing> thingList = edgeCell.GetThingList(map);
				bool flag = false;
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def == thingDef)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					num = Mathf.Min(num, cell.DistanceToSquared(edgeCell));
				}
			}
			return num;
		}
	}
}
