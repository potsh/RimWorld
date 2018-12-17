using RimWorld;
using System.Collections.Generic;
using Verse.AI;

namespace Verse
{
	public class PriorityWork : IExposable
	{
		private Pawn pawn;

		private IntVec3 prioritizedCell = IntVec3.Invalid;

		private WorkTypeDef prioritizedWorkType;

		private int prioritizeTick = Find.TickManager.TicksGame;

		private const int Timeout = 30000;

		public bool IsPrioritized
		{
			get
			{
				if (prioritizedCell.IsValid)
				{
					if (Find.TickManager.TicksGame < prioritizeTick + 30000)
					{
						return true;
					}
					Clear();
				}
				return false;
			}
		}

		public IntVec3 Cell => prioritizedCell;

		public WorkTypeDef WorkType => prioritizedWorkType;

		public PriorityWork()
		{
		}

		public PriorityWork(Pawn pawn)
		{
			this.pawn = pawn;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref prioritizedCell, "prioritizedCell");
			Scribe_Defs.Look(ref prioritizedWorkType, "prioritizedWorkType");
			Scribe_Values.Look(ref prioritizeTick, "prioritizeTick", 0);
		}

		public void Set(IntVec3 prioritizedCell, WorkTypeDef prioritizedWorkType)
		{
			this.prioritizedCell = prioritizedCell;
			this.prioritizedWorkType = prioritizedWorkType;
			prioritizeTick = Find.TickManager.TicksGame;
		}

		public void Clear()
		{
			prioritizedCell = IntVec3.Invalid;
			prioritizedWorkType = null;
			prioritizeTick = 0;
		}

		public void ClearPrioritizedWorkAndJobQueue()
		{
			Clear();
			pawn.jobs.ClearQueuedJobs();
		}

		public IEnumerable<Gizmo> GetGizmos()
		{
			if ((IsPrioritized || (pawn.CurJob != null && pawn.CurJob.playerForced) || pawn.jobs.jobQueue.AnyPlayerForced) && !pawn.Drafted)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandClearPrioritizedWork".Translate(),
					defaultDesc = "CommandClearPrioritizedWorkDesc".Translate(),
					icon = TexCommand.ClearPrioritizedWork,
					activateSound = SoundDefOf.Tick_Low,
					action = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_00ef: stateMachine*/)._0024this.ClearPrioritizedWorkAndJobQueue();
						if (((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_00ef: stateMachine*/)._0024this.pawn.CurJob.playerForced)
						{
							((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_00ef: stateMachine*/)._0024this.pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
						}
					},
					hotKey = KeyBindingDefOf.Designator_Cancel,
					groupKey = 6165612
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
