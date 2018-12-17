using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_OnOffCycle : StorytellerComp
	{
		protected StorytellerCompProperties_OnOffCycle Props => (StorytellerCompProperties_OnOffCycle)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float difficultyFactor = (!Props.applyRaidBeaconThreatMtbFactor) ? 1f : Find.Storyteller.difficulty.raidBeaconThreatCountFactor;
			float acceptFraction = 1f;
			if (Props.acceptFractionByDaysPassedCurve != null)
			{
				acceptFraction *= Props.acceptFractionByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			if (Props.acceptPercentFactorPerThreatPointsCurve != null)
			{
				acceptFraction *= Props.acceptPercentFactorPerThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(target));
			}
			int incCount = IncidentCycleUtility.IncidentCountThisInterval(target, Find.Storyteller.storytellerComps.IndexOf(this), Props.minDaysPassed, Props.onDays, Props.offDays, Props.minSpacingDays, Props.numIncidentsRange.min * difficultyFactor, Props.numIncidentsRange.max * difficultyFactor, acceptFraction);
			int i = 0;
			FiringIncident fi;
			while (true)
			{
				if (i >= incCount)
				{
					yield break;
				}
				fi = GenerateIncident(target);
				if (fi != null)
				{
					break;
				}
				i++;
			}
			yield return fi;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private FiringIncident GenerateIncident(IIncidentTarget target)
		{
			IncidentParms parms = GenerateParms(Props.IncidentCategory, target);
			IncidentDef result;
			if ((float)GenDate.DaysPassed < Props.forceRaidEnemyBeforeDaysPassed)
			{
				if (!IncidentDefOf.RaidEnemy.Worker.CanFireNow(parms))
				{
					return null;
				}
				result = IncidentDefOf.RaidEnemy;
			}
			else if (Props.incident != null)
			{
				if (!Props.incident.Worker.CanFireNow(parms))
				{
					return null;
				}
				result = Props.incident;
			}
			else if (!(from def in UsableIncidentsInCategory(Props.IncidentCategory, parms)
			where parms.points >= def.minThreatPoints
			select def).TryRandomElementByWeight(base.IncidentChanceFinal, out result))
			{
				return null;
			}
			FiringIncident firingIncident = new FiringIncident(result, this);
			firingIncident.parms = parms;
			return firingIncident;
		}

		public override string ToString()
		{
			return base.ToString() + " (" + ((Props.incident == null) ? Props.IncidentCategory.defName : Props.incident.defName) + ")";
		}
	}
}
