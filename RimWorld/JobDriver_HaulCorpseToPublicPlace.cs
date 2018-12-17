using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_HaulCorpseToPublicPlace : JobDriver
	{
		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex GraveInd = TargetIndex.B;

		private const TargetIndex CellInd = TargetIndex.C;

		private static List<IntVec3> tmpCells = new List<IntVec3>();

		private Corpse Corpse => (Corpse)job.GetTarget(TargetIndex.A).Thing;

		private Building_Grave Grave => (Building_Grave)job.GetTarget(TargetIndex.B).Thing;

		private bool InGrave => Grave != null;

		private Thing Target => (Thing)(((object)Grave) ?? ((object)Corpse));

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Target;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		public override string GetReport()
		{
			if (InGrave && Grave.def == ThingDefOf.Grave)
			{
				return "ReportDiggingUpCorpse".Translate();
			}
			return base.GetReport();
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.A);
			Toil gotoCorpse = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).FailOnDespawnedOrNull(TargetIndex.A);
			yield return Toils_Jump.JumpIfTargetInvalid(TargetIndex.B, gotoCorpse);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private Toil FindCellToDropCorpseToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				IntVec3 result = IntVec3.Invalid;
				if (!Rand.Chance(0.8f) || !TryFindTableCell(out result))
				{
					bool flag = false;
					if (RCellFinder.TryFindRandomSpotJustOutsideColony(pawn, out IntVec3 result2) && CellFinder.TryRandomClosewalkCellNear(result2, pawn.Map, 5, out result, (IntVec3 x) => pawn.CanReserve(x) && x.GetFirstItem(pawn.Map) == null))
					{
						flag = true;
					}
					if (!flag)
					{
						result = CellFinder.RandomClosewalkCellNear(pawn.Position, pawn.Map, 10, (IntVec3 x) => pawn.CanReserve(x) && x.GetFirstItem(pawn.Map) == null);
					}
				}
				job.SetTarget(TargetIndex.C, result);
			};
			toil.atomicWithPrevious = true;
			return toil;
		}

		private Toil ForbidAndNotifyMentalStateToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Corpse?.SetForbidden(value: true);
				(pawn.MentalState as MentalState_CorpseObsession)?.Notify_CorpseHauled();
			};
			toil.atomicWithPrevious = true;
			return toil;
		}

		private bool TryFindTableCell(out IntVec3 cell)
		{
			tmpCells.Clear();
			List<Building> allBuildingsColonist = pawn.Map.listerBuildings.allBuildingsColonist;
			for (int i = 0; i < allBuildingsColonist.Count; i++)
			{
				Building building = allBuildingsColonist[i];
				if (building.def.IsTable)
				{
					CellRect.CellRectIterator iterator = building.OccupiedRect().GetIterator();
					while (!iterator.Done())
					{
						IntVec3 current = iterator.Current;
						if (pawn.CanReserveAndReach(current, PathEndMode.OnCell, Danger.Deadly) && current.GetFirstItem(pawn.Map) == null)
						{
							tmpCells.Add(current);
						}
						iterator.MoveNext();
					}
				}
			}
			if (!tmpCells.Any())
			{
				cell = IntVec3.Invalid;
				return false;
			}
			cell = tmpCells.RandomElement();
			return true;
		}
	}
}
