using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	internal abstract class IncidentWorker_ShipPartCrash : IncidentWorker
	{
		private const float ShipPointsFactor = 0.9f;

		private const int IncidentMinimumPoints = 300;

		protected virtual int CountToSpawn => 1;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.listerThings.ThingsOfDef(def.shipPart).Count > 0)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int num = 0;
			int countToSpawn = CountToSpawn;
			List<TargetInfo> list = new List<TargetInfo>();
			float shrapnelDirection = Rand.Range(0f, 360f);
			for (int i = 0; i < countToSpawn; i++)
			{
				IntVec3 cell = default(IntVec3);
				if (!CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.CrashedShipPartIncoming, map, out cell, 14, default(IntVec3), -1, allowRoofedCells: false, allowCellsWithItems: true, allowCellsWithBuildings: true, colonyReachable: true))
				{
					break;
				}
				Building_CrashedShipPart building_CrashedShipPart = (Building_CrashedShipPart)ThingMaker.MakeThing(def.shipPart);
				building_CrashedShipPart.SetFaction(Faction.OfMechanoids);
				building_CrashedShipPart.GetComp<CompSpawnerMechanoidsOnDamaged>().pointsLeft = Mathf.Max(parms.points * 0.9f, 300f);
				Skyfaller skyfaller = SkyfallerMaker.MakeSkyfaller(ThingDefOf.CrashedShipPartIncoming, building_CrashedShipPart);
				skyfaller.shrapnelDirection = shrapnelDirection;
				GenSpawn.Spawn(skyfaller, cell, map);
				num++;
				list.Add(new TargetInfo(cell, map));
			}
			if (num > 0)
			{
				SendStandardLetter(list, null);
			}
			return num > 0;
		}
	}
}
