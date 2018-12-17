using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_AttackStatic : JobDriver
	{
		private bool startedIncapacitated;

		private int numAttacksMade;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref startedIncapacitated, "startedIncapacitated", defaultValue: false);
			Scribe_Values.Look(ref numAttacksMade, "numAttacksMade", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Misc.ThrowColonistAttackingMote(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
