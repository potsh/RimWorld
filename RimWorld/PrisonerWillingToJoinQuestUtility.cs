using Verse;

namespace RimWorld
{
	public static class PrisonerWillingToJoinQuestUtility
	{
		private const float RelationWithColonistWeight = 75f;

		public static Pawn GeneratePrisoner(int tile, Faction hostFaction)
		{
			PawnKindDef slave = PawnKindDefOf.Slave;
			PawnGenerationRequest request = new PawnGenerationRequest(slave, hostFaction, PawnGenerationContext.NonPlayer, tile, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 75f, forceAddFreeWarmLayerIfNeeded: true, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: true, worldPawnFactionDoesntMatter: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			pawn.guest.SetGuestStatus(hostFaction, prisoner: true);
			return pawn;
		}
	}
}
