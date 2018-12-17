using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class InteractionUtility
	{
		public const float MaxInteractRange = 6f;

		public static bool CanInitiateInteraction(Pawn pawn)
		{
			if (pawn.interactions == null)
			{
				return false;
			}
			if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking))
			{
				return false;
			}
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.IsBurning())
			{
				return false;
			}
			return true;
		}

		public static bool CanReceiveInteraction(Pawn pawn)
		{
			if (!pawn.Awake())
			{
				return false;
			}
			if (pawn.IsBurning())
			{
				return false;
			}
			return true;
		}

		public static bool CanInitiateRandomInteraction(Pawn p)
		{
			if (!CanInitiateInteraction(p))
			{
				return false;
			}
			if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState)
			{
				return false;
			}
			if (p.Faction == null)
			{
				return false;
			}
			return true;
		}

		public static bool CanReceiveRandomInteraction(Pawn p)
		{
			if (!CanReceiveInteraction(p))
			{
				return false;
			}
			if (!p.RaceProps.Humanlike || p.Downed || p.InAggroMentalState)
			{
				return false;
			}
			return true;
		}

		public static bool IsGoodPositionForInteraction(Pawn p, Pawn recipient)
		{
			return IsGoodPositionForInteraction(p.Position, recipient.Position, p.Map);
		}

		public static bool IsGoodPositionForInteraction(IntVec3 cell, IntVec3 recipientCell, Map map)
		{
			return cell.InHorDistOf(recipientCell, 6f) && GenSight.LineOfSight(cell, recipientCell, map, skipFirstCell: true);
		}

		public static bool HasAnyVerbForSocialFight(Pawn p)
		{
			if (p.Dead)
			{
				return false;
			}
			List<Verb> allVerbs = p.verbTracker.AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				if (allVerbs[i].IsMeleeAttack && allVerbs[i].IsStillUsableBy(p))
				{
					return true;
				}
			}
			return false;
		}

		public static bool TryGetRandomVerbForSocialFight(Pawn p, out Verb verb)
		{
			if (p.Dead)
			{
				verb = null;
				return false;
			}
			List<Verb> allVerbs = p.verbTracker.AllVerbs;
			return (from x in allVerbs
			where x.IsMeleeAttack && x.IsStillUsableBy(p)
			select x).TryRandomElementByWeight((Verb x) => x.verbProps.AdjustedMeleeDamageAmount(x, p), out verb);
		}
	}
}
