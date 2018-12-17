using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsIncidents
	{
		[DebugOutput]
		[Category("Incidents")]
		public static void MiscIncidentChances()
		{
			List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				StorytellerComp_CategoryMTB storytellerComp_CategoryMTB = storytellerComps[i] as StorytellerComp_CategoryMTB;
				if (storytellerComp_CategoryMTB != null && ((StorytellerCompProperties_CategoryMTB)storytellerComp_CategoryMTB.props).category == IncidentCategoryDefOf.Misc)
				{
					storytellerComp_CategoryMTB.DebugTablesIncidentChances(IncidentCategoryDefOf.Misc);
				}
			}
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void TradeRequestsSampled()
		{
			Map currentMap = Find.CurrentMap;
			IncidentWorker_QuestTradeRequest incidentWorker_QuestTradeRequest = (IncidentWorker_QuestTradeRequest)IncidentDefOf.Quest_TradeRequest.Worker;
			Dictionary<ThingDef, int> counts = new Dictionary<ThingDef, int>();
			for (int i = 0; i < 100; i++)
			{
				SettlementBase settlementBase = IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(currentMap.Tile);
				if (settlementBase == null)
				{
					break;
				}
				TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
				if (incidentWorker_QuestTradeRequest.TryGenerateTradeRequest(component, currentMap))
				{
					if (!counts.ContainsKey(component.requestThingDef))
					{
						counts.Add(component.requestThingDef, 0);
					}
					Dictionary<ThingDef, int> dictionary;
					ThingDef requestThingDef;
					(dictionary = counts)[requestThingDef = component.requestThingDef] = dictionary[requestThingDef] + 1;
				}
				settlementBase.GetComponent<TradeRequestComp>().Disable();
			}
			DebugTables.MakeTablesDialog(from d in DefDatabase<ThingDef>.AllDefs
			where counts.ContainsKey(d)
			orderby counts[d] descending
			select d, new TableDataGetter<ThingDef>("defName", (ThingDef d) => d.defName), new TableDataGetter<ThingDef>("appearance rate in " + 100 + " trade requests", (ThingDef d) => ((float)counts[d] / 100f).ToStringPercent()));
		}

		[DebugOutput]
		[Category("Incidents")]
		[ModeRestrictionPlay]
		public static void FutureIncidents()
		{
			StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(currentMapOnly: false);
		}

		[DebugOutput]
		[Category("Incidents")]
		[ModeRestrictionPlay]
		public static void FutureIncidentsCurrentMap()
		{
			StorytellerUtility.ShowFutureIncidentsDebugLogFloatMenu(currentMapOnly: true);
		}

		[DebugOutput]
		[Category("Incidents")]
		[ModeRestrictionPlay]
		public static void IncidentTargetsList()
		{
			StorytellerUtility.DebugLogTestIncidentTargets();
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void TradeRequests()
		{
			Map currentMap = Find.CurrentMap;
			IncidentWorker_QuestTradeRequest incidentWorker_QuestTradeRequest = (IncidentWorker_QuestTradeRequest)IncidentDefOf.Quest_TradeRequest.Worker;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Randomly-generated trade requests for map " + currentMap.ToString() + ":");
			stringBuilder.AppendLine();
			for (int i = 0; i < 50; i++)
			{
				SettlementBase settlementBase = IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(currentMap.Tile);
				if (settlementBase == null)
				{
					break;
				}
				stringBuilder.AppendLine("Settlement: " + settlementBase.LabelCap);
				TradeRequestComp component = settlementBase.GetComponent<TradeRequestComp>();
				if (incidentWorker_QuestTradeRequest.TryGenerateTradeRequest(component, currentMap))
				{
					stringBuilder.AppendLine("Duration: " + (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays());
					string str = GenLabel.ThingLabel(component.requestThingDef, null, component.requestCount) + " ($" + (component.requestThingDef.BaseMarketValue * (float)component.requestCount).ToString("F0") + ")";
					stringBuilder.AppendLine("Request: " + str);
					string str2 = GenThing.ThingsToCommaList(component.rewards) + " ($" + (from t in component.rewards
					select t.MarketValue * (float)t.stackCount).Sum().ToString("F0") + ")";
					stringBuilder.AppendLine("Reward: " + str2);
				}
				else
				{
					stringBuilder.AppendLine("TryGenerateTradeRequest failed.");
				}
				stringBuilder.AppendLine();
				settlementBase.GetComponent<TradeRequestComp>().Disable();
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void PawnArrivalCandidates()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(IncidentDefOf.RaidEnemy.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidEnemy.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.RaidFriendly.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.RaidFriendly.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.VisitorGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.VisitorGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TravelerGroup.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TravelerGroup.Worker).DebugListingOfGroupSources());
			stringBuilder.AppendLine(IncidentDefOf.TraderCaravanArrival.defName);
			stringBuilder.AppendLine(((IncidentWorker_PawnsArrive)IncidentDefOf.TraderCaravanArrival.Worker).DebugListingOfGroupSources());
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void TraderStockMarketValues()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
			{
				stringBuilder.AppendLine(allDef.defName + " : " + ((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).AverageTotalStockValue(allDef).ToString("F0"));
			}
			Log.Message(stringBuilder.ToString());
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void TraderStockGeneration()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			foreach (TraderKindDef allDef in DefDatabase<TraderKindDef>.AllDefs)
			{
				TraderKindDef localDef = allDef;
				FloatMenuOption item = new FloatMenuOption(localDef.defName, delegate
				{
					Log.Message(((ThingSetMaker_TraderStock)ThingSetMakerDefOf.TraderStock.root).GenerationDataFor(localDef));
				});
				list.Add(item);
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void TraderStockGeneratorsDefs()
		{
			if (Find.CurrentMap == null)
			{
				Log.Error("Requires visible map.");
			}
			else
			{
				StringBuilder sb = new StringBuilder();
				Action<StockGenerator> action = delegate(StockGenerator gen)
				{
					sb.AppendLine(gen.GetType().ToString());
					sb.AppendLine("ALLOWED DEFS:");
					foreach (ThingDef item in from d in DefDatabase<ThingDef>.AllDefs
					where gen.HandlesThingDef(d)
					select d)
					{
						sb.AppendLine(item.defName + " [" + item.BaseMarketValue + "]");
					}
					sb.AppendLine();
					sb.AppendLine("GENERATION TEST:");
					gen.countRange = IntRange.one;
					for (int i = 0; i < 30; i++)
					{
						foreach (Thing item2 in gen.GenerateThings(Find.CurrentMap.Tile))
						{
							sb.AppendLine(item2.Label + " [" + item2.MarketValue + "]");
						}
					}
					sb.AppendLine("---------------------------------------------------------");
				};
				action(new StockGenerator_Armor());
				action(new StockGenerator_WeaponsRanged());
				action(new StockGenerator_Clothes());
				action(new StockGenerator_Art());
				Log.Message(sb.ToString());
			}
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void PawnGroupGenSampled()
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				if (allFaction.def.pawnGroupMakers != null && allFaction.def.pawnGroupMakers.Any((PawnGroupMaker x) => x.kindDef == PawnGroupKindDefOf.Combat))
				{
					Faction localFac = allFaction;
					float localP;
					float maxPawnCost;
					list.Add(new DebugMenuOption(localFac.Name + " (" + localFac.def.defName + ")", DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float item in Dialog_DebugActionsMenu.PointsOptions(extended: true))
						{
							float num = item;
							localP = num;
							maxPawnCost = PawnGroupMakerUtility.MaxPawnCost(localFac, localP, null, PawnGroupKindDefOf.Combat);
							string defName = (from op in localFac.def.pawnGroupMakers.SelectMany((PawnGroupMaker gm) => gm.options)
							where op.Cost <= maxPawnCost
							select op).MaxBy((PawnGenOption op) => op.Cost).kind.defName;
							string label = localP.ToString() + ", max " + maxPawnCost.ToString("F0") + " " + defName;
							list2.Add(new DebugMenuOption(label, DebugMenuOptionMode.Action, delegate
							{
								Dictionary<ThingDef, int>[] weaponsCount = new Dictionary<ThingDef, int>[20];
								string[] pawnKinds = new string[20];
								for (int i = 0; i < 20; i++)
								{
									weaponsCount[i] = new Dictionary<ThingDef, int>();
									List<Pawn> list3 = PawnGroupMakerUtility.GeneratePawns(new PawnGroupMakerParms
									{
										groupKind = PawnGroupKindDefOf.Combat,
										tile = Find.CurrentMap.Tile,
										points = localP,
										faction = localFac
									}, warnOnZeroResults: false).ToList();
									pawnKinds[i] = PawnUtility.PawnKindsToCommaList(list3, useAnd: true);
									foreach (Pawn item2 in list3)
									{
										if (item2.equipment.Primary != null)
										{
											if (!weaponsCount[i].ContainsKey(item2.equipment.Primary.def))
											{
												weaponsCount[i].Add(item2.equipment.Primary.def, 0);
											}
											Dictionary<ThingDef, int> dictionary;
											ThingDef def;
											(dictionary = weaponsCount[i])[def = item2.equipment.Primary.def] = dictionary[def] + 1;
										}
										item2.Destroy();
									}
								}
								int totalPawns = weaponsCount.Sum((Dictionary<ThingDef, int> x) => x.Sum((KeyValuePair<ThingDef, int> y) => y.Value));
								List<TableDataGetter<int>> list4 = new List<TableDataGetter<int>>();
								list4.Add(new TableDataGetter<int>(string.Empty, (int x) => (x != 20) ? (x + 1).ToString() : "avg"));
								list4.Add(new TableDataGetter<int>("pawns", (int x) => " " + ((x != 20) ? weaponsCount[x].Sum((KeyValuePair<ThingDef, int> y) => y.Value).ToString() : ((float)totalPawns / 20f).ToString("0.#"))));
								list4.Add(new TableDataGetter<int>("kinds", (int x) => (x != 20) ? pawnKinds[x] : string.Empty));
								list4.AddRange(from x in DefDatabase<ThingDef>.AllDefs
								where x.IsWeapon && !x.weaponTags.NullOrEmpty() && weaponsCount.Any((Dictionary<ThingDef, int> wc) => wc.ContainsKey(x))
								orderby x.IsMeleeWeapon descending, x.techLevel, x.BaseMarketValue
								select new TableDataGetter<int>(x.label.Shorten(), delegate(int y)
								{
									if (y == 20)
									{
										return " " + ((float)weaponsCount.Sum((Dictionary<ThingDef, int> z) => z.ContainsKey(x) ? z[x] : 0) / 20f).ToString("0.#");
									}
									return (!weaponsCount[y].ContainsKey(x)) ? string.Empty : (" " + weaponsCount[y][x] + " (" + ((float)weaponsCount[y][x] / (float)weaponsCount[y].Sum((KeyValuePair<ThingDef, int> z) => z.Value)).ToStringPercent("F0") + ")");
								}));
								DebugTables.MakeTablesDialog(Enumerable.Range(0, 21), list4.ToArray());
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
			}
			Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void RaidFactionSampled()
		{
			((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidFactionSampled();
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void RaidStrategySampled()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(null);
			}));
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				Faction faction = allFaction;
				list.Add(new FloatMenuOption(faction.Name + " (" + faction.def.defName + ")", delegate
				{
					((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidStrategySampled(allFaction);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		[DebugOutput]
		[Category("Incidents")]
		public static void RaidArrivemodeSampled()
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("Choose factions randomly like a real raid", delegate
			{
				((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(null);
			}));
			foreach (Faction allFaction in Find.FactionManager.AllFactions)
			{
				Faction faction = allFaction;
				list.Add(new FloatMenuOption(faction.Name + " (" + faction.def.defName + ")", delegate
				{
					((IncidentWorker_Raid)IncidentDefOf.RaidEnemy.Worker).DoTable_RaidArrivalModeSampled(allFaction);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}
	}
}
