using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class JobDriver_HaulToCell : JobDriver
	{
		private bool forbiddenInitially;

		private const TargetIndex HaulableInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref forbiddenInitially, "forbiddenInitially", defaultValue: false);
		}

		public override string GetReport()
		{
			IntVec3 cell = job.targetB.Cell;
			Thing thing = null;
			if (pawn.CurJob == job && pawn.carryTracker.CarriedThing != null)
			{
				thing = pawn.carryTracker.CarriedThing;
			}
			else if (base.TargetThingA != null && base.TargetThingA.Spawned)
			{
				thing = base.TargetThingA;
			}
			if (thing == null)
			{
				return "ReportHaulingUnknown".Translate();
			}
			string text = null;
			SlotGroup slotGroup = cell.GetSlotGroup(base.Map);
			if (slotGroup != null)
			{
				text = slotGroup.parent.SlotYielderLabel();
			}
			if (text != null)
			{
				return "ReportHaulingTo".Translate(thing.Label, text.Named("DESTINATION"), thing.Named("THING"));
			}
			return "ReportHauling".Translate(thing.Label, thing);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = base.job.GetTarget(TargetIndex.B);
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			int result;
			if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				pawn = base.pawn;
				target = base.job.GetTarget(TargetIndex.A);
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				result = (pawn.Reserve(target, job, 1, -1, null, errorOnFailed2) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			if (base.TargetThingA != null)
			{
				forbiddenInitially = base.TargetThingA.IsForbidden(pawn);
			}
			else
			{
				forbiddenInitially = false;
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			_003CMakeNewToils_003Ec__Iterator0 _003CMakeNewToils_003Ec__Iterator = (_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0046: stateMachine*/;
			this.FailOnDestroyedOrNull(TargetIndex.A);
			this.FailOnBurningImmobile(TargetIndex.B);
			if (!forbiddenInitially)
			{
				this.FailOnForbidden(TargetIndex.A);
			}
			Toil reserveTargetA = Toils_Reserve.Reserve(TargetIndex.A);
			yield return reserveTargetA;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
