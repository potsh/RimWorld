using RimWorld;
using System;
using System.Collections.Generic;
using Verse.AI.Group;

namespace Verse.AI
{
	public class Job : IExposable, ILoadReferenceable
	{
		public JobDef def;

		public LocalTargetInfo targetA = LocalTargetInfo.Invalid;

		public LocalTargetInfo targetB = LocalTargetInfo.Invalid;

		public LocalTargetInfo targetC = LocalTargetInfo.Invalid;

		public List<LocalTargetInfo> targetQueueA;

		public List<LocalTargetInfo> targetQueueB;

		public int count = -1;

		public List<int> countQueue;

		public int loadID;

		public int startTick = -1;

		public int expiryInterval = -1;

		public bool checkOverrideOnExpire;

		public bool playerForced;

		public List<ThingCountClass> placedThings;

		public int maxNumMeleeAttacks = 2147483647;

		public int maxNumStaticAttacks = 2147483647;

		public LocomotionUrgency locomotionUrgency = LocomotionUrgency.Jog;

		public HaulMode haulMode;

		public Bill bill;

		public ICommunicable commTarget;

		public ThingDef plantDefToSow;

		public Verb verbToUse;

		public bool haulOpportunisticDuplicates;

		public bool exitMapOnArrival;

		public bool failIfCantJoinOrCreateCaravan;

		public bool killIncappedTarget;

		public bool ignoreForbidden;

		public bool ignoreDesignations;

		public bool canBash;

		public bool haulDroppedApparel;

		public bool restUntilHealed;

		public bool ignoreJoyTimeAssignment;

		public bool overeat;

		public bool attackDoorIfTargetLost;

		public int takeExtraIngestibles;

		public bool expireRequiresEnemiesNearby;

		public Lord lord;

		public bool collideWithPawns;

		public bool forceSleep;

		public InteractionDef interaction;

		public bool endIfCantShootTargetFromCurPos;

		public bool endIfCantShootInMelee;

		public bool checkEncumbrance;

		public float followRadius;

		public bool endAfterTendedOnce;

		private JobDriver cachedDriver;

		public RecipeDef RecipeDef => (bill == null) ? null : bill.recipe;

		public Job()
		{
		}

		public Job(JobDef def)
			: this(def, null)
		{
		}

		public Job(JobDef def, LocalTargetInfo targetA)
			: this(def, targetA, null)
		{
		}

		public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB)
		{
			this.def = def;
			this.targetA = targetA;
			this.targetB = targetB;
			loadID = Find.UniqueIDsManager.GetNextJobID();
		}

		public Job(JobDef def, LocalTargetInfo targetA, LocalTargetInfo targetB, LocalTargetInfo targetC)
		{
			this.def = def;
			this.targetA = targetA;
			this.targetB = targetB;
			this.targetC = targetC;
			loadID = Find.UniqueIDsManager.GetNextJobID();
		}

		public Job(JobDef def, LocalTargetInfo targetA, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			this.def = def;
			this.targetA = targetA;
			this.expiryInterval = expiryInterval;
			checkOverrideOnExpire = checkOverrideOnExpiry;
			loadID = Find.UniqueIDsManager.GetNextJobID();
		}

		public Job(JobDef def, int expiryInterval, bool checkOverrideOnExpiry = false)
		{
			this.def = def;
			this.expiryInterval = expiryInterval;
			checkOverrideOnExpire = checkOverrideOnExpiry;
			loadID = Find.UniqueIDsManager.GetNextJobID();
		}

		public LocalTargetInfo GetTarget(TargetIndex ind)
		{
			switch (ind)
			{
			case TargetIndex.A:
				return targetA;
			case TargetIndex.B:
				return targetB;
			case TargetIndex.C:
				return targetC;
			default:
				throw new ArgumentException();
			}
		}

		public List<LocalTargetInfo> GetTargetQueue(TargetIndex ind)
		{
			switch (ind)
			{
			case TargetIndex.A:
				if (targetQueueA == null)
				{
					targetQueueA = new List<LocalTargetInfo>();
				}
				return targetQueueA;
			case TargetIndex.B:
				if (targetQueueB == null)
				{
					targetQueueB = new List<LocalTargetInfo>();
				}
				return targetQueueB;
			default:
				throw new ArgumentException();
			}
		}

		public void SetTarget(TargetIndex ind, LocalTargetInfo pack)
		{
			switch (ind)
			{
			case TargetIndex.A:
				targetA = pack;
				break;
			case TargetIndex.B:
				targetB = pack;
				break;
			case TargetIndex.C:
				targetC = pack;
				break;
			default:
				throw new ArgumentException();
			}
		}

		public void AddQueuedTarget(TargetIndex ind, LocalTargetInfo target)
		{
			GetTargetQueue(ind).Add(target);
		}

		public void ExposeData()
		{
			ILoadReferenceable refee = (ILoadReferenceable)commTarget;
			Scribe_References.Look(ref refee, "commTarget");
			commTarget = (ICommunicable)refee;
			Scribe_References.Look(ref verbToUse, "verbToUse");
			Scribe_References.Look(ref bill, "bill");
			Scribe_References.Look(ref lord, "lord");
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref loadID, "loadID", 0);
			Scribe_TargetInfo.Look(ref targetA, "targetA");
			Scribe_TargetInfo.Look(ref targetB, "targetB");
			Scribe_TargetInfo.Look(ref targetC, "targetC");
			Scribe_Collections.Look(ref targetQueueA, "targetQueueA", LookMode.Undefined);
			Scribe_Collections.Look(ref targetQueueB, "targetQueueB", LookMode.Undefined);
			Scribe_Values.Look(ref count, "count", -1);
			Scribe_Collections.Look(ref countQueue, "countQueue", LookMode.Undefined);
			Scribe_Values.Look(ref startTick, "startTick", -1);
			Scribe_Values.Look(ref expiryInterval, "expiryInterval", -1);
			Scribe_Values.Look(ref checkOverrideOnExpire, "checkOverrideOnExpire", defaultValue: false);
			Scribe_Values.Look(ref playerForced, "playerForced", defaultValue: false);
			Scribe_Collections.Look(ref placedThings, "placedThings", LookMode.Undefined);
			Scribe_Values.Look(ref maxNumMeleeAttacks, "maxNumMeleeAttacks", 2147483647);
			Scribe_Values.Look(ref maxNumStaticAttacks, "maxNumStaticAttacks", 2147483647);
			Scribe_Values.Look(ref exitMapOnArrival, "exitMapOnArrival", defaultValue: false);
			Scribe_Values.Look(ref failIfCantJoinOrCreateCaravan, "failIfCantJoinOrCreateCaravan", defaultValue: false);
			Scribe_Values.Look(ref killIncappedTarget, "killIncappedTarget", defaultValue: false);
			Scribe_Values.Look(ref haulOpportunisticDuplicates, "haulOpportunisticDuplicates", defaultValue: false);
			Scribe_Values.Look(ref haulMode, "haulMode", HaulMode.Undefined);
			Scribe_Defs.Look(ref plantDefToSow, "plantDefToSow");
			Scribe_Values.Look(ref locomotionUrgency, "locomotionUrgency", LocomotionUrgency.Jog);
			Scribe_Values.Look(ref ignoreDesignations, "ignoreDesignations", defaultValue: false);
			Scribe_Values.Look(ref canBash, "canBash", defaultValue: false);
			Scribe_Values.Look(ref haulDroppedApparel, "haulDroppedApparel", defaultValue: false);
			Scribe_Values.Look(ref restUntilHealed, "restUntilHealed", defaultValue: false);
			Scribe_Values.Look(ref ignoreJoyTimeAssignment, "ignoreJoyTimeAssignment", defaultValue: false);
			Scribe_Values.Look(ref overeat, "overeat", defaultValue: false);
			Scribe_Values.Look(ref attackDoorIfTargetLost, "attackDoorIfTargetLost", defaultValue: false);
			Scribe_Values.Look(ref takeExtraIngestibles, "takeExtraIngestibles", 0);
			Scribe_Values.Look(ref expireRequiresEnemiesNearby, "expireRequiresEnemiesNearby", defaultValue: false);
			Scribe_Values.Look(ref collideWithPawns, "collideWithPawns", defaultValue: false);
			Scribe_Values.Look(ref forceSleep, "forceSleep", defaultValue: false);
			Scribe_Defs.Look(ref interaction, "interaction");
			Scribe_Values.Look(ref endIfCantShootTargetFromCurPos, "endIfCantShootTargetFromCurPos", defaultValue: false);
			Scribe_Values.Look(ref endIfCantShootInMelee, "endIfCantShootInMelee", defaultValue: false);
			Scribe_Values.Look(ref checkEncumbrance, "checkEncumbrance", defaultValue: false);
			Scribe_Values.Look(ref followRadius, "followRadius", 0f);
			Scribe_Values.Look(ref endAfterTendedOnce, "endAfterTendedOnce", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && verbToUse != null && verbToUse.BuggedAfterLoading)
			{
				verbToUse = null;
				Log.Warning(GetType() + " had a bugged verbToUse after loading.");
			}
		}

		public JobDriver MakeDriver(Pawn driverPawn)
		{
			JobDriver jobDriver = (JobDriver)Activator.CreateInstance(def.driverClass);
			jobDriver.pawn = driverPawn;
			jobDriver.job = this;
			return jobDriver;
		}

		public JobDriver GetCachedDriver(Pawn driverPawn)
		{
			if (cachedDriver == null)
			{
				cachedDriver = MakeDriver(driverPawn);
			}
			if (cachedDriver.pawn != driverPawn)
			{
				Log.Error("Tried to use the same driver for 2 pawns: " + cachedDriver.ToStringSafe() + ", first pawn= " + cachedDriver.pawn.ToStringSafe() + ", second pawn=" + driverPawn.ToStringSafe());
			}
			return cachedDriver;
		}

		public bool TryMakePreToilReservations(Pawn driverPawn, bool errorOnFailed)
		{
			return GetCachedDriver(driverPawn).TryMakePreToilReservations(errorOnFailed);
		}

		public string GetReport(Pawn driverPawn)
		{
			return GetCachedDriver(driverPawn).GetReport();
		}

		public LocalTargetInfo GetDestination(Pawn driverPawn)
		{
			return targetA;
		}

		public bool CanBeginNow(Pawn pawn, bool whileLyingDown = false)
		{
			if (pawn.Downed)
			{
				whileLyingDown = true;
			}
			if (whileLyingDown)
			{
				return GetCachedDriver(pawn).CanBeginNowWhileLyingDown();
			}
			return true;
		}

		public bool JobIsSameAs(Job other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (def != other.def || targetA != other.targetA || targetB != other.targetB || verbToUse != other.verbToUse || targetC != other.targetC || commTarget != other.commTarget || bill != other.bill)
			{
				return false;
			}
			return true;
		}

		public bool AnyTargetIs(LocalTargetInfo target)
		{
			if (!target.IsValid)
			{
				return false;
			}
			return targetA == target || targetB == target || targetC == target || (targetQueueA != null && targetQueueA.Contains(target)) || (targetQueueB != null && targetQueueB.Contains(target));
		}

		public override string ToString()
		{
			string text = def.ToString() + " (" + GetUniqueLoadID() + ")";
			if (targetA.IsValid)
			{
				text = text + " A=" + targetA.ToString();
			}
			if (targetB.IsValid)
			{
				text = text + " B=" + targetB.ToString();
			}
			if (targetC.IsValid)
			{
				text = text + " C=" + targetC.ToString();
			}
			return text;
		}

		public string GetUniqueLoadID()
		{
			return "Job_" + loadID;
		}
	}
}
