using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_Merge : WorkGiver_Scanner
	{
		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			return pawn.Map.listerMergeables.ThingsPotentiallyNeedingMerging();
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.stackCount == t.def.stackLimit)
			{
				return null;
			}
			if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t, forced))
			{
				return null;
			}
			SlotGroup slotGroup = t.GetSlotGroup();
			if (slotGroup == null)
			{
				return null;
			}
			LocalTargetInfo target = t.Position;
			bool ignoreOtherReservations = forced;
			if (!pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
			{
				return null;
			}
			foreach (Thing heldThing in slotGroup.HeldThings)
			{
				if (heldThing != t && heldThing.def == t.def && (forced || heldThing.stackCount >= t.stackCount) && heldThing.stackCount < heldThing.def.stackLimit)
				{
					target = heldThing.Position;
					ignoreOtherReservations = forced;
					if (pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations) && pawn.CanReserve(heldThing) && heldThing.Position.IsValidStorageFor(heldThing.Map, t))
					{
						Job job = new Job(JobDefOf.HaulToCell, t, heldThing.Position);
						job.count = Mathf.Min(heldThing.def.stackLimit - heldThing.stackCount, t.stackCount);
						job.haulMode = HaulMode.ToCellStorage;
						return job;
					}
				}
			}
			JobFailReason.Is("NoMergeTarget".Translate());
			return null;
		}
	}
}
