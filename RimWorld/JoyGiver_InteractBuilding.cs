using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class JoyGiver_InteractBuilding : JoyGiver
	{
		private static List<Thing> tmpCandidates = new List<Thing>();

		protected virtual bool CanDoDuringParty => false;

		public override Job TryGiveJob(Pawn pawn)
		{
			Thing thing = FindBestGame(pawn, inBed: false, IntVec3.Invalid);
			if (thing != null)
			{
				return TryGivePlayJob(pawn, thing);
			}
			return null;
		}

		public override Job TryGiveJobWhileInBed(Pawn pawn)
		{
			Thing thing = FindBestGame(pawn, inBed: true, IntVec3.Invalid);
			if (thing != null)
			{
				return TryGivePlayJobWhileInBed(pawn, thing);
			}
			return null;
		}

		public override Job TryGiveJobInPartyArea(Pawn pawn, IntVec3 partySpot)
		{
			if (!CanDoDuringParty)
			{
				return null;
			}
			Thing thing = FindBestGame(pawn, inBed: false, partySpot);
			if (thing != null)
			{
				return TryGivePlayJob(pawn, thing);
			}
			return null;
		}

		private Thing FindBestGame(Pawn pawn, bool inBed, IntVec3 partySpot)
		{
			tmpCandidates.Clear();
			GetSearchSet(pawn, tmpCandidates);
			if (tmpCandidates.Count == 0)
			{
				return null;
			}
			Predicate<Thing> predicate = (Thing t) => CanInteractWith(pawn, t, inBed);
			if (partySpot.IsValid)
			{
				Predicate<Thing> oldValidator = predicate;
				predicate = ((Thing x) => PartyUtility.InPartyArea(x.Position, partySpot, pawn.Map) && oldValidator(x));
			}
			IntVec3 position = pawn.Position;
			Map map = pawn.Map;
			List<Thing> searchSet = tmpCandidates;
			PathEndMode peMode = PathEndMode.OnCell;
			TraverseParms traverseParams = TraverseParms.For(pawn);
			Predicate<Thing> validator = predicate;
			Thing result = GenClosest.ClosestThing_Global_Reachable(position, map, searchSet, peMode, traverseParams, 9999f, validator);
			tmpCandidates.Clear();
			return result;
		}

		protected virtual bool CanInteractWith(Pawn pawn, Thing t, bool inBed)
		{
			if (!pawn.CanReserve(t, def.jobDef.joyMaxParticipants))
			{
				return false;
			}
			if (t.IsForbidden(pawn))
			{
				return false;
			}
			if (!t.IsSociallyProper(pawn))
			{
				return false;
			}
			if (!t.IsPoliticallyProper(pawn))
			{
				return false;
			}
			CompPowerTrader compPowerTrader = t.TryGetComp<CompPowerTrader>();
			if (compPowerTrader != null && !compPowerTrader.PowerOn)
			{
				return false;
			}
			if (def.unroofedOnly && t.Position.Roofed(t.Map))
			{
				return false;
			}
			return true;
		}

		protected abstract Job TryGivePlayJob(Pawn pawn, Thing bestGame);

		protected virtual Job TryGivePlayJobWhileInBed(Pawn pawn, Thing bestGame)
		{
			Building_Bed t = pawn.CurrentBed();
			return new Job(def.jobDef, bestGame, pawn.Position, t);
		}
	}
}
