using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_Disease : StorytellerComp
	{
		protected StorytellerCompProperties_Disease Props => (StorytellerCompProperties_Disease)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			_003CMakeIntervalIncidents_003Ec__Iterator0 _003CMakeIntervalIncidents_003Ec__Iterator = (_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/;
			if (DebugSettings.enableRandomDiseases && target.Tile != -1)
			{
				BiomeDef biome = Find.WorldGrid[target.Tile].biome;
				float mtb2 = biome.diseaseMtbDays;
				mtb2 *= Find.Storyteller.difficulty.diseaseIntervalFactor;
				if (Rand.MTBEventOccurs(mtb2, 60000f, 1000f) && UsableIncidentsInCategory(Props.category, target).TryRandomElementByWeight((IncidentDef d) => biome.CommonalityOfDisease(d), out IncidentDef inc))
				{
					yield return new FiringIncident(inc, this, GenerateParms(inc.category, target));
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.category;
		}
	}
}
