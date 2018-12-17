using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_CategoryMTB : StorytellerComp
	{
		protected StorytellerCompProperties_CategoryMTB Props => (StorytellerCompProperties_CategoryMTB)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			float mtbNow = Props.mtbDays;
			if (Props.mtbDaysFactorByDaysPassedCurve != null)
			{
				mtbNow *= Props.mtbDaysFactorByDaysPassedCurve.Evaluate(GenDate.DaysPassedFloat);
			}
			if (Rand.MTBEventOccurs(mtbNow, 60000f, 1000f) && UsableIncidentsInCategory(Props.category, target).TryRandomElementByWeight((IncidentDef incDef) => ((_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_00ae: stateMachine*/)._0024this.IncidentChanceFinal(incDef), out IncidentDef selectedDef))
			{
				yield return new FiringIncident(selectedDef, this, GenerateParms(selectedDef.category, target));
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
