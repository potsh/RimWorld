using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class IncidentWorker_HerdMigration : IncidentWorker
	{
		private static readonly IntRange AnimalsCount = new IntRange(3, 5);

		private const float MinTotalBodySize = 4f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			PawnKindDef animalKind;
			IntVec3 start;
			IntVec3 end;
			return TryFindAnimalKind(map.Tile, out animalKind) && TryFindStartAndEndCells(map, out start, out end);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindAnimalKind(map.Tile, out PawnKindDef animalKind))
			{
				return false;
			}
			if (!TryFindStartAndEndCells(map, out IntVec3 start, out IntVec3 end))
			{
				return false;
			}
			Rot4 rot = Rot4.FromAngleFlat((map.Center - start).AngleFlat);
			List<Pawn> list = GenerateAnimals(animalKind, map.Tile);
			for (int i = 0; i < list.Count; i++)
			{
				Pawn newThing = list[i];
				IntVec3 loc = CellFinder.RandomClosewalkCellNear(start, map, 10);
				GenSpawn.Spawn(newThing, loc, map, rot);
			}
			LordMaker.MakeNewLord(null, new LordJob_ExitMapNear(end, LocomotionUrgency.Walk), map, list);
			string text = string.Format(def.letterText, animalKind.GetLabelPlural()).CapitalizeFirst();
			string label = string.Format(def.letterLabel, animalKind.GetLabelPlural().CapitalizeFirst());
			Find.LetterStack.ReceiveLetter(label, text, def.letterDef, list[0]);
			return true;
		}

		private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
		{
			return (from k in DefDatabase<PawnKindDef>.AllDefs
			where k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)
			select k).TryRandomElementByWeight((PawnKindDef x) => Mathf.Lerp(0.2f, 1f, x.RaceProps.wildness), out animalKind);
		}

		private bool TryFindStartAndEndCells(Map map, out IntVec3 start, out IntVec3 end)
		{
			if (!RCellFinder.TryFindRandomPawnEntryCell(out start, map, CellFinder.EdgeRoadChance_Animal))
			{
				end = IntVec3.Invalid;
				return false;
			}
			end = IntVec3.Invalid;
			for (int i = 0; i < 8; i++)
			{
				IntVec3 startLocal = start;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => map.reachability.CanReach(startLocal, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly), map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 result))
				{
					break;
				}
				if (!end.IsValid || result.DistanceToSquared(start) > end.DistanceToSquared(start))
				{
					end = result;
				}
			}
			return end.IsValid;
		}

		private List<Pawn> GenerateAnimals(PawnKindDef animalKind, int tile)
		{
			int randomInRange = AnimalsCount.RandomInRange;
			randomInRange = Mathf.Max(randomInRange, Mathf.CeilToInt(4f / animalKind.RaceProps.baseBodySize));
			List<Pawn> list = new List<Pawn>();
			for (int i = 0; i < randomInRange; i++)
			{
				PawnGenerationRequest request = new PawnGenerationRequest(animalKind, null, PawnGenerationContext.NonPlayer, tile);
				Pawn item = PawnGenerator.GeneratePawn(request);
				list.Add(item);
			}
			return list;
		}
	}
}
