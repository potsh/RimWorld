using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_DiseaseAnimal : IncidentWorker_Disease
	{
		protected override IEnumerable<Pawn> PotentialVictimCandidates(IIncidentTarget target)
		{
			Map map = target as Map;
			if (map != null)
			{
				return from p in map.mapPawns.PawnsInFaction(Faction.OfPlayer)
				where p.HostFaction == null && !p.RaceProps.Humanlike
				select p;
			}
			return from p in ((Caravan)target).PawnsListForReading
			where !p.RaceProps.Humanlike
			select p;
		}

		protected override IEnumerable<Pawn> ActualVictims(IncidentParms parms)
		{
			Pawn[] potentialVictims = PotentialVictims(parms.target).ToArray();
			IEnumerable<ThingDef> source = (from v in potentialVictims
			select v.def).Distinct();
			ThingDef targetRace = source.RandomElementByWeightWithFallback((ThingDef race) => (from v in potentialVictims
			where v.def == race
			select v.BodySize).Sum());
			IEnumerable<Pawn> source2 = from v in potentialVictims
			where v.def == targetRace
			select v;
			int num = source2.Count();
			int randomInRange = new IntRange(Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.min), Mathf.RoundToInt((float)num * def.diseaseVictimFractionRange.max)).RandomInRange;
			randomInRange = Mathf.Clamp(randomInRange, 1, def.diseaseMaxVictims);
			return source2.InRandomOrder().Take(randomInRange);
		}
	}
}
