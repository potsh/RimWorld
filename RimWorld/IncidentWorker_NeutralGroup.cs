using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_NeutralGroup : IncidentWorker_PawnsArrive
	{
		protected virtual PawnGroupKindDef PawnGroupKindDef => PawnGroupKindDefOf.Peaceful;

		protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			return base.FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) && !NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, f);
		}

		protected bool TryResolveParms(IncidentParms parms)
		{
			if (!TryResolveParmsGeneral(parms))
			{
				return false;
			}
			ResolveParmsPoints(parms);
			return true;
		}

		protected virtual bool TryResolveParmsGeneral(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!parms.spawnCenter.IsValid && !RCellFinder.TryFindRandomPawnEntryCell(out parms.spawnCenter, map, CellFinder.EdgeRoadChance_Neutral))
			{
				return false;
			}
			if (parms.faction == null && !CandidateFactions(map).TryRandomElement(out parms.faction) && !CandidateFactions(map, desperate: true).TryRandomElement(out parms.faction))
			{
				return false;
			}
			return true;
		}

		protected abstract void ResolveParmsPoints(IncidentParms parms);

		protected List<Pawn> SpawnPawns(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(PawnGroupKindDef, parms, ensureCanGenerateAtLeastOnePawn: true);
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, warnOnZeroResults: false).ToList();
			foreach (Pawn item in list)
			{
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(parms.spawnCenter, map, 5);
				GenSpawn.Spawn(item, loc, map);
			}
			return list;
		}
	}
}
