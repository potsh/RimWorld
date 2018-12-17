using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public static class GiveToPackAnimalUtility
	{
		[CompilerGenerated]
		private static Func<Pawn, bool> _003C_003Ef__mg_0024cache0;

		public static IEnumerable<Pawn> CarrierCandidatesFor(Pawn pawn)
		{
			IEnumerable<Pawn> source = (!pawn.IsFormingCaravan()) ? pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction) : pawn.GetLord().ownedPawns;
			source = from x in source
			where x.RaceProps.packAnimal && !x.inventory.UnloadEverything
			select x;
			if (pawn.Map.IsPlayerHome)
			{
				source = source.Where(CaravanFormingUtility.IsFormingCaravan);
			}
			return source;
		}

		public static Pawn UsablePackAnimalWithTheMostFreeSpace(Pawn pawn)
		{
			IEnumerable<Pawn> enumerable = CarrierCandidatesFor(pawn);
			Pawn pawn2 = null;
			float num = 0f;
			foreach (Pawn item in enumerable)
			{
				if (item.RaceProps.packAnimal && item != pawn && pawn.CanReach(item, PathEndMode.Touch, Danger.Deadly))
				{
					float num2 = MassUtility.FreeSpace(item);
					if (pawn2 == null || num2 > num)
					{
						pawn2 = item;
						num = num2;
					}
				}
			}
			return pawn2;
		}
	}
}
