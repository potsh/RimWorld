using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class CompProperties_Hibernatable : CompProperties
	{
		public float startupDays = 15f;

		public IncidentTargetTagDef incidentTargetWhileStarting;

		public CompProperties_Hibernatable()
		{
			compClass = typeof(CompHibernatable);
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
				yield return "CompHibernatable needs tickerType " + TickerType.Normal + ", has " + parentDef.tickerType;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0125:
			/*Error near IL_0126: Unexpected return in MoveNext()*/;
		}
	}
}
