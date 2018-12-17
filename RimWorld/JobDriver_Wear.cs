using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Wear : JobDriver
	{
		private int duration;

		private int unequipBuffer;

		private const TargetIndex ApparelInd = TargetIndex.A;

		private Apparel Apparel => (Apparel)job.GetTarget(TargetIndex.A).Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref duration, "duration", 0);
			Scribe_Values.Look(ref unequipBuffer, "unequipBuffer", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Apparel;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			duration = (int)(Apparel.GetStatValue(StatDefOf.EquipDelay) * 60f);
			Apparel apparel = Apparel;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			for (int num = wornApparel.Count - 1; num >= 0; num--)
			{
				if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
				{
					duration += (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
				}
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnBurningImmobile(TargetIndex.A);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private void TryUnequipSomething()
		{
			Apparel apparel = Apparel;
			List<Apparel> wornApparel = pawn.apparel.WornApparel;
			int num = wornApparel.Count - 1;
			while (true)
			{
				if (num < 0)
				{
					return;
				}
				if (!ApparelUtility.CanWearTogether(apparel.def, wornApparel[num].def, pawn.RaceProps.body))
				{
					break;
				}
				num--;
			}
			int num2 = (int)(wornApparel[num].GetStatValue(StatDefOf.EquipDelay) * 60f);
			if (unequipBuffer >= num2)
			{
				bool forbid = pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer);
				if (!pawn.apparel.TryDrop(wornApparel[num], out Apparel _, pawn.PositionHeld, forbid))
				{
					Log.Error(pawn + " could not drop " + wornApparel[num].ToStringSafe());
					EndJobWith(JobCondition.Errored);
				}
			}
		}
	}
}
