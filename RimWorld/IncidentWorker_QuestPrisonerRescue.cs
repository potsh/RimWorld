using RimWorld.Planet;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestPrisonerRescue : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			if (Find.AnyPlayerHomeMap == null)
			{
				return false;
			}
			if (!TryFindTile(out int _))
			{
				return false;
			}
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.PrisonerWillingToJoin, "PrisonerRescueQuestThreat", out SitePartDef _, out Faction _))
			{
				return false;
			}
			return true;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!TryFindTile(out int tile))
			{
				return false;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.PrisonerWillingToJoin, "PrisonerRescueQuestThreat", tile);
			if (site == null)
			{
				return false;
			}
			site.sitePartsKnown = true;
			Pawn pawn = PrisonerWillingToJoinQuestUtility.GeneratePrisoner(tile, site.Faction);
			site.GetComponent<PrisonerWillingToJoinComp>().pawn.TryAdd(pawn);
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			GetLetterText(pawn, site, site.parts.FirstOrDefault(), randomInRange, out string letter, out string label);
			Find.LetterStack.ReceiveLetter(label, letter, def.letterDef, site, site.Faction);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange prisonerRescueQuestSiteDistanceRange = SiteTuning.PrisonerRescueQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, prisonerRescueQuestSiteDistanceRange.min, prisonerRescueQuestSiteDistanceRange.max, allowCaravans: false, preferCloserTiles: false);
		}

		private void GetLetterText(Pawn prisoner, Site site, SitePart sitePart, int days, out string letter, out string label)
		{
			letter = def.letterText.Formatted(site.Faction.Name, prisoner.ageTracker.AgeBiologicalYears, prisoner.story.Title, SitePartUtility.GetDescriptionDialogue(site, sitePart), prisoner.Named("PAWN")).AdjustedFor(prisoner).CapitalizeFirst();
			if (PawnUtility.EverBeenColonistOrTameAnimal(prisoner))
			{
				letter = letter + "\n\n" + "PawnWasFormerlyColonist".Translate(prisoner.LabelShort, prisoner);
			}
			PawnRelationUtility.Notify_PawnsSeenByPlayer(Gen.YieldSingle(prisoner), out string pawnRelationsInfo, informEvenIfSeenBefore: true, writeSeenPawnsNames: false);
			label = def.letterLabel;
			if (!pawnRelationsInfo.NullOrEmpty())
			{
				string text = letter;
				letter = text + "\n\n" + "PawnHasTheseRelationshipsWithColonists".Translate(prisoner.LabelShort, prisoner) + "\n\n" + pawnRelationsInfo;
				label = label + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			letter = letter + "\n\n" + "PrisonerRescueTimeout".Translate(days, prisoner.LabelShort, prisoner.Named("PRISONER"));
		}
	}
}
