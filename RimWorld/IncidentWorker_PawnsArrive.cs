using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public abstract class IncidentWorker_PawnsArrive : IncidentWorker
	{
		protected IEnumerable<Faction> CandidateFactions(Map map, bool desperate = false)
		{
			return from f in Find.FactionManager.AllFactions
			where FactionCanBeGroupSource(f, map, desperate)
			select f;
		}

		protected virtual bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
		{
			if (f.IsPlayer)
			{
				return false;
			}
			if (f.defeated)
			{
				return false;
			}
			if (!desperate && (!f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.OutdoorTemp) || !f.def.allowedArrivalTemperatureRange.Includes(map.mapTemperature.SeasonalTemp)))
			{
				return false;
			}
			return true;
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return parms.faction != null || CandidateFactions(map).Any();
		}

		public string DebugListingOfGroupSources()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				stringBuilder.Append(allFaction.Name);
				if (FactionCanBeGroupSource(allFaction, Find.CurrentMap))
				{
					stringBuilder.Append("    YES");
				}
				else if (FactionCanBeGroupSource(allFaction, Find.CurrentMap, desperate: true))
				{
					stringBuilder.Append("    YES-DESPERATE");
				}
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}
	}
}
