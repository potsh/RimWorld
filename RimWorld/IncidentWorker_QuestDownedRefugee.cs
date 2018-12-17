using RimWorld.Planet;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestDownedRefugee : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			int tile;
			Faction faction;
			return TryFindTile(out tile) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.DownedRefugee, null, out faction);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!TryFindTile(out int tile))
			{
				return false;
			}
			Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.DownedRefugee, (!Rand.Chance(0.3f)) ? "DownedRefugeeQuestThreat" : null, tile);
			if (site == null)
			{
				return false;
			}
			site.sitePartsKnown = true;
			Pawn pawn = DownedRefugeeQuestUtility.GenerateRefugee(tile);
			site.GetComponent<DownedRefugeeComp>().pawn.TryAdd(pawn);
			int randomInRange = SiteTuning.QuestSiteRefugeeTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text = def.letterLabel;
			string text2 = def.letterText.Formatted(randomInRange, pawn.ageTracker.AgeBiologicalYears, pawn.story.Title, SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
			Pawn mostImportantColonyRelative = PawnRelationUtility.GetMostImportantColonyRelative(pawn);
			if (mostImportantColonyRelative != null)
			{
				PawnRelationDef mostImportantRelation = mostImportantColonyRelative.GetMostImportantRelation(pawn);
				if (mostImportantRelation != null && mostImportantRelation.opinionOffset > 0)
				{
					pawn.relations.relativeInvolvedInRescueQuest = mostImportantColonyRelative;
					text2 = text2 + "\n\n" + "RelatedPawnInvolvedInQuest".Translate(mostImportantColonyRelative.LabelShort, mostImportantRelation.GetGenderSpecificLabel(pawn), mostImportantColonyRelative.Named("RELATIVE"), pawn.Named("PAWN")).AdjustedFor(pawn);
				}
				else
				{
					PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text2, pawn);
				}
				text = text + " " + "RelationshipAppendedLetterSuffix".Translate();
			}
			if (pawn.relations != null)
			{
				pawn.relations.everSeenByPlayer = true;
			}
			Find.LetterStack.ReceiveLetter(text, text2, def.letterDef, site);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange downedRefugeeQuestSiteDistanceRange = SiteTuning.DownedRefugeeQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, downedRefugeeQuestSiteDistanceRange.min, downedRefugeeQuestSiteDistanceRange.max, allowCaravans: true, preferCloserTiles: false);
		}
	}
}
