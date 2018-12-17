using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_SelfTame : IncidentWorker
	{
		private IEnumerable<Pawn> Candidates(Map map)
		{
			return from x in map.mapPawns.AllPawnsSpawned
			where x.RaceProps.Animal && x.Faction == null && !x.Position.Fogged(x.Map) && !x.InMentalState && !x.Downed && x.RaceProps.wildness > 0f
			select x;
		}

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			return Candidates(map).Any();
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Pawn result = null;
			if (!Candidates(map).TryRandomElement(out result))
			{
				return false;
			}
			if (result.guest != null)
			{
				result.guest.SetGuestStatus(null);
			}
			string value = result.LabelIndefinite();
			bool flag = result.Name != null;
			result.SetFaction(Faction.OfPlayer);
			string text = (flag || result.Name == null) ? "LetterAnimalSelfTame".Translate(result).CapitalizeFirst() : ((!result.Name.Numerical) ? "LetterAnimalSelfTameAndName".Translate(value, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst() : "LetterAnimalSelfTameAndNameNumerical".Translate(value, result.Name.ToStringFull, result.Named("ANIMAL")).CapitalizeFirst());
			Find.LetterStack.ReceiveLetter("LetterLabelAnimalSelfTame".Translate(result.KindLabel, result).CapitalizeFirst(), text, LetterDefOf.PositiveEvent, result);
			return true;
		}
	}
}
