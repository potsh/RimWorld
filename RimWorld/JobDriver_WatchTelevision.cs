using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_WatchTelevision : JobDriver_WatchBuilding
	{
		protected override void WatchTickAction()
		{
			Building thing = (Building)base.TargetA.Thing;
			if (!thing.TryGetComp<CompPowerTrader>().PowerOn)
			{
				EndJobWith(JobCondition.Incompletable);
			}
			else
			{
				base.WatchTickAction();
			}
		}
	}
}
