using System.Collections.Generic;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class GenStep_SleepingMechanoids : GenStep
	{
		public FloatRange defaultPointsRange = new FloatRange(340f, 1000f);

		public override int SeedPart => 341176078;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (SiteGenStepUtility.TryFindRootToSpawnAroundRectOfInterest(out CellRect rectToDefend, out IntVec3 singleCellToSpawnNear, map))
			{
				List<Pawn> list = new List<Pawn>();
				foreach (Pawn item in GeneratePawns(parms, map))
				{
					if (!SiteGenStepUtility.TryFindSpawnCellAroundOrNear(rectToDefend, singleCellToSpawnNear, map, out IntVec3 spawnCell))
					{
						Find.WorldPawns.PassToWorld(item);
						break;
					}
					GenSpawn.Spawn(item, spawnCell, map);
					list.Add(item);
				}
				if (list.Any())
				{
					LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_SleepThenAssaultColony(Faction.OfMechanoids, Rand.Bool), map, list);
					for (int i = 0; i < list.Count; i++)
					{
						list[i].jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
		}

		private IEnumerable<Pawn> GeneratePawns(GenStepParams parms, Map map)
		{
			float points = (parms.siteCoreOrPart == null) ? defaultPointsRange.RandomInRange : parms.siteCoreOrPart.parms.threatPoints;
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Combat;
			pawnGroupMakerParms.tile = map.Tile;
			pawnGroupMakerParms.faction = Faction.OfMechanoids;
			pawnGroupMakerParms.points = points;
			if (parms.siteCoreOrPart != null)
			{
				pawnGroupMakerParms.seed = SleepingMechanoidsSitePartUtility.GetPawnGroupMakerSeed(parms.siteCoreOrPart.parms);
			}
			return PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms);
		}
	}
}
