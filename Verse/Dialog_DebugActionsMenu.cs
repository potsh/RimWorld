using RimWorld;
using RimWorld.BaseGen;
using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using Verse.Profile;
using Verse.Sound;

namespace Verse
{
	public class Dialog_DebugActionsMenu : Dialog_DebugOptionLister
	{
		private static List<PawnKindDef> pawnKindsForDamageTypeBattleRoyale;

		private static Map mapLeak;

		[CompilerGenerated]
		private static Func<Pawn, bool> _003C_003Ef__mg_0024cache0;

		public override bool IsDebug => true;

		public Dialog_DebugActionsMenu()
		{
			forcePause = true;
		}

		protected override void DoListingItems()
		{
			if (KeyBindingDefOf.Dev_ToggleDebugActionsMenu.KeyDownEvent)
			{
				Event.current.Use();
				Close();
			}
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (WorldRendererUtility.WorldRenderedNow)
				{
					DoListingItems_World();
				}
				else if (Find.CurrentMap != null)
				{
					DoListingItems_MapActions();
					DoListingItems_MapTools();
				}
				DoListingItems_AllModePlayActions();
			}
			else
			{
				DoListingItems_Entry();
			}
		}

		private void DoListingItems_Entry()
		{
			DoLabel("Translation tools");
			DebugAction("Write backstory translation file", delegate
			{
				LanguageDataWriter.WriteBackstoryFile();
			});
			DebugAction("Save translation report", delegate
			{
				LanguageReportGenerator.SaveTranslationReport();
			});
		}

		private void DoListingItems_AllModePlayActions()
		{
			DoGap();
			DoLabel("Actions - Map management");
			DebugAction("Generate map", delegate
			{
				MapParent mapParent3 = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				mapParent3.Tile = TileFinder.RandomStartingTile();
				mapParent3.SetFaction(Faction.OfPlayer);
				Find.WorldObjects.Add(mapParent3);
				GetOrGenerateMapUtility.GetOrGenerateMap(mapParent3.Tile, new IntVec3(50, 1, 50), null);
			});
			DebugAction("Destroy map", delegate
			{
				List<DebugMenuOption> list5 = new List<DebugMenuOption>();
				List<Map> maps4 = Find.Maps;
				for (int m = 0; m < maps4.Count; m++)
				{
					Map map5 = maps4[m];
					list5.Add(new DebugMenuOption(map5.ToString(), DebugMenuOptionMode.Action, delegate
					{
						Current.Game.DeinitAndRemoveMap(map5);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list5));
			});
			DebugAction("Leak map", delegate
			{
				List<DebugMenuOption> list4 = new List<DebugMenuOption>();
				List<Map> maps3 = Find.Maps;
				for (int l = 0; l < maps3.Count; l++)
				{
					Map map4 = maps3[l];
					list4.Add(new DebugMenuOption(map4.ToString(), DebugMenuOptionMode.Action, delegate
					{
						mapLeak = map4;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
			});
			DebugAction("Print leaked map", delegate
			{
				Log.Message($"Leaked map {mapLeak}");
			});
			DebugToolMap("Transfer", delegate
			{
				List<Thing> toTransfer = Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList();
				if (toTransfer.Any())
				{
					List<DebugMenuOption> list3 = new List<DebugMenuOption>();
					List<Map> maps2 = Find.Maps;
					for (int j = 0; j < maps2.Count; j++)
					{
						Map map2 = maps2[j];
						if (map2 != Find.CurrentMap)
						{
							list3.Add(new DebugMenuOption(map2.ToString(), DebugMenuOptionMode.Action, delegate
							{
								for (int k = 0; k < toTransfer.Count; k++)
								{
									IntVec3 center = map2.Center;
									Map map3 = map2;
									IntVec3 size2 = map2.Size;
									int x2 = size2.x;
									IntVec3 size3 = map2.Size;
									if (CellFinder.TryFindRandomCellNear(center, map3, Mathf.Max(x2, size3.z), (IntVec3 x) => !x.Fogged(map2) && x.Standable(map2), out IntVec3 result))
									{
										toTransfer[k].DeSpawn();
										GenPlace.TryPlaceThing(toTransfer[k], result, map2, ThingPlaceMode.Near);
									}
									else
									{
										Log.Error("Could not find spawn cell.");
									}
								}
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
				}
			});
			DebugAction("Change map", delegate
			{
				List<DebugMenuOption> list2 = new List<DebugMenuOption>();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					Map map = maps[i];
					if (map != Find.CurrentMap)
					{
						list2.Add(new DebugMenuOption(map.ToString(), DebugMenuOptionMode.Action, delegate
						{
							Current.Game.CurrentMap = map;
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
			});
			DebugAction("Regenerate current map", delegate
			{
				RememberedCameraPos rememberedCameraPos = Find.CurrentMap.rememberedCameraPos;
				int tile3 = Find.CurrentMap.Tile;
				MapParent parent = Find.CurrentMap.Parent;
				IntVec3 size = Find.CurrentMap.Size;
				Current.Game.DeinitAndRemoveMap(Find.CurrentMap);
				Map orGenerateMap2 = GetOrGenerateMapUtility.GetOrGenerateMap(tile3, size, parent.def);
				Current.Game.CurrentMap = orGenerateMap2;
				Find.World.renderer.wantedMode = WorldRenderMode.None;
				Find.CameraDriver.SetRootPosAndSize(rememberedCameraPos.rootPos, rememberedCameraPos.rootSize);
			});
			DebugAction("Generate map with caves", delegate
			{
				int tile2 = TileFinder.RandomSettlementTileFor(Faction.OfPlayer, mustBeAutoChoosable: false, (int x) => Find.World.HasCaves(x));
				if (Find.CurrentMap != null)
				{
					Find.WorldObjects.Remove(Find.CurrentMap.Parent);
				}
				MapParent mapParent2 = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
				mapParent2.Tile = tile2;
				mapParent2.SetFaction(Faction.OfPlayer);
				Find.WorldObjects.Add(mapParent2);
				Map orGenerateMap = GetOrGenerateMapUtility.GetOrGenerateMap(tile2, Find.World.info.initialMapSize, null);
				Current.Game.CurrentMap = orGenerateMap;
				Find.World.renderer.wantedMode = WorldRenderMode.None;
			});
			DebugAction("Run MapGenerator", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (MapGeneratorDef item in DefDatabase<MapGeneratorDef>.AllDefsListForReading)
				{
					list.Add(new DebugMenuOption(item.defName, DebugMenuOptionMode.Action, delegate
					{
						MapParent mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
						mapParent.Tile = (from tile in Enumerable.Range(0, Find.WorldGrid.TilesCount)
						where Find.WorldGrid[tile].biome.canBuildBase
						select tile).RandomElement();
						mapParent.SetFaction(Faction.OfPlayer);
						Find.WorldObjects.Add(mapParent);
						Map currentMap = MapGenerator.GenerateMap(Find.World.info.initialMapSize, mapParent, item);
						Current.Game.CurrentMap = currentMap;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			DebugAction("Force reform in current map", delegate
			{
				if (Find.CurrentMap != null)
				{
					TimedForcedExit.ForceReform(Find.CurrentMap.Parent);
				}
			});
		}

		private void DoListingItems_MapActions()
		{
			Text.Font = GameFont.Tiny;
			DoLabel("Incidents");
			if (Find.CurrentMap != null)
			{
				DoIncidentDebugAction(Find.CurrentMap);
				DoIncidentWithPointsAction(Find.CurrentMap);
			}
			DoIncidentDebugAction(Find.World);
			float localP;
			DebugAction("Execute raid with points...", delegate
			{
				Close();
				List<FloatMenuOption> list14 = new List<FloatMenuOption>();
				foreach (float item2 in PointsOptions(extended: true))
				{
					localP = item2;
					list14.Add(new FloatMenuOption(localP.ToString() + " points", delegate
					{
						IncidentParms parms3 = new IncidentParms
						{
							target = Find.CurrentMap,
							points = localP
						};
						IncidentDefOf.RaidEnemy.Worker.TryExecute(parms3);
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list14));
			});
			Faction localFac2;
			float localPoints;
			RaidStrategyDef localStrat;
			PawnsArrivalModeDef localArrival;
			DebugAction("Execute raid with specifics...", delegate
			{
				Dialog_DebugActionsMenu dialog_DebugActionsMenu = this;
				StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
				IncidentParms parms2 = storytellerComp.GenerateParms(IncidentCategoryDefOf.ThreatBig, Find.CurrentMap);
				List<DebugMenuOption> list10 = new List<DebugMenuOption>();
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					localFac2 = allFaction;
					list10.Add(new DebugMenuOption(localFac2.Name + " (" + localFac2.def.defName + ")", DebugMenuOptionMode.Action, delegate
					{
						parms2.faction = localFac2;
						List<DebugMenuOption> list11 = new List<DebugMenuOption>();
						foreach (float item3 in PointsOptions(extended: true))
						{
							localPoints = item3;
							list11.Add(new DebugMenuOption(item3 + " points", DebugMenuOptionMode.Action, delegate
							{
								parms2.points = localPoints;
								List<DebugMenuOption> list12 = new List<DebugMenuOption>();
								foreach (RaidStrategyDef allDef in DefDatabase<RaidStrategyDef>.AllDefs)
								{
									localStrat = allDef;
									string text2 = localStrat.defName;
									if (!localStrat.Worker.CanUseWith(parms2, PawnGroupKindDefOf.Combat))
									{
										text2 += " [NO]";
									}
									list12.Add(new DebugMenuOption(text2, DebugMenuOptionMode.Action, delegate
									{
										parms2.raidStrategy = localStrat;
										List<DebugMenuOption> list13 = new List<DebugMenuOption>
										{
											new DebugMenuOption("-Random-", DebugMenuOptionMode.Action, delegate
											{
												dialog_DebugActionsMenu.DoRaid(parms2);
											})
										};
										foreach (PawnsArrivalModeDef allDef2 in DefDatabase<PawnsArrivalModeDef>.AllDefs)
										{
											localArrival = allDef2;
											string text3 = localArrival.defName;
											if (!localArrival.Worker.CanUseWith(parms2) || !localStrat.arriveModes.Contains(localArrival))
											{
												text3 += " [NO]";
											}
											list13.Add(new DebugMenuOption(text3, DebugMenuOptionMode.Action, delegate
											{
												parms2.raidArrivalMode = localArrival;
												dialog_DebugActionsMenu.DoRaid(parms2);
											}));
										}
										Find.WindowStack.Add(new Dialog_DebugOptionListLister(list13));
									}));
								}
								Find.WindowStack.Add(new Dialog_DebugOptionListLister(list12));
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list11));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list10));
			});
			DoGap();
			DoLabel("Actions - Misc");
			DebugAction("Destroy all plants", delegate
			{
				foreach (Thing item4 in Find.CurrentMap.listerThings.AllThings.ToList())
				{
					if (item4 is Plant)
					{
						item4.Destroy();
					}
				}
			});
			DebugAction("Destroy all things", delegate
			{
				foreach (Thing item5 in Find.CurrentMap.listerThings.AllThings.ToList())
				{
					item5.Destroy();
				}
			});
			DebugAction("Finish all research", delegate
			{
				Find.ResearchManager.DebugSetAllProjectsFinished();
				Messages.Message("All research finished.", MessageTypeDefOf.TaskCompletion, historical: false);
			});
			DebugAction("Replace all trade ships", delegate
			{
				Find.CurrentMap.passingShipManager.DebugSendAllShipsAway();
				for (int j = 0; j < 5; j++)
				{
					IncidentParms parms = new IncidentParms
					{
						target = Find.CurrentMap
					};
					IncidentDefOf.OrbitalTraderArrival.Worker.TryExecute(parms);
				}
			});
			WeatherDef localWeather;
			DebugAction("Change weather...", delegate
			{
				List<DebugMenuOption> list9 = new List<DebugMenuOption>();
				foreach (WeatherDef allDef3 in DefDatabase<WeatherDef>.AllDefs)
				{
					localWeather = allDef3;
					list9.Add(new DebugMenuOption(localWeather.LabelCap, DebugMenuOptionMode.Action, delegate
					{
						Find.CurrentMap.weatherManager.TransitionTo(localWeather);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list9));
			});
			SongDef localSong;
			DebugAction("Play song...", delegate
			{
				List<DebugMenuOption> list8 = new List<DebugMenuOption>();
				foreach (SongDef allDef4 in DefDatabase<SongDef>.AllDefs)
				{
					localSong = allDef4;
					list8.Add(new DebugMenuOption(localSong.defName, DebugMenuOptionMode.Action, delegate
					{
						Find.MusicManagerPlay.ForceStartSong(localSong, ignorePrefsVolume: false);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list8));
			});
			SoundDef localSd;
			DebugAction("Play sound...", delegate
			{
				List<DebugMenuOption> list7 = new List<DebugMenuOption>();
				foreach (SoundDef item6 in from s in DefDatabase<SoundDef>.AllDefs
				where !s.sustain
				select s)
				{
					localSd = item6;
					list7.Add(new DebugMenuOption(localSd.defName, DebugMenuOptionMode.Action, delegate
					{
						if (localSd.subSounds.Any((SubSoundDef sub) => sub.onCamera))
						{
							localSd.PlayOneShotOnCamera();
						}
						else
						{
							localSd.PlayOneShot(SoundInfo.InMap(new TargetInfo(Find.CameraDriver.MapPosition, Find.CurrentMap)));
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list7));
			});
			if (Find.CurrentMap.gameConditionManager.ActiveConditions.Count > 0)
			{
				GameCondition localMc;
				DebugAction("End game condition ...", delegate
				{
					List<DebugMenuOption> list6 = new List<DebugMenuOption>();
					foreach (GameCondition activeCondition in Find.CurrentMap.gameConditionManager.ActiveConditions)
					{
						localMc = activeCondition;
						list6.Add(new DebugMenuOption(localMc.LabelCap, DebugMenuOptionMode.Action, delegate
						{
							localMc.End();
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list6));
				});
			}
			DebugAction("Add prisoner", delegate
			{
				AddGuest(prisoner: true);
			});
			DebugAction("Add guest", delegate
			{
				AddGuest(prisoner: false);
			});
			DebugAction("Force enemy assault", delegate
			{
				foreach (Lord lord in Find.CurrentMap.lordManager.lords)
				{
					LordToil_Stage lordToil_Stage = lord.CurLordToil as LordToil_Stage;
					if (lordToil_Stage != null)
					{
						foreach (Transition transition in lord.Graph.transitions)
						{
							if (transition.sources.Contains(lordToil_Stage) && transition.target is LordToil_AssaultColony)
							{
								Messages.Message("Debug forcing to assault toil: " + lord.faction, MessageTypeDefOf.TaskCompletion, historical: false);
								lord.GotoToil(transition.target);
								return;
							}
						}
					}
				}
			});
			DebugAction("Force enemy flee", delegate
			{
				foreach (Lord lord2 in Find.CurrentMap.lordManager.lords)
				{
					if (lord2.faction != null && lord2.faction.HostileTo(Faction.OfPlayer) && lord2.faction.def.autoFlee)
					{
						LordToil lordToil = lord2.Graph.lordToils.FirstOrDefault((LordToil st) => st is LordToil_PanicFlee);
						if (lordToil != null)
						{
							lord2.GotoToil(lordToil);
						}
					}
				}
			});
			DebugAction("Adaptation progress 10 days", delegate
			{
				Find.StoryWatcher.watcherAdaptation.Debug_OffsetAdaptDays(10f);
			});
			DebugAction("Unload unused assets", delegate
			{
				MemoryUtility.UnloadUnusedUnityAssets();
			});
			DebugAction("Name settlement...", delegate
			{
				List<DebugMenuOption> list5 = new List<DebugMenuOption>
				{
					new DebugMenuOption("Faction", DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFaction());
					})
				};
				if (Find.CurrentMap != null && Find.CurrentMap.IsPlayerHome && Find.CurrentMap.Parent is Settlement)
				{
					Settlement factionBase = (Settlement)Find.CurrentMap.Parent;
					list5.Add(new DebugMenuOption("Faction base", DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_NamePlayerSettlement(factionBase));
					}));
					list5.Add(new DebugMenuOption("Faction and faction base", DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_NamePlayerFactionAndSettlement(factionBase));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list5));
			});
			DebugAction("Next lesson", delegate
			{
				LessonAutoActivator.DebugForceInitiateBestLessonNow();
			});
			DebugAction("Regen all map mesh sections", delegate
			{
				Find.CurrentMap.mapDrawer.RegenerateEverythingNow();
			});
			Type localType;
			DebugAction("Change camera config...", delegate
			{
				List<DebugMenuOption> list4 = new List<DebugMenuOption>();
				foreach (Type item7 in typeof(CameraMapConfig).AllSubclasses())
				{
					localType = item7;
					string text = localType.Name;
					if (text.StartsWith("CameraMapConfig_"))
					{
						text = text.Substring("CameraMapConfig_".Length);
					}
					list4.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						Find.CameraDriver.config = (CameraMapConfig)Activator.CreateInstance(localType);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
			});
			DebugAction("Force ship countdown", delegate
			{
				ShipCountdown.InitiateCountdown(null);
			});
			DebugAction("Flash trade drop spot", delegate
			{
				IntVec3 intVec = DropCellFinder.TradeDropSpot(Find.CurrentMap);
				Find.CurrentMap.debugDrawer.FlashCell(intVec);
				Log.Message("trade drop spot: " + intVec);
			});
			DebugAction("Kill faction leader", delegate
			{
				Pawn leader = (from x in Find.FactionManager.AllFactions
				where x.leader != null
				select x).RandomElement().leader;
				int num = 0;
				while (true)
				{
					if (leader.Dead)
					{
						return;
					}
					if (++num > 1000)
					{
						break;
					}
					leader.TakeDamage(new DamageInfo(DamageDefOf.Bullet, 30f, 999f));
				}
				Log.Warning("Could not kill faction leader.");
			});
			Faction localFac;
			DebugAction("Set faction relations", delegate
			{
				List<FloatMenuOption> list3 = new List<FloatMenuOption>();
				foreach (Faction item8 in Find.FactionManager.AllFactionsVisibleInViewOrder)
				{
					localFac = item8;
					IEnumerator enumerator4 = Enum.GetValues(typeof(FactionRelationKind)).GetEnumerator();
					try
					{
						while (enumerator4.MoveNext())
						{
							FactionRelationKind factionRelationKind = (FactionRelationKind)enumerator4.Current;
							FactionRelationKind localRk = factionRelationKind;
							FloatMenuOption item = new FloatMenuOption(localFac + " - " + localRk, delegate
							{
								localFac.TrySetRelationKind(Faction.OfPlayer, localRk);
							});
							list3.Add(item);
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator4 as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
				}
				Find.WindowStack.Add(new FloatMenu(list3));
			});
			DebugAction("Visitor gift", delegate
			{
				List<Pawn> list2 = new List<Pawn>();
				foreach (Pawn item9 in Find.CurrentMap.mapPawns.AllPawnsSpawned)
				{
					if (item9.Faction != null && !item9.Faction.IsPlayer && !item9.Faction.HostileTo(Faction.OfPlayer))
					{
						list2.Add(item9);
						break;
					}
				}
				VisitorGiftForPlayerUtility.GiveGift(list2, list2[0].Faction);
			});
			DebugAction("Refog map", delegate
			{
				FloodFillerFog.DebugRefogMap(Find.CurrentMap);
			});
			Type localGenStep;
			DebugAction("Use GenStep", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Type item10 in typeof(GenStep).AllSubclassesNonAbstract())
				{
					localGenStep = item10;
					list.Add(new DebugMenuOption(localGenStep.Name, DebugMenuOptionMode.Action, delegate
					{
						GenStep genStep = (GenStep)Activator.CreateInstance(localGenStep);
						genStep.Generate(Find.CurrentMap, default(GenStepParams));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			DebugAction("Increment time 1 hour", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 2500);
			});
			DebugAction("Increment time 6 hours", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 15000);
			});
			DebugAction("Increment time 1 day", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 60000);
			});
			DebugAction("Increment time 1 season", delegate
			{
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 900000);
			});
			DebugAction("StoryWatcher tick 1 day", delegate
			{
				for (int i = 0; i < 60000; i++)
				{
					Find.StoryWatcher.StoryWatcherTick();
					Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1);
				}
			});
		}

		private void DoListingItems_MapTools()
		{
			DoGap();
			DoLabel("Tools - General");
			DebugToolMap("T: Destroy", delegate
			{
				foreach (Thing item2 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item2.Destroy();
				}
			});
			DebugToolMap("T: Kill", delegate
			{
				foreach (Thing item3 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item3.Kill();
				}
			});
			DebugToolMap("T: 10 damage", delegate
			{
				foreach (Thing item4 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item4.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
				}
			});
			DebugToolMap("T: 5000 damage", delegate
			{
				foreach (Thing item5 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item5.TakeDamage(new DamageInfo(DamageDefOf.Crush, 5000f));
				}
			});
			DebugToolMap("T: 5000 flame damage", delegate
			{
				foreach (Thing item6 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					item6.TakeDamage(new DamageInfo(DamageDefOf.Flame, 5000f));
				}
			});
			DebugToolMap("T: Clear area 21x21", delegate
			{
				CellRect r = CellRect.CenteredOn(UI.MouseCell(), 10);
				GenDebug.ClearArea(r, Find.CurrentMap);
			});
			DebugToolMap("T: Rock 21x21", delegate
			{
				CellRect cellRect2 = CellRect.CenteredOn(UI.MouseCell(), 10);
				cellRect2.ClipInsideMap(Find.CurrentMap);
				foreach (IntVec3 item7 in cellRect2)
				{
					GenSpawn.Spawn(ThingDefOf.Granite, item7, Find.CurrentMap);
				}
			});
			DebugToolMap("T: Destroy trees 21x21", delegate
			{
				CellRect cellRect = CellRect.CenteredOn(UI.MouseCell(), 10);
				cellRect.ClipInsideMap(Find.CurrentMap);
				foreach (IntVec3 item8 in cellRect)
				{
					List<Thing> thingList = item8.GetThingList(Find.CurrentMap);
					for (int num20 = thingList.Count - 1; num20 >= 0; num20--)
					{
						if (thingList[num20].def.category == ThingCategory.Plant && thingList[num20].def.plant.IsTree)
						{
							thingList[num20].Destroy();
						}
					}
				}
			});
			DoGap();
			DebugToolMap("T: Explosion (bomb)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Bomb, null);
			});
			DebugToolMap("T: Explosion (flame)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Flame, null);
			});
			DebugToolMap("T: Explosion (stun)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.Stun, null);
			});
			DebugToolMap("T: Explosion (EMP)", delegate
			{
				GenExplosion.DoExplosion(UI.MouseCell(), Find.CurrentMap, 3.9f, DamageDefOf.EMP, null);
			});
			DebugToolMap("T: Explosion (extinguisher)", delegate
			{
				IntVec3 center2 = UI.MouseCell();
				Map currentMap2 = Find.CurrentMap;
				float radius2 = 10f;
				DamageDef extinguish = DamageDefOf.Extinguish;
				Thing instigator2 = null;
				ThingDef filth_FireFoam = ThingDefOf.Filth_FireFoam;
				GenExplosion.DoExplosion(center2, currentMap2, radius2, extinguish, instigator2, -1, -1f, null, null, null, null, filth_FireFoam, 1f, 3, applyDamageToExplosionCellsNeighbors: true);
			});
			DebugToolMap("T: Explosion (smoke)", delegate
			{
				IntVec3 center = UI.MouseCell();
				Map currentMap = Find.CurrentMap;
				float radius = 10f;
				DamageDef smoke = DamageDefOf.Smoke;
				Thing instigator = null;
				ThingDef gas_Smoke = ThingDefOf.Gas_Smoke;
				GenExplosion.DoExplosion(center, currentMap, radius, smoke, instigator, -1, -1f, null, null, null, null, gas_Smoke, 1f);
			});
			DebugToolMap("T: Lightning strike", delegate
			{
				Find.CurrentMap.weatherManager.eventHandler.AddEvent(new WeatherEvent_LightningStrike(Find.CurrentMap, UI.MouseCell()));
			});
			DoGap();
			DebugToolMap("T: Add snow", delegate
			{
				SnowUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, 1f);
			});
			DebugToolMap("T: Remove snow", delegate
			{
				SnowUtility.AddSnowRadial(UI.MouseCell(), Find.CurrentMap, 5f, -1f);
			});
			DebugAction("Clear all snow", delegate
			{
				foreach (IntVec3 allCell in Find.CurrentMap.AllCells)
				{
					Find.CurrentMap.snowGrid.SetDepth(allCell, 0f);
				}
			});
			DebugToolMap("T: Push heat (10)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, 10f);
			});
			DebugToolMap("T: Push heat (10000)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, 10000f);
			});
			DebugToolMap("T: Push heat (-1000)", delegate
			{
				GenTemperature.PushHeat(UI.MouseCell(), Find.CurrentMap, -1000f);
			});
			DoGap();
			DebugToolMap("T: Finish plant growth", delegate
			{
				foreach (Thing item9 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					Plant plant3 = item9 as Plant;
					if (plant3 != null)
					{
						plant3.Growth = 1f;
					}
				}
			});
			DebugToolMap("T: Grow 1 day", delegate
			{
				IntVec3 intVec3 = UI.MouseCell();
				Plant plant2 = intVec3.GetPlant(Find.CurrentMap);
				if (plant2 != null && plant2.def.plant != null)
				{
					int num19 = (int)((1f - plant2.Growth) * plant2.def.plant.growDays);
					if (num19 >= 60000)
					{
						plant2.Age += 60000;
					}
					else if (num19 > 0)
					{
						plant2.Age += num19;
					}
					plant2.Growth += 1f / plant2.def.plant.growDays;
					if ((double)plant2.Growth > 1.0)
					{
						plant2.Growth = 1f;
					}
					Find.CurrentMap.mapDrawer.SectionAt(intVec3).RegenerateAllLayers();
				}
			});
			DebugToolMap("T: Grow to maturity", delegate
			{
				IntVec3 intVec2 = UI.MouseCell();
				Plant plant = intVec2.GetPlant(Find.CurrentMap);
				if (plant != null && plant.def.plant != null)
				{
					int num18 = (int)((1f - plant.Growth) * plant.def.plant.growDays);
					plant.Age += num18;
					plant.Growth = 1f;
					Find.CurrentMap.mapDrawer.SectionAt(intVec2).RegenerateAllLayers();
				}
			});
			DoGap();
			DebugToolMap("T: Regen section", delegate
			{
				Find.CurrentMap.mapDrawer.SectionAt(UI.MouseCell()).RegenerateAllLayers();
			});
			DebugToolMap("T: Randomize color", delegate
			{
				foreach (Thing item10 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompColorable compColorable = item10.TryGetComp<CompColorable>();
					if (compColorable != null)
					{
						item10.SetColor(GenColor.RandomColorOpaque());
					}
				}
			});
			DebugToolMap("T: Rot 1 day", delegate
			{
				foreach (Thing item11 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompRottable compRottable = item11.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress += 60000f;
					}
				}
			});
			DebugToolMap("T: Fuel -20%", delegate
			{
				foreach (Thing item12 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompRefuelable compRefuelable = item12.TryGetComp<CompRefuelable>();
					compRefuelable?.ConsumeFuel(compRefuelable.Props.fuelCapacity * 0.2f);
				}
			});
			DebugToolMap("T: Break down...", delegate
			{
				foreach (Thing item13 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()))
				{
					CompBreakdownable compBreakdownable = item13.TryGetComp<CompBreakdownable>();
					if (compBreakdownable != null && !compBreakdownable.BrokenDown)
					{
						compBreakdownable.DoBreakdown();
					}
				}
			});
			DebugAction("T: Use scatterer", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_MapGen.Options_Scatterers()));
			});
			string localSymbol;
			DebugAction("T: BaseGen", delegate
			{
				List<DebugMenuOption> list31 = new List<DebugMenuOption>();
				foreach (string item14 in (from x in DefDatabase<RuleDef>.AllDefs
				select x.symbol).Distinct())
				{
					localSymbol = item14;
					list31.Add(new DebugMenuOption(item14, DebugMenuOptionMode.Action, delegate
					{
						DebugTool tool5 = null;
						IntVec3 firstCorner2;
						tool5 = new DebugTool("first corner...", delegate
						{
							firstCorner2 = UI.MouseCell();
							DebugTools.curTool = new DebugTool("second corner...", delegate
							{
								IntVec3 second2 = UI.MouseCell();
								CellRect rect = CellRect.FromLimits(firstCorner2, second2).ClipInsideMap(Find.CurrentMap);
								BaseGen.globalSettings.map = Find.CurrentMap;
								BaseGen.symbolStack.Push(localSymbol, rect);
								BaseGen.Generate();
								DebugTools.curTool = tool5;
							}, firstCorner2);
						});
						DebugTools.curTool = tool5;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list31));
			});
			DebugToolMap("T: Make roof", delegate
			{
				CellRect.CellRectIterator iterator2 = CellRect.CenteredOn(UI.MouseCell(), 1).GetIterator();
				while (!iterator2.Done())
				{
					Find.CurrentMap.roofGrid.SetRoof(iterator2.Current, RoofDefOf.RoofConstructed);
					iterator2.MoveNext();
				}
			});
			DebugToolMap("T: Delete roof", delegate
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(UI.MouseCell(), 1).GetIterator();
				while (!iterator.Done())
				{
					Find.CurrentMap.roofGrid.SetRoof(iterator.Current, null);
					iterator.MoveNext();
				}
			});
			DebugToolMap("T: Test flood unfog", delegate
			{
				FloodFillerFog.DebugFloodUnfog(UI.MouseCell(), Find.CurrentMap);
			});
			DebugToolMap("T: Flash closewalk cell 30", delegate
			{
				IntVec3 c4 = CellFinder.RandomClosewalkCellNear(UI.MouseCell(), Find.CurrentMap, 30);
				Find.CurrentMap.debugDrawer.FlashCell(c4);
			});
			DebugToolMap("T: Flash walk path", delegate
			{
				WalkPathFinder.DebugFlashWalkPath(UI.MouseCell());
			});
			DebugToolMap("T: Flash skygaze cell", delegate
			{
				Pawn pawn12 = Find.CurrentMap.mapPawns.FreeColonists.First();
				RCellFinder.TryFindSkygazeCell(UI.MouseCell(), pawn12, out IntVec3 result6);
				Find.CurrentMap.debugDrawer.FlashCell(result6);
				MoteMaker.ThrowText(result6.ToVector3Shifted(), Find.CurrentMap, "for " + pawn12.Label, Color.white);
			});
			DebugToolMap("T: Flash direct flee dest", delegate
			{
				Pawn pawn11 = Find.Selector.SingleSelectedThing as Pawn;
				IntVec3 result5;
				if (pawn11 == null)
				{
					Find.CurrentMap.debugDrawer.FlashCell(UI.MouseCell(), 0f, "select a pawn");
				}
				else if (RCellFinder.TryFindDirectFleeDestination(UI.MouseCell(), 9f, pawn11, out result5))
				{
					Find.CurrentMap.debugDrawer.FlashCell(result5, 0.5f);
				}
				else
				{
					Find.CurrentMap.debugDrawer.FlashCell(UI.MouseCell(), 0.8f, "not found");
				}
			});
			DebugAction("T: Flash spectators cells", delegate
			{
				Action<bool> act4 = delegate(bool bestSideOnly)
				{
					DebugTool tool4 = null;
					IntVec3 firstCorner;
					tool4 = new DebugTool("first watch rect corner...", delegate
					{
						firstCorner = UI.MouseCell();
						DebugTools.curTool = new DebugTool("second watch rect corner...", delegate
						{
							IntVec3 second = UI.MouseCell();
							CellRect spectateRect = CellRect.FromLimits(firstCorner, second).ClipInsideMap(Find.CurrentMap);
							SpectateRectSide allowedSides = SpectateRectSide.All;
							if (bestSideOnly)
							{
								allowedSides = SpectatorCellFinder.FindSingleBestSide(spectateRect, Find.CurrentMap);
							}
							SpectatorCellFinder.DebugFlashPotentialSpectatorCells(spectateRect, Find.CurrentMap, allowedSides);
							DebugTools.curTool = tool4;
						}, firstCorner);
					});
					DebugTools.curTool = tool4;
				};
				List<DebugMenuOption> options3 = new List<DebugMenuOption>
				{
					new DebugMenuOption("All sides", DebugMenuOptionMode.Action, delegate
					{
						act4(obj: false);
					}),
					new DebugMenuOption("Best side only", DebugMenuOptionMode.Action, delegate
					{
						act4(obj: true);
					})
				};
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(options3));
			});
			DebugAction("T: Check reachability", delegate
			{
				List<DebugMenuOption> list30 = new List<DebugMenuOption>();
				TraverseMode[] array6 = (TraverseMode[])Enum.GetValues(typeof(TraverseMode));
				for (int num17 = 0; num17 < array6.Length; num17++)
				{
					TraverseMode traverseMode2 = array6[num17];
					TraverseMode traverseMode = traverseMode2;
					list30.Add(new DebugMenuOption(traverseMode2.ToString(), DebugMenuOptionMode.Action, delegate
					{
						DebugTool tool3 = null;
						IntVec3 from;
						Pawn fromPawn;
						tool3 = new DebugTool("from...", delegate
						{
							from = UI.MouseCell();
							fromPawn = from.GetFirstPawn(Find.CurrentMap);
							string text5 = "to...";
							if (fromPawn != null)
							{
								text5 = text5 + " (pawn=" + fromPawn.LabelShort + ")";
							}
							DebugTools.curTool = new DebugTool(text5, delegate
							{
								DebugTools.curTool = tool3;
							}, delegate
							{
								IntVec3 c3 = UI.MouseCell();
								bool flag2;
								IntVec3 intVec;
								if (fromPawn != null)
								{
									Pawn pawn10 = fromPawn;
									LocalTargetInfo dest = c3;
									PathEndMode peMode = PathEndMode.OnCell;
									Danger maxDanger = Danger.Deadly;
									TraverseMode mode = traverseMode;
									flag2 = pawn10.CanReach(dest, peMode, maxDanger, canBash: false, mode);
									intVec = fromPawn.Position;
								}
								else
								{
									flag2 = Find.CurrentMap.reachability.CanReach(from, c3, PathEndMode.OnCell, traverseMode, Danger.Deadly);
									intVec = from;
								}
								Color color = (!flag2) ? Color.red : Color.green;
								Widgets.DrawLine(intVec.ToUIPosition(), c3.ToUIPosition(), color, 2f);
							});
						});
						DebugTools.curTool = tool3;
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list30));
			});
			DebugToolMapForPawns("T: Flash TryFindRandomPawnExitCell", delegate(Pawn p)
			{
				if (CellFinder.TryFindRandomPawnExitCell(p, out IntVec3 result4))
				{
					p.Map.debugDrawer.FlashCell(result4, 0.5f);
					p.Map.debugDrawer.FlashLine(p.Position, result4);
				}
				else
				{
					p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no exit cell");
				}
			});
			DebugToolMapForPawns("T: RandomSpotJustOutsideColony", delegate(Pawn p)
			{
				if (RCellFinder.TryFindRandomSpotJustOutsideColony(p, out IntVec3 result3))
				{
					p.Map.debugDrawer.FlashCell(result3, 0.5f);
					p.Map.debugDrawer.FlashLine(p.Position, result3);
				}
				else
				{
					p.Map.debugDrawer.FlashCell(p.Position, 0.2f, "no cell");
				}
			});
			DoGap();
			DoLabel("Tools - Pawns");
			DebugToolMap("T: Resurrect", delegate
			{
				foreach (Thing item15 in UI.MouseCell().GetThingList(Find.CurrentMap).ToList())
				{
					Corpse corpse = item15 as Corpse;
					if (corpse != null)
					{
						ResurrectionUtility.Resurrect(corpse.InnerPawn);
					}
				}
			});
			DebugToolMapForPawns("T: Damage to down", delegate(Pawn p)
			{
				HealthUtility.DamageUntilDowned(p);
			});
			DebugToolMapForPawns("T: Damage legs", delegate(Pawn p)
			{
				HealthUtility.DamageLegsUntilIncapableOfMoving(p);
			});
			DebugToolMapForPawns("T: Damage to death", delegate(Pawn p)
			{
				HealthUtility.DamageUntilDead(p);
			});
			DebugToolMap("T: 10 damage until dead", delegate
			{
				foreach (Thing item16 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					for (int num16 = 0; num16 < 1000; num16++)
					{
						item16.TakeDamage(new DamageInfo(DamageDefOf.Crush, 10f));
						if (item16.Destroyed)
						{
							string str = "Took " + (num16 + 1) + " hits";
							Pawn pawn9 = item16 as Pawn;
							if (pawn9 != null)
							{
								if (pawn9.health.ShouldBeDeadFromLethalDamageThreshold())
								{
									str = str + " (reached lethal damage threshold of " + pawn9.health.LethalDamageThreshold.ToString("0.#") + ")";
								}
								else if (PawnCapacityUtility.CalculatePartEfficiency(pawn9.health.hediffSet, pawn9.RaceProps.body.corePart) <= 0.0001f)
								{
									str += " (core part hp reached 0)";
								}
								else
								{
									PawnCapacityDef pawnCapacityDef = pawn9.health.ShouldBeDeadFromRequiredCapacity();
									if (pawnCapacityDef != null)
									{
										str = str + " (incapable of " + pawnCapacityDef.defName + ")";
									}
								}
							}
							Log.Message(str + ".");
							break;
						}
					}
				}
			});
			DebugToolMap("T: Damage held pawn to death", delegate
			{
				foreach (Thing item17 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).ToList())
				{
					Pawn pawn8 = item17 as Pawn;
					if (pawn8 != null && pawn8.carryTracker.CarriedThing != null && pawn8.carryTracker.CarriedThing is Pawn)
					{
						HealthUtility.DamageUntilDead((Pawn)pawn8.carryTracker.CarriedThing);
					}
				}
			});
			DebugToolMapForPawns("T: Surgery fail minor", delegate(Pawn p)
			{
				BodyPartRecord bodyPartRecord2 = (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x).RandomElement();
				Log.Message("part is " + bodyPartRecord2);
				HealthUtility.GiveInjuriesOperationFailureMinor(p, bodyPartRecord2);
			});
			DebugToolMapForPawns("T: Surgery fail catastrophic", delegate(Pawn p)
			{
				BodyPartRecord bodyPartRecord = (from x in p.health.hediffSet.GetNotMissingParts()
				where !x.def.conceptual
				select x).RandomElement();
				Log.Message("part is " + bodyPartRecord);
				HealthUtility.GiveInjuriesOperationFailureCatastrophic(p, bodyPartRecord);
			});
			DebugToolMapForPawns("T: Surgery fail ridiculous", delegate(Pawn p)
			{
				HealthUtility.GiveInjuriesOperationFailureRidiculous(p);
			});
			DebugToolMapForPawns("T: Restore body part...", delegate(Pawn p)
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RestorePart(p)));
			});
			DebugAction("T: Apply damage...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_ApplyDamage()));
			});
			DebugAction("T: Add Hediff...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_AddHediff()));
			});
			DebugToolMapForPawns("T: Remove Hediff...", delegate(Pawn p)
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugTools_Health.Options_RemoveHediff(p)));
			});
			DebugToolMapForPawns("T: Heal random injury (10)", delegate(Pawn p)
			{
				if ((from x in p.health.hediffSet.GetHediffs<Hediff_Injury>()
				where x.CanHealNaturally() || x.CanHealFromTending()
				select x).TryRandomElement(out Hediff_Injury result2))
				{
					result2.Heal(10f);
				}
			});
			HediffGiver localHdg;
			DebugToolMapForPawns("T: Activate HediffGiver", delegate(Pawn p)
			{
				Dialog_DebugActionsMenu dialog_DebugActionsMenu6 = this;
				List<FloatMenuOption> list29 = new List<FloatMenuOption>();
				if (p.RaceProps.hediffGiverSets != null)
				{
					foreach (HediffGiver item18 in p.RaceProps.hediffGiverSets.SelectMany((HediffGiverSetDef set) => set.hediffGivers))
					{
						localHdg = item18;
						list29.Add(new FloatMenuOption(localHdg.hediff.defName, delegate
						{
							localHdg.TryApply(p);
						}));
					}
				}
				if (list29.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list29));
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: Discover Hediffs", delegate(Pawn p)
			{
				foreach (Hediff hediff in p.health.hediffSet.hediffs)
				{
					if (!hediff.Visible)
					{
						hediff.Severity = Mathf.Max(hediff.Severity, hediff.def.stages.First((HediffStage s) => s.becomeVisible).minSeverity);
					}
				}
			});
			DebugToolMapForPawns("T: Grant immunities", delegate(Pawn p)
			{
				foreach (Hediff hediff2 in p.health.hediffSet.hediffs)
				{
					ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(hediff2.def);
					if (immunityRecord != null)
					{
						immunityRecord.immunity = 1f;
					}
				}
			});
			DebugToolMapForPawns("T: Give birth", delegate(Pawn p)
			{
				Hediff_Pregnant.DoBirthSpawn(p, null);
				DustPuffFrom(p);
			});
			DebugToolMapForPawns("T: Resistance -1", delegate(Pawn p)
			{
				if (p.guest != null && p.guest.resistance > 0f)
				{
					p.guest.resistance = Mathf.Max(0f, p.guest.resistance - 1f);
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: List melee verbs", delegate(Pawn p)
			{
				Log.Message(string.Format("Verb list (currently usable):\nNormal\n  {0}\nTerrain\n  {1}", (from verb in p.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: false)
				select verb.ToString()).ToLineList("  "), (from verb in p.meleeVerbs.GetUpdatedAvailableVerbsList(terrainTools: true)
				select verb.ToString()).ToLineList("  ")));
			});
			PawnRelationDef defLocal;
			Pawn otherLocal3;
			DebugToolMapForPawns("T: Add/remove pawn relation", delegate(Pawn p)
			{
				if (p.RaceProps.IsFlesh)
				{
					Action<bool> act3 = delegate(bool add)
					{
						if (add)
						{
							List<DebugMenuOption> list26 = new List<DebugMenuOption>();
							foreach (PawnRelationDef allDef in DefDatabase<PawnRelationDef>.AllDefs)
							{
								if (!allDef.implied)
								{
									defLocal = allDef;
									list26.Add(new DebugMenuOption(defLocal.defName, DebugMenuOptionMode.Action, delegate
									{
										List<DebugMenuOption> list28 = new List<DebugMenuOption>();
										IOrderedEnumerable<Pawn> orderedEnumerable = (from x in PawnsFinder.AllMapsWorldAndTemporary_Alive
										where x.RaceProps.IsFlesh
										orderby x.def == p.def descending
										select x).ThenBy(WorldPawnsUtility.IsWorldPawn);
										foreach (Pawn item19 in orderedEnumerable)
										{
											if (p != item19 && (!defLocal.familyByBloodRelation || item19.def == p.def) && !p.relations.DirectRelationExists(defLocal, item19))
											{
												otherLocal3 = item19;
												list28.Add(new DebugMenuOption(otherLocal3.LabelShort + " (" + otherLocal3.KindLabel + ")", DebugMenuOptionMode.Action, delegate
												{
													p.relations.AddDirectRelation(defLocal, otherLocal3);
												}));
											}
										}
										Find.WindowStack.Add(new Dialog_DebugOptionListLister(list28));
									}));
								}
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list26));
						}
						else
						{
							List<DebugMenuOption> list27 = new List<DebugMenuOption>();
							List<DirectPawnRelation> directRelations = p.relations.DirectRelations;
							for (int num15 = 0; num15 < directRelations.Count; num15++)
							{
								DirectPawnRelation rel = directRelations[num15];
								list27.Add(new DebugMenuOption(rel.def.defName + " - " + rel.otherPawn.LabelShort, DebugMenuOptionMode.Action, delegate
								{
									p.relations.RemoveDirectRelation(rel);
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list27));
						}
					};
					List<DebugMenuOption> options2 = new List<DebugMenuOption>
					{
						new DebugMenuOption("Add", DebugMenuOptionMode.Action, delegate
						{
							act3(obj: true);
						}),
						new DebugMenuOption("Remove", DebugMenuOptionMode.Action, delegate
						{
							act3(obj: false);
						})
					};
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(options2));
				}
			});
			DebugToolMapForPawns("T: Add opinion thoughts about", delegate(Pawn p)
			{
				if (p.RaceProps.Humanlike)
				{
					Action<bool> act2 = delegate(bool good)
					{
						foreach (Pawn item20 in from x in p.Map.mapPawns.AllPawnsSpawned
						where x.RaceProps.Humanlike
						select x)
						{
							if (p != item20)
							{
								IEnumerable<ThoughtDef> source3 = DefDatabase<ThoughtDef>.AllDefs.Where((ThoughtDef x) => typeof(Thought_MemorySocial).IsAssignableFrom(x.thoughtClass) && ((good && x.stages[0].baseOpinionOffset > 0f) || (!good && x.stages[0].baseOpinionOffset < 0f)));
								if (source3.Any())
								{
									int num13 = Rand.Range(2, 5);
									for (int num14 = 0; num14 < num13; num14++)
									{
										ThoughtDef def3 = source3.RandomElement();
										item20.needs.mood.thoughts.memories.TryGainMemory(def3, p);
									}
								}
							}
						}
					};
					List<DebugMenuOption> options = new List<DebugMenuOption>
					{
						new DebugMenuOption("Good", DebugMenuOptionMode.Action, delegate
						{
							act2(obj: true);
						}),
						new DebugMenuOption("Bad", DebugMenuOptionMode.Action, delegate
						{
							act2(obj: false);
						})
					};
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(options));
				}
			});
			DebugToolMapForPawns("T: Force vomit...", delegate(Pawn p)
			{
				p.jobs.StartJob(new Job(JobDefOf.Vomit), JobCondition.InterruptForced, null, resumeCurJobAfterwards: true);
			});
			DebugToolMap("T: Food -20%", delegate
			{
				OffsetNeed(NeedDefOf.Food, -0.2f);
			});
			DebugToolMap("T: Rest -20%", delegate
			{
				OffsetNeed(NeedDefOf.Rest, -0.2f);
			});
			DebugToolMap("T: Joy -20%", delegate
			{
				OffsetNeed(NeedDefOf.Joy, -0.2f);
			});
			DebugToolMap("T: Chemical -20%", delegate
			{
				List<NeedDef> allDefsListForReading3 = DefDatabase<NeedDef>.AllDefsListForReading;
				for (int num12 = 0; num12 < allDefsListForReading3.Count; num12++)
				{
					if (typeof(Need_Chemical).IsAssignableFrom(allDefsListForReading3[num12].needClass))
					{
						OffsetNeed(allDefsListForReading3[num12], -0.2f);
					}
				}
			});
			Dialog_DebugActionsMenu dialog_DebugActionsMenu5;
			SkillDef localDef6;
			DebugAction("T: Set skill", delegate
			{
				List<DebugMenuOption> list24 = new List<DebugMenuOption>();
				foreach (SkillDef allDef2 in DefDatabase<SkillDef>.AllDefs)
				{
					dialog_DebugActionsMenu5 = this;
					localDef6 = allDef2;
					list24.Add(new DebugMenuOption(localDef6.defName, DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list25 = new List<DebugMenuOption>();
						for (int num11 = 0; num11 <= 20; num11++)
						{
							int level = num11;
							list25.Add(new DebugMenuOption(level.ToString(), DebugMenuOptionMode.Tool, delegate
							{
								Pawn pawn7 = (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
								where t is Pawn
								select t).Cast<Pawn>().FirstOrDefault();
								if (pawn7 != null)
								{
									SkillRecord skill = pawn7.skills.GetSkill(localDef6);
									skill.Level = level;
									skill.xpSinceLastLevel = skill.XpRequiredForLevelUp / 2f;
									dialog_DebugActionsMenu5.DustPuffFrom(pawn7);
								}
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list25));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list24));
			});
			DebugToolMapForPawns("T: Max skills", delegate(Pawn p)
			{
				if (p.skills != null)
				{
					foreach (SkillDef allDef3 in DefDatabase<SkillDef>.AllDefs)
					{
						p.skills.Learn(allDef3, 1E+08f);
					}
					DustPuffFrom(p);
				}
				if (p.training != null)
				{
					foreach (TrainableDef allDef4 in DefDatabase<TrainableDef>.AllDefs)
					{
						Pawn trainer = p.Map.mapPawns.FreeColonistsSpawned.RandomElement();
						if (p.training.CanAssignToTrain(allDef4, out bool _).Accepted)
						{
							p.training.Train(allDef4, trainer);
						}
					}
				}
			});
			Dialog_DebugActionsMenu dialog_DebugActionsMenu4;
			MentalBreakDef locBrDef2;
			DebugAction("T: Mental break...", delegate
			{
				List<DebugMenuOption> list23 = new List<DebugMenuOption>
				{
					new DebugMenuOption("(log possibles)", DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item21 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
						{
							item21.mindState.mentalBreaker.LogPossibleMentalBreaks();
							DustPuffFrom(item21);
						}
					}),
					new DebugMenuOption("(natural mood break)", DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item22 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
						{
							item22.mindState.mentalBreaker.TryDoRandomMoodCausedMentalBreak();
							DustPuffFrom(item22);
						}
					})
				};
				foreach (MentalBreakDef item23 in from x in DefDatabase<MentalBreakDef>.AllDefs
				orderby x.intensity descending
				select x)
				{
					dialog_DebugActionsMenu4 = this;
					locBrDef2 = item23;
					string text4 = locBrDef2.defName;
					if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef2.Worker.BreakCanOccur(x)))
					{
						text4 += " [NO]";
					}
					list23.Add(new DebugMenuOption(text4, DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item24 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).Where((Thing t) => t is Pawn).Cast<Pawn>())
						{
							locBrDef2.Worker.TryStart(item24, null, causedByMood: false);
							dialog_DebugActionsMenu4.DustPuffFrom(item24);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list23));
			});
			Dialog_DebugActionsMenu dialog_DebugActionsMenu3;
			MentalStateDef locBrDef;
			Pawn locP;
			DebugAction("T: Mental state...", delegate
			{
				List<DebugMenuOption> list22 = new List<DebugMenuOption>();
				foreach (MentalStateDef allDef5 in DefDatabase<MentalStateDef>.AllDefs)
				{
					dialog_DebugActionsMenu3 = this;
					locBrDef = allDef5;
					string text3 = locBrDef.defName;
					if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => locBrDef.Worker.StateCanOccur(x)))
					{
						text3 += " [NO]";
					}
					list22.Add(new DebugMenuOption(text3, DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item25 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
						where t is Pawn
						select t).Cast<Pawn>())
						{
							locP = item25;
							if (locBrDef != MentalStateDefOf.SocialFighting)
							{
								locP.mindState.mentalStateHandler.TryStartMentalState(locBrDef, null, forceWake: true);
								dialog_DebugActionsMenu3.DustPuffFrom(locP);
							}
							else
							{
								DebugTools.curTool = new DebugTool("...with", delegate
								{
									Pawn pawn6 = (Pawn)(from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
									where t is Pawn
									select t).FirstOrDefault();
									if (pawn6 != null)
									{
										locP.interactions.StartSocialFight(pawn6);
										DebugTools.curTool = null;
									}
								});
							}
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list22));
			});
			Dialog_DebugActionsMenu dialog_DebugActionsMenu2;
			InspirationDef localDef5;
			DebugAction("T: Inspiration...", delegate
			{
				List<DebugMenuOption> list21 = new List<DebugMenuOption>();
				foreach (InspirationDef allDef6 in DefDatabase<InspirationDef>.AllDefs)
				{
					dialog_DebugActionsMenu2 = this;
					localDef5 = allDef6;
					string text2 = localDef5.defName;
					if (!Find.CurrentMap.mapPawns.FreeColonists.Any((Pawn x) => localDef5.Worker.InspirationCanOccur(x)))
					{
						text2 += " [NO]";
					}
					list21.Add(new DebugMenuOption(text2, DebugMenuOptionMode.Tool, delegate
					{
						foreach (Pawn item26 in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell()).OfType<Pawn>())
						{
							item26.mindState.inspirationHandler.TryStartInspiration(localDef5);
							dialog_DebugActionsMenu2.DustPuffFrom(item26);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list21));
			});
			Dialog_DebugActionsMenu dialog_DebugActionsMenu;
			TraitDef trDef;
			DebugAction("T: Give trait...", delegate
			{
				List<DebugMenuOption> list20 = new List<DebugMenuOption>();
				foreach (TraitDef allDef7 in DefDatabase<TraitDef>.AllDefs)
				{
					dialog_DebugActionsMenu = this;
					trDef = allDef7;
					for (int num10 = 0; num10 < allDef7.degreeDatas.Count; num10++)
					{
						int i2 = num10;
						list20.Add(new DebugMenuOption(trDef.degreeDatas[i2].label + " (" + trDef.degreeDatas[num10].degree + ")", DebugMenuOptionMode.Tool, delegate
						{
							foreach (Pawn item27 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
							where t is Pawn
							select t).Cast<Pawn>())
							{
								if (item27.story != null)
								{
									Trait item = new Trait(trDef, trDef.degreeDatas[i2].degree);
									item27.story.traits.allTraits.Add(item);
									dialog_DebugActionsMenu.DustPuffFrom(item27);
								}
							}
						}));
					}
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list20));
			});
			DebugToolMapForPawns("T: Give good thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugGood);
				}
			});
			DebugToolMapForPawns("T: Give bad thought", delegate(Pawn p)
			{
				if (p.needs.mood != null)
				{
					p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.DebugBad);
				}
			});
			DebugToolMap("T: Clear bound unfinished things", delegate
			{
				foreach (Building_WorkTable item28 in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
				where t is Building_WorkTable
				select t).Cast<Building_WorkTable>())
				{
					foreach (Bill item29 in item28.BillStack)
					{
						(item29 as Bill_ProductionWithUft)?.ClearBoundUft();
					}
				}
			});
			DebugToolMapForPawns("T: Force birthday", delegate(Pawn p)
			{
				p.ageTracker.AgeBiologicalTicks = (p.ageTracker.AgeBiologicalYears + 1) * 3600000 + 1;
				p.ageTracker.DebugForceBirthdayBiological();
			});
			DebugToolMapForPawns("T: Recruit", delegate(Pawn p)
			{
				if (p.Faction != Faction.OfPlayer && p.RaceProps.Humanlike)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.RandomElement(), p, 1f);
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: Damage apparel", delegate(Pawn p)
			{
				if (p.apparel != null && p.apparel.WornApparelCount > 0)
				{
					p.apparel.WornApparel.RandomElement().TakeDamage(new DamageInfo(DamageDefOf.Deterioration, 30f));
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: Tame animal", delegate(Pawn p)
			{
				if (p.AnimalOrWildMan() && p.Faction != Faction.OfPlayer)
				{
					InteractionWorker_RecruitAttempt.DoRecruit(p.Map.mapPawns.FreeColonists.FirstOrDefault(), p, 1f);
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: Train animal", delegate(Pawn p)
			{
				if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && p.training != null)
				{
					DustPuffFrom(p);
					bool flag = false;
					foreach (TrainableDef allDef8 in DefDatabase<TrainableDef>.AllDefs)
					{
						if (p.training.GetWanted(allDef8))
						{
							p.training.Train(allDef8, null, complete: true);
							flag = true;
						}
					}
					if (!flag)
					{
						foreach (TrainableDef allDef9 in DefDatabase<TrainableDef>.AllDefs)
						{
							if (p.training.CanAssignToTrain(allDef9).Accepted)
							{
								p.training.Train(allDef9, null, complete: true);
							}
						}
					}
				}
			});
			DebugToolMapForPawns("T: Name animal by nuzzling", delegate(Pawn p)
			{
				if ((p.Name == null || p.Name.Numerical) && p.RaceProps.Animal)
				{
					PawnUtility.GiveNameBecauseOfNuzzle(p.Map.mapPawns.FreeColonists.First(), p);
					DustPuffFrom(p);
				}
			});
			DebugToolMapForPawns("T: Try develop bond relation", delegate(Pawn p)
			{
				if (p.Faction != null)
				{
					if (p.RaceProps.Humanlike)
					{
						IEnumerable<Pawn> source = from x in p.Map.mapPawns.AllPawnsSpawned
						where x.RaceProps.Animal && x.Faction == p.Faction
						select x;
						if (source.Any())
						{
							RelationsUtility.TryDevelopBondRelation(p, source.RandomElement(), 999999f);
						}
					}
					else if (p.RaceProps.Animal)
					{
						IEnumerable<Pawn> source2 = from x in p.Map.mapPawns.AllPawnsSpawned
						where x.RaceProps.Humanlike && x.Faction == p.Faction
						select x;
						if (source2.Any())
						{
							RelationsUtility.TryDevelopBondRelation(source2.RandomElement(), p, 999999f);
						}
					}
				}
			});
			DebugToolMapForPawns("T: Queue training decay", delegate(Pawn p)
			{
				if (p.RaceProps.Animal && p.Faction == Faction.OfPlayer && p.training != null)
				{
					p.training.Debug_MakeDegradeHappenSoon();
					DustPuffFrom(p);
				}
			});
			Pawn otherLocal2;
			DebugToolMapForPawns("T: Start marriage ceremony", delegate(Pawn p)
			{
				if (p.RaceProps.Humanlike)
				{
					List<DebugMenuOption> list19 = new List<DebugMenuOption>();
					foreach (Pawn item30 in from x in p.Map.mapPawns.AllPawnsSpawned
					where x.RaceProps.Humanlike
					select x)
					{
						if (p != item30)
						{
							otherLocal2 = item30;
							list19.Add(new DebugMenuOption(otherLocal2.LabelShort + " (" + otherLocal2.KindLabel + ")", DebugMenuOptionMode.Action, delegate
							{
								if (!p.relations.DirectRelationExists(PawnRelationDefOf.Fiance, otherLocal2))
								{
									p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Lover, otherLocal2);
									p.relations.TryRemoveDirectRelation(PawnRelationDefOf.Spouse, otherLocal2);
									p.relations.AddDirectRelation(PawnRelationDefOf.Fiance, otherLocal2);
									Messages.Message("Dev: Auto added fiance relation.", p, MessageTypeDefOf.TaskCompletion, historical: false);
								}
								if (!p.Map.lordsStarter.TryStartMarriageCeremony(p, otherLocal2))
								{
									Messages.Message("Could not find any valid marriage site.", MessageTypeDefOf.RejectInput, historical: false);
								}
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list19));
				}
			});
			Pawn otherLocal;
			InteractionDef interactionLocal;
			DebugToolMapForPawns("T: Force interaction", delegate(Pawn p)
			{
				if (p.Faction != null)
				{
					List<DebugMenuOption> list17 = new List<DebugMenuOption>();
					foreach (Pawn item31 in p.Map.mapPawns.SpawnedPawnsInFaction(p.Faction))
					{
						if (item31 != p)
						{
							otherLocal = item31;
							list17.Add(new DebugMenuOption(otherLocal.LabelShort + " (" + otherLocal.KindLabel + ")", DebugMenuOptionMode.Action, delegate
							{
								List<DebugMenuOption> list18 = new List<DebugMenuOption>();
								foreach (InteractionDef item32 in DefDatabase<InteractionDef>.AllDefsListForReading)
								{
									interactionLocal = item32;
									list18.Add(new DebugMenuOption(interactionLocal.label, DebugMenuOptionMode.Action, delegate
									{
										p.interactions.TryInteractWith(otherLocal, interactionLocal);
									}));
								}
								Find.WindowStack.Add(new Dialog_DebugOptionListLister(list18));
							}));
						}
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list17));
				}
			});
			DebugAction("T: Start party", delegate
			{
				if (!Find.CurrentMap.lordsStarter.TryStartParty())
				{
					Messages.Message("Could not find any valid party spot or organizer.", MessageTypeDefOf.RejectInput, historical: false);
				}
			});
			DebugToolMapForPawns("T: Start prison break", delegate(Pawn p)
			{
				if (p.IsPrisoner)
				{
					PrisonBreakUtility.StartPrisonBreak(p);
				}
			});
			DebugToolMapForPawns("T: Pass to world", delegate(Pawn p)
			{
				p.DeSpawn();
				Find.WorldPawns.PassToWorld(p, PawnDiscardDecideMode.KeepForever);
			});
			DebugToolMapForPawns("T: Make 1 year older", delegate(Pawn p)
			{
				p.ageTracker.DebugMake1YearOlder();
			});
			DoGap();
			Type localType;
			DebugToolMapForPawns("T: Try job giver", delegate(Pawn p)
			{
				List<DebugMenuOption> list16 = new List<DebugMenuOption>();
				foreach (Type item33 in typeof(ThinkNode_JobGiver).AllSubclasses())
				{
					localType = item33;
					list16.Add(new DebugMenuOption(localType.Name, DebugMenuOptionMode.Action, delegate
					{
						ThinkNode_JobGiver thinkNode_JobGiver = (ThinkNode_JobGiver)Activator.CreateInstance(localType);
						thinkNode_JobGiver.ResolveReferences();
						ThinkResult thinkResult = thinkNode_JobGiver.TryIssueJobPackage(p, default(JobIssueParams));
						if (thinkResult.Job != null)
						{
							p.jobs.StartJob(thinkResult.Job);
						}
						else
						{
							Messages.Message("Failed to give job", MessageTypeDefOf.RejectInput, historical: false);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list16));
			});
			DebugToolMapForPawns("T: Try joy giver", delegate(Pawn p)
			{
				List<DebugMenuOption> list15 = new List<DebugMenuOption>();
				foreach (JoyGiverDef item34 in DefDatabase<JoyGiverDef>.AllDefsListForReading)
				{
					list15.Add(new DebugMenuOption(item34.defName, DebugMenuOptionMode.Action, delegate
					{
						Job job = item34.Worker.TryGiveJob(p);
						if (job != null)
						{
							p.jobs.StartJob(job, JobCondition.InterruptForced);
						}
						else
						{
							Messages.Message("Failed to give job", MessageTypeDefOf.RejectInput, historical: false);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list15));
			});
			DebugToolMapForPawns("T: EndCurrentJob(" + 5.ToString() + ")", delegate(Pawn p)
			{
				p.jobs.EndCurrentJob(JobCondition.InterruptForced);
				DustPuffFrom(p);
			});
			DebugToolMapForPawns("T: CheckForJobOverride", delegate(Pawn p)
			{
				p.jobs.CheckForJobOverride();
				DustPuffFrom(p);
			});
			DebugToolMapForPawns("T: Toggle job logging", delegate(Pawn p)
			{
				p.jobs.debugLog = !p.jobs.debugLog;
				DustPuffFrom(p);
				MoteMaker.ThrowText(p.DrawPos, p.Map, p.LabelShort + "\n" + ((!p.jobs.debugLog) ? "OFF" : "ON"));
			});
			DebugToolMapForPawns("T: Toggle stance logging", delegate(Pawn p)
			{
				p.stances.debugLog = !p.stances.debugLog;
				DustPuffFrom(p);
			});
			DoGap();
			DoLabel("Tools - Spawning");
			PawnKindDef localKindDef;
			DebugAction("T: Spawn pawn", delegate
			{
				List<DebugMenuOption> list14 = new List<DebugMenuOption>();
				foreach (PawnKindDef item35 in from kd in DefDatabase<PawnKindDef>.AllDefs
				orderby kd.defName
				select kd)
				{
					localKindDef = item35;
					list14.Add(new DebugMenuOption(localKindDef.defName, DebugMenuOptionMode.Tool, delegate
					{
						Faction faction = FactionUtility.DefaultFactionFrom(localKindDef.defaultFactionType);
						Pawn newPawn = PawnGenerator.GeneratePawn(localKindDef, faction);
						GenSpawn.Spawn(newPawn, UI.MouseCell(), Find.CurrentMap);
						if (faction != null && faction != Faction.OfPlayer)
						{
							Lord lord = null;
							if (newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction).Any((Pawn p) => p != newPawn))
							{
								Pawn p2 = (Pawn)GenClosest.ClosestThing_Global(newPawn.Position, newPawn.Map.mapPawns.SpawnedPawnsInFaction(faction), 99999f, (Thing p) => p != newPawn && ((Pawn)p).GetLord() != null);
								lord = p2.GetLord();
							}
							if (lord == null)
							{
								LordJob_DefendPoint lordJob = new LordJob_DefendPoint(newPawn.Position);
								lord = LordMaker.MakeNewLord(faction, lordJob, Find.CurrentMap);
							}
							lord.AddPawn(newPawn);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list14));
			});
			ThingDef localDef4;
			DebugAction("T: Spawn weapon...", delegate
			{
				List<DebugMenuOption> list13 = new List<DebugMenuOption>();
				foreach (ThingDef item36 in from def in DefDatabase<ThingDef>.AllDefs
				where def.equipmentType == EquipmentType.Primary
				select def into d
				orderby d.defName
				select d)
				{
					localDef4 = item36;
					list13.Add(new DebugMenuOption(localDef4.defName, DebugMenuOptionMode.Tool, delegate
					{
						DebugThingPlaceHelper.DebugSpawn(localDef4, UI.MouseCell());
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list13));
			});
			ThingDef localDef3;
			DebugAction("T: Spawn apparel...", delegate
			{
				List<DebugMenuOption> list12 = new List<DebugMenuOption>();
				foreach (ThingDef item37 in from def in DefDatabase<ThingDef>.AllDefs
				where def.IsApparel
				select def into d
				orderby d.defName
				select d)
				{
					localDef3 = item37;
					list12.Add(new DebugMenuOption(localDef3.defName, DebugMenuOptionMode.Tool, delegate
					{
						DebugThingPlaceHelper.DebugSpawn(localDef3, UI.MouseCell());
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list12));
			});
			DebugAction("T: Try place near thing...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, direct: false)));
			});
			DebugAction("T: Try place near stacks of 25...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, direct: false)));
			});
			DebugAction("T: Try place near stacks of 75...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(75, direct: false)));
			});
			DebugAction("T: Try place direct thing...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(1, direct: true)));
			});
			DebugAction("T: Try place direct stacks of 25...", delegate
			{
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.TryPlaceOptionsForStackCount(25, direct: true)));
			});
			DebugAction("T: Spawn thing with wipe mode...", delegate
			{
				List<DebugMenuOption> list11 = new List<DebugMenuOption>();
				WipeMode[] array5 = (WipeMode[])Enum.GetValues(typeof(WipeMode));
				for (int num9 = 0; num9 < array5.Length; num9++)
				{
					WipeMode wipeMode = array5[num9];
					WipeMode localWipeMode = wipeMode;
					list11.Add(new DebugMenuOption(wipeMode.ToString(), DebugMenuOptionMode.Action, delegate
					{
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(DebugThingPlaceHelper.SpawnOptions(localWipeMode)));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list11));
			});
			TerrainDef localDef2;
			DebugAction("T: Set terrain...", delegate
			{
				List<DebugMenuOption> list10 = new List<DebugMenuOption>();
				foreach (TerrainDef allDef10 in DefDatabase<TerrainDef>.AllDefs)
				{
					localDef2 = allDef10;
					list10.Add(new DebugMenuOption(localDef2.LabelCap, DebugMenuOptionMode.Tool, delegate
					{
						if (UI.MouseCell().InBounds(Find.CurrentMap))
						{
							Find.CurrentMap.terrainGrid.SetTerrain(UI.MouseCell(), localDef2);
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list10));
			});
			DebugToolMap("T: Make filth x100", delegate
			{
				for (int num8 = 0; num8 < 100; num8++)
				{
					IntVec3 c2 = UI.MouseCell() + GenRadial.RadialPattern[num8];
					if (c2.InBounds(Find.CurrentMap) && c2.Walkable(Find.CurrentMap))
					{
						FilthMaker.MakeFilth(c2, Find.CurrentMap, ThingDefOf.Filth_Dirt, 2);
						MoteMaker.ThrowMetaPuff(c2.ToVector3Shifted(), Find.CurrentMap);
					}
				}
			});
			Faction localFac;
			DebugToolMap("T: Spawn faction leader", delegate
			{
				List<FloatMenuOption> list9 = new List<FloatMenuOption>();
				foreach (Faction allFaction in Find.FactionManager.AllFactions)
				{
					localFac = allFaction;
					if (localFac.leader != null)
					{
						list9.Add(new FloatMenuOption(localFac.Name + " - " + localFac.leader.Name.ToStringFull, delegate
						{
							GenSpawn.Spawn(localFac.leader, UI.MouseCell(), Find.CurrentMap);
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list9));
			});
			PawnKindDef kLocal;
			Pawn pLocal;
			DebugAction("T: Spawn world pawn...", delegate
			{
				List<DebugMenuOption> list7 = new List<DebugMenuOption>();
				Action<Pawn> act = delegate(Pawn p)
				{
					List<DebugMenuOption> list8 = new List<DebugMenuOption>();
					foreach (PawnKindDef item38 in from x in DefDatabase<PawnKindDef>.AllDefs
					where x.race == p.def
					select x)
					{
						kLocal = item38;
						list8.Add(new DebugMenuOption(kLocal.defName, DebugMenuOptionMode.Tool, delegate
						{
							PawnGenerationRequest request = new PawnGenerationRequest(kLocal, p.Faction);
							PawnGenerator.RedressPawn(p, request);
							GenSpawn.Spawn(p, UI.MouseCell(), Find.CurrentMap);
							DebugTools.curTool = null;
						}));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list8));
				};
				foreach (Pawn item39 in Find.WorldPawns.AllPawnsAlive)
				{
					pLocal = item39;
					list7.Add(new DebugMenuOption(item39.LabelShort, DebugMenuOptionMode.Action, delegate
					{
						act(pLocal);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list7));
			});
			DebugAction("T: Spawn thing set...", delegate
			{
				List<DebugMenuOption> list5 = new List<DebugMenuOption>();
				List<ThingSetMakerDef> allDefsListForReading2 = DefDatabase<ThingSetMakerDef>.AllDefsListForReading;
				for (int num4 = 0; num4 < allDefsListForReading2.Count; num4++)
				{
					ThingSetMakerDef localGenerator = allDefsListForReading2[num4];
					list5.Add(new DebugMenuOption(localGenerator.defName, DebugMenuOptionMode.Tool, delegate
					{
						if (UI.MouseCell().InBounds(Find.CurrentMap))
						{
							StringBuilder stringBuilder2 = new StringBuilder();
							string nonNullFieldsDebugInfo = Gen.GetNonNullFieldsDebugInfo(localGenerator.debugParams);
							List<Thing> list6 = localGenerator.root.Generate(localGenerator.debugParams);
							stringBuilder2.Append(localGenerator.defName + " generated " + list6.Count + " things");
							if (!nonNullFieldsDebugInfo.NullOrEmpty())
							{
								stringBuilder2.Append(" (used custom debug params: " + nonNullFieldsDebugInfo + ")");
							}
							stringBuilder2.AppendLine(":");
							float num5 = 0f;
							float num6 = 0f;
							for (int num7 = 0; num7 < list6.Count; num7++)
							{
								stringBuilder2.AppendLine("   - " + list6[num7].LabelCap);
								num5 += list6[num7].MarketValue * (float)list6[num7].stackCount;
								if (!(list6[num7] is Pawn))
								{
									num6 += list6[num7].GetStatValue(StatDefOf.Mass) * (float)list6[num7].stackCount;
								}
								if (!GenPlace.TryPlaceThing(list6[num7], UI.MouseCell(), Find.CurrentMap, ThingPlaceMode.Near))
								{
									list6[num7].Destroy();
								}
							}
							stringBuilder2.AppendLine("Total market value: " + num5.ToString("0.##"));
							stringBuilder2.AppendLine("Total mass: " + num6.ToStringMass());
							Log.Message(stringBuilder2.ToString());
						}
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list5));
			});
			DebugAction("T: Trigger effecter...", delegate
			{
				List<DebugMenuOption> list4 = new List<DebugMenuOption>();
				List<EffecterDef> allDefsListForReading = DefDatabase<EffecterDef>.AllDefsListForReading;
				for (int num3 = 0; num3 < allDefsListForReading.Count; num3++)
				{
					EffecterDef localDef = allDefsListForReading[num3];
					list4.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Tool, delegate
					{
						Effecter effecter = localDef.Spawn();
						effecter.Trigger(new TargetInfo(UI.MouseCell(), Find.CurrentMap), new TargetInfo(UI.MouseCell(), Find.CurrentMap));
						effecter.Cleanup();
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
			});
			DoGap();
			DoLabel("Autotests");
			DebugAction("Make colony (full)", delegate
			{
				Autotests_ColonyMaker.MakeColony_Full();
			});
			DebugAction("Make colony (animals)", delegate
			{
				Autotests_ColonyMaker.MakeColony_Animals();
			});
			DebugAction("Test force downed x100", delegate
			{
				int num2 = 0;
				Pawn pawn5;
				while (true)
				{
					if (num2 >= 100)
					{
						return;
					}
					PawnKindDef random5 = DefDatabase<PawnKindDef>.GetRandom();
					pawn5 = PawnGenerator.GeneratePawn(random5, FactionUtility.DefaultFactionFrom(random5.defaultFactionType));
					GenSpawn.Spawn(pawn5, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					HealthUtility.DamageUntilDowned(pawn5);
					if (pawn5.Dead)
					{
						break;
					}
					num2++;
				}
				Log.Error("Pawn died while force downing: " + pawn5 + " at " + pawn5.Position);
			});
			DebugAction("Test force kill x100", delegate
			{
				int num = 0;
				Pawn pawn4;
				while (true)
				{
					if (num >= 100)
					{
						return;
					}
					PawnKindDef random4 = DefDatabase<PawnKindDef>.GetRandom();
					pawn4 = PawnGenerator.GeneratePawn(random4, FactionUtility.DefaultFactionFrom(random4.defaultFactionType));
					GenSpawn.Spawn(pawn4, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					HealthUtility.DamageUntilDead(pawn4);
					if (!pawn4.Dead)
					{
						break;
					}
					num++;
				}
				Log.Error("Pawn died not die: " + pawn4 + " at " + pawn4.Position);
			});
			DebugAction("Test Surgery fail catastrophic x100", delegate
			{
				for (int n = 0; n < 100; n++)
				{
					PawnKindDef random3 = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn3 = PawnGenerator.GeneratePawn(random3, FactionUtility.DefaultFactionFrom(random3.defaultFactionType));
					GenSpawn.Spawn(pawn3, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					pawn3.health.forceIncap = true;
					BodyPartRecord part = pawn3.health.hediffSet.GetNotMissingParts().RandomElement();
					HealthUtility.GiveInjuriesOperationFailureCatastrophic(pawn3, part);
					pawn3.health.forceIncap = false;
					if (pawn3.Dead)
					{
						Log.Error("Pawn died: " + pawn3 + " at " + pawn3.Position);
					}
				}
			});
			DebugAction("Test Surgery fail ridiculous x100", delegate
			{
				for (int m = 0; m < 100; m++)
				{
					PawnKindDef random2 = DefDatabase<PawnKindDef>.GetRandom();
					Pawn pawn2 = PawnGenerator.GeneratePawn(random2, FactionUtility.DefaultFactionFrom(random2.defaultFactionType));
					GenSpawn.Spawn(pawn2, CellFinderLoose.RandomCellWith((IntVec3 c) => c.Standable(Find.CurrentMap), Find.CurrentMap), Find.CurrentMap);
					pawn2.health.forceIncap = true;
					HealthUtility.GiveInjuriesOperationFailureRidiculous(pawn2);
					pawn2.health.forceIncap = false;
					if (pawn2.Dead)
					{
						Log.Error("Pawn died: " + pawn2 + " at " + pawn2.Position);
					}
				}
			});
			DebugAction("Test generate pawn x1000", delegate
			{
				float[] array3 = new float[10]
				{
					10f,
					20f,
					50f,
					100f,
					200f,
					500f,
					1000f,
					2000f,
					5000f,
					1E+20f
				};
				int[] array4 = new int[array3.Length];
				for (int j = 0; j < 1000; j++)
				{
					PawnKindDef random = DefDatabase<PawnKindDef>.GetRandom();
					PerfLogger.Reset();
					Pawn pawn = PawnGenerator.GeneratePawn(random, FactionUtility.DefaultFactionFrom(random.defaultFactionType));
					float ms = PerfLogger.Duration() * 1000f;
					array4[array3.FirstIndexOf((float time) => ms <= time)]++;
					if (pawn.Dead)
					{
						Log.Error("Pawn is dead");
					}
					Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Pawn creation time histogram:");
				for (int l = 0; l < array4.Length; l++)
				{
					stringBuilder.AppendLine($"<{array3[l]}ms: {array4[l]}");
				}
				Log.Message(stringBuilder.ToString());
			});
			DebugAction("Check region listers", delegate
			{
				Autotests_RegionListers.CheckBugs(Find.CurrentMap);
			});
			DebugAction("Test time-to-down", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (PawnKindDef item40 in from kd in DefDatabase<PawnKindDef>.AllDefs
				orderby kd.defName
				select kd)
				{
					list.Add(new DebugMenuOption(item40.label, DebugMenuOptionMode.Action, delegate
					{
						if (item40 == PawnKindDefOf.Colonist)
						{
							Log.Message("Current colonist TTD reference point: 22.3 seconds, stddev 8.35 seconds");
						}
						List<float> results = new List<float>();
						List<PawnKindDef> list2 = new List<PawnKindDef>();
						List<PawnKindDef> list3 = new List<PawnKindDef>();
						list2.Add(item40);
						list3.Add(item40);
						ArenaUtility.BeginArenaFightSet(1000, list2, list3, delegate(ArenaUtility.ArenaResult result)
						{
							if (result.winner != 0)
							{
								results.Add(result.tickDuration.TicksToSeconds());
							}
						}, delegate
						{
							Log.Message($"Finished {results.Count} tests; time-to-down {results.Average()}, stddev {GenMath.Stddev(results)}\n\nraw: {results.Select((float res) => res.ToString()).ToLineList()}");
						});
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
			DebugAction("Battle Royale All PawnKinds", delegate
			{
				ArenaUtility.PerformBattleRoyale(DefDatabase<PawnKindDef>.AllDefs);
			});
			DebugAction("Battle Royale Humanlikes", delegate
			{
				ArenaUtility.PerformBattleRoyale(from k in DefDatabase<PawnKindDef>.AllDefs
				where k.RaceProps.Humanlike
				select k);
			});
			DebugAction("Battle Royale by Damagetype", delegate
			{
				PawnKindDef[] array = new PawnKindDef[2]
				{
					PawnKindDefOf.Colonist,
					PawnKindDefOf.Muffalo
				};
				IEnumerable<ToolCapacityDef> enumerable = from tc in DefDatabase<ToolCapacityDef>.AllDefsListForReading
				where tc != ToolCapacityDefOf.KickMaterialInEyes
				select tc;
				Func<PawnKindDef, ToolCapacityDef, string> func = (PawnKindDef pkd, ToolCapacityDef dd) => $"{pkd.label}_{dd.defName}";
				if (pawnKindsForDamageTypeBattleRoyale == null)
				{
					pawnKindsForDamageTypeBattleRoyale = new List<PawnKindDef>();
					PawnKindDef[] array2 = array;
					foreach (PawnKindDef pawnKindDef in array2)
					{
						foreach (ToolCapacityDef item41 in enumerable)
						{
							string text = func(pawnKindDef, item41);
							ThingDef thingDef = Gen.MemberwiseClone(pawnKindDef.race);
							thingDef.defName = text;
							thingDef.label = text;
							thingDef.tools = new List<Tool>(pawnKindDef.race.tools.Select(delegate(Tool tool)
							{
								Tool tool2 = Gen.MemberwiseClone(tool);
								tool2.capacities = new List<ToolCapacityDef>();
								tool2.capacities.Add(item41);
								return tool2;
							}));
							PawnKindDef pawnKindDef2 = Gen.MemberwiseClone(pawnKindDef);
							pawnKindDef2.defName = text;
							pawnKindDef2.label = text;
							pawnKindDef2.race = thingDef;
							pawnKindsForDamageTypeBattleRoyale.Add(pawnKindDef2);
						}
					}
				}
				ArenaUtility.PerformBattleRoyale(pawnKindsForDamageTypeBattleRoyale);
			});
		}

		private void DoListingItems_World()
		{
			DoLabel("Tools - World");
			Text.Font = GameFont.Tiny;
			DoLabel("Incidents");
			IIncidentTarget incidentTarget = Find.WorldSelector.SingleSelectedObject as IIncidentTarget;
			if (incidentTarget == null)
			{
				incidentTarget = Find.CurrentMap;
			}
			if (incidentTarget != null)
			{
				DoIncidentDebugAction(incidentTarget);
				DoIncidentWithPointsAction(incidentTarget);
			}
			DoIncidentDebugAction(Find.World);
			DoIncidentWithPointsAction(Find.World);
			DoLabel("Tools - Spawning");
			DebugToolWorld("Spawn random caravan", delegate
			{
				int num2 = GenWorld.MouseTile();
				Tile tile6 = Find.WorldGrid[num2];
				if (!tile6.biome.impassable)
				{
					List<Pawn> list8 = new List<Pawn>();
					int num3 = Rand.RangeInclusive(1, 10);
					for (int i = 0; i < num3; i++)
					{
						Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
						list8.Add(pawn);
						if (!pawn.story.WorkTagIsDisabled(WorkTags.Violent) && Rand.Value < 0.9f)
						{
							ThingDef thingDef = (from def in DefDatabase<ThingDef>.AllDefs
							where def.IsWeapon && def.PlayerAcquirable
							select def).RandomElementWithFallback();
							pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(thingDef, GenStuff.RandomStuffFor(thingDef)));
						}
					}
					int num4 = Rand.RangeInclusive(-4, 10);
					for (int j = 0; j < num4; j++)
					{
						PawnKindDef kindDef = (from d in DefDatabase<PawnKindDef>.AllDefs
						where d.RaceProps.Animal && d.RaceProps.wildness < 1f
						select d).RandomElement();
						Pawn item = PawnGenerator.GeneratePawn(kindDef, Faction.OfPlayer);
						list8.Add(item);
					}
					Caravan caravan = CaravanMaker.MakeCaravan(list8, Faction.OfPlayer, num2, addToWorldPawnsIfNotAlready: true);
					List<Thing> list9 = ThingSetMakerDefOf.DebugCaravanInventory.root.Generate();
					for (int k = 0; k < list9.Count; k++)
					{
						Thing thing = list9[k];
						if (thing.GetStatValue(StatDefOf.Mass) * (float)thing.stackCount > caravan.MassCapacity - caravan.MassUsage)
						{
							break;
						}
						CaravanInventoryUtility.GiveThing(caravan, thing);
					}
				}
			});
			DebugToolWorld("Spawn random faction base", delegate
			{
				if ((from x in Find.FactionManager.AllFactions
				where !x.IsPlayer && !x.def.hidden
				select x).TryRandomElement(out Faction result))
				{
					int num = GenWorld.MouseTile();
					Tile tile5 = Find.WorldGrid[num];
					if (!tile5.biome.impassable)
					{
						Settlement settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
						settlement.SetFaction(result);
						settlement.Tile = num;
						settlement.Name = SettlementNameGenerator.GenerateSettlementName(settlement);
						Find.WorldObjects.Add(settlement);
					}
				}
			});
			SiteCoreDef localDef3;
			Action addPart2;
			SitePartDef localPart2;
			DebugToolWorld("Spawn site", delegate
			{
				int tile4 = GenWorld.MouseTile();
				if (tile4 < 0 || Find.World.Impassable(tile4))
				{
					Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					List<DebugMenuOption> list6 = new List<DebugMenuOption>();
					List<SitePartDef> parts2 = new List<SitePartDef>();
					foreach (SiteCoreDef allDef in DefDatabase<SiteCoreDef>.AllDefs)
					{
						localDef3 = allDef;
						addPart2 = null;
						addPart2 = delegate
						{
							List<DebugMenuOption> list7 = new List<DebugMenuOption>
							{
								new DebugMenuOption("-Done (" + parts2.Count + " parts)-", DebugMenuOptionMode.Action, delegate
								{
									Site site2 = SiteMaker.TryMakeSite(localDef3, parts2, tile4);
									if (site2 == null)
									{
										Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, historical: false);
									}
									else
									{
										Find.WorldObjects.Add(site2);
									}
								})
							};
							foreach (SitePartDef allDef2 in DefDatabase<SitePartDef>.AllDefs)
							{
								localPart2 = allDef2;
								list7.Add(new DebugMenuOption(allDef2.defName, DebugMenuOptionMode.Action, delegate
								{
									parts2.Add(localPart2);
									addPart2();
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list7));
						};
						list6.Add(new DebugMenuOption(localDef3.defName, DebugMenuOptionMode.Action, addPart2));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list6));
				}
			});
			SiteCoreDef localDef2;
			Action addPart;
			float localPoints;
			SitePartDef localPart;
			DebugToolWorld("Spawn site with points", delegate
			{
				int tile2 = GenWorld.MouseTile();
				if (tile2 < 0 || Find.World.Impassable(tile2))
				{
					Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					List<DebugMenuOption> list3 = new List<DebugMenuOption>();
					List<SitePartDef> parts = new List<SitePartDef>();
					foreach (SiteCoreDef allDef3 in DefDatabase<SiteCoreDef>.AllDefs)
					{
						localDef2 = allDef3;
						addPart = null;
						addPart = delegate
						{
							List<DebugMenuOption> list4 = new List<DebugMenuOption>
							{
								new DebugMenuOption("-Done (" + parts.Count + " parts)-", DebugMenuOptionMode.Action, delegate
								{
									List<DebugMenuOption> list5 = new List<DebugMenuOption>();
									foreach (float item2 in PointsOptions(extended: true))
									{
										localPoints = item2;
										list5.Add(new DebugMenuOption(item2.ToString("F0"), DebugMenuOptionMode.Action, delegate
										{
											SiteCoreDef core = localDef2;
											List<SitePartDef> siteParts = parts;
											int tile3 = tile2;
											float? threatPoints = localPoints;
											Site site = SiteMaker.TryMakeSite(core, siteParts, tile3, disallowNonHostileFactions: true, null, ifHostileThenMustRemainHostile: true, threatPoints);
											if (site == null)
											{
												Messages.Message("Could not find any valid faction for this site.", MessageTypeDefOf.RejectInput, historical: false);
											}
											else
											{
												Find.WorldObjects.Add(site);
											}
										}));
									}
									Find.WindowStack.Add(new Dialog_DebugOptionListLister(list5));
								})
							};
							foreach (SitePartDef allDef4 in DefDatabase<SitePartDef>.AllDefs)
							{
								localPart = allDef4;
								list4.Add(new DebugMenuOption(allDef4.defName, DebugMenuOptionMode.Action, delegate
								{
									parts.Add(localPart);
									addPart();
								}));
							}
							Find.WindowStack.Add(new Dialog_DebugOptionListLister(list4));
						};
						list3.Add(new DebugMenuOption(localDef2.defName, DebugMenuOptionMode.Action, addPart));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list3));
				}
			});
			WorldObjectDef localDef;
			DebugToolWorld("Spawn world object", delegate
			{
				int tile = GenWorld.MouseTile();
				if (tile < 0 || Find.World.Impassable(tile))
				{
					Messages.Message("Impassable", MessageTypeDefOf.RejectInput, historical: false);
				}
				else
				{
					List<DebugMenuOption> list2 = new List<DebugMenuOption>();
					foreach (WorldObjectDef allDef5 in DefDatabase<WorldObjectDef>.AllDefs)
					{
						localDef = allDef5;
						Action action = null;
						action = delegate
						{
							WorldObject worldObject = WorldObjectMaker.MakeWorldObject(localDef);
							worldObject.Tile = tile;
							Find.WorldObjects.Add(worldObject);
						};
						list2.Add(new DebugMenuOption(localDef.defName, DebugMenuOptionMode.Action, action));
					}
					Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
				}
			});
			DoLabel("Tools - Misc");
			Type localType;
			DebugAction("Change camera config...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (Type item3 in typeof(WorldCameraConfig).AllSubclasses())
				{
					localType = item3;
					string text = localType.Name;
					if (text.StartsWith("WorldCameraConfig_"))
					{
						text = text.Substring("WorldCameraConfig_".Length);
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						Find.WorldCameraDriver.config = (WorldCameraConfig)Activator.CreateInstance(localType);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private void DoRaid(IncidentParms parms)
		{
			IncidentDef incidentDef = (!parms.faction.HostileTo(Faction.OfPlayer)) ? IncidentDefOf.RaidFriendly : IncidentDefOf.RaidEnemy;
			incidentDef.Worker.TryExecute(parms);
		}

		private void DoIncidentDebugAction(IIncidentTarget target)
		{
			IncidentDef localDef;
			IncidentParms parms;
			DebugAction("Do incident (" + GetIncidentTargetLabel(target) + ")...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef item in from d in DefDatabase<IncidentDef>.AllDefs
				where d.TargetAllowed(target)
				orderby d.defName
				select d)
				{
					localDef = item;
					string text = localDef.defName;
					parms = StorytellerUtility.DefaultParmsNow(localDef.category, target);
					if (!localDef.Worker.CanFireNow(parms))
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						if (localDef.pointsScaleable)
						{
							StorytellerComp storytellerComp = Find.Storyteller.storytellerComps.First((StorytellerComp x) => x is StorytellerComp_OnOffCycle || x is StorytellerComp_RandomMain);
							parms = storytellerComp.GenerateParms(localDef.category, parms.target);
						}
						localDef.Worker.TryExecute(parms);
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private void DoIncidentWithPointsAction(IIncidentTarget target)
		{
			IncidentDef localDef;
			IncidentParms parms;
			float localPoints;
			DebugAction("Do incident w/ points (" + GetIncidentTargetLabel(target) + ")...", delegate
			{
				List<DebugMenuOption> list = new List<DebugMenuOption>();
				foreach (IncidentDef item in from d in DefDatabase<IncidentDef>.AllDefs
				where d.TargetAllowed(target) && d.pointsScaleable
				orderby d.defName
				select d)
				{
					localDef = item;
					string text = localDef.defName;
					parms = StorytellerUtility.DefaultParmsNow(localDef.category, target);
					if (!localDef.Worker.CanFireNow(parms))
					{
						text += " [NO]";
					}
					list.Add(new DebugMenuOption(text, DebugMenuOptionMode.Action, delegate
					{
						List<DebugMenuOption> list2 = new List<DebugMenuOption>();
						foreach (float item2 in PointsOptions(extended: true))
						{
							localPoints = item2;
							list2.Add(new DebugMenuOption(item2 + " points", DebugMenuOptionMode.Action, delegate
							{
								parms.points = localPoints;
								localDef.Worker.TryExecute(parms);
							}));
						}
						Find.WindowStack.Add(new Dialog_DebugOptionListLister(list2));
					}));
				}
				Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
			});
		}

		private string GetIncidentTargetLabel(IIncidentTarget target)
		{
			if (target == null)
			{
				return "null target";
			}
			if (target is Map)
			{
				return "Map";
			}
			if (target is World)
			{
				return "World";
			}
			if (target is Caravan)
			{
				return ((Caravan)target).LabelCap;
			}
			return target.ToString();
		}

		private void DebugGiveResource(ThingDef resDef, int count)
		{
			Pawn pawn = Find.CurrentMap.mapPawns.FreeColonistsSpawned.RandomElement();
			int num = count;
			int num2 = 0;
			while (num > 0)
			{
				int num3 = Math.Min(resDef.stackLimit, num);
				num -= num3;
				Thing thing = ThingMaker.MakeThing(resDef);
				thing.stackCount = num3;
				if (!GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near))
				{
					break;
				}
				num2 += num3;
			}
			Messages.Message("Made " + num2 + " " + resDef + " near " + pawn + ".", MessageTypeDefOf.TaskCompletion, historical: false);
		}

		private void OffsetNeed(NeedDef nd, float offsetPct)
		{
			foreach (Pawn item in (from t in Find.CurrentMap.thingGrid.ThingsAt(UI.MouseCell())
			where t is Pawn
			select t).Cast<Pawn>())
			{
				Need need = item.needs.TryGetNeed(nd);
				if (need != null)
				{
					need.CurLevel += offsetPct * need.MaxLevel;
					DustPuffFrom(item);
				}
			}
		}

		private void DustPuffFrom(Thing t)
		{
			(t as Pawn)?.Drawer.Notify_DebugAffected();
		}

		private void AddGuest(bool prisoner)
		{
			foreach (Building_Bed item in Find.CurrentMap.listerBuildings.AllBuildingsColonistOfClass<Building_Bed>())
			{
				if (item.ForPrisoners == prisoner && (!item.owners.Any() || (prisoner && item.AnyUnownedSleepingSlot)))
				{
					PawnKindDef pawnKindDef = prisoner ? (from pk in DefDatabase<PawnKindDef>.AllDefs
					where pk.defaultFactionType != null && !pk.defaultFactionType.isPlayer && pk.RaceProps.Humanlike
					select pk).RandomElement() : PawnKindDefOf.SpaceRefugee;
					Faction faction = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
					Pawn pawn = PawnGenerator.GeneratePawn(pawnKindDef, faction);
					GenSpawn.Spawn(pawn, item.Position, Find.CurrentMap);
					foreach (ThingWithComps item2 in pawn.equipment.AllEquipmentListForReading.ToList())
					{
						if (pawn.equipment.TryDropEquipment(item2, out ThingWithComps resultingEq, pawn.Position))
						{
							resultingEq.Destroy();
						}
					}
					pawn.inventory.innerContainer.Clear();
					pawn.ownership.ClaimBedIfNonMedical(item);
					pawn.guest.SetGuestStatus(Faction.OfPlayer, prisoner);
					break;
				}
			}
		}

		public static IEnumerable<float> PointsOptions(bool extended)
		{
			if (!extended)
			{
				yield return 35f;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			int l = 20;
			if (l < 100)
			{
				yield return (float)l;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			int k = 100;
			if (k < 500)
			{
				yield return (float)k;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			int j = 500;
			if (j < 1500)
			{
				yield return (float)j;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			int i = 1500;
			if (i <= 5000)
			{
				yield return (float)i;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
