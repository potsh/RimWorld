using Verse;

namespace RimWorld
{
	public abstract class CompUseEffect : ThingComp
	{
		private const float CameraShakeMag = 1f;

		public virtual float OrderPriority => 0f;

		private CompProperties_UseEffect Props => (CompProperties_UseEffect)props;

		public virtual void DoEffect(Pawn usedBy)
		{
			if (Props.doCameraShake && usedBy.Spawned && usedBy.Map == Find.CurrentMap)
			{
				Find.CameraDriver.shaker.DoShake(1f);
			}
		}

		public virtual bool SelectedUseOption(Pawn p)
		{
			return false;
		}

		public virtual bool CanBeUsedBy(Pawn p, out string failReason)
		{
			failReason = null;
			return true;
		}
	}
}
