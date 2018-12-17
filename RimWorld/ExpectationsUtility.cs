using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class ExpectationsUtility
	{
		private static List<ExpectationDef> expectationsInOrder;

		public static void Reset()
		{
			expectationsInOrder = (from ed in DefDatabase<ExpectationDef>.AllDefs
			orderby ed.maxMapWealth
			select ed).ToList();
		}

		public static ExpectationDef CurrentExpectationFor(Pawn p)
		{
			if (Current.ProgramState != ProgramState.Playing)
			{
				return null;
			}
			if (p.Faction != Faction.OfPlayer && !p.IsPrisonerOfColony)
			{
				return ExpectationDefOf.ExtremelyLow;
			}
			if (p.MapHeld != null)
			{
				return CurrentExpectationFor(p.MapHeld);
			}
			return ExpectationDefOf.VeryLow;
		}

		public static ExpectationDef CurrentExpectationFor(Map m)
		{
			float wealthTotal = m.wealthWatcher.WealthTotal;
			for (int i = 0; i < expectationsInOrder.Count; i++)
			{
				ExpectationDef expectationDef = expectationsInOrder[i];
				if (wealthTotal < expectationDef.maxMapWealth)
				{
					return expectationDef;
				}
			}
			return expectationsInOrder[expectationsInOrder.Count - 1];
		}
	}
}
