using System;
using System.Runtime.CompilerServices;

namespace Verse.AI
{
	public class JobGiver_WanderCurrentRoom : JobGiver_Wander
	{
		[CompilerGenerated]
		private static Func<Pawn, IntVec3, IntVec3, bool> _003C_003Ef__mg_0024cache0;

		public JobGiver_WanderCurrentRoom()
		{
			wanderRadius = 7f;
			ticksBetweenWandersRange = new IntRange(125, 200);
			locomotionUrgency = LocomotionUrgency.Amble;
			wanderDestValidator = WanderRoomUtility.IsValidWanderDest;
		}

		protected override IntVec3 GetWanderRoot(Pawn pawn)
		{
			return pawn.Position;
		}
	}
}
