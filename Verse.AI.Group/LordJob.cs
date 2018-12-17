namespace Verse.AI.Group
{
	public abstract class LordJob : IExposable
	{
		public Lord lord;

		public virtual bool LostImportantReferenceDuringLoading => false;

		public virtual bool AllowStartNewGatherings => true;

		public virtual bool NeverInRestraints => false;

		public virtual bool GuiltyOnDowned => false;

		public virtual bool CanBlockHostileVisitors => true;

		public virtual bool AddFleeToil => true;

		protected Map Map => lord.lordManager.map;

		public abstract StateGraph CreateGraph();

		public virtual void LordJobTick()
		{
		}

		public virtual void ExposeData()
		{
		}

		public virtual void Cleanup()
		{
		}

		public virtual void Notify_PawnAdded(Pawn p)
		{
		}

		public virtual void Notify_PawnLost(Pawn p, PawnLostCondition condition)
		{
		}

		public virtual string GetReport()
		{
			return null;
		}

		public virtual bool CanOpenAnyDoor(Pawn p)
		{
			return false;
		}

		public virtual bool ValidateAttackTarget(Pawn searcher, Thing target)
		{
			return true;
		}
	}
}
