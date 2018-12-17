using RimWorld.Planet;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class SitePartWorker_Outpost : SitePartWorker
	{
		public override string GetArrivedLetterPart(Map map, out LetterDef preferredLetterDef, out LookTargets lookTargets)
		{
			string arrivedLetterPart = base.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
			lookTargets = (from x in map.mapPawns.AllPawnsSpawned
			where x.RaceProps.Humanlike && x.HostileTo(Faction.OfPlayer)
			select x).FirstOrDefault();
			return arrivedLetterPart;
		}

		public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), GetEnemiesCount(site, siteCoreOrPart.parms));
		}

		public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
		{
			return base.GetPostProcessedThreatLabel(site, siteCoreOrPart) + " (" + GetEnemiesCount(site, siteCoreOrPart.parms) + " " + "Enemies".Translate() + ")";
		}

		public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
		{
			SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
			siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints, site.Faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Settlement));
			return siteCoreOrPartParams;
		}

		private int GetEnemiesCount(Site site, SiteCoreOrPartParams parms)
		{
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms();
			pawnGroupMakerParms.tile = site.Tile;
			pawnGroupMakerParms.faction = site.Faction;
			pawnGroupMakerParms.groupKind = PawnGroupKindDefOf.Settlement;
			pawnGroupMakerParms.points = parms.threatPoints;
			pawnGroupMakerParms.inhabitants = true;
			pawnGroupMakerParms.seed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms);
			return PawnGroupMakerUtility.GeneratePawnKindsExample(pawnGroupMakerParms).Count();
		}
	}
}
