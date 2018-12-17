using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_RemoveApparel : JobDriver
	{
		private int duration;

		private const TargetIndex ApparelInd = TargetIndex.A;

		private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref duration, "duration", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			yield return Toils_General.Wait(duration).WithProgressBarToilDelay(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
