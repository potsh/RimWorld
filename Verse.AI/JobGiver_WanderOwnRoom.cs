using System;
using System.Runtime.CompilerServices;

namespace Verse.AI
{
	public class JobGiver_WanderOwnRoom : JobGiver_Wander
	{
		[CompilerGenerated]
		private static Func<Pawn, IntVec3, IntVec3, bool> _003C_003Ef__mg_0024cache0;

		public JobGiver_WanderOwnRoom()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(300, 600);
			locomotionUrgency = LocomotionUrgency.Amble;
			wanderDestValidator = WanderRoomUtility.IsValidWanderDest;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return (pawn.MentalState as MentalState_WanderOwnRoom)?.target ?? pawn.Position;
		}
	}
}
