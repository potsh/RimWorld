using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StockGenerator_SingleDef : StockGenerator
	{
		private ThingDef thingDef;

		public override IEnumerable<Thing> GenerateThings(int forTile)
		{
			using (IEnumerator<Thing> enumerator = StockGeneratorUtility.TryMakeForStock(thingDef, RandomCountOf(thingDef)).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Thing th = enumerator.Current;
					yield return th;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00d4:
			/*Error near IL_00d5: Unexpected return in MoveNext()*/;
		}

		public override bool HandlesThingDef(ThingDef thingDef)
		{
			return thingDef == this.thingDef;
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
			if (!thingDef.tradeability.TraderCanSell())
			{
				yield return thingDef + " tradeability doesn't allow traders to sell this thing";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_010c:
			/*Error near IL_010d: Unexpected return in MoveNext()*/;
		}
	}
}
