using RimWorld;

namespace Verse.AI
{
	public class MentalState_MurderousRage : MentalState
	{
		public Pawn target;

		private const int NoLongerValidTargetCheckInterval = 120;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref target, "target");
		}

		public override RandomSocialMode SocialModeMax()
		{
			return RandomSocialMode.Off;
		}

		public override void PreStart()
		{
			base.PreStart();
			TryFindNewTarget();
		}

		public override void MentalStateTick()
		{
			base.MentalStateTick();
			if (target != null && target.Dead)
			{
				RecoverFromState();
			}
			if (pawn.IsHashIntervalTick(120) && !IsTargetStillValidAndReachable())
			{
				if (!TryFindNewTarget())
				{
					RecoverFromState();
				}
				else
				{
					Messages.Message("MessageMurderousRageChangedTarget".Translate(pawn.LabelShort, target.Label, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn), pawn, MessageTypeDefOf.NegativeEvent);
					base.MentalStateTick();
				}
			}
		}

		public override string GetBeginLetterText()
		{
			if (target == null)
			{
				Log.Error("No target. This should have been checked in this mental state's worker.");
				return string.Empty;
			}
			return def.beginLetter.Formatted(pawn.LabelShort, target.LabelShort, pawn.Named("PAWN"), target.Named("TARGET")).AdjustedFor(pawn).CapitalizeFirst();
		}

		private bool TryFindNewTarget()
		{
			target = MurderousRageMentalStateUtility.FindPawnToKill(pawn);
			return target != null;
		}

		public bool IsTargetStillValidAndReachable()
		{
			return target != null && target.SpawnedParentOrMe != null && (!(target.SpawnedParentOrMe is Pawn) || target.SpawnedParentOrMe == target) && pawn.CanReach(target.SpawnedParentOrMe, PathEndMode.Touch, Danger.Deadly, canBash: true);
		}
	}
}
