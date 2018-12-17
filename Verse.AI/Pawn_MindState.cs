using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace Verse.AI
{
	public class Pawn_MindState : IExposable
	{
		public Pawn pawn;

		public MentalStateHandler mentalStateHandler;

		public MentalBreaker mentalBreaker;

		public InspirationHandler inspirationHandler;

		public PriorityWork priorityWork;

		private bool activeInt = true;

		public JobTag lastJobTag;

		public int lastIngestTick = -99999;

		public int nextApparelOptimizeTick = -99999;

		public ThinkNode lastJobGiver;

		public ThinkTreeDef lastJobGiverThinkTree;

		public WorkTypeDef lastGivenWorkType;

		public bool canFleeIndividual = true;

		public int exitMapAfterTick = -99999;

		public int lastDisturbanceTick = -99999;

		public IntVec3 forcedGotoPosition = IntVec3.Invalid;

		public Thing knownExploder;

		public bool wantsToTradeWithColony;

		public Thing lastMannedThing;

		public int canLovinTick = -99999;

		public int canSleepTick = -99999;

		public Pawn meleeThreat;

		public int lastMeleeThreatHarmTick = -99999;

		public int lastEngageTargetTick = -99999;

		public int lastAttackTargetTick = -99999;

		public LocalTargetInfo lastAttackedTarget;

		public Thing enemyTarget;

		public PawnDuty duty;

		public Dictionary<int, int> thinkData = new Dictionary<int, int>();

		public int lastAssignedInteractTime = -99999;

		public int interactionsToday;

		public int lastInventoryRawFoodUseTick;

		public bool nextMoveOrderIsWait;

		public int lastTakeCombatEnhancingDrugTick = -99999;

		public int lastHarmTick = -99999;

		public bool anyCloseHostilesRecently;

		public int applyBedThoughtsTick;

		public bool applyBedThoughtsOnLeave;

		public bool willJoinColonyIfRescuedInt;

		private bool wildManEverReachedOutsideInt;

		public int timesGuestTendedToByPlayer;

		public int lastSelfTendTick = -99999;

		public bool spawnedByInfestationThingComp;

		public int lastPredatorHuntingPlayerNotificationTick = -99999;

		public float maxDistToSquadFlag = -1f;

		private int lastJobGiverKey = -1;

		private const int UpdateAnyCloseHostilesRecentlyEveryTicks = 100;

		private const int AnyCloseHostilesRecentlyRegionsToScan_ToActivate = 18;

		private const int AnyCloseHostilesRecentlyRegionsToScan_ToDeactivate = 24;

		private const float HarmForgetDistance = 3f;

		private const int MeleeHarmForgetDelay = 400;

		public bool Active
		{
			get
			{
				return activeInt;
			}
			set
			{
				if (value != activeInt)
				{
					activeInt = value;
					if (pawn.Spawned)
					{
						pawn.Map.mapPawns.UpdateRegistryForPawn(pawn);
					}
				}
			}
		}

		public bool IsIdle
		{
			get
			{
				if (pawn.Downed)
				{
					return false;
				}
				if (!pawn.Spawned)
				{
					return false;
				}
				return lastJobTag == JobTag.Idle;
			}
		}

		public bool MeleeThreatStillThreat => meleeThreat != null && meleeThreat.Spawned && !meleeThreat.Downed && pawn.Spawned && Find.TickManager.TicksGame <= lastMeleeThreatHarmTick + 400 && (float)(pawn.Position - meleeThreat.Position).LengthHorizontalSquared <= 9f && GenSight.LineOfSight(pawn.Position, meleeThreat.Position, pawn.Map);

		public bool WildManEverReachedOutside
		{
			get
			{
				return wildManEverReachedOutsideInt;
			}
			set
			{
				if (wildManEverReachedOutsideInt != value)
				{
					wildManEverReachedOutsideInt = value;
					ReachabilityUtility.ClearCacheFor(pawn);
				}
			}
		}

		public bool WillJoinColonyIfRescued
		{
			get
			{
				return willJoinColonyIfRescuedInt;
			}
			set
			{
				if (willJoinColonyIfRescuedInt != value)
				{
					willJoinColonyIfRescuedInt = value;
					if (pawn.Spawned)
					{
						pawn.Map.attackTargetsCache.UpdateTarget(pawn);
					}
				}
			}
		}

		public bool AnythingPreventsJoiningColonyIfRescued
		{
			get
			{
				if (pawn.Faction == Faction.OfPlayer)
				{
					return true;
				}
				if (pawn.IsPrisoner && !pawn.HostFaction.HostileTo(Faction.OfPlayer))
				{
					return true;
				}
				if (!pawn.IsPrisoner && pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer) && !pawn.Downed)
				{
					return true;
				}
				return false;
			}
		}

		public Pawn_MindState()
		{
		}

		public Pawn_MindState(Pawn pawn)
		{
			this.pawn = pawn;
			mentalStateHandler = new MentalStateHandler(pawn);
			mentalBreaker = new MentalBreaker(pawn);
			inspirationHandler = new InspirationHandler(pawn);
			priorityWork = new PriorityWork(pawn);
		}

		public void Reset(bool clearInspiration = false)
		{
			mentalStateHandler.Reset();
			mentalBreaker.Reset();
			if (clearInspiration)
			{
				inspirationHandler.Reset();
			}
			activeInt = true;
			lastJobTag = JobTag.Misc;
			lastIngestTick = -99999;
			nextApparelOptimizeTick = -99999;
			lastJobGiver = null;
			lastJobGiverThinkTree = null;
			lastGivenWorkType = null;
			canFleeIndividual = true;
			exitMapAfterTick = -99999;
			lastDisturbanceTick = -99999;
			forcedGotoPosition = IntVec3.Invalid;
			knownExploder = null;
			wantsToTradeWithColony = false;
			lastMannedThing = null;
			canLovinTick = -99999;
			canSleepTick = -99999;
			meleeThreat = null;
			lastMeleeThreatHarmTick = -99999;
			lastEngageTargetTick = -99999;
			lastAttackTargetTick = -99999;
			lastAttackedTarget = LocalTargetInfo.Invalid;
			enemyTarget = null;
			duty = null;
			thinkData.Clear();
			lastAssignedInteractTime = -99999;
			interactionsToday = 0;
			lastInventoryRawFoodUseTick = 0;
			priorityWork.Clear();
			nextMoveOrderIsWait = true;
			lastTakeCombatEnhancingDrugTick = -99999;
			lastHarmTick = -99999;
			anyCloseHostilesRecently = false;
			WillJoinColonyIfRescued = false;
			WildManEverReachedOutside = false;
			timesGuestTendedToByPlayer = 0;
			lastSelfTendTick = -99999;
			spawnedByInfestationThingComp = false;
			lastPredatorHuntingPlayerNotificationTick = -99999;
		}

		public void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				lastJobGiverKey = ((lastJobGiver == null) ? (-1) : lastJobGiver.UniqueSaveKey);
			}
			Scribe_Values.Look(ref lastJobGiverKey, "lastJobGiverKey", -1);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && lastJobGiverKey != -1 && !lastJobGiverThinkTree.TryGetThinkNodeWithSaveKey(lastJobGiverKey, out lastJobGiver))
			{
				Log.Warning("Could not find think node with key " + lastJobGiverKey);
			}
			Scribe_References.Look(ref meleeThreat, "meleeThreat");
			Scribe_References.Look(ref enemyTarget, "enemyTarget");
			Scribe_References.Look(ref knownExploder, "knownExploder");
			Scribe_References.Look(ref lastMannedThing, "lastMannedThing");
			Scribe_Defs.Look(ref lastJobGiverThinkTree, "lastJobGiverThinkTree");
			Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
			Scribe_Collections.Look(ref thinkData, "thinkData", LookMode.Value, LookMode.Value);
			Scribe_Values.Look(ref activeInt, "active", defaultValue: true);
			Scribe_Values.Look(ref lastJobTag, "lastJobTag", JobTag.Misc);
			Scribe_Values.Look(ref lastIngestTick, "lastIngestTick", -99999);
			Scribe_Values.Look(ref nextApparelOptimizeTick, "nextApparelOptimizeTick", -99999);
			Scribe_Values.Look(ref lastEngageTargetTick, "lastEngageTargetTick", 0);
			Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);
			Scribe_Values.Look(ref canFleeIndividual, "canFleeIndividual", defaultValue: false);
			Scribe_Values.Look(ref exitMapAfterTick, "exitMapAfterTick", -99999);
			Scribe_Values.Look(ref forcedGotoPosition, "forcedGotoPosition", IntVec3.Invalid);
			Scribe_Values.Look(ref lastMeleeThreatHarmTick, "lastMeleeThreatHarmTick", 0);
			Scribe_Values.Look(ref lastAssignedInteractTime, "lastAssignedInteractTime", -99999);
			Scribe_Values.Look(ref interactionsToday, "interactionsToday", 0);
			Scribe_Values.Look(ref lastInventoryRawFoodUseTick, "lastInventoryRawFoodUseTick", 0);
			Scribe_Values.Look(ref lastDisturbanceTick, "lastDisturbanceTick", -99999);
			Scribe_Values.Look(ref wantsToTradeWithColony, "wantsToTradeWithColony", defaultValue: false);
			Scribe_Values.Look(ref canLovinTick, "canLovinTick", -99999);
			Scribe_Values.Look(ref canSleepTick, "canSleepTick", -99999);
			Scribe_Values.Look(ref nextMoveOrderIsWait, "nextMoveOrderIsWait", defaultValue: true);
			Scribe_Values.Look(ref lastTakeCombatEnhancingDrugTick, "lastTakeCombatEnhancingDrugTick", -99999);
			Scribe_Values.Look(ref lastHarmTick, "lastHarmTick", -99999);
			Scribe_Values.Look(ref anyCloseHostilesRecently, "anyCloseHostilesRecently", defaultValue: false);
			Scribe_Deep.Look(ref duty, "duty");
			Scribe_Deep.Look(ref mentalStateHandler, "mentalStateHandler", pawn);
			Scribe_Deep.Look(ref mentalBreaker, "mentalBreaker", pawn);
			Scribe_Deep.Look(ref inspirationHandler, "inspirationHandler", pawn);
			Scribe_Deep.Look(ref priorityWork, "priorityWork", pawn);
			Scribe_Values.Look(ref applyBedThoughtsTick, "applyBedThoughtsTick", 0);
			Scribe_Values.Look(ref applyBedThoughtsOnLeave, "applyBedThoughtsOnLeave", defaultValue: false);
			Scribe_Values.Look(ref willJoinColonyIfRescuedInt, "willJoinColonyIfRescued", defaultValue: false);
			Scribe_Values.Look(ref wildManEverReachedOutsideInt, "wildManEverReachedOutside", defaultValue: false);
			Scribe_Values.Look(ref timesGuestTendedToByPlayer, "timesGuestTendedToByPlayer", 0);
			Scribe_Values.Look(ref lastSelfTendTick, "lastSelfTendTick", 0);
			Scribe_Values.Look(ref spawnedByInfestationThingComp, "spawnedByInfestationThingComp", defaultValue: false);
			Scribe_Values.Look(ref lastPredatorHuntingPlayerNotificationTick, "lastPredatorHuntingPlayerNotificationTick", -99999);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.MindStatePostLoadInit(this);
			}
		}

		public void MindStateTick()
		{
			if (wantsToTradeWithColony)
			{
				TradeUtility.CheckInteractWithTradersTeachOpportunity(pawn);
			}
			if (meleeThreat != null && !MeleeThreatStillThreat)
			{
				meleeThreat = null;
			}
			mentalStateHandler.MentalStateHandlerTick();
			mentalBreaker.MentalBreakerTick();
			inspirationHandler.InspirationHandlerTick();
			if (!pawn.GetPosture().Laying())
			{
				applyBedThoughtsTick = 0;
			}
			if (pawn.IsHashIntervalTick(100))
			{
				if (pawn.Spawned)
				{
					int regionsToScan = (!anyCloseHostilesRecently) ? 18 : 24;
					anyCloseHostilesRecently = PawnUtility.EnemiesAreNearby(pawn, regionsToScan, passDoors: true);
				}
				else
				{
					anyCloseHostilesRecently = false;
				}
			}
			if (WillJoinColonyIfRescued && AnythingPreventsJoiningColonyIfRescued)
			{
				WillJoinColonyIfRescued = false;
			}
			if (pawn.Spawned && pawn.IsWildMan() && !WildManEverReachedOutside && pawn.GetRoom() != null && pawn.GetRoom().TouchesMapEdge)
			{
				WildManEverReachedOutside = true;
			}
			if (Find.TickManager.TicksGame % 123 == 0 && pawn.Spawned && pawn.RaceProps.IsFlesh && pawn.needs.mood != null)
			{
				TerrainDef terrain = pawn.Position.GetTerrain(pawn.Map);
				if (terrain.traversedThought != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemoryFast(terrain.traversedThought);
				}
				WeatherDef curWeatherLerped = pawn.Map.weatherManager.CurWeatherLerped;
				if (curWeatherLerped.exposedThought != null && !pawn.Position.Roofed(pawn.Map))
				{
					pawn.needs.mood.thoughts.memories.TryGainMemoryFast(curWeatherLerped.exposedThought);
				}
			}
			if (GenLocalDate.DayTick(pawn) == 0)
			{
				interactionsToday = 0;
			}
		}

		public void JoinColonyBecauseRescuedBy(Pawn by)
		{
			WillJoinColonyIfRescued = false;
			if (!AnythingPreventsJoiningColonyIfRescued)
			{
				InteractionWorker_RecruitAttempt.DoRecruit(by, pawn, 1f, useAudiovisualEffects: false);
				if (pawn.needs != null && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued);
				}
				Find.LetterStack.ReceiveLetter("LetterLabelRescueQuestFinished".Translate(), "LetterRescueQuestFinished".Translate(pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst(), LetterDefOf.PositiveEvent, pawn);
			}
		}

		public void ResetLastDisturbanceTick()
		{
			lastDisturbanceTick = -9999999;
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if (pawn.IsColonistPlayerControlled)
			{
				using (IEnumerator<Gizmo> enumerator = priorityWork.GetGizmos().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Gizmo g2 = enumerator.Current;
						yield return g2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			using (IEnumerator<Gizmo> enumerator2 = CaravanFormingUtility.GetGizmos(pawn).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Gizmo g = enumerator2.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0169:
			/*Error near IL_016a: Unexpected return in MoveNext()*/;
		}

		public void Notify_OutfitChanged()
		{
			nextApparelOptimizeTick = Find.TickManager.TicksGame;
		}

		public void Notify_WorkPriorityDisabled(WorkTypeDef wType)
		{
			JobGiver_Work jobGiver_Work = lastJobGiver as JobGiver_Work;
			if (jobGiver_Work != null && lastGivenWorkType == wType)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
		}

		public void Notify_DamageTaken(DamageInfo dinfo)
		{
			mentalStateHandler.Notify_DamageTaken(dinfo);
			if (dinfo.Def.ExternalViolenceFor(this.pawn))
			{
				lastHarmTick = Find.TickManager.TicksGame;
				if (this.pawn.Spawned)
				{
					Pawn pawn = dinfo.Instigator as Pawn;
					if (!mentalStateHandler.InMentalState && dinfo.Instigator != null && (pawn != null || dinfo.Instigator is Building_Turret) && dinfo.Instigator.Faction != null && (dinfo.Instigator.Faction.def.humanlikeFaction || (pawn != null && (int)pawn.def.race.intelligence >= 1)) && this.pawn.Faction == null && (this.pawn.RaceProps.Animal || this.pawn.IsWildMan()) && (this.pawn.CurJob == null || this.pawn.CurJob.def != JobDefOf.PredatorHunt || dinfo.Instigator != ((JobDriver_PredatorHunt)this.pawn.jobs.curDriver).Prey) && Rand.Chance(PawnUtility.GetManhunterOnDamageChance(this.pawn, dinfo.Instigator)))
					{
						StartManhunterBecauseOfPawnAction("AnimalManhunterFromDamage");
					}
					else if (dinfo.Instigator != null && dinfo.Def.makesAnimalsFlee && CanStartFleeingBecauseOfPawnAction(this.pawn))
					{
						StartFleeingBecauseOfPawnAction(dinfo.Instigator);
					}
				}
				if (this.pawn.GetPosture() != 0)
				{
					lastDisturbanceTick = Find.TickManager.TicksGame;
				}
			}
		}

		internal void Notify_EngagedTarget()
		{
			lastEngageTargetTick = Find.TickManager.TicksGame;
		}

		internal void Notify_AttackedTarget(LocalTargetInfo target)
		{
			lastAttackTargetTick = Find.TickManager.TicksGame;
			lastAttackedTarget = target;
		}

		internal bool CheckStartMentalStateBecauseRecruitAttempted(Pawn tamer)
		{
			if (!pawn.RaceProps.Animal && (!pawn.IsWildMan() || pawn.IsPrisoner))
			{
				return false;
			}
			if (!mentalStateHandler.InMentalState && pawn.Faction == null && Rand.Chance(pawn.RaceProps.manhunterOnTameFailChance))
			{
				StartManhunterBecauseOfPawnAction("AnimalManhunterFromTaming");
				return true;
			}
			return false;
		}

		internal void Notify_DangerousExploderAboutToExplode(Thing exploder)
		{
			if ((int)pawn.RaceProps.intelligence >= 2)
			{
				knownExploder = exploder;
				pawn.jobs.CheckForJobOverride();
			}
		}

		public void Notify_Explosion(Explosion explosion)
		{
			if (pawn.Faction == null && !(explosion.radius < 3.5f) && pawn.Position.InHorDistOf(explosion.Position, explosion.radius + 7f) && CanStartFleeingBecauseOfPawnAction(pawn))
			{
				StartFleeingBecauseOfPawnAction(explosion);
			}
		}

		public void Notify_TuckedIntoBed()
		{
			if (pawn.IsWildMan())
			{
				WildManEverReachedOutside = false;
			}
		}

		public void Notify_SelfTended()
		{
			lastSelfTendTick = Find.TickManager.TicksGame;
		}

		public void Notify_PredatorHuntingPlayerNotification()
		{
			lastPredatorHuntingPlayerNotificationTick = Find.TickManager.TicksGame;
		}

		private IEnumerable<Pawn> GetPackmates(Pawn pawn, float radius)
		{
			Room pawnRoom = pawn.GetRoom();
			List<Pawn> raceMates = pawn.Map.mapPawns.AllPawnsSpawned;
			int i = 0;
			while (true)
			{
				if (i >= raceMates.Count)
				{
					yield break;
				}
				if (pawn != raceMates[i] && raceMates[i].def == pawn.def && raceMates[i].Faction == pawn.Faction && raceMates[i].Position.InHorDistOf(pawn.Position, radius) && raceMates[i].GetRoom() == pawnRoom)
				{
					break;
				}
				i++;
			}
			yield return raceMates[i];
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private void StartManhunterBecauseOfPawnAction(string letterTextKey)
		{
			if (mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter))
			{
				string text = letterTextKey.Translate(pawn.Label, pawn.Named("PAWN")).AdjustedFor(pawn);
				GlobalTargetInfo target = pawn;
				int num = 1;
				if (Find.Storyteller.difficulty.allowBigThreats && Rand.Value < 0.5f)
				{
					foreach (Pawn packmate in GetPackmates(pawn, 24f))
					{
						if (packmate.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter))
						{
							num++;
						}
					}
					if (num > 1)
					{
						target = new TargetInfo(pawn.Position, pawn.Map);
						text += "\n\n";
						text += "AnimalManhunterOthers".Translate(pawn.kindDef.GetLabelPlural(), pawn);
					}
				}
				string value = (!pawn.RaceProps.Animal) ? pawn.def.label : pawn.Label;
				string label = "LetterLabelAnimalManhunterRevenge".Translate(value).CapitalizeFirst();
				Find.LetterStack.ReceiveLetter(label, text, (num != 1) ? LetterDefOf.ThreatBig : LetterDefOf.ThreatSmall, target);
			}
		}

		private static bool CanStartFleeingBecauseOfPawnAction(Pawn p)
		{
			return p.RaceProps.Animal && !p.InMentalState && !p.IsFighting() && !p.Downed && !p.Dead && !ThinkNode_ConditionalShouldFollowMaster.ShouldFollowMaster(p);
		}

		public void StartFleeingBecauseOfPawnAction(Thing instigator)
		{
			List<Thing> list = new List<Thing>();
			list.Add(instigator);
			List<Thing> threats = list;
			IntVec3 fleeDest = CellFinderLoose.GetFleeDest(pawn, threats, pawn.Position.DistanceTo(instigator.Position) + 14f);
			if (fleeDest != pawn.Position)
			{
				pawn.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest, instigator), JobCondition.InterruptOptional);
			}
			if (pawn.RaceProps.herdAnimal && Rand.Chance(0.1f))
			{
				foreach (Pawn packmate in GetPackmates(pawn, 24f))
				{
					if (CanStartFleeingBecauseOfPawnAction(packmate))
					{
						IntVec3 fleeDest2 = CellFinderLoose.GetFleeDest(packmate, threats, packmate.Position.DistanceTo(instigator.Position) + 14f);
						if (fleeDest2 != packmate.Position)
						{
							packmate.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest2, instigator), JobCondition.InterruptOptional);
						}
					}
				}
			}
		}
	}
}
