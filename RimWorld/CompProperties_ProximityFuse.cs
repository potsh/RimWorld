using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_ProximityFuse : CompProperties
	{
		public ThingDef target;

		public float radius;

		public CompProperties_ProximityFuse()
		{
			compClass = typeof(CompProximityFuse);
		}

		public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors(parentDef).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (parentDef.tickerType != TickerType.Normal)
			{
				yield return "CompProximityFuse needs tickerType " + TickerType.Rare + " or faster, has " + parentDef.tickerType;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (parentDef.CompDefFor<CompExplosive>() == null)
			{
				yield return "CompProximityFuse requires a CompExplosive";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0158:
			/*Error near IL_0159: Unexpected return in MoveNext()*/;
		}
	}
}
