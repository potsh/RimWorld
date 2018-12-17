using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FoodFeedPatient : JobDriver
	{
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;

		private const float FeedDurationMultiplier = 1.5f;

		protected Thing Food => job.targetA.Thing;

		protected Pawn Deliveree => (Pawn)job.targetB.Thing;

		public override string GetReport()
		{
			if (job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && Deliveree != null)
			{
				return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", Deliveree.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Deliveree;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
			{
				return false;
			}
			if (!(base.TargetThingA is Building_NutrientPasteDispenser) && (base.pawn.inventory == null || !base.pawn.inventory.Contains(base.TargetThingA)))
			{
				pawn = base.pawn;
				target = Food;
				job = base.job;
				errorOnFailed2 = errorOnFailed;
				if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
				{
					return false;
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
			this.FailOn(() => !FoodUtility.ShouldBeFedBySomeone(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0050: stateMachine*/)._0024this.Deliveree));
			if (pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!(base.TargetThingA is Building_NutrientPasteDispenser))
			{
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnForbidden(TargetIndex.A);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
