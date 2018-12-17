using RimWorld;

namespace Verse.AI
{
	public class MentalState : IExposable
	{
		public Pawn pawn;

		public MentalStateDef def;

		private int age;

		public bool causedByMood;

		private const int TickInterval = 150;

		public int Age => age;

		public virtual string InspectLine => def.baseInspectLine;

		protected virtual bool CanEndBeforeMaxDurationNow => true;

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref age, "age", 0);
			Scribe_Values.Look(ref causedByMood, "causedByMood", defaultValue: false);
		}

		public virtual void PostStart(string reason)
		{
		}

		public virtual void PreStart()
		{
		}

		public virtual void PostEnd()
		{
			if (!def.recoveryMessage.NullOrEmpty() && PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				string text = def.recoveryMessage.Formatted(pawn.LabelShort, pawn.Named("PAWN"));
				if (!text.NullOrEmpty())
				{
					Messages.Message(text.AdjustedFor(pawn).CapitalizeFirst(), pawn, MessageTypeDefOf.SituationResolved);
				}
			}
		}

		public virtual void MentalStateTick()
		{
			if (pawn.IsHashIntervalTick(150))
			{
				age += 150;
				if (age >= def.maxTicksBeforeRecovery || (age >= def.minTicksBeforeRecovery && CanEndBeforeMaxDurationNow && Rand.MTBEventOccurs(def.recoveryMtbDays, 60000f, 150f)))
				{
					RecoverFromState();
				}
				else if (def.recoverFromSleep && !pawn.Awake())
				{
					RecoverFromState();
				}
			}
		}

		public void RecoverFromState()
		{
			if (pawn.MentalState != this)
			{
				Log.Error("Recovered from " + def + " but pawn's mental state is not this, it is " + pawn.MentalState);
			}
			if (!pawn.Dead)
			{
				pawn.mindState.mentalStateHandler.ClearMentalStateDirect();
				if (causedByMood && def.moodRecoveryThought != null && pawn.needs.mood != null)
				{
					pawn.needs.mood.thoughts.memories.TryGainMemory(def.moodRecoveryThought);
				}
				pawn.mindState.mentalBreaker.Notify_RecoveredFromMentalState();
			}
			if (pawn.Spawned)
			{
				pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
			}
			PostEnd();
		}

		public virtual bool ForceHostileTo(Thing t)
		{
			return false;
		}

		public virtual bool ForceHostileTo(Faction f)
		{
			return false;
		}

		public EffecterDef CurrentStateEffecter()
		{
			return def.stateEffecter;
		}

		public virtual RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.SuperActive;
		}

		public virtual string GetBeginLetterText()
		{
			if (def.beginLetter.NullOrEmpty())
			{
				return null;
			}
			return def.beginLetter.Formatted(pawn.LabelShort, pawn.Named("PAWN")).AdjustedFor(pawn).CapitalizeFirst();
		}

		public virtual void Notify_AttackedTarget(LocalTargetInfo hitTarget)
		{
		}

		public virtual void Notify_SlaughteredAnimal()
		{
		}
	}
}
