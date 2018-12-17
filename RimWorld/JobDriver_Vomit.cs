using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Vomit : JobDriver
	{
		private int ticksLeft;

		public override void SetInitialPosture()
		{
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil to = new Toil
			{
				initAction = delegate
				{
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.ticksLeft = Rand.Range(300, 900);
					int num = 0;
					IntVec3 c;
					do
					{
						c = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.pawn.Position + GenAdj.AdjacentCellsAndInside[Rand.Range(0, 9)];
						num++;
						if (num > 12)
						{
							c = ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.pawn.Position;
							break;
						}
					}
					while (!c.InBounds(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.pawn.Map) || !c.Standable(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.pawn.Map));
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.job.targetA = c;
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/)._0024this.pawn.pather.StopDead();
				},
				tickAction = delegate
				{
					if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.ticksLeft % 150 == 149)
					{
						FilthMaker.MakeFilth(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.job.targetA.Cell, ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.Map, ThingDefOf.Filth_Vomit, ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.pawn.LabelIndefinite());
						if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.pawn.needs.food.CurLevelPercentage > 0.1f)
						{
							((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.pawn.needs.food.CurLevel -= ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.pawn.needs.food.MaxLevel * 0.04f;
						}
					}
					((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.ticksLeft--;
					if (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.ticksLeft <= 0)
					{
						((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.ReadyForNextToil();
						TaleRecorder.RecordTale(TaleDefOf.Vomited, ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0049: stateMachine*/)._0024this.pawn);
					}
				},
				defaultCompleteMode = ToilCompleteMode.Never
			};
			to.WithEffect(EffecterDefOf.Vomit, TargetIndex.A);
			to.PlaySustainerOrSound(() => SoundDefOf.Vomit);
			yield return to;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
