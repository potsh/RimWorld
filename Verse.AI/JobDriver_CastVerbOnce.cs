using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_CastVerbOnce : JobDriver
	{
		public override string GetReport()
		{
			string value = (!base.TargetA.HasThing) ? "AreaLower".Translate() : base.TargetThingA.LabelCap;
			return "UsingVerb".Translate(job.verbToUse.verbProps.label, value);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Combat.GotoCastPosition(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
