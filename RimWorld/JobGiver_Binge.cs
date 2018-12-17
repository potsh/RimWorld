using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JobGiver_Binge : ThinkNode_JobGiver
	{
		protected bool IgnoreForbid(Pawn pawn)
		{
			return pawn.InMentalState;
		}

		protected abstract int IngestInterval(Pawn pawn);

		protected override Job TryGiveJob(Pawn pawn)
		{
			int num = Find.TickManager.TicksGame - pawn.mindState.lastIngestTick;
			if (num > IngestInterval(pawn))
			{
				Job job = IngestJob(pawn);
				if (job != null)
				{
					return job;
				}
			}
			return null;
		}

		private Job IngestJob(Pawn pawn)
		{
			Thing thing = BestIngestTarget(pawn);
			if (thing == null)
			{
				return null;
			}
			ThingDef finalIngestibleDef = FoodUtility.GetFinalIngestibleDef(thing);
			Job job = new Job(JobDefOf.Ingest, thing);
			job.count = finalIngestibleDef.ingestible.maxNumToIngestAtOnce;
			job.ignoreForbidden = IgnoreForbid(pawn);
			job.overeat = true;
			return job;
		}

		protected abstract Thing BestIngestTarget(Pawn pawn);
	}
}
