using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawnAndTrader : Tale_DoublePawn
	{
		public TaleData_Trader traderData;

		public Tale_DoublePawnAndTrader()
		{
		}

		public Tale_DoublePawnAndTrader(Pawn firstPawn, Pawn secondPawn, ITrader trader)
			: base(firstPawn, secondPawn)
		{
			traderData = TaleData_Trader.GenerateFrom(trader);
		}

		public override bool Concerns(Thing th)
		{
			return base.Concerns(th) || traderData.pawnID == th.thingIDNumber;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref traderData, "traderData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			using (IEnumerator<Rule> enumerator = base.SpecialTextGenerationRules().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Rule r2 = enumerator.Current;
					yield return r2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Rule> enumerator2 = traderData.GetRules("TRADER").GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Rule r = enumerator2.Current;
					yield return r;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0154:
			/*Error near IL_0155: Unexpected return in MoveNext()*/;
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			traderData = TaleData_Trader.GenerateRandom();
		}
	}
}
