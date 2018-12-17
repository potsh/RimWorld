using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class MainButtonWorker
	{
		public MainButtonDef def;

		private const float CompactModeMargin = 2f;

		public virtual float ButtonBarPercent => 0f;

		public virtual bool Disabled
		{
			get
			{
				if (Find.CurrentMap == null && (!def.validWithoutMap || def == MainButtonDefOf.World))
				{
					return true;
				}
				if (Find.WorldRoutePlanner.Active && Find.WorldRoutePlanner.FormingCaravan && (!def.validWithoutMap || def == MainButtonDefOf.World))
				{
					return true;
				}
				return false;
			}
		}

		public abstract void Activate();

		public virtual void InterfaceTryActivate()
		{
			if (!TutorSystem.TutorialMode || !def.canBeTutorDenied || Find.MainTabsRoot.OpenTab == def || TutorSystem.AllowAction("MainTab-" + def.defName + "-Open"))
			{
				Activate();
			}
		}

		public virtual void DoButton(Rect rect)
		{
			Text.Font = GameFont.Small;
			string text = def.LabelCap;
			float num = def.LabelCapWidth;
			if (num > rect.width - 2f)
			{
				text = def.ShortenedLabelCap;
				num = def.ShortenedLabelCapWidth;
			}
			if (Disabled)
			{
				Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
				if (Event.current.type == EventType.MouseDown && Mouse.IsOver(rect))
				{
					Event.current.Use();
				}
			}
			else
			{
				bool flag = num > 0.85f * rect.width - 1f;
				Rect rect2 = rect;
				string label = text;
				float textLeftMargin = (!flag) ? (-1f) : 2f;
				if (Widgets.ButtonTextSubtle(rect2, label, ButtonBarPercent, textLeftMargin, SoundDefOf.Mouseover_Category))
				{
					InterfaceTryActivate();
				}
				if (Find.MainTabsRoot.OpenTab != def && !Find.WindowStack.NonImmediateDialogWindowOpen)
				{
					UIHighlighter.HighlightOpportunity(rect, def.cachedHighlightTagClosed);
				}
				if (!def.description.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect, def.description);
				}
			}
		}
	}
}
