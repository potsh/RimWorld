using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public abstract class RaidStrategyWorker
	{
		public RaidStrategyDef def;

		public virtual float SelectionWeight(Map map, float basePoints)
		{
			return def.selectionWeightPerPointsCurve.Evaluate(basePoints);
		}

		protected abstract LordJob MakeLordJob(IncidentParms parms, Map map, List<Pawn> pawns, int raidSeed);

		public virtual void MakeLords(IncidentParms parms, List<Pawn> pawns)
		{
			Map map = (Map)parms.target;
			List<List<Pawn>> list = IncidentParmsUtility.SplitIntoGroups(pawns, parms.pawnGroups);
			int @int = Rand.Int;
			for (int i = 0; i < list.Count; i++)
			{
				List<Pawn> list2 = list[i];
				Lord lord = LordMaker.MakeNewLord(parms.faction, MakeLordJob(parms, map, list2, @int), map, list2);
				if (DebugViewSettings.drawStealDebug && parms.faction.HostileTo(Faction.OfPlayer))
				{
					Log.Message("Market value threshold to start stealing (raiders=" + lord.ownedPawns.Count + "): " + StealAIUtility.StartStealingMarketValueThreshold(lord) + " (colony wealth=" + map.wealthWatcher.WealthTotal + ")");
				}
			}
		}

		public virtual bool CanUseWith(IncidentParms parms, PawnGroupKindDef groupKind)
		{
			if (parms.points < MinimumPoints(parms.faction, groupKind))
			{
				return false;
			}
			return true;
		}

		public virtual float MinimumPoints(Faction faction, PawnGroupKindDef groupKind)
		{
			return faction.def.MinPointsToGeneratePawnGroup(groupKind);
		}

		public virtual float MinMaxAllowedPawnGenOptionCost(Faction faction, PawnGroupKindDef groupKind)
		{
			return 0f;
		}

		public virtual bool CanUsePawnGenOption(PawnGenOption g, List<PawnGenOption> chosenGroups)
		{
			return true;
		}

		public virtual bool CanUsePawn(Pawn p, List<Pawn> otherPawns)
		{
			return true;
		}
	}
}
