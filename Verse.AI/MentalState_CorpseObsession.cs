namespace Verse.AI
{
	public class MentalState_CorpseObsession : MentalState
	{
		public Corpse corpse;

		private const int AnyCorpseStillValidCheckInterval = 500;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref corpse, "corpse");
		}

		public override void MentalStateTick()
		{
			bool flag = false;
			if (pawn.IsHashIntervalTick(500) && !CorpseObsessionMentalStateUtility.IsCorpseValid(corpse, pawn))
			{
				corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(pawn);
				if (corpse == null)
				{
					RecoverFromState();
					flag = true;
				}
			}
			if (!flag)
			{
				base.MentalStateTick();
			}
		}

		public override void PostStart(string reason)
		{
			base.PostStart(reason);
			corpse = CorpseObsessionMentalStateUtility.GetClosestCorpseToDigUp(pawn);
		}

		public void Notify_CorpseHauled()
		{
			RecoverFromState();
		}
	}
}
