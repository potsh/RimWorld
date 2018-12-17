using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_DeepDrillInfestation : StorytellerComp
	{
		private static List<Thing> tmpDrills = new List<Thing>();

		protected StorytellerCompProperties_DeepDrillInfestation Props => (StorytellerCompProperties_DeepDrillInfestation)props;

		private float DeepDrillInfestationMTBDaysPerDrill
		{
			get
			{
				DifficultyDef difficulty = Find.Storyteller.difficulty;
				if (difficulty.deepDrillInfestationChanceFactor <= 0f)
				{
					return -1f;
				}
				return Props.baseMtbDaysPerDrill / difficulty.deepDrillInfestationChanceFactor;
			}
		}

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			Map map = (Map)target;
			tmpDrills.Clear();
			DeepDrillInfestationIncidentUtility.GetUsableDeepDrills(map, tmpDrills);
			if (tmpDrills.Any())
			{
				float mtb = DeepDrillInfestationMTBDaysPerDrill;
				int i = 0;
				IncidentDef def;
				while (true)
				{
					if (i >= tmpDrills.Count)
					{
						yield break;
					}
					if (Rand.MTBEventOccurs(mtb, 60000f, 1000f) && UsableIncidentsInCategory(IncidentCategoryDefOf.DeepDrillInfestation, target).TryRandomElement(out def))
					{
						break;
					}
					i++;
				}
				yield return new FiringIncident(parms: GenerateParms(def.category, target), def: def, source: this);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
