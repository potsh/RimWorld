using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class StartingPawnUtility
	{
		private static List<Pawn> StartingAndOptionalPawns => Find.GameInitData.startingAndOptionalPawns;

		public static void ClearAllStartingPawns()
		{
			for (int num = StartingAndOptionalPawns.Count - 1; num >= 0; num--)
			{
				StartingAndOptionalPawns[num].relations.ClearAllRelations();
				if (Find.World != null)
				{
					PawnUtility.DestroyStartingColonistFamily(StartingAndOptionalPawns[num]);
					PawnComponentsUtility.RemoveComponentsOnDespawned(StartingAndOptionalPawns[num]);
					Find.WorldPawns.PassToWorld(StartingAndOptionalPawns[num], PawnDiscardDecideMode.Discard);
				}
				StartingAndOptionalPawns.RemoveAt(num);
			}
		}

		public static Pawn RandomizeInPlace(Pawn p)
		{
			int index = StartingAndOptionalPawns.IndexOf(p);
			return RegenerateStartingPawnInPlace(index);
		}

		private static Pawn RegenerateStartingPawnInPlace(int index)
		{
			Pawn pawn = StartingAndOptionalPawns[index];
			PawnUtility.TryDestroyStartingColonistFamily(pawn);
			pawn.relations.ClearAllRelations();
			PawnComponentsUtility.RemoveComponentsOnDespawned(pawn);
			Find.WorldPawns.PassToWorld(pawn, PawnDiscardDecideMode.Discard);
			StartingAndOptionalPawns[index] = null;
			for (int i = 0; i < StartingAndOptionalPawns.Count; i++)
			{
				if (StartingAndOptionalPawns[i] != null)
				{
					PawnUtility.TryDestroyStartingColonistFamily(StartingAndOptionalPawns[i]);
				}
			}
			Pawn pawn2 = NewGeneratedStartingPawn();
			StartingAndOptionalPawns[index] = pawn2;
			return pawn2;
		}

		public static Pawn NewGeneratedStartingPawn()
		{
			PawnGenerationRequest request = new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer, PawnGenerationContext.PlayerStarter, -1, forceGenerateNewPawn: true, newborn: false, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, TutorSystem.TutorialMode, 20f);
			Pawn pawn = null;
			try
			{
				pawn = PawnGenerator.GeneratePawn(request);
			}
			catch (Exception arg)
			{
				Log.Error("There was an exception thrown by the PawnGenerator during generating a starting pawn. Trying one more time...\nException: " + arg);
				pawn = PawnGenerator.GeneratePawn(request);
			}
			pawn.relations.everSeenByPlayer = true;
			PawnComponentsUtility.AddComponentsForSpawn(pawn);
			return pawn;
		}

		public static bool WorkTypeRequirementsSatisfied()
		{
			if (StartingAndOptionalPawns.Count == 0)
			{
				return false;
			}
			List<WorkTypeDef> allDefsListForReading = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				WorkTypeDef workTypeDef = allDefsListForReading[i];
				if (workTypeDef.requireCapableColonist)
				{
					bool flag = false;
					for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
					{
						if (!StartingAndOptionalPawns[j].story.WorkTypeIsDisabled(workTypeDef))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			if (TutorSystem.TutorialMode && StartingAndOptionalPawns.Take(Find.GameInitData.startingPawnCount).Any((Pawn p) => p.story.WorkTagIsDisabled(WorkTags.Violent)))
			{
				return false;
			}
			return true;
		}

		public static IEnumerable<WorkTypeDef> RequiredWorkTypesDisabledForEveryone()
		{
			List<WorkTypeDef> workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			int i = 0;
			WorkTypeDef wt;
			while (true)
			{
				if (i >= workTypes.Count)
				{
					yield break;
				}
				wt = workTypes[i];
				if (wt.requireCapableColonist)
				{
					bool oneCanDoWt = false;
					List<Pawn> startingPawns = StartingAndOptionalPawns;
					for (int j = 0; j < Find.GameInitData.startingPawnCount; j++)
					{
						if (!startingPawns[j].story.WorkTypeIsDisabled(wt))
						{
							oneCanDoWt = true;
							break;
						}
					}
					if (!oneCanDoWt)
					{
						break;
					}
				}
				i++;
			}
			yield return wt;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
