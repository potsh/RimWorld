using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_MapGen_AncientPodContents : ThingSetMaker
	{
		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			PodContentsType? podContentsType = parms.podContentsType;
			PodContentsType podContentsType2 = (!podContentsType.HasValue) ? Gen.RandomEnumValue<PodContentsType>(disallowFirstValue: true) : podContentsType.Value;
			switch (podContentsType2)
			{
			case PodContentsType.Empty:
				break;
			case PodContentsType.AncientFriendly:
				outThings.Add(GenerateFriendlyAncient());
				break;
			case PodContentsType.AncientIncapped:
				outThings.Add(GenerateIncappedAncient());
				break;
			case PodContentsType.AncientHostile:
				outThings.Add(GenerateAngryAncient());
				break;
			case PodContentsType.Slave:
				outThings.Add(GenerateSlave());
				break;
			case PodContentsType.AncientHalfEaten:
				outThings.Add(GenerateHalfEatenAncient());
				outThings.AddRange(GenerateScarabs());
				break;
			default:
				Log.Error("Pod contents type not handled: " + podContentsType2);
				break;
			}
		}

		private Pawn GenerateFriendlyAncient()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateIncappedAncient()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn);
			GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateSlave()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Slave, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			HealthUtility.DamageUntilDowned(pawn);
			GiveRandomLootInventoryForTombPawn(pawn);
			if (Rand.Value < 0.5f)
			{
				HealthUtility.DamageUntilDead(pawn);
			}
			return pawn;
		}

		private Pawn GenerateAngryAncient()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private Pawn GenerateHalfEatenAncient()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AncientSoldier, Faction.OfAncients, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: false, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowFood: true, inhabitant: false, certainlyBeenInCryptosleep: true);
			Pawn pawn = PawnGenerator.GeneratePawn(request);
			int num = Rand.Range(6, 10);
			for (int i = 0; i < num; i++)
			{
				Pawn pawn2 = pawn;
				DamageDef bite = DamageDefOf.Bite;
				float amount = (float)Rand.Range(3, 8);
				Pawn instigator = pawn;
				pawn2.TakeDamage(new DamageInfo(bite, amount, 0f, -1f, instigator));
			}
			GiveRandomLootInventoryForTombPawn(pawn);
			return pawn;
		}

		private List<Thing> GenerateScarabs()
		{
			List<Thing> list = new List<Thing>();
			int num = Rand.Range(3, 6);
			for (int i = 0; i < num; i++)
			{
				Pawn pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Megascarab);
				pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
				list.Add(pawn);
			}
			return list;
		}

		private void GiveRandomLootInventoryForTombPawn(Pawn p)
		{
			if (Rand.Value < 0.65f)
			{
				MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Gold, Rand.Range(10, 50));
			}
			else
			{
				MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.Plasteel, Rand.Range(10, 50));
			}
			if (Rand.Value < 0.7f)
			{
				MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentIndustrial, Rand.Range(-2, 4));
			}
			else
			{
				MakeIntoContainer(p.inventory.innerContainer, ThingDefOf.ComponentSpacer, Rand.Range(-2, 4));
			}
		}

		private void MakeIntoContainer(ThingOwner container, ThingDef def, int count)
		{
			if (count > 0)
			{
				Thing thing = ThingMaker.MakeThing(def);
				thing.stackCount = count;
				container.TryAdd(thing);
			}
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			yield return PawnKindDefOf.AncientSoldier.race;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
