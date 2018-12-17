using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class SiteCoreWorker : SiteCoreOrPartWorkerBase
	{
		public static readonly IntVec3 MapSize = new IntVec3(120, 1, 120);

		private static List<SiteCoreOrPartDefBase> tmpDefs = new List<SiteCoreOrPartDefBase>();

		private static List<SiteCoreOrPartDefBase> tmpUsedDefs = new List<SiteCoreOrPartDefBase>();

		public SiteCoreDef Def => (SiteCoreDef)def;

		public virtual void SiteCoreWorkerTick(Site site)
		{
		}

		public virtual void VisitAction(Caravan caravan, Site site)
		{
			Enter(caravan, site);
		}

		public IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				using (IEnumerator<FloatMenuOption> enumerator = CaravanArrivalAction_VisitSite.GetFloatMenuOptions(caravan, site).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FloatMenuOption f = enumerator.Current;
						yield return f;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_00cf:
			/*Error near IL_00d0: Unexpected return in MoveNext()*/;
		}

		public virtual IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptions(IEnumerable<IThingHolder> pods, CompLaunchable representative, Site site)
		{
			using (IEnumerator<FloatMenuOption> enumerator = TransportPodsArrivalAction_VisitSite.GetFloatMenuOptions(representative, pods, site).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption f = enumerator.Current;
					yield return f;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00c5:
			/*Error near IL_00c6: Unexpected return in MoveNext()*/;
		}

		protected void Enter(Caravan caravan, Site site)
		{
			if (!site.HasMap)
			{
				LongEventHandler.QueueLongEvent(delegate
				{
					DoEnter(caravan, site);
				}, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
			}
			else
			{
				DoEnter(caravan, site);
			}
		}

		private void DoEnter(Caravan caravan, Site site)
		{
			Pawn t = caravan.PawnsListForReading[0];
			bool flag = site.Faction == null || site.Faction.HostileTo(Faction.OfPlayer);
			bool flag2 = !site.HasMap;
			Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(site.Tile, MapSize, null);
			if (flag2)
			{
				Find.TickManager.Notify_GeneratedPotentiallyHostileMap();
				PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter_Send(orGenerateMap.mapPawns.AllPawns, "LetterRelatedPawnsSite".Translate(Faction.OfPlayer.def.pawnsPlural), LetterDefOf.NeutralEvent, informEvenIfSeenBefore: true);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst());
				AppendThreatInfo(stringBuilder, site, orGenerateMap, out LetterDef letterDef, out LookTargets allLookTargets);
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(site), stringBuilder.ToString(), letterDef ?? LetterDefOf.NeutralEvent, (!allLookTargets.IsValid()) ? ((LookTargets)t) : allLookTargets);
			}
			else
			{
				Find.LetterStack.ReceiveLetter("LetterLabelCaravanEnteredMap".Translate(site), "LetterCaravanEnteredMap".Translate(caravan.Label, site).CapitalizeFirst(), LetterDefOf.NeutralEvent, t);
			}
			Map map = orGenerateMap;
			CaravanEnterMode enterMode = CaravanEnterMode.Edge;
			bool draftColonists = flag;
			CaravanEnterMapUtility.Enter(caravan, map, enterMode, CaravanDropInventoryMode.DoNotDrop, draftColonists);
		}

		private void AppendThreatInfo(StringBuilder sb, Site site, Map map, out LetterDef letterDef, out LookTargets allLookTargets)
		{
			allLookTargets = new LookTargets();
			tmpUsedDefs.Clear();
			tmpDefs.Clear();
			tmpDefs.Add(def);
			for (int i = 0; i < site.parts.Count; i++)
			{
				tmpDefs.Add(site.parts[i].def);
			}
			letterDef = null;
			for (int j = 0; j < tmpDefs.Count; j++)
			{
				LetterDef preferredLetterDef;
				LookTargets lookTargets;
				string arrivedLetterPart = tmpDefs[j].Worker.GetArrivedLetterPart(map, out preferredLetterDef, out lookTargets);
				if (arrivedLetterPart != null)
				{
					if (!tmpUsedDefs.Contains(tmpDefs[j]))
					{
						tmpUsedDefs.Add(tmpDefs[j]);
						if (sb.Length > 0)
						{
							sb.AppendLine();
							sb.AppendLine();
						}
						sb.Append(arrivedLetterPart);
					}
					if (letterDef == null)
					{
						letterDef = preferredLetterDef;
					}
					if (lookTargets.IsValid())
					{
						allLookTargets = new LookTargets(allLookTargets.targets.Concat(lookTargets.targets));
					}
				}
			}
		}
	}
}
