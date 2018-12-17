using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JoyGiver_InPrivateRoom : JoyGiver
	{
		public override Job TryGiveJob(Pawn pawn)
		{
			if (pawn.ownership == null)
			{
				return null;
			}
			Room ownedRoom = pawn.ownership.OwnedRoom;
			if (ownedRoom == null)
			{
				return null;
			}
			if (!(from c in ownedRoom.Cells
			where c.Standable(pawn.Map) && !c.IsForbidden(pawn) && pawn.CanReserveAndReach(c, PathEndMode.OnCell, Danger.None)
			select c).TryRandomElement(out IntVec3 result))
			{
				return null;
			}
			return new Job(def.jobDef, result);
		}

		public override Job TryGiveJobWhileInBed(Pawn pawn)
		{
			return new Job(def.jobDef, pawn.CurrentBed());
		}
	}
}
