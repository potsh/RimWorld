using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_ClassicIntro : StorytellerComp
	{
		protected int IntervalsPassed => Find.TickManager.TicksGame / 1000;

		public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			_003CMakeIntervalIncidents_003Ec__Iterator0 _003CMakeIntervalIncidents_003Ec__Iterator = (_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_003e: stateMachine*/;
			if (target == Find.Maps.Find((Map x) => x.IsPlayerHome))
			{
				if (IntervalsPassed == 150)
				{
					IncidentDef inc2 = IncidentDefOf.VisitorGroup;
					if (inc2.TargetAllowed(target))
					{
						yield return new FiringIncident(inc2, this)
						{
							parms = 
							{
								target = target,
								points = (float)Rand.Range(40, 100)
							}
						};
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (IntervalsPassed == 204)
				{
					_003CMakeIntervalIncidents_003Ec__Iterator0 _003CMakeIntervalIncidents_003Ec__Iterator2 = (_003CMakeIntervalIncidents_003Ec__Iterator0)/*Error near IL_015f: stateMachine*/;
					IncidentCategoryDef threatCategory = (!Find.Storyteller.difficulty.allowIntroThreats) ? IncidentCategoryDefOf.Misc : IncidentCategoryDefOf.ThreatSmall;
					if ((from def in DefDatabase<IncidentDef>.AllDefs
					where def.TargetAllowed(target) && def.category == threatCategory
					select def).TryRandomElementByWeight(base.IncidentChanceFinal, out IncidentDef incDef2))
					{
						yield return new FiringIncident(incDef2, this)
						{
							parms = StorytellerUtility.DefaultParmsNow(incDef2.category, target)
						};
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (IntervalsPassed == 264 && (from def in DefDatabase<IncidentDef>.AllDefs
				where def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc
				select def).TryRandomElementByWeight(base.IncidentChanceFinal, out IncidentDef incDef))
				{
					yield return new FiringIncident(incDef, this)
					{
						parms = StorytellerUtility.DefaultParmsNow(incDef.category, target)
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (IntervalsPassed == 324)
				{
					IncidentDef inc = IncidentDefOf.RaidEnemy;
					if (!Find.Storyteller.difficulty.allowIntroThreats)
					{
						inc = (from def in DefDatabase<IncidentDef>.AllDefs
						where def.TargetAllowed(target) && def.category == IncidentCategoryDefOf.Misc
						select def).RandomElementByWeightWithFallback(base.IncidentChanceFinal);
					}
					if (inc != null && inc.TargetAllowed(target))
					{
						FiringIncident fi = new FiringIncident(inc, this);
						fi.parms = GenerateParms(inc.category, target);
						fi.parms.points = 40f;
						fi.parms.raidForceOneIncap = true;
						fi.parms.raidNeverFleeIndividual = true;
						yield return fi;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
		}
	}
}
