using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_DisallowBuilding : ScenPart_Rule
	{
		private ThingDef building;

		private const string DisallowBuildingTag = "DisallowBuilding";

		protected override void ApplyRule()
		{
			Current.Game.Rules.SetAllowBuilding(building, allowed: false);
		}

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "DisallowBuilding", "ScenPart_DisallowBuilding".Translate());
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "DisallowBuilding")
			{
				yield return building.LabelCap;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref building, "building");
		}

		public override void Randomize()
		{
			building = RandomizableBuildingDefs().RandomElement();
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, building.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (ThingDef item in from t in PossibleBuildingDefs()
				orderby t.label
				select t)
				{
					ThingDef localTd = item;
					list.Add(new FloatMenuOption(localTd.LabelCap, delegate
					{
						building = localTd;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_DisallowBuilding scenPart_DisallowBuilding = other as ScenPart_DisallowBuilding;
			if (scenPart_DisallowBuilding != null && scenPart_DisallowBuilding.building == building)
			{
				return true;
			}
			return false;
		}

		protected virtual IEnumerable<ThingDef> PossibleBuildingDefs()
		{
			return from d in DefDatabase<ThingDef>.AllDefs
			where d.category == ThingCategory.Building && d.BuildableByPlayer
			select d;
		}

		private IEnumerable<ThingDef> RandomizableBuildingDefs()
		{
			yield return ThingDefOf.Wall;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
