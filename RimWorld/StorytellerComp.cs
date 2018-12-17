using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class StorytellerComp
	{
		public StorytellerCompProperties props;

		public virtual IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
		{
			yield break;
		}

		public virtual void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = default(DamageInfo?))
		{
		}

		public virtual IncidentParms GenerateParms(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			return StorytellerUtility.DefaultParmsNow(incCat, target);
		}

		protected IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, IIncidentTarget target)
		{
			return UsableIncidentsInCategory(cat, (IncidentDef x) => GenerateParms(cat, target));
		}

		protected IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, IncidentParms parms)
		{
			return UsableIncidentsInCategory(cat, (IncidentDef x) => parms);
		}

		protected virtual IEnumerable<IncidentDef> UsableIncidentsInCategory(IncidentCategoryDef cat, Func<IncidentDef, IncidentParms> parmsGetter)
		{
			return from x in DefDatabase<IncidentDef>.AllDefsListForReading
			where x.category == cat && x.Worker.CanFireNow(parmsGetter(x))
			select x;
		}

		protected float IncidentChanceFactor_CurrentPopulation(IncidentDef def)
		{
			if (def.chanceFactorByPopulationCurve == null)
			{
				return 1f;
			}
			int num = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count();
			return def.chanceFactorByPopulationCurve.Evaluate((float)num);
		}

		protected float IncidentChanceFactor_PopulationIntent(IncidentDef def)
		{
			if (def.populationEffect == IncidentPopulationEffect.None)
			{
				return 1f;
			}
			float num;
			switch (def.populationEffect)
			{
			case IncidentPopulationEffect.IncreaseHard:
				num = 0.4f;
				break;
			case IncidentPopulationEffect.IncreaseMedium:
				num = 0f;
				break;
			case IncidentPopulationEffect.IncreaseEasy:
				num = -0.4f;
				break;
			default:
				throw new Exception();
			}
			float a = StorytellerUtilityPopulation.PopulationIntent + num;
			return Mathf.Max(a, props.minIncChancePopulationIntentFactor);
		}

		protected float IncidentChanceFinal(IncidentDef def)
		{
			float adjustedChance = def.Worker.AdjustedChance;
			adjustedChance *= IncidentChanceFactor_CurrentPopulation(def);
			adjustedChance *= IncidentChanceFactor_PopulationIntent(def);
			return Mathf.Max(0f, adjustedChance);
		}

		public override string ToString()
		{
			string text = GetType().Name;
			string text2 = typeof(StorytellerComp).Name + "_";
			if (text.StartsWith(text2))
			{
				text = text.Substring(text2.Length);
			}
			if (!props.allowedTargetTags.NullOrEmpty())
			{
				text = text + " (" + (from x in props.allowedTargetTags
				select x.ToString()).ToCommaList() + ")";
			}
			return text;
		}

		public virtual void DebugTablesIncidentChances(IncidentCategoryDef cat)
		{
			DebugTables.MakeTablesDialog(from d in DefDatabase<IncidentDef>.AllDefs
			where d.category == cat
			orderby IncidentChanceFinal(d) descending
			select d, new TableDataGetter<IncidentDef>("defName", (IncidentDef d) => d.defName), new TableDataGetter<IncidentDef>("baseChance", (IncidentDef d) => d.baseChance.ToString()), new TableDataGetter<IncidentDef>("AdjustedChance", (IncidentDef d) => d.Worker.AdjustedChance.ToString()), new TableDataGetter<IncidentDef>("Factor-PopCurrent", (IncidentDef d) => IncidentChanceFactor_CurrentPopulation(d).ToString()), new TableDataGetter<IncidentDef>("Factor-PopIntent", (IncidentDef d) => IncidentChanceFactor_PopulationIntent(d).ToString()), new TableDataGetter<IncidentDef>("final chance", (IncidentDef d) => IncidentChanceFinal(d).ToString()), new TableDataGetter<IncidentDef>("vismap-usable", (IncidentDef d) => (Find.CurrentMap == null) ? "-" : ((!UsableIncidentsInCategory(cat, Find.CurrentMap).Contains(d)) ? string.Empty : "V")), new TableDataGetter<IncidentDef>("world-usable", (IncidentDef d) => (!UsableIncidentsInCategory(cat, Find.World).Contains(d)) ? string.Empty : "W"), new TableDataGetter<IncidentDef>("pop-current", (IncidentDef d) => PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists.Count().ToString()), new TableDataGetter<IncidentDef>("pop-intent", (IncidentDef d) => StorytellerUtilityPopulation.PopulationIntent.ToString("F3")));
		}
	}
}
