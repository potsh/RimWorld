using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		private const float HivePoints = 220f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell;
			return base.CanFireNowSub(parms) && HiveUtility.TotalSpawnedHivesCount(map) < 30 && InfestationCellFinder.TryFindCell(out cell, map);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			int hiveCount = Mathf.Max(GenMath.RoundRandom(parms.points / 220f), 1);
			Thing t = SpawnTunnels(hiveCount, map);
			SendStandardLetter(t, null);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		private Thing SpawnTunnels(int hiveCount, Map map)
		{
			if (!InfestationCellFinder.TryFindCell(out IntVec3 cell, map))
			{
				return null;
			}
			Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
			for (int i = 0; i < hiveCount - 1; i++)
			{
				cell = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), ignoreRoofedRequirement: false, allowUnreachable: true);
				if (cell.IsValid)
				{
					thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner), cell, map, WipeMode.FullRefund);
				}
			}
			return thing;
		}
	}
}
