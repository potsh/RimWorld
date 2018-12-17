using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld
{
	public static class ResurrectionUtility
	{
		private static SimpleCurve DementiaChancePerRotDaysCurve = new SimpleCurve
		{
			new CurvePoint(0.1f, 0.02f),
			new CurvePoint(5f, 0.8f)
		};

		private static SimpleCurve BlindnessChancePerRotDaysCurve = new SimpleCurve
		{
			new CurvePoint(0.1f, 0.02f),
			new CurvePoint(5f, 0.8f)
		};

		private static SimpleCurve ResurrectionPsychosisChancePerRotDaysCurve = new SimpleCurve
		{
			new CurvePoint(0.1f, 0.02f),
			new CurvePoint(5f, 0.8f)
		};

		public static void Resurrect(Pawn pawn)
		{
			if (!pawn.Dead)
			{
				Log.Error("Tried to resurrect a pawn who is not dead: " + pawn.ToStringSafe());
			}
			else if (pawn.Discarded)
			{
				Log.Error("Tried to resurrect a discarded pawn: " + pawn.ToStringSafe());
			}
			else
			{
				Corpse corpse = pawn.Corpse;
				bool flag = false;
				IntVec3 loc = IntVec3.Invalid;
				Map map = null;
				if (corpse != null)
				{
					flag = corpse.Spawned;
					loc = corpse.Position;
					map = corpse.Map;
					corpse.InnerPawn = null;
					corpse.Destroy();
				}
				if (flag && pawn.IsWorldPawn())
				{
					Find.WorldPawns.RemovePawn(pawn);
				}
				pawn.ForceSetStateToUnspawned();
				PawnComponentsUtility.CreateInitialComponents(pawn);
				pawn.health.Notify_Resurrected();
				if (pawn.Faction != null && pawn.Faction.IsPlayer)
				{
					if (pawn.workSettings != null)
					{
						pawn.workSettings.EnableAndInitialize();
					}
					Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
				}
				if (flag)
				{
					GenSpawn.Spawn(pawn, loc, map);
					for (int i = 0; i < 10; i++)
					{
						MoteMaker.ThrowAirPuffUp(pawn.DrawPos, map);
					}
					if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostileTo(Faction.OfPlayer))
					{
						LordMaker.MakeNewLord(pawn.Faction, new LordJob_AssaultColony(pawn.Faction), pawn.Map, Gen.YieldSingle(pawn));
					}
					if (pawn.apparel != null)
					{
						List<Apparel> wornApparel = pawn.apparel.WornApparel;
						for (int j = 0; j < wornApparel.Count; j++)
						{
							wornApparel[j].Notify_PawnResurrected();
						}
					}
				}
				PawnDiedOrDownedThoughtsUtility.RemoveDiedThoughts(pawn);
			}
		}

		public static void ResurrectWithSideEffects(Pawn pawn)
		{
			Corpse corpse = pawn.Corpse;
			float x2;
			if (corpse != null)
			{
				CompRottable comp = corpse.GetComp<CompRottable>();
				x2 = comp.RotProgress / 60000f;
			}
			else
			{
				x2 = 0f;
			}
			Resurrect(pawn);
			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.ResurrectionSickness, pawn);
			if (!pawn.health.WouldDieAfterAddingHediff(hediff))
			{
				pawn.health.AddHediff(hediff);
			}
			float chance = DementiaChancePerRotDaysCurve.Evaluate(x2);
			if (Rand.Chance(chance) && brain != null)
			{
				Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.Dementia, pawn, brain);
				if (!pawn.health.WouldDieAfterAddingHediff(hediff2))
				{
					pawn.health.AddHediff(hediff2);
				}
			}
			float chance2 = BlindnessChancePerRotDaysCurve.Evaluate(x2);
			if (Rand.Chance(chance2))
			{
				IEnumerable<BodyPartRecord> enumerable = from x in pawn.health.hediffSet.GetNotMissingParts()
				where x.def == BodyPartDefOf.Eye
				select x;
				foreach (BodyPartRecord item in enumerable)
				{
					Hediff hediff3 = HediffMaker.MakeHediff(HediffDefOf.Blindness, pawn, item);
					pawn.health.AddHediff(hediff3);
				}
			}
			if (brain != null)
			{
				float chance3 = ResurrectionPsychosisChancePerRotDaysCurve.Evaluate(x2);
				if (Rand.Chance(chance3))
				{
					Hediff hediff4 = HediffMaker.MakeHediff(HediffDefOf.ResurrectionPsychosis, pawn, brain);
					if (!pawn.health.WouldDieAfterAddingHediff(hediff4))
					{
						pawn.health.AddHediff(hediff4);
					}
				}
			}
			if (pawn.Dead)
			{
				Log.Error("The pawn has died while being resurrected.");
				Resurrect(pawn);
			}
		}
	}
}
