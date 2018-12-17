using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_RescueDowned : WorkGiver_TakeToBed
	{
		private const float MinDistFromEnemy = 40f;

		public override PathEndMode PathEndMode => PathEndMode.OnCell;

		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			if (pawn2 != null && pawn2.Downed && pawn2.Faction == pawn.Faction && !pawn2.InBed())
			{
				LocalTargetInfo target = pawn2;
				bool ignoreOtherReservations = forced;
				if (pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations) && !GenAI.EnemyIsNear(pawn2, 40f))
				{
					Thing thing = FindBed(pawn, pawn2);
					if (thing != null && pawn2.CanReserve(thing))
					{
						return true;
					}
					return false;
				}
			}
			return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Pawn pawn2 = t as Pawn;
			Thing t2 = FindBed(pawn, pawn2);
			Job job = new Job(JobDefOf.Rescue, pawn2, t2);
			job.count = 1;
			return job;
		}
	}
}
