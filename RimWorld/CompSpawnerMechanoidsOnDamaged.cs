using RimWorld.Planet;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class CompSpawnerMechanoidsOnDamaged : ThingComp
	{
		public float pointsLeft;

		private Lord lord;

		private const float MechanoidsDefendRadius = 21f;

		public static readonly string MemoDamaged = "ShipPartDamaged";

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_References.Look(ref lord, "defenseLord");
			Scribe_Values.Look(ref pointsLeft, "mechanoidPointsLeft", 0f);
		}

		public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
		{
			base.PostPreApplyDamage(dinfo, out absorbed);
			if (!absorbed)
			{
				if (dinfo.Def.harmsHealth)
				{
					if (lord != null)
					{
						lord.ReceiveMemo(MemoDamaged);
					}
					float num = (float)parent.HitPoints - dinfo.Amount;
					if ((num < (float)parent.MaxHitPoints * 0.98f && dinfo.Instigator != null && dinfo.Instigator.Faction != null) || num < (float)parent.MaxHitPoints * 0.9f)
					{
						TrySpawnMechanoids();
					}
				}
				absorbed = false;
			}
		}

		public void Notify_BlueprintReplacedWithSolidThingNearby(Pawn by)
		{
			if (by.Faction != Faction.OfMechanoids)
			{
				TrySpawnMechanoids();
			}
		}

		private void TrySpawnMechanoids()
		{
			if (!(pointsLeft <= 0f) && parent.Spawned)
			{
				if (lord == null)
				{
					if (!CellFinder.TryFindRandomCellNear(parent.Position, parent.Map, 5, (IntVec3 c) => c.Standable(parent.Map) && parent.Map.reachability.CanReach(c, parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)), out IntVec3 result))
					{
						Log.Error("Found no place for mechanoids to defend " + this);
						result = IntVec3.Invalid;
					}
					LordJob_MechanoidsDefendShip lordJob = new LordJob_MechanoidsDefendShip(parent, parent.Faction, 21f, result);
					lord = LordMaker.MakeNewLord(Faction.OfMechanoids, lordJob, parent.Map);
				}
				try
				{
					IntVec3 result3;
					PawnKindDef result2;
					while (pointsLeft > 0f && (from def in DefDatabase<PawnKindDef>.AllDefs
					where def.RaceProps.IsMechanoid && def.isFighter && def.combatPower <= pointsLeft
					select def).TryRandomElement(out result2) && (from cell in GenAdj.CellsAdjacent8Way(parent)
					where CanSpawnMechanoidAt(cell)
					select cell).TryRandomElement(out result3))
					{
						PawnGenerationRequest request = new PawnGenerationRequest(result2, Faction.OfMechanoids, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true);
						Pawn pawn = PawnGenerator.GeneratePawn(request);
						if (!GenPlace.TryPlaceThing(pawn, result3, parent.Map, ThingPlaceMode.Near))
						{
							Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
							break;
						}
						lord.AddPawn(pawn);
						pointsLeft -= pawn.kindDef.combatPower;
					}
				}
				finally
				{
					pointsLeft = 0f;
				}
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
			}
		}

		private bool CanSpawnMechanoidAt(IntVec3 c)
		{
			return c.Walkable(parent.Map);
		}
	}
}
