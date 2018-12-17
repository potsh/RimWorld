using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_QuestBanditCamp : IncidentWorker
	{
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Faction alliedFaction;
			Faction enemyFaction;
			int tile;
			return base.CanFireNowSub(parms) && TryFindFactions(out alliedFaction, out enemyFaction) && TryFindTile(out tile);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			if (!TryFindFactions(out Faction alliedFaction, out Faction enemyFaction))
			{
				return false;
			}
			if (!TryFindTile(out int tile))
			{
				return false;
			}
			Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, SitePartDefOf.Outpost, tile, enemyFaction);
			site.sitePartsKnown = true;
			List<Thing> list = GenerateRewards(alliedFaction, site.desiredThreatPoints);
			site.GetComponent<DefeatAllEnemiesQuestComp>().StartQuest(alliedFaction, 18, list);
			int randomInRange = SiteTuning.QuestSiteTimeoutDaysRange.RandomInRange;
			site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
			Find.WorldObjects.Add(site);
			string text = def.letterText.Formatted(alliedFaction.leader.LabelShort, alliedFaction.def.leaderTitle, alliedFaction.Name, GenLabel.ThingsLabel(list, string.Empty), randomInRange.ToString(), SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), GenThing.GetMarketValue(list).ToStringMoney()).CapitalizeFirst();
			GenThing.TryAppendSingleRewardInfo(ref text, list);
			Find.LetterStack.ReceiveLetter(def.letterLabel, text, def.letterDef, site, alliedFaction);
			return true;
		}

		private bool TryFindTile(out int tile)
		{
			IntRange banditCampQuestSiteDistanceRange = SiteTuning.BanditCampQuestSiteDistanceRange;
			return TileFinder.TryFindNewSiteTile(out tile, banditCampQuestSiteDistanceRange.min, banditCampQuestSiteDistanceRange.max);
		}

		private List<Thing> GenerateRewards(Faction alliedFaction, float siteThreatPoints)
		{
			ThingSetMakerParams parms = default(ThingSetMakerParams);
			parms.totalMarketValueRange = SiteTuning.BanditCampQuestRewardMarketValueRange * SiteTuning.QuestRewardMarketValueThreatPointsFactor.Evaluate(siteThreatPoints);
			return ThingSetMakerDefOf.Reward_StandardByDropPod.root.Generate(parms);
		}

		private bool TryFindFactions(out Faction alliedFaction, out Faction enemyFaction)
		{
			if ((from x in Find.FactionManager.AllFactions
			where !x.def.hidden && !x.defeated && !x.IsPlayer && !x.HostileTo(Faction.OfPlayer) && CommonHumanlikeEnemyFactionExists(Faction.OfPlayer, x) && !AnyQuestExistsFrom(x)
			select x).TryRandomElement(out alliedFaction))
			{
				enemyFaction = CommonHumanlikeEnemyFaction(Faction.OfPlayer, alliedFaction);
				return true;
			}
			alliedFaction = null;
			enemyFaction = null;
			return false;
		}

		private bool AnyQuestExistsFrom(Faction faction)
		{
			List<Site> sites = Find.WorldObjects.Sites;
			for (int i = 0; i < sites.Count; i++)
			{
				DefeatAllEnemiesQuestComp component = sites[i].GetComponent<DefeatAllEnemiesQuestComp>();
				if (component != null && component.Active && component.requestingFaction == faction)
				{
					return true;
				}
			}
			return false;
		}

		private bool CommonHumanlikeEnemyFactionExists(Faction f1, Faction f2)
		{
			return CommonHumanlikeEnemyFaction(f1, f2) != null;
		}

		private Faction CommonHumanlikeEnemyFaction(Faction f1, Faction f2)
		{
			if ((from x in Find.FactionManager.AllFactions
			where x != f1 && x != f2 && !x.def.hidden && x.def.humanlikeFaction && !x.defeated && x.HostileTo(f1) && x.HostileTo(f2)
			select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}
	}
}
