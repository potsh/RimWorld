using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class IncidentWorker_PsychicEmanation : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone) || map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicSoothe))
			{
				return false;
			}
			if (map.listerThings.ThingsOfDef(ThingDefOf.CrashedPsychicEmanatorShipPart).Count > 0)
			{
				return false;
			}
			if (map.mapPawns.FreeColonistsCount == 0)
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			DoConditionAndLetter(map, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), map.mapPawns.FreeColonists.RandomElement().gender, parms.points);
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(map);
			return true;
		}

		protected abstract void DoConditionAndLetter(Map map, int duration, Gender gender, float points);
	}
}
