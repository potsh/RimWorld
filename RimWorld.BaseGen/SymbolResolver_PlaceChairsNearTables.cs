using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_PlaceChairsNearTables : SymbolResolver
	{
		private static List<Thing> tables = new List<Thing>();

		public override void Resolve(ResolveParams rp)
		{
			Map map = BaseGen.globalSettings.map;
			tables.Clear();
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				List<Thing> thingList = iterator.Current.GetThingList(map);
				for (int i = 0; i < thingList.Count; i++)
				{
					if (thingList[i].def.IsTable && !tables.Contains(thingList[i]))
					{
						tables.Add(thingList[i]);
					}
				}
				iterator.MoveNext();
			}
			for (int j = 0; j < tables.Count; j++)
			{
				CellRect cellRect = tables[j].OccupiedRect().ExpandedBy(1);
				bool flag = false;
				foreach (IntVec3 item in cellRect.EdgeCells.InRandomOrder())
				{
					IntVec3 current = item;
					if (!cellRect.IsCorner(current) && rp.rect.Contains(current) && current.Standable(map) && current.GetEdifice(map) == null && (!flag || !Rand.Bool))
					{
						Rot4 value = (current.x == cellRect.minX) ? Rot4.East : ((current.x == cellRect.maxX) ? Rot4.West : ((current.z != cellRect.minZ) ? Rot4.South : Rot4.North));
						ResolveParams resolveParams = rp;
						resolveParams.rect = CellRect.SingleCell(current);
						resolveParams.singleThingDef = ThingDefOf.DiningChair;
						resolveParams.singleThingStuff = (rp.singleThingStuff ?? ThingDefOf.WoodLog);
						resolveParams.thingRot = value;
						BaseGen.symbolStack.Push("thing", resolveParams);
						flag = true;
					}
				}
			}
			tables.Clear();
		}
	}
}
