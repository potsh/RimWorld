using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_Manhunters : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			lookTargets = (from x in map.mapPawns.AllPawnsSpawned
			where x.MentalStateDef == MentalStateDefOf.Manhunter || x.MentalStateDef == MentalStateDefOf.ManhunterPermanent
			select x).FirstOrDefault();
			return arrivedLetterPart;
		}

		public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
			if (ManhunterPackGenStepUtility.TryGetAnimalsKind(siteCoreOrPartParams.threatPoints, site.Tile, out siteCoreOrPartParams.animalKind))
			{
				siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints, siteCoreOrPartParams.animalKind.combatPower);
			}
			return siteCoreOrPartParams;
		}

		public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			int animalsCount = GetAnimalsCount(siteCoreOrPart.parms);
			return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), animalsCount, GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, plural: true, animalsCount));
		}

		public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			int animalsCount = GetAnimalsCount(siteCoreOrPart.parms);
			return base.GetPostProcessedThreatLabel(site, siteCoreOrPart) + " (" + animalsCount + " " + GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, plural: true, animalsCount) + ")";
		}

		private int GetAnimalsCount(SiteCoreOrPartParams parms)
		{
			return ManhunterPackIncidentUtility.GetAnimalsCount(parms.animalKind, parms.threatPoints);
		}
	}
}
