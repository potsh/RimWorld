using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_MultiDef : StockGenerator
	{
		private List<ThingDef> thingDefs = new List<ThingDef>();

		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			ThingDef td = thingDefs.RandomElement();
			using (IEnumerator<Thing> enumerator = StockGeneratorUtility.TryMakeForStock(td, RandomCountOf(td)).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Thing th = enumerator.Current;
					yield return th;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00e0:
			/*Error near IL_00e1: Unexpected return in MoveNext()*/;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDefs.Contains(thingDef);
		}

		public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			int i = 0;
			while (true)
			{
				if (i >= thingDefs.Count)
				{
					yield break;
				}
				if (!thingDefs[i].tradeability.TraderCanSell())
				{
					break;
				}
				i++;
			}
			yield return thingDefs[i] + " tradeability doesn't allow traders to sell this thing";
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0157:
			/*Error near IL_0158: Unexpected return in MoveNext()*/;
		}
	}
}
