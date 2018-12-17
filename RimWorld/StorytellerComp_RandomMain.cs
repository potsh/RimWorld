using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_RandomMain : StorytellerComp
	{
		protected StorytellerCompProperties_RandomMain Props => (StorytellerCompProperties_RandomMain)props;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			if (Rand.MTBEventOccurs(Props.mtbDays, 60000f, 1000f))
			{
				bool targetIsRaidBeacon = target.IncidentTargetTags().Contains(IncidentTargetTagDefOf.Map_RaidBeacon);
				List<IncidentCategoryDef> triedCategories = new List<IncidentCategoryDef>();
				IncidentParms parms;
				IncidentDef incDef;
				while (true)
				{
					_003CMakeIntervalIncidents_003Ec__Iterator0 _003CMakeIntervalIncidents_003Ec__Iterator = (_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_007c: stateMachine*/;
					IncidentCategoryDef category = ChooseRandomCategory(target, triedCategories);
					parms = GenerateParms(category, target);
					IEnumerable<IncidentDef> options = from d in UsableIncidentsInCategory(category, target)
					where !d.NeedsParmsPoints || parms.points >= d.minThreatPoints
					select d;
					if (options.TryRandomElementByWeight(base.IncidentChanceFinal, out incDef))
					{
						break;
					}
					triedCategories.Add(category);
					if (triedCategories.Count >= Props.categoryWeights.Count)
					{
						yield break;
					}
				}
				if (!Props.skipThreatBigIfRaidBeacon || !targetIsRaidBeacon || incDef.category != IncidentCategoryDefOf.ThreatBig)
				{
					yield return new FiringIncident(incDef, this, parms);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		private IncidentCategoryDef ChooseRandomCategory(IIncidentTarget target, List<IncidentCategoryDef> skipCategories)
		{
			if (!skipCategories.Contains(IncidentCategoryDefOf.ThreatBig))
			{
				int num = Find.TickManager.TicksGame - target.StoryState.LastThreatBigTick;
				if (target.StoryState.LastThreatBigTick >= 0 && (float)num > 60000f * Props.maxThreatBigIntervalDays)
				{
					return IncidentCategoryDefOf.ThreatBig;
				}
			}
			return (from cw in Props.categoryWeights
			where !skipCategories.Contains(cw.category)
			select cw).RandomElementByWeight((IncidentCategoryEntry cw) => cw.weight).category;
		}

		public override IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat, target);
			incidentParms.points *= Props.randomPointsFactorRange.RandomInRange;
			return incidentParms;
		}
	}
}
