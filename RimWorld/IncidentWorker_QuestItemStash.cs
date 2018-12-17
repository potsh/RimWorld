using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestItemStash : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			if (!base.CanFireNowSub(parms))
			{
				return false;
			}
			int tile;
			Faction faction;
			return Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false) != null && TryFindTile(out tile) && SiteMakerHelper.TryFindRandomFactionFor(SiteCoreDefOf.ItemStash, null, out faction);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Faction faction = parms.faction;
			if (faction == null)
			{
				faction = Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);
			}
			if (faction == null)
			{
				return false;
			}
			if (!TryFindTile(out int tile))
			{
				return false;
			}
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(SiteCoreDefOf.ItemStash, (!Rand.Chance(0.15f)) ? "ItemStashQuestThreat" : null, out SitePartDef sitePart, out Faction faction2))
			{
				return false;
			}
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			Site site = CreateSite(tile, sitePart, randomInRange, faction2);
			List<Thing> list = GenerateItems(faction2, site.desiredThreatPoints);
			site.GetComponent<ItemStashContentsComp>().contents.TryAddRangeOrTransfer(list, canMergeWithExistingStacks: false);
			string letterText = GetLetterText(faction, list, randomInRange, site, site.parts.FirstOrDefault());
			Find.LetterStack.ReceiveLetter(def.letterLabel, letterText, def.letterDef, site, faction);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange itemStashQuestSiteDistanceRange = SiteTuning.ItemStashQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, itemStashQuestSiteDistanceRange.min, itemStashQuestSiteDistanceRange.max);
		}

		protected virtual List<Thing> GenerateItems(Faction siteFaction, float siteThreatPoints)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = SiteTuning.ItemStashQuestMarketValueRange * SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints);
			return ThingSetMakerDefOf.Reward_ItemStashQuestContents.root.Generate(parms);
		}

		public static Site CreateSite(int tile, SitePartDef sitePart, int days, Faction siteFaction)
		{
			Site site = SiteMaker.MakeSite(SiteCoreDefOf.ItemStash, sitePart, tile, siteFaction);
			site.sitePartsKnown = true;
			site.GetComponent<TimeoutComp>().StartTimeout(days * 60000);
			Find.WorldObjects.Add(site);
			return site;
		}

		private string GetLetterText(Faction alliedFaction, List<Thing> items, int days, Site site, SitePart sitePart)
		{
			string text = def.letterText.Formatted(alliedFaction.leader.LabelShort, alliedFaction.def.leaderTitle, alliedFaction.Name, GenLabel.ThingsLabel(items), days.ToString(), SitePartUtility.GetDescriptionDialogue(site, sitePart), GenThing.GetMarketValue(items).ToStringMoney()).CapitalizeFirst();
			GenThing.TryAppendSingleRewardInfo(ref text, items);
			return text;
		}
	}
}
