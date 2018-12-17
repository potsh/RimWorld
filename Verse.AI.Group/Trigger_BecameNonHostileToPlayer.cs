using RimWorld;

namespace Verse.AI.Group
{
	public class Trigger_BecameNonHostileToPlayer : Trigger
	{
		public override bool ActivateOn(Lord lord, TriggerSignal signal)
		{
			if (signal.type == TriggerSignalType.FactionRelationsChanged)
			{
				FactionRelationKind? previousRelationKind = signal.previousRelationKind;
				return previousRelationKind.GetValueOrDefault() == FactionRelationKind.Hostile && previousRelationKind.HasValue && lord.faction != null && !lord.faction.HostileTo(Faction.OfPlayer);
			}
			return false;
		}
	}
}
