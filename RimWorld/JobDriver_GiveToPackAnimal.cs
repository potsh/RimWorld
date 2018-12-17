using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_GiveToPackAnimal : JobDriver
	{
		private const TargetIndex ItemInd = TargetIndex.A;

		private const TargetIndex AnimalInd = TargetIndex.B;

		private Thing Item => job.GetTarget(TargetIndex.A).Thing;

		private Pawn Animal => (Pawn)job.GetTarget(TargetIndex.B).Thing;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Item;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private Toil FindCarrierToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Pawn pawn = FindCarrier();
				if (pawn == null)
				{
					base.pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					job.SetTarget(TargetIndex.B, pawn);
				}
			};
			return toil;
		}

		private Pawn FindCarrier()
		{
			IEnumerable<Pawn> enumerable = GiveToPackAnimalUtility.CarrierCandidatesFor(base.pawn);
			Pawn animal = Animal;
			if (animal != null && enumerable.Contains(animal) && animal.RaceProps.packAnimal && CanCarryAtLeastOne(animal))
			{
				return animal;
			}
			Pawn pawn = null;
			float num = -1f;
			foreach (Pawn item in enumerable)
			{
				if (item.RaceProps.packAnimal && CanCarryAtLeastOne(item))
				{
					float num2 = (float)item.Position.DistanceToSquared(base.pawn.Position);
					if (pawn == null || num2 < num)
					{
						pawn = item;
						num = num2;
					}
				}
			}
			return pawn;
		}

		private bool CanCarryAtLeastOne(Pawn carrier)
		{
			return !MassUtility.WillBeOverEncumberedAfterPickingUp(carrier, Item, 1);
		}

		private Toil GiveToCarrierAsMuchAsPossibleToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (Item == null)
				{
					pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					int count = Mathf.Min(MassUtility.CountToPickUpUntilOverEncumbered(Animal, Item), Item.stackCount);
					pawn.carryTracker.innerContainer.TryTransferToContainer(Item, Animal.inventory.innerContainer, count);
				}
			};
			return toil;
		}
	}
}
