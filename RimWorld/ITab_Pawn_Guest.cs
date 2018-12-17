namespace RimWorld
{
	public class ITab_Pawn_Guest : ITab_Pawn_Visitor
	{
		public override bool IsVisible => base.SelPawn.HostFaction == Faction.OfPlayer && !base.SelPawn.IsPrisoner;

		public ITab_Pawn_Guest()
		{
			labelKey = "TabGuest";
			tutorTag = "Guest";
		}
	}
}
