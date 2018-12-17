using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FactionUtility
	{
		public static bool HostileTo(this Faction fac, Faction other)
		{
			if (fac == null || other == null || other == fac)
			{
				return false;
			}
			return fac.RelationWith(other).kind == FactionRelationKind.Hostile;
		}

		public static Faction DefaultFactionFrom(FactionDef ft)
		{
			if (ft == null)
			{
				return null;
			}
			if (ft.isPlayer)
			{
				return Faction.OfPlayer;
			}
			if ((from fac in Find.FactionManager.AllFactions
			where fac.def == ft
			select fac).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public static bool IsPoliticallyProper(this Thing thing, Pawn pawn)
		{
			if (thing.Faction == null)
			{
				return true;
			}
			if (pawn.Faction == null)
			{
				return true;
			}
			if (thing.Faction == pawn.Faction)
			{
				return true;
			}
			if (thing.Faction == pawn.HostFaction)
			{
				return true;
			}
			return false;
		}
	}
}
