using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_JourneyOffer : StorytellerComp
	{
		private const int StartOnDay = 14;

		private int IntervalsPassed => Find.TickManager.TicksGame / 1000;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (IntervalsPassed == 840)
			{
				IncidentDef inc = IncidentDefOf.Quest_JourneyOffer;
				if (inc.TargetAllowed(target))
				{
					FiringIncident fi = new FiringIncident(inc, this, GenerateParms(inc.category, target));
					yield return fi;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}
	}
}
