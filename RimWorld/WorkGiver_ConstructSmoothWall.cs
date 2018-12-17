using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class WorkGiver_ConstructSmoothWall : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfPlayer)
			{
				using (IEnumerator<Designation> enumerator = pawn.Map.designationManager.SpawnedDesignationsOfDef(DesignationDefOf.SmoothWall).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Designation des = enumerator.Current;
						yield return des.target.Cell;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_00ec:
			/*Error near IL_00ed: Unexpected return in MoveNext()*/;
		}

		public override bool HasJobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			if (c.IsForbidden(pawn) || pawn.Map.designationManager.DesignationAt(c, DesignationDefOf.SmoothWall) == null)
			{
				return false;
			}
			Building edifice = c.GetEdifice(pawn.Map);
			if (edifice == null || !edifice.def.IsSmoothable)
			{
				Log.ErrorOnce("Failed to find valid edifice when trying to smooth a wall", 58988176);
				pawn.Map.designationManager.TryRemoveDesignation(c, DesignationDefOf.SmoothWall);
				return false;
			}
			LocalTargetInfo target = edifice;
			bool ignoreOtherReservations = forced;
			if (pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
			{
				target = c;
				ignoreOtherReservations = forced;
				if (pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
				{
					return true;
				}
			}
			return false;
		}

		public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
		{
			return new Job(JobDefOf.SmoothWall, c.GetEdifice(pawn.Map));
		}
	}
}
