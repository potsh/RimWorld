using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanNameGenerator
	{
		public static string GenerateCaravanName(Caravan caravan)
		{
			Pawn pawn = BestCaravanPawnUtility.FindBestNegotiator(caravan) ?? BestCaravanPawnUtility.FindBestDiplomat(caravan) ?? caravan.PawnsListForReading.Find((Pawn x) => caravan.IsOwner(x));
			string text = (pawn == null) ? caravan.def.label : "CaravanLeaderCaravanName".Translate(pawn.LabelShort, pawn).CapitalizeFirst();
			for (int i = 1; i <= 1000; i++)
			{
				string text2 = text;
				if (i != 1)
				{
					text2 = text2 + " " + i;
				}
				if (!CaravanNameInUse(text2))
				{
					return text2;
				}
			}
			Log.Error("Ran out of caravan names.");
			return caravan.def.label;
		}

		private static bool CaravanNameInUse(string name)
		{
			List<Caravan> caravans = Find.WorldObjects.Caravans;
			for (int i = 0; i < caravans.Count; i++)
			{
				if (caravans[i].Name == name)
				{
					return true;
				}
			}
			return false;
		}
	}
}
