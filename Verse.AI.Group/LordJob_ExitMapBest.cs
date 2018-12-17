namespace Verse.AI.Group
{
	public class LordJob_ExitMapBest : LordJob
	{
		private LocomotionUrgency locomotion = LocomotionUrgency.Jog;

		private bool canDig;

		public LordJob_ExitMapBest()
		{
		}

		public LordJob_ExitMapBest(LocomotionUrgency locomotion, bool canDig = false)
		{
			this.locomotion = locomotion;
			this.canDig = canDig;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(locomotion, canDig);
			lordToil_ExitMap.useAvoidGrid = true;
			stateGraph.AddToil(lordToil_ExitMap);
			return stateGraph;
		}

		public override void ExposeData()
		{
			Scribe_Values.Look(ref locomotion, "locomotion", LocomotionUrgency.Jog);
			Scribe_Values.Look(ref canDig, "canDig", defaultValue: false);
		}
	}
}
