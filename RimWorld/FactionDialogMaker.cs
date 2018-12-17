using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public static class FactionDialogMaker
	{
		[CompilerGenerated]
		private static Func<IAttackTarget, bool> _003C_003Ef__mg_0024cache0;

		public static DiaNode FactionDialogFor(Pawn negotiator, Faction faction)
		{
			Map map = negotiator.Map;
			Pawn pawn;
			string value;
			if (faction.leader != null)
			{
				pawn = faction.leader;
				value = faction.leader.Name.ToStringFull;
			}
			else
			{
				Log.Error("Faction " + faction + " has no leader.");
				pawn = negotiator;
				value = faction.Name;
			}
			DiaNode diaNode;
			if (faction.PlayerRelationKind != 0)
			{
				diaNode = ((faction.PlayerRelationKind != FactionRelationKind.Neutral) ? new DiaNode("FactionGreetingWarm".Translate(value, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn)) : new DiaNode("FactionGreetingWary".Translate(value, negotiator.LabelShort, negotiator.Named("NEGOTIATOR"), pawn.Named("LEADER")).AdjustedFor(pawn)));
			}
			else
			{
				string key = (faction.def.permanentEnemy || !"FactionGreetingHostileAppreciative".CanTranslate()) ? "FactionGreetingHostile" : "FactionGreetingHostileAppreciative";
				diaNode = new DiaNode(key.Translate(value).AdjustedFor(pawn));
			}
			if (map != null && map.IsPlayerHome)
			{
				diaNode.options.Add(RequestTraderOption(map, faction, negotiator));
				diaNode.options.Add(RequestMilitaryAidOption(map, faction, negotiator));
				if (DefDatabase<ResearchProjectDef>.AllDefsListForReading.Any((ResearchProjectDef rp) => rp.HasTag(ResearchProjectTagDefOf.ShipRelated) && rp.IsFinished))
				{
					diaNode.options.Add(RequestAICoreQuest(map, faction, negotiator));
				}
			}
			if (Prefs.DevMode)
			{
				foreach (DiaOption item in DebugOptions(faction, negotiator))
				{
					diaNode.options.Add(item);
				}
			}
			DiaOption diaOption = new DiaOption("(" + "Disconnect".Translate() + ")");
			diaOption.resolveTree = true;
			diaNode.options.Add(diaOption);
			return diaNode;
		}

		private static IEnumerable<DiaOption> DebugOptions(Faction faction, Pawn negotiator)
		{
			yield return new DiaOption("(Debug) Goodwill +10")
			{
				action = delegate
				{
					faction.TryAffectGoodwillWith(Faction.OfPlayer, 10, canSendMessage: false);
				},
				linkLateBind = (() => FactionDialogFor(negotiator, faction))
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private static int AmountSendableSilver(Map map)
		{
			return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
			where t.def == ThingDefOf.Silver
			select t).Sum((Thing t) => t.stackCount);
		}

		private static DiaOption RequestAICoreQuest(Map map, Faction faction, Pawn negotiator)
		{
			string text = "RequestAICoreInformation".Translate(ThingDefOf.AIPersonaCore.label, 1500.ToString());
			if (faction.PlayerGoodwill < 40)
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("NeedGoodwill".Translate(40.ToString("F0")));
				return diaOption;
			}
			IncidentDef def = IncidentDefOf.Quest_ItemStashAICore;
			bool flag = PlayerItemAccessibilityUtility.ItemStashHas(ThingDefOf.AIPersonaCore);
			IncidentParms coreIncidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Find.World);
			coreIncidentParms.faction = faction;
			bool flag2 = def.Worker.CanFireNow(coreIncidentParms);
			if (flag || !flag2)
			{
				DiaOption diaOption2 = new DiaOption(text);
				diaOption2.Disable("NoKnownAICore".Translate(1500));
				return diaOption2;
			}
			if (AmountSendableSilver(map) < 1500)
			{
				DiaOption diaOption3 = new DiaOption(text);
				diaOption3.Disable("NeedSilverLaunchable".Translate(1500));
				return diaOption3;
			}
			DiaOption diaOption4 = new DiaOption(text);
			diaOption4.action = delegate
			{
				if (def.Worker.TryExecute(coreIncidentParms))
				{
					TradeUtility.LaunchThingsOfType(ThingDefOf.Silver, 1500, map, null);
				}
				Current.Game.GetComponent<GameComponent_OnetimeNotification>().sendAICoreRequestReminder = false;
			};
			string text2 = "RequestAICoreInformationResult".Translate(faction.leader).CapitalizeFirst();
			DiaNode diaNode = new DiaNode(text2);
			diaNode.options.Add(OKToRoot(faction, negotiator));
			diaOption4.link = diaNode;
			return diaOption4;
		}

		private static DiaOption RequestTraderOption(Map map, Faction faction, Pawn negotiator)
		{
			string text = "RequestTrader".Translate(15);
			if (faction.PlayerRelationKind != FactionRelationKind.Ally)
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("MustBeAlly".Translate());
				return diaOption;
			}
			if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption2 = new DiaOption(text);
				diaOption2.Disable("BadTemperature".Translate());
				return diaOption2;
			}
			int num = faction.lastTraderRequestTick + 240000 - Find.TickManager.TicksGame;
			if (num > 0)
			{
				DiaOption diaOption3 = new DiaOption(text);
				diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
				return diaOption3;
			}
			DiaOption diaOption4 = new DiaOption(text);
			DiaNode diaNode = new DiaNode("TraderSent".Translate(faction.leader).CapitalizeFirst());
			diaNode.options.Add(OKToRoot(faction, negotiator));
			DiaNode diaNode2 = new DiaNode("ChooseTraderKind".Translate(faction.leader));
			foreach (TraderKindDef item in from x in faction.def.caravanTraderKinds
			where x.requestable
			select x)
			{
				TraderKindDef localTk = item;
				DiaOption diaOption5 = new DiaOption(localTk.LabelCap);
				diaOption5.action = delegate
				{
					IncidentParms parms = new IncidentParms
					{
						target = map,
						faction = faction,
						traderKind = localTk,
						forced = true
					};
					Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + 120000, parms, 240000);
					faction.lastTraderRequestTick = Find.TickManager.TicksGame;
					Faction faction2 = faction;
					Faction ofPlayer = Faction.OfPlayer;
					int goodwillChange = -15;
					bool canSendMessage = false;
					string reason = "GoodwillChangedReason_RequestedTrader".Translate();
					faction2.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, canSendHostilityLetter: true, reason);
				};
				diaOption5.link = diaNode;
				diaNode2.options.Add(diaOption5);
			}
			DiaOption diaOption6 = new DiaOption("GoBack".Translate());
			diaOption6.linkLateBind = ResetToRoot(faction, negotiator);
			diaNode2.options.Add(diaOption6);
			diaOption4.link = diaNode2;
			return diaOption4;
		}

		private static DiaOption RequestMilitaryAidOption(Map map, Faction faction, Pawn negotiator)
		{
			string text = "RequestMilitaryAid".Translate(25);
			if (faction.PlayerRelationKind != FactionRelationKind.Ally)
			{
				DiaOption diaOption = new DiaOption(text);
				diaOption.Disable("MustBeAlly".Translate());
				return diaOption;
			}
			if (!faction.def.allowedArrivalTemperatureRange.ExpandedBy(-4f).Includes(map.mapTemperature.SeasonalTemp))
			{
				DiaOption diaOption2 = new DiaOption(text);
				diaOption2.Disable("BadTemperature".Translate());
				return diaOption2;
			}
			int num = faction.lastMilitaryAidRequestTick + 60000 - Find.TickManager.TicksGame;
			if (num > 0)
			{
				DiaOption diaOption3 = new DiaOption(text);
				diaOption3.Disable("WaitTime".Translate(num.ToStringTicksToPeriod()));
				return diaOption3;
			}
			if (NeutralGroupIncidentUtility.AnyBlockingHostileLord(map, faction))
			{
				DiaOption diaOption4 = new DiaOption(text);
				diaOption4.Disable("HostileVisitorsPresent".Translate());
				return diaOption4;
			}
			DiaOption diaOption5 = new DiaOption(text);
			if ((int)faction.def.techLevel < 4)
			{
				diaOption5.link = CantMakeItInTime(faction, negotiator);
			}
			else
			{
				IEnumerable<Faction> source = (from x in map.attackTargetsCache.TargetsHostileToColony.Where(GenHostility.IsActiveThreatToPlayer)
				select ((Thing)x).Faction into x
				where x != null && !x.HostileTo(faction)
				select x).Distinct();
				if (source.Any())
				{
					DiaNode diaNode = new DiaNode("MilitaryAidConfirmMutualEnemy".Translate(faction.Name, (from fa in source
					select fa.Name).ToCommaList(useAnd: true)));
					DiaOption diaOption6 = new DiaOption("CallConfirm".Translate());
					diaOption6.action = delegate
					{
						CallForAid(map, faction);
					};
					diaOption6.link = FightersSent(faction, negotiator);
					DiaOption diaOption7 = new DiaOption("CallCancel".Translate());
					diaOption7.linkLateBind = ResetToRoot(faction, negotiator);
					diaNode.options.Add(diaOption6);
					diaNode.options.Add(diaOption7);
					diaOption5.link = diaNode;
				}
				else
				{
					diaOption5.action = delegate
					{
						CallForAid(map, faction);
					};
					diaOption5.link = FightersSent(faction, negotiator);
				}
			}
			return diaOption5;
		}

		private static DiaNode CantMakeItInTime(Faction faction, Pawn negotiator)
		{
			DiaNode diaNode = new DiaNode("CantSendMilitaryAidInTime".Translate(faction.leader).CapitalizeFirst());
			diaNode.options.Add(OKToRoot(faction, negotiator));
			return diaNode;
		}

		private static DiaNode FightersSent(Faction faction, Pawn negotiator)
		{
			DiaNode diaNode = new DiaNode("MilitaryAidSent".Translate(faction.leader).CapitalizeFirst());
			diaNode.options.Add(OKToRoot(faction, negotiator));
			return diaNode;
		}

		private static void CallForAid(Map map, Faction faction)
		{
			Faction ofPlayer = Faction.OfPlayer;
			int goodwillChange = -25;
			bool canSendMessage = false;
			string reason = "GoodwillChangedReason_RequestedMilitaryAid".Translate();
			faction.TryAffectGoodwillWith(ofPlayer, goodwillChange, canSendMessage, canSendHostilityLetter: true, reason);
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = map;
			incidentParms.faction = faction;
			incidentParms.raidArrivalModeForQuickMilitaryAid = true;
			incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
			faction.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
			IncidentDefOf.RaidFriendly.Worker.TryExecute(incidentParms);
		}

		private static DiaOption OKToRoot(Faction faction, Pawn negotiator)
		{
			DiaOption diaOption = new DiaOption("OK".Translate());
			diaOption.linkLateBind = ResetToRoot(faction, negotiator);
			return diaOption;
		}

		private static Func<DiaNode> ResetToRoot(Faction faction, Pawn negotiator)
		{
			return () => FactionDialogFor(negotiator, faction);
		}
	}
}
