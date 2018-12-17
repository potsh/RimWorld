using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Hunt : JobDriver
	{
		private int jobStartTick = -1;

		private const TargetIndex VictimInd = TargetIndex.A;

		private const TargetIndex CorpseInd = TargetIndex.A;

		private const TargetIndex StoreCellInd = TargetIndex.B;

		private const int MaxHuntTicks = 5000;

		public Pawn Victim
		{
			get
			{
				Corpse corpse = Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				return (Pawn)job.GetTarget(TargetIndex.A).Thing;
			}
		}

		private Corpse Corpse => job.GetTarget(TargetIndex.A).Thing as Corpse;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref jobStartTick, "jobStartTick", 0);
		}

		public override string GetReport()
		{
			if (Victim != null)
			{
				return job.def.reportString.Replace("TargetA", Victim.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = base.pawn;
			LocalTargetInfo target = Victim;
			Job job = base.job;
			bool errorOnFailed2 = errorOnFailed;
			return pawn.Reserve(target, job, 1, -1, null, errorOnFailed2);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(delegate
			{
				if (!((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006f: stateMachine*/)._0024this.job.ignoreDesignations)
				{
					Pawn victim = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006f: stateMachine*/)._0024this.Victim;
					if (victim != null && !victim.Dead && ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_006f: stateMachine*/)._0024this.Map.designationManager.DesignationOn(victim, DesignationDefOf.Hunt) == null)
					{
						return true;
					}
				}
				return false;
			});
			yield return new Toil
			{
				initAction = delegate
				{
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0092: stateMachine*/)._0024this.jobStartTick = Find.TickManager.TicksGame;
				}
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private Toil StartCollectCorpseToil()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (Victim == null)
				{
					toil.actor.jobs.EndCurrentJob(JobCondition.Incompletable);
				}
				else
				{
					TaleRecorder.RecordTale(TaleDefOf.Hunted, pawn, Victim);
					Corpse corpse = Victim.Corpse;
					if (corpse == null || !pawn.CanReserveAndReach(corpse, PathEndMode.ClosestTouch, Danger.Deadly))
					{
						pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
					}
					else
					{
						corpse.SetForbidden(value: false);
						if (StoreUtility.TryFindBestBetterStoreCellFor(corpse, pawn, base.Map, StoragePriority.Unstored, pawn.Faction, out IntVec3 foundCell))
						{
							pawn.Reserve(corpse, job);
							pawn.Reserve(foundCell, job);
							job.SetTarget(TargetIndex.B, foundCell);
							job.SetTarget(TargetIndex.A, corpse);
							job.count = 1;
							job.haulMode = HaulMode.ToCellStorage;
						}
						else
						{
							pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
						}
					}
				}
			};
			return toil;
		}
	}
}
