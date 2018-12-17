using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ScenPart_StatFactor : ScenPart
	{
		private StatDef stat;

		private float factor;

		private string factorBuf;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref stat, "stat");
			Scribe_Values.Look(ref factor, "factor", 0f);
		}

		public override void DoEditInterface(Listing_ScenEdit listing)
		{
			Rect scenPartRect = listing.GetScenPartRect(this, ScenPart.RowHeight * 2f);
			Rect rect = scenPartRect.TopHalf();
			if (Widgets.ButtonText(rect, stat.LabelCap))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (StatDef allDef in DefDatabase<StatDef>.AllDefs)
				{
					StatDef localSd = allDef;
					list.Add(new FloatMenuOption(localSd.LabelCap, delegate
					{
						stat = localSd;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Rect rect2 = scenPartRect.BottomHalf();
			Rect rect3 = rect2.LeftHalf().Rounded();
			Rect rect4 = rect2.RightHalf().Rounded();
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect3, "multiplier".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Widgets.TextFieldPercent(rect4, ref factor, ref factorBuf, 0f, 100f);
		}

		public override string Summary(Scenario scen)
		{
			return "ScenPart_StatFactor".Translate(stat.label, factor.ToStringPercent());
		}

		public override void Randomize()
		{
			stat = (from d in DefDatabase<StatDef>.AllDefs
			where d.scenarioRandomizable
			select d).RandomElement();
			factor = GenMath.RoundedHundredth(Rand.Range(0.1f, 3f));
		}

		public override bool TryMerge(ScenPart other)
		{
			ScenPart_StatFactor scenPart_StatFactor = other as ScenPart_StatFactor;
			if (scenPart_StatFactor != null && scenPart_StatFactor.stat == stat)
			{
				factor *= scenPart_StatFactor.factor;
				return true;
			}
			return false;
		}

		public float GetStatFactor(StatDef stat)
		{
			if (stat == this.stat)
			{
				return factor;
			}
			return 1f;
		}
	}
}
