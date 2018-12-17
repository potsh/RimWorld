using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_FoodDeliver : JobDriver
	{
		private bool usingNutrientPasteDispenser;

		private bool eatingFromInventory;

		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;

		private Pawn Deliveree => (Pawn)job.targetB.Thing;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser", defaultValue: false);
			Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory", defaultValue: false);
		}

		public override string GetReport()
		{
			if (job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && Deliveree != null)
			{
				return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", Deliveree.LabelShort);
			}
			return base.GetReport();
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			usingNutrientPasteDispenser = (base.TargetThingA is Building_NutrientPasteDispenser);
			eatingFromInventory = (pawn.inventory != null && pawn.inventory.Contains(base.TargetThingA));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Deliveree;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.B);
			if (!eatingFromInventory)
			{
				if (!usingNutrientPasteDispenser)
				{
					yield return Toils_Reserve.Reserve(TargetIndex.A);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnForbidden(TargetIndex.A);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
