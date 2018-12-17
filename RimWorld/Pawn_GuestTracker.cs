using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public class Pawn_GuestTracker : IExposable
	{
		private Pawn pawn;

		public PrisonerInteractionModeDef interactionMode = PrisonerInteractionModeDefOf.NoInteraction;

		private Faction hostFactionInt;

		public bool isPrisonerInt;

		private bool releasedInt;

		private int ticksWhenAllowedToEscapeAgain;

		public IntVec3 spotToWaitInsteadOfEscaping = IntVec3.Invalid;

		public int lastPrisonBreakTicks = -1;

		public bool everParticipatedInPrisonBreak;

		public float resistance = -1f;

		public bool getRescuedThoughtOnUndownedBecauseOfPlayer;

		private const int DefaultWaitInsteadOfEscapingTicks = 25000;

		public const int MinInteractionInterval = 10000;

		public const int MaxInteractionsPerDay = 2;

		private const int CheckInitiatePrisonBreakIntervalTicks = 2500;

		private static readonly SimpleCurve StartingResistancePerRecruitDifficultyCurve = new SimpleCurve
		{
			new CurvePoint(0.1f, 0f),
			new CurvePoint(0.5f, 15f),
			new CurvePoint(0.9f, 25f),
			new CurvePoint(1f, 50f)
		};

		private static readonly SimpleCurve StartingResistanceFactorFromPopulationIntentCurve = new SimpleCurve
		{
			new CurvePoint(-1f, 2f),
			new CurvePoint(0f, 1.5f),
			new CurvePoint(1f, 1f),
			new CurvePoint(2f, 0.8f)
		};

		private static readonly FloatRange StartingResistanceRandomFactorRange = new FloatRange(0.8f, 1.2f);

		public Faction HostFaction => hostFactionInt;

		public bool CanBeBroughtFood => interactionMode != PrisonerInteractionModeDefOf.Execution && (interactionMode != PrisonerInteractionModeDefOf.Release || pawn.Downed);

		public bool IsPrisoner => isPrisonerInt;

		public bool ScheduledForInteraction => pawn.mindState.lastAssignedInteractTime < Find.TickManager.TicksGame - 10000 && pawn.mindState.interactionsToday < 2;

		public bool Released
		{
			get
			{
				return releasedInt;
			}
			set
			{
				if (value != releasedInt)
				{
					releasedInt = value;
					ReachabilityUtility.ClearCacheFor(pawn);
				}
			}
		}

		public bool PrisonerIsSecure
		{
			get
			{
				if (Released)
				{
					return false;
				}
				if (pawn.HostFaction == null)
				{
					return false;
				}
				if (pawn.InMentalState)
				{
					return false;
				}
				if (pawn.Spawned)
				{
					if (pawn.jobs.curJob != null && pawn.jobs.curJob.exitMapOnArrival)
					{
						return false;
					}
					if (PrisonBreakUtility.IsPrisonBreaking(pawn))
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool ShouldWaitInsteadOfEscaping
		{
			get
			{
				if (!IsPrisoner)
				{
					return false;
				}
				Map mapHeld = pawn.MapHeld;
				if (mapHeld == null)
				{
					return false;
				}
				if (mapHeld.mapPawns.FreeColonistsSpawnedCount == 0)
				{
					return false;
				}
				return Find.TickManager.TicksGame < ticksWhenAllowedToEscapeAgain;
			}
		}

		public float Resistance => resistance;

		public Pawn_GuestTracker()
		{
		}

		public Pawn_GuestTracker(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void GuestTrackerTick()
		{
			if (pawn.IsHashIntervalTick(2500))
			{
				float num = PrisonBreakUtility.InitiatePrisonBreakMtbDays(pawn);
				if (num >= 0f && Rand.MTBEventOccurs(num, 60000f, 2500f))
				{
					PrisonBreakUtility.StartPrisonBreak(pawn);
				}
			}
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref hostFactionInt, "hostFaction");
			Scribe_Values.Look(ref isPrisonerInt, "prisoner", defaultValue: false);
			Scribe_Defs.Look(ref interactionMode, "interactionMode");
			Scribe_Values.Look(ref releasedInt, "released", defaultValue: false);
			Scribe_Values.Look(ref ticksWhenAllowedToEscapeAgain, "ticksWhenAllowedToEscapeAgain", 0);
			Scribe_Values.Look(ref spotToWaitInsteadOfEscaping, "spotToWaitInsteadOfEscaping");
			Scribe_Values.Look(ref lastPrisonBreakTicks, "lastPrisonBreakTicks", 0);
			Scribe_Values.Look(ref everParticipatedInPrisonBreak, "everParticipatedInPrisonBreak", defaultValue: false);
			Scribe_Values.Look(ref getRescuedThoughtOnUndownedBecauseOfPlayer, "getRescuedThoughtOnUndownedBecauseOfPlayer", defaultValue: false);
			Scribe_Values.Look(ref resistance, "resistance", -1f);
		}

		public void SetGuestStatus(Faction newHost, bool prisoner = false)
		{
			if (newHost != null)
			{
				Released = false;
			}
			if (newHost != HostFaction || prisoner != IsPrisoner)
			{
				if (!prisoner && pawn.Faction.HostileTo(newHost))
				{
					Log.Error("Tried to make " + pawn + " a guest of " + newHost + " but their faction " + pawn.Faction + " is hostile to " + newHost);
				}
				else if (newHost != null && newHost == pawn.Faction && !prisoner)
				{
					Log.Error("Tried to make " + pawn + " a guest of their own faction " + pawn.Faction);
				}
				else
				{
					bool flag = prisoner && (!IsPrisoner || HostFaction != newHost);
					isPrisonerInt = prisoner;
					hostFactionInt = newHost;
					pawn.ClearMind();
					if (flag)
					{
						pawn.DropAndForbidEverything();
						pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.MadePrisoner);
						if (newHost == Faction.OfPlayer)
						{
							Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedPrisoner);
						}
						if (pawn.Drafted)
						{
							pawn.drafter.Drafted = false;
						}
						float x = pawn.RecruitDifficulty(Faction.OfPlayer);
						resistance = StartingResistancePerRecruitDifficultyCurve.Evaluate(x);
						resistance *= StartingResistanceFactorFromPopulationIntentCurve.Evaluate(StorytellerUtilityPopulation.PopulationIntent);
						resistance *= StartingResistanceRandomFactorRange.RandomInRange;
						resistance = (float)GenMath.RoundRandom(resistance);
					}
					PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
					pawn.health.surgeryBills.Clear();
					if (pawn.ownership != null)
					{
						pawn.ownership.Notify_ChangedGuestStatus();
					}
					ReachabilityUtility.ClearCacheFor(pawn);
					if (pawn.Spawned)
					{
						pawn.Map.mapPawns.UpdateRegistryForPawn(pawn);
						pawn.Map.attackTargetsCache.UpdateTarget(pawn);
					}
					AddictionUtility.CheckDrugAddictionTeachOpportunity(pawn);
					if (prisoner && pawn.playerSettings != null)
					{
						pawn.playerSettings.Notify_MadePrisoner();
					}
				}
			}
		}

		public void CapturedBy(Faction by, Pawn byPawn = null)
		{
			if (pawn.Faction != null)
			{
				pawn.Faction.Notify_MemberCaptured(pawn, by);
			}
			SetGuestStatus(by, prisoner: true);
			if (IsPrisoner && byPawn != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.Captured, byPawn, pawn);
				byPawn.records.Increment(RecordDefOf.PeopleCaptured);
			}
		}

		public void WaitInsteadOfEscapingForDefaultTicks()
		{
			WaitInsteadOfEscapingFor(25000);
		}

		public void WaitInsteadOfEscapingFor(int ticks)
		{
			if (IsPrisoner)
			{
				ticksWhenAllowedToEscapeAgain = Find.TickManager.TicksGame + ticks;
				spotToWaitInsteadOfEscaping = IntVec3.Invalid;
			}
		}

		internal void Notify_PawnUndowned()
		{
			if (pawn.RaceProps.Humanlike && (HostFaction == Faction.OfPlayer || (pawn.IsWildMan() && pawn.InBed() && pawn.CurrentBed().Faction == Faction.OfPlayer)) && !IsPrisoner && pawn.SpawnedOrAnyParentSpawned)
			{
				if (getRescuedThoughtOnUndownedBecauseOfPlayer && pawn.needs != null && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.Rescued);
				}
				if (pawn.Faction == null || pawn.Faction.def.rescueesCanJoin)
				{
					Map mapHeld = pawn.MapHeld;
					float num = (pawn.SafeTemperatureRange().Includes(mapHeld.mapTemperature.OutdoorTemp) && !mapHeld.gameConditionManager.ConditionIsActive(GameConditionDefOf.ToxicFallout)) ? 0.5f : 1f;
					if (Rand.ValueSeeded(pawn.thingIDNumber ^ 0x88F8E4) < num)
					{
						pawn.SetFaction(Faction.OfPlayer);
						Messages.Message("MessageRescueeJoined".Translate(pawn.LabelShort, pawn).AdjustedFor(pawn), pawn, MessageTypeDefOf.PositiveEvent);
					}
				}
			}
			getRescuedThoughtOnUndownedBecauseOfPlayer = false;
		}
	}
}
