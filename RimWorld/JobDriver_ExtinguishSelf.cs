using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_ExtinguishSelf : JobDriver
	{
		protected Fire TargetFire => (Fire)job.targetA.Thing;

		public override string GetReport()
		{
			if (TargetFire != null && TargetFire.parent != null)
			{
				return "ReportExtinguishingFireOn".Translate(TargetFire.parent.LabelCap, TargetFire.parent.Named("TARGET"));
			}
			return "ReportExtinguishingFire".Translate();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return new Toil
			{
				defaultCompleteMode = ToilCompleteMode.Delay,
				defaultDuration = 150
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
