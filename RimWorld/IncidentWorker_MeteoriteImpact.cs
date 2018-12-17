using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_MeteoriteImpact : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			return TryFindCell(out cell, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindCell(out IntVec3 cell, map))
			{
				return false;
			}
			List<Thing> list = ThingSetMakerDefOf.Meteorite.root.Generate();
			SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, cell, map);
			LetterDef textLetterDef = (!list[0].def.building.isResourceRock) ? LetterDefOf.NeutralEvent : LetterDefOf.PositiveEvent;
			string text = string.Format(def.letterText, list[0].def.label).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(def.letterLabel, text, textLetterDef, new TargetInfo(cell, map));
			return true;
		}

		private bool TryFindCell(out IntVec3 cell, Map map)
		{
			IntRange mineablesCountRange = ThingSetMaker_Meteorite.MineablesCountRange;
			int maxMineables = mineablesCountRange.max;
			return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, allowRoofedCells: true, allowCellsWithItems: false, allowCellsWithBuildings: false, colonyReachable: false, avoidColonistsIfExplosive: true, alwaysAvoidColonists: true, delegate(IntVec3 x)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)maxMineables)) + 2;
				CellRect cellRect = CellRect.CenteredOn(x, num, num);
				int num2 = 0;
				CellRect.CellRectIterator iterator = cellRect.GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.InBounds(map) && iterator.Current.Standable(map))
					{
						num2++;
					}
					iterator.MoveNext();
				}
				return num2 >= maxMineables;
			});
		}
	}
}
