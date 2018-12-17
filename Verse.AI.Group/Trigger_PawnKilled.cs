namespace Verse.AI.Group
{
	public class Trigger_PawnKilled : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.PawnLost)
			{
				return signal.condition == PawnLostCondition.IncappedOrKilled && signal.Pawn.Dead;
			}
			return false;
		}
	}
}
