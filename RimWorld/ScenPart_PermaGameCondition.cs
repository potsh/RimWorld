using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_PermaGameCondition : ScenPart
	{
		private GameConditionDef gameCondition;

		public const string PermaGameConditionTag = "PermaGameCondition";

		public override string Label => "Permanent".Translate().CapitalizeFirst() + ": " + gameCondition.label.CapitalizeFirst();

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight);
			if (Widgets.ButtonText(scenPartRect, gameCondition.LabelCap))
			{
				FloatMenuUtility.MakeMenu(AllowedGameConditions(), (GameConditionDef d) => d.LabelCap, delegate(GameConditionDef d)
				{
					ScenPart_PermaGameCondition scenPart_PermaGameCondition = this;
					return delegate
					{
						scenPart_PermaGameCondition.gameCondition = d;
					};
				});
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref gameCondition, "gameCondition");
		}

		public override void Randomize()
		{
			gameCondition = AllowedGameConditions().RandomElement();
		}

		private IEnumerable<GameConditionDef> AllowedGameConditions()
		{
			return from d in DefDatabase<GameConditionDef>.AllDefs
			where d.canBePermanent
			select d;
		}

		public override string Summary(Scenario scen)
		{
			return ScenSummaryList.SummaryWithList(scen, "PermaGameCondition", "ScenPart_PermaGameCondition".Translate());
		}

		public override IEnumerable<string> GetSummaryListEntries(string tag)
		{
			if (tag == "PermaGameCondition")
			{
				yield return gameCondition.LabelCap + ": " + gameCondition.description.CapitalizeFirst();
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void GenerateIntoMap(Map map)
		{
			GameCondition cond = GameConditionMaker.MakeConditionPermanent(gameCondition);
			map.gameConditionManager.RegisterCondition(cond);
		}

		public override bool CanCoexistWith(ScenPart other)
		{
			if (gameCondition == null)
			{
				return true;
			}
			ScenPart_PermaGameCondition scenPart_PermaGameCondition = other as ScenPart_PermaGameCondition;
			if (scenPart_PermaGameCondition != null && !gameCondition.CanCoexistWith(scenPart_PermaGameCondition.gameCondition))
			{
				return false;
			}
			return true;
		}
	}
}
