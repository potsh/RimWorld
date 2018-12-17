using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class PawnGenerator
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct PawnGenerationStatus
		{
			public Pawn Pawn
			{
				get;
				private set;
			}

			public List<Pawn> PawnsGeneratedInTheMeantime
			{
				get;
				private set;
			}

			public PawnGenerationStatus(Pawn pawn, List<Pawn> pawnsGeneratedInTheMeantime)
			{
				this = default(PawnGenerationStatus);
				Pawn = pawn;
				PawnsGeneratedInTheMeantime = pawnsGeneratedInTheMeantime;
			}
		}

		private static List<PawnGenerationStatus> pawnsBeingGenerated = new List<PawnGenerationStatus>();

		private static PawnRelationDef[] relationsGeneratableBlood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
		where rel.familyByBloodRelation && rel.generationChanceFactor > 0f
		select rel).ToArray();

		private static PawnRelationDef[] relationsGeneratableNonblood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
		where !rel.familyByBloodRelation && rel.generationChanceFactor > 0f
		select rel).ToArray();

		public const float MaxStartMentalBreakThreshold = 0.4f;

		private static SimpleCurve DefaultAgeGenerationCurve = new SimpleCurve
		{
			new CurvePoint(0.05f, 0f),
			new CurvePoint(0.1f, 100f),
			new CurvePoint(0.675f, 100f),
			new CurvePoint(0.75f, 30f),
			new CurvePoint(0.875f, 18f),
			new CurvePoint(1f, 10f),
			new CurvePoint(1.125f, 3f),
			new CurvePoint(1.25f, 0f)
		};

		public const float MaxGeneratedMechanoidAge = 2500f;

		private static readonly SimpleCurve AgeSkillMaxFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(10f, 0.7f),
			new CurvePoint(35f, 1f),
			new CurvePoint(60f, 1.6f)
		};

		private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(10f, 10f),
			new CurvePoint(20f, 16f),
			new CurvePoint(27f, 20f)
		};

		private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(0.5f, 150f),
			new CurvePoint(4f, 150f),
			new CurvePoint(5f, 25f),
			new CurvePoint(10f, 5f),
			new CurvePoint(15f, 0f)
		};

		[CompilerGenerated]
		private static Func<Pawn, bool> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<Pawn, float> _003C_003Ef__mg_0024cache1;

		[CompilerGenerated]
		private static Func<Pawn, float> _003C_003Ef__mg_0024cache2;

		[CompilerGenerated]
		private static Func<Pawn, float> _003C_003Ef__mg_0024cache3;

		public static void Reset()
		{
			relationsGeneratableBlood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
			where rel.familyByBloodRelation && rel.generationChanceFactor > 0f
			select rel).ToArray();
			relationsGeneratableNonblood = (from rel in DefDatabase<PawnRelationDef>.AllDefsListForReading
			where !rel.familyByBloodRelation && rel.generationChanceFactor > 0f
			select rel).ToArray();
		}

		public static Pawn GeneratePawn(PawnKindDef kindDef, Faction faction = null)
		{
			return GeneratePawn(new PawnGenerationRequest(kindDef, faction));
		}

		public static Pawn GeneratePawn(PawnGenerationRequest request)
		{
			try
			{
				Pawn pawn = GenerateOrRedressPawnInternal(request);
				if (pawn != null && !request.AllowDead && pawn.health.hediffSet.hediffs.Any())
				{
					bool dead = pawn.Dead;
					bool downed = pawn.Downed;
					pawn.health.hediffSet.DirtyCache();
					pawn.health.CheckForStateChange(null, null);
					if (pawn.Dead)
					{
						Log.Error("Pawn was generated dead but the pawn generation request specified the pawn must be alive. This shouldn't ever happen even if we ran out of tries because null pawn should have been returned instead in this case. Resetting health...\npawn.Dead=" + pawn.Dead + " pawn.Downed=" + pawn.Downed + " deadBefore=" + dead + " downedBefore=" + downed + "\nrequest=" + request);
						pawn.health.Reset();
					}
				}
				if (pawn.Faction == Faction.OfPlayerSilentFail)
				{
					Find.StoryWatcher.watcherPopAdaptation.Notify_PawnEvent(pawn, PopAdaptationEvent.GainedColonist);
				}
				return pawn;
			}
			catch (Exception arg)
			{
				Log.Error("Error while generating pawn. Rethrowing. Exception: " + arg);
				throw;
			}
			finally
			{
			}
		}

		private static Pawn GenerateOrRedressPawnInternal(PawnGenerationRequest request)
		{
			Pawn result = null;
			if (!request.Newborn && !request.ForceGenerateNewPawn)
			{
				if (request.ForceRedressWorldPawnIfFormerColonist)
				{
					IEnumerable<Pawn> validCandidatesToRedress = GetValidCandidatesToRedress(request);
					if (validCandidatesToRedress.Where(PawnUtility.EverBeenColonistOrTameAnimal).TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
					{
						RedressPawn(result, request);
						Find.WorldPawns.RemovePawn(result);
					}
				}
				if (result == null && request.Inhabitant && request.Tile != -1)
				{
					SettlementBase settlement = Find.WorldObjects.WorldObjectAt<SettlementBase>(request.Tile);
					if (settlement != null && settlement.previouslyGeneratedInhabitants.Any())
					{
						IEnumerable<Pawn> validCandidatesToRedress2 = GetValidCandidatesToRedress(request);
						if ((from x in validCandidatesToRedress2
						where settlement.previouslyGeneratedInhabitants.Contains(x)
						select x).TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
						{
							RedressPawn(result, request);
							Find.WorldPawns.RemovePawn(result);
						}
					}
				}
				if (result == null && Rand.Chance(ChanceToRedressAnyWorldPawn(request)))
				{
					IEnumerable<Pawn> validCandidatesToRedress3 = GetValidCandidatesToRedress(request);
					if (validCandidatesToRedress3.TryRandomElementByWeight(WorldPawnSelectionWeight, out result))
					{
						RedressPawn(result, request);
						Find.WorldPawns.RemovePawn(result);
					}
				}
			}
			bool redressed;
			if (result == null)
			{
				redressed = false;
				result = GenerateNewPawnInternal(ref request);
				if (result == null)
				{
					return null;
				}
				if (request.Inhabitant && request.Tile != -1)
				{
					Find.WorldObjects.WorldObjectAt<SettlementBase>(request.Tile)?.previouslyGeneratedInhabitants.Add(result);
				}
			}
			else
			{
				redressed = true;
			}
			if (Find.Scenario != null)
			{
				Find.Scenario.Notify_PawnGenerated(result, request.Context, redressed);
			}
			return result;
		}

		public static void RedressPawn(Pawn pawn, PawnGenerationRequest request)
		{
			try
			{
				pawn.ChangeKind(request.KindDef);
				GenerateGearFor(pawn, request);
				if (pawn.Faction != request.Faction)
				{
					pawn.SetFaction(request.Faction);
				}
				if (pawn.guest != null)
				{
					pawn.guest.SetGuestStatus(null);
				}
			}
			finally
			{
			}
		}

		public static bool IsBeingGenerated(Pawn pawn)
		{
			for (int i = 0; i < pawnsBeingGenerated.Count; i++)
			{
				if (pawnsBeingGenerated[i].Pawn == pawn)
				{
					return true;
				}
			}
			return false;
		}

		private static bool IsValidCandidateToRedress(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.def != request.KindDef.race)
			{
				return false;
			}
			if (!request.WorldPawnFactionDoesntMatter && pawn.Faction != request.Faction)
			{
				return false;
			}
			if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
			{
				return false;
			}
			if (!request.AllowDowned && pawn.Downed)
			{
				return false;
			}
			if (pawn.health.hediffSet.BleedRateTotal > 0.001f)
			{
				return false;
			}
			if (!request.CanGeneratePawnRelations && pawn.RaceProps.IsFlesh && pawn.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				return false;
			}
			if (!request.AllowGay && pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
			{
				return false;
			}
			if (request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
			{
				return false;
			}
			if (request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
			{
				return false;
			}
			if (request.FixedBiologicalAge.HasValue && pawn.ageTracker.AgeBiologicalYearsFloat != request.FixedBiologicalAge)
			{
				return false;
			}
			if (request.FixedChronologicalAge.HasValue && (float)pawn.ageTracker.AgeChronologicalYears != request.FixedChronologicalAge)
			{
				return false;
			}
			if (request.FixedGender.HasValue && pawn.gender != request.FixedGender)
			{
				return false;
			}
			if (request.FixedLastName != null && ((NameTriple)pawn.Name).Last != request.FixedLastName)
			{
				return false;
			}
			if (request.FixedMelanin.HasValue && pawn.story != null && pawn.story.melanin != request.FixedMelanin)
			{
				return false;
			}
			if (request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, tryingToRedress: true, request))
			{
				return false;
			}
			if (request.MustBeCapableOfViolence)
			{
				if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
				{
					return false;
				}
				if (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
				{
					return false;
				}
			}
			return true;
		}

		private static Pawn GenerateNewPawnInternal(ref PawnGenerationRequest request)
		{
			Pawn pawn = null;
			string error = null;
			bool ignoreScenarioRequirements = false;
			bool ignoreValidator = false;
			for (int i = 0; i < 120; i++)
			{
				if (i == 70)
				{
					Log.Error("Could not generate a pawn after " + 70 + " tries. Last error: " + error + " Ignoring scenario requirements.");
					ignoreScenarioRequirements = true;
				}
				if (i == 100)
				{
					Log.Error("Could not generate a pawn after " + 100 + " tries. Last error: " + error + " Ignoring validator.");
					ignoreValidator = true;
				}
				PawnGenerationRequest request2 = request;
				pawn = TryGenerateNewPawnInternal(ref request2, out error, ignoreScenarioRequirements, ignoreValidator);
				if (pawn != null)
				{
					request = request2;
					break;
				}
			}
			if (pawn == null)
			{
				Log.Error("Pawn generation error: " + error + " Too many tries (" + 120 + "), returning null. Generation request: " + request);
				return null;
			}
			return pawn;
		}

		private static Pawn TryGenerateNewPawnInternal(ref PawnGenerationRequest request, out string error, bool ignoreScenarioRequirements, bool ignoreValidator)
		{
			error = null;
			Pawn pawn = (Pawn)ThingMaker.MakeThing(request.KindDef.race);
			pawnsBeingGenerated.Add(new PawnGenerationStatus(pawn, null));
			try
			{
				pawn.kindDef = request.KindDef;
				pawn.SetFactionDirect(request.Faction);
				PawnComponentsUtility.CreateInitialComponents(pawn);
				if (request.FixedGender.HasValue)
				{
					pawn.gender = request.FixedGender.Value;
				}
				else if (pawn.RaceProps.hasGenders)
				{
					if (Rand.Value < 0.5f)
					{
						pawn.gender = Gender.Male;
					}
					else
					{
						pawn.gender = Gender.Female;
					}
				}
				else
				{
					pawn.gender = Gender.None;
				}
				GenerateRandomAge(pawn, request);
				pawn.needs.SetInitialLevels();
				if (!request.Newborn && request.CanGeneratePawnRelations)
				{
					GeneratePawnRelations(pawn, ref request);
				}
				if (pawn.RaceProps.Humanlike)
				{
					Faction faction;
					FactionDef factionType = (request.Faction != null) ? request.Faction.def : ((!Find.FactionManager.TryGetRandomNonColonyHumanlikeFaction(out faction, tryMedievalOrBetter: false, allowDefeated: true)) ? Faction.OfAncients.def : faction.def);
					pawn.story.melanin = ((!request.FixedMelanin.HasValue) ? PawnSkinColors.RandomMelanin(request.Faction) : request.FixedMelanin.Value);
					pawn.story.crownType = ((Rand.Value < 0.5f) ? CrownType.Average : CrownType.Narrow);
					pawn.story.hairColor = PawnHairColors.RandomHairColor(pawn.story.SkinColor, pawn.ageTracker.AgeBiologicalYears);
					PawnBioAndNameGenerator.GiveAppropriateBioAndNameTo(pawn, request.FixedLastName, factionType);
					pawn.story.hairDef = PawnHairChooser.RandomHairDefFor(pawn, factionType);
					GenerateTraits(pawn, request);
					GenerateBodyType(pawn);
					GenerateSkills(pawn);
				}
				if (pawn.RaceProps.Animal && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.training.SetWantedRecursive(TrainableDefOf.Tameness, checkOn: true);
					pawn.training.Train(TrainableDefOf.Tameness, null, complete: true);
				}
				GenerateInitialHediffs(pawn, request);
				if (pawn.workSettings != null && request.Faction != null && request.Faction.IsPlayer)
				{
					pawn.workSettings.EnableAndInitialize();
				}
				if (request.Faction != null && pawn.RaceProps.Animal)
				{
					pawn.GenerateNecessaryName();
				}
				if (Find.Scenario != null)
				{
					Find.Scenario.Notify_NewPawnGenerating(pawn, request.Context);
				}
				if (!request.AllowDead && (pawn.Dead || pawn.Destroyed))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated dead pawn.";
					return null;
				}
				if (!request.AllowDowned && pawn.Downed)
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated downed pawn.";
					return null;
				}
				if (request.MustBeCapableOfViolence && ((pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent)) || (pawn.RaceProps.ToolUser && !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn incapable of violence.";
					return null;
				}
				if (!ignoreScenarioRequirements && request.Context == PawnGenerationContext.PlayerStarter && Find.Scenario != null && !Find.Scenario.AllowPlayerStartingPawn(pawn, tryingToRedress: false, request))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn doesn't meet scenario requirements.";
					return null;
				}
				if (!ignoreValidator && request.ValidatorPreGear != null && !request.ValidatorPreGear(pawn))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn didn't pass validator check (pre-gear).";
					return null;
				}
				if (!request.Newborn)
				{
					GenerateGearFor(pawn, request);
				}
				if (!ignoreValidator && request.ValidatorPostGear != null && !request.ValidatorPostGear(pawn))
				{
					DiscardGeneratedPawn(pawn);
					error = "Generated pawn didn't pass validator check (post-gear).";
					return null;
				}
				for (int i = 0; i < pawnsBeingGenerated.Count - 1; i++)
				{
					if (pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime == null)
					{
						pawnsBeingGenerated[i] = new PawnGenerationStatus(pawnsBeingGenerated[i].Pawn, new List<Pawn>());
					}
					pawnsBeingGenerated[i].PawnsGeneratedInTheMeantime.Add(pawn);
				}
				return pawn;
			}
			finally
			{
				pawnsBeingGenerated.RemoveLast();
			}
		}

		private static void DiscardGeneratedPawn(Pawn pawn)
		{
			if (Find.WorldPawns.Contains(pawn))
			{
				Find.WorldPawns.RemovePawn(pawn);
			}
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			List<Pawn> pawnsGeneratedInTheMeantime = pawnsBeingGenerated.Last().PawnsGeneratedInTheMeantime;
			if (pawnsGeneratedInTheMeantime != null)
			{
				for (int i = 0; i < pawnsGeneratedInTheMeantime.Count; i++)
				{
					Pawn pawn2 = pawnsGeneratedInTheMeantime[i];
					if (Find.WorldPawns.Contains(pawn2))
					{
						Find.WorldPawns.RemovePawn(pawn2);
					}
					Find.WorldPawns.PassToWorld(pawn2, PawnDiscardDecideMode.Discard);
					for (int j = 0; j < pawnsBeingGenerated.Count; j++)
					{
						pawnsBeingGenerated[j].PawnsGeneratedInTheMeantime.Remove(pawn2);
					}
				}
			}
		}

		private static IEnumerable<Pawn> GetValidCandidatesToRedress(PawnGenerationRequest request)
		{
			IEnumerable<Pawn> enumerable = Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.Free);
			if (request.KindDef.factionLeader)
			{
				enumerable = enumerable.Concat(Find.WorldPawns.GetPawnsBySituation(WorldPawnSituation.FactionLeader));
			}
			return from x in enumerable
			where IsValidCandidateToRedress(x, request)
			select x;
		}

		private static float ChanceToRedressAnyWorldPawn(PawnGenerationRequest request)
		{
			int pawnsBySituationCount = Find.WorldPawns.GetPawnsBySituationCount(WorldPawnSituation.Free);
			float num = Mathf.Min(0.02f + 0.01f * ((float)pawnsBySituationCount / 10f), 0.8f);
			if (request.MinChanceToRedressWorldPawn.HasValue)
			{
				num = Mathf.Max(num, request.MinChanceToRedressWorldPawn.Value);
			}
			return num;
		}

		private static float WorldPawnSelectionWeight(Pawn p)
		{
			if (p.RaceProps.IsFlesh && !p.relations.everSeenByPlayer && p.relations.RelatedToAnyoneOrAnyoneRelatedToMe)
			{
				return 0.1f;
			}
			return 1f;
		}

		private static void GenerateGearFor(Pawn pawn, PawnGenerationRequest request)
		{
			PawnApparelGenerator.GenerateStartingApparelFor(pawn, request);
			PawnWeaponGenerator.TryGenerateWeaponFor(pawn);
			PawnInventoryGenerator.GenerateInventoryFor(pawn, request);
		}

		private static void GenerateInitialHediffs(Pawn pawn, PawnGenerationRequest request)
		{
			int num = 0;
			while (true)
			{
				AgeInjuryUtility.GenerateRandomOldAgeInjuries(pawn, !request.AllowDead);
				PawnTechHediffsGenerator.GenerateTechHediffsFor(pawn);
				PawnAddictionHediffsGenerator.GenerateAddictionsAndTolerancesFor(pawn);
				if ((request.AllowDead && pawn.Dead) || request.AllowDowned || !pawn.Downed)
				{
					break;
				}
				pawn.health.Reset();
				num++;
				if (num > 80)
				{
					Log.Warning("Could not generate old age injuries for " + pawn.ThingID + " of age " + pawn.ageTracker.AgeBiologicalYears + " that allow pawn to move after " + 80 + " tries. request=" + request);
					break;
				}
			}
			if (!pawn.Dead && (request.Faction == null || !request.Faction.IsPlayer))
			{
				int num2 = 0;
				while (true)
				{
					if (!pawn.health.HasHediffsNeedingTend())
					{
						return;
					}
					num2++;
					if (num2 > 10000)
					{
						break;
					}
					TendUtility.DoTend(null, pawn, null);
				}
				Log.Error("Too many iterations.");
			}
		}

		private static void GenerateRandomAge(Pawn pawn, PawnGenerationRequest request)
		{
			if (request.FixedBiologicalAge.HasValue && request.FixedChronologicalAge.HasValue)
			{
				float? fixedBiologicalAge = request.FixedBiologicalAge;
				bool hasValue = fixedBiologicalAge.HasValue;
				float? fixedChronologicalAge = request.FixedChronologicalAge;
				if ((hasValue & fixedChronologicalAge.HasValue) && fixedBiologicalAge.GetValueOrDefault() > fixedChronologicalAge.GetValueOrDefault())
				{
					Log.Warning("Tried to generate age for pawn " + pawn + ", but pawn generation request demands biological age (" + request.FixedBiologicalAge + ") to be greater than chronological age (" + request.FixedChronologicalAge + ").");
				}
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeBiologicalTicks = 0L;
			}
			else if (request.FixedBiologicalAge.HasValue)
			{
				pawn.ageTracker.AgeBiologicalTicks = (long)(request.FixedBiologicalAge.Value * 3600000f);
			}
			else
			{
				float num = 0f;
				int num2 = 0;
				do
				{
					num = ((pawn.RaceProps.ageGenerationCurve != null) ? ((float)Mathf.RoundToInt(Rand.ByCurve(pawn.RaceProps.ageGenerationCurve))) : ((!pawn.RaceProps.IsMechanoid) ? (Rand.ByCurve(DefaultAgeGenerationCurve) * pawn.RaceProps.lifeExpectancy) : Rand.Range(0f, 2500f)));
					num2++;
					if (num2 > 300)
					{
						Log.Error("Tried 300 times to generate age for " + pawn);
						break;
					}
				}
				while (num > (float)pawn.kindDef.maxGenerationAge || num < (float)pawn.kindDef.minGenerationAge);
				pawn.ageTracker.AgeBiologicalTicks = (long)(num * 3600000f) + Rand.Range(0, 3600000);
			}
			if (request.Newborn)
			{
				pawn.ageTracker.AgeChronologicalTicks = 0L;
			}
			else if (request.FixedChronologicalAge.HasValue)
			{
				pawn.ageTracker.AgeChronologicalTicks = (long)(request.FixedChronologicalAge.Value * 3600000f);
			}
			else
			{
				int num3;
				if (request.CertainlyBeenInCryptosleep || Rand.Value < pawn.kindDef.backstoryCryptosleepCommonality)
				{
					float value = Rand.Value;
					if (value < 0.7f)
					{
						num3 = Rand.Range(0, 100);
					}
					else if (value < 0.95f)
					{
						num3 = Rand.Range(100, 1000);
					}
					else
					{
						int max = GenDate.Year(GenTicks.TicksAbs, 0f) - 2026 - pawn.ageTracker.AgeBiologicalYears;
						num3 = Rand.Range(1000, max);
					}
				}
				else
				{
					num3 = 0;
				}
				int ticksAbs = GenTicks.TicksAbs;
				long num4 = ticksAbs - pawn.ageTracker.AgeBiologicalTicks;
				num4 -= (long)num3 * 3600000L;
				pawn.ageTracker.BirthAbsTicks = num4;
			}
			if (pawn.ageTracker.AgeBiologicalTicks > pawn.ageTracker.AgeChronologicalTicks)
			{
				pawn.ageTracker.AgeChronologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
			}
		}

		public static int RandomTraitDegree(TraitDef traitDef)
		{
			if (traitDef.degreeDatas.Count == 1)
			{
				return traitDef.degreeDatas[0].degree;
			}
			return traitDef.degreeDatas.RandomElementByWeight((TraitDegreeData dd) => dd.commonality).degree;
		}

		private static void GenerateTraits(Pawn pawn, PawnGenerationRequest request)
		{
			if (pawn.story != null)
			{
				if (pawn.story.childhood.forcedTraits != null)
				{
					List<TraitEntry> forcedTraits = pawn.story.childhood.forcedTraits;
					for (int i = 0; i < forcedTraits.Count; i++)
					{
						TraitEntry traitEntry = forcedTraits[i];
						if (traitEntry.def == null)
						{
							Log.Error("Null forced trait def on " + pawn.story.childhood);
						}
						else if (!pawn.story.traits.HasTrait(traitEntry.def))
						{
							pawn.story.traits.GainTrait(new Trait(traitEntry.def, traitEntry.degree));
						}
					}
				}
				if (pawn.story.adulthood != null && pawn.story.adulthood.forcedTraits != null)
				{
					List<TraitEntry> forcedTraits2 = pawn.story.adulthood.forcedTraits;
					for (int j = 0; j < forcedTraits2.Count; j++)
					{
						TraitEntry traitEntry2 = forcedTraits2[j];
						if (traitEntry2.def == null)
						{
							Log.Error("Null forced trait def on " + pawn.story.adulthood);
						}
						else if (!pawn.story.traits.HasTrait(traitEntry2.def))
						{
							pawn.story.traits.GainTrait(new Trait(traitEntry2.def, traitEntry2.degree));
						}
					}
				}
				int num = Rand.RangeInclusive(2, 3);
				if (request.AllowGay && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheSameGender(pawn) || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheSameGender(pawn)))
				{
					Trait trait = new Trait(TraitDefOf.Gay, RandomTraitDegree(TraitDefOf.Gay));
					pawn.story.traits.GainTrait(trait);
				}
				while (pawn.story.traits.allTraits.Count < num)
				{
					TraitDef newTraitDef = DefDatabase<TraitDef>.AllDefsListForReading.RandomElementByWeight((TraitDef tr) => tr.GetGenderSpecificCommonality(pawn.gender));
					if (!pawn.story.traits.HasTrait(newTraitDef) && (newTraitDef != TraitDefOf.Gay || (request.AllowGay && !LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) && !LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))) && (request.Faction == null || Faction.OfPlayerSilentFail == null || !request.Faction.HostileTo(Faction.OfPlayer) || newTraitDef.allowOnHostileSpawn) && !pawn.story.traits.allTraits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) && (newTraitDef.conflictingTraits == null || !newTraitDef.conflictingTraits.Any((TraitDef tr) => pawn.story.traits.HasTrait(tr))) && (newTraitDef.requiredWorkTypes == null || !pawn.story.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes)) && !pawn.story.WorkTagIsDisabled(newTraitDef.requiredWorkTags))
					{
						int degree = RandomTraitDegree(newTraitDef);
						if (!pawn.story.childhood.DisallowsTrait(newTraitDef, degree) && (pawn.story.adulthood == null || !pawn.story.adulthood.DisallowsTrait(newTraitDef, degree)))
						{
							Trait trait2 = new Trait(newTraitDef, degree);
							if (pawn.mindState != null && pawn.mindState.mentalBreaker != null)
							{
								float breakThresholdExtreme = pawn.mindState.mentalBreaker.BreakThresholdExtreme;
								breakThresholdExtreme += trait2.OffsetOfStat(StatDefOf.MentalBreakThreshold);
								breakThresholdExtreme *= trait2.MultiplierOfStat(StatDefOf.MentalBreakThreshold);
								if (breakThresholdExtreme > 0.4f)
								{
									continue;
								}
							}
							pawn.story.traits.GainTrait(trait2);
						}
					}
				}
			}
		}

		private static void GenerateBodyType(Pawn pawn)
		{
			if (pawn.story.adulthood != null)
			{
				pawn.story.bodyType = pawn.story.adulthood.BodyTypeFor(pawn.gender);
			}
			else if (Rand.Value < 0.5f)
			{
				pawn.story.bodyType = BodyTypeDefOf.Thin;
			}
			else
			{
				pawn.story.bodyType = ((pawn.gender != Gender.Female) ? BodyTypeDefOf.Male : BodyTypeDefOf.Female);
			}
		}

		private static void GenerateSkills(Pawn pawn)
		{
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				int num = FinalLevelOfSkill(pawn, skillDef);
				SkillRecord skill = pawn.skills.GetSkill(skillDef);
				skill.Level = num;
				if (!skill.TotallyDisabled)
				{
					float num2 = (float)num * 0.11f;
					float value = Rand.Value;
					if (value < num2)
					{
						if (value < num2 * 0.2f)
						{
							skill.passion = Passion.Major;
						}
						else
						{
							skill.passion = Passion.Minor;
						}
					}
					skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
				}
			}
		}

		private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
		{
			float num = (!sk.usuallyDefinedInBackstories) ? Rand.ByCurve(LevelRandomCurve) : ((float)Rand.RangeInclusive(0, 4));
			foreach (Backstory item in from bs in pawn.story.AllBackstories
			where bs != null
			select bs)
			{
				foreach (KeyValuePair<SkillDef, int> item2 in item.skillGainsResolved)
				{
					if (item2.Key == sk)
					{
						num += (float)item2.Value * Rand.Range(1f, 1.4f);
					}
				}
			}
			for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
			{
				int value = 0;
				if (pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out value))
				{
					num += (float)value;
				}
			}
			float num2 = Rand.Range(1f, AgeSkillMaxFactorCurve.Evaluate((float)pawn.ageTracker.AgeBiologicalYears));
			num *= num2;
			num = LevelFinalAdjustmentCurve.Evaluate(num);
			return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
		}

		public static void PostProcessGeneratedGear(Thing gear, Pawn pawn)
		{
			gear.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityGeneratingPawn(pawn.kindDef), ArtGenerationContext.Outsider);
			if (gear.def.useHitPoints)
			{
				float randomInRange = pawn.kindDef.gearHealthRange.RandomInRange;
				if (randomInRange < 1f)
				{
					int b = Mathf.RoundToInt(randomInRange * (float)gear.MaxHitPoints);
					b = (gear.HitPoints = Mathf.Max(1, b));
				}
			}
		}

		private static void GeneratePawnRelations(Pawn pawn, ref PawnGenerationRequest request)
		{
			if (pawn.RaceProps.Humanlike)
			{
				Pawn[] array = (from x in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead
				where x.def == pawn.def
				select x).ToArray();
				if (array.Length != 0)
				{
					int num = 0;
					Pawn[] array2 = array;
					foreach (Pawn pawn2 in array2)
					{
						if (pawn2.Discarded)
						{
							Log.Warning("Warning during generating pawn relations for " + pawn + ": Pawn " + pawn2 + " is discarded, yet he was yielded by PawnUtility. Discarding a pawn means that he is no longer managed by anything.");
						}
						else if (pawn2.Faction != null && pawn2.Faction.IsPlayer)
						{
							num++;
						}
					}
					float num2 = 45f;
					num2 += (float)num * 2.7f;
					PawnGenerationRequest localReq = request;
					Pair<Pawn, PawnRelationDef> pair = GenerateSamples(array, relationsGeneratableBlood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * relationsGeneratableBlood.Length));
					if (pair.First != null)
					{
						pair.Second.Worker.CreateRelation(pawn, pair.First, ref request);
					}
					Pair<Pawn, PawnRelationDef> pair2 = GenerateSamples(array, relationsGeneratableNonblood, 40).RandomElementByWeightWithDefault((Pair<Pawn, PawnRelationDef> x) => x.Second.generationChanceFactor * x.Second.Worker.GenerationChance(pawn, x.First, localReq), num2 * 40f / (float)(array.Length * relationsGeneratableNonblood.Length));
					if (pair2.First != null)
					{
						pair2.Second.Worker.CreateRelation(pawn, pair2.First, ref request);
					}
				}
			}
		}

		private static Pair<Pawn, PawnRelationDef>[] GenerateSamples(Pawn[] pawns, PawnRelationDef[] relations, int count)
		{
			Pair<Pawn, PawnRelationDef>[] array = new Pair<Pawn, PawnRelationDef>[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = new Pair<Pawn, PawnRelationDef>(pawns[Rand.Range(0, pawns.Length)], relations[Rand.Range(0, relations.Length)]);
			}
			return array;
		}

		[DebugOutput]
		[Category("Performance")]
		public static void PawnGenerationHistogram()
		{
			DebugHistogram debugHistogram = new DebugHistogram((from x in Enumerable.Range(1, 20)
			select (float)x * 10f).ToArray());
			for (int i = 0; i < 100; i++)
			{
				long timestamp = Stopwatch.GetTimestamp();
				Pawn pawn = GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, null, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true));
				debugHistogram.Add((float)((Stopwatch.GetTimestamp() - timestamp) * 1000 / Stopwatch.Frequency));
				pawn.Destroy();
			}
			debugHistogram.Display();
		}
	}
}
