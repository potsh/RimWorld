using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class Page : Window
	{
		public Page prev;

		public Page next;

		public Action nextAct;

		public static readonly Vector2 StandardSize = new Vector2(1020f, 764f);

		public const float TitleAreaHeight = 45f;

		public const float BottomButHeight = 38f;

		protected static readonly Vector2 BottomButSize = new Vector2(150f, 38f);

		public override Vector2 InitialSize => StandardSize;

		public virtual string PageTitle => null;

		public Page()
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnAccept = false;
			closeOnCancel = false;
			forceCatchAcceptAndCancelEventEvenIfUnfocused = true;
		}

		protected void DrawPageTitle(Rect rect)
		{
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(0f, 0f, rect.width, 45f), PageTitle);
			Text.Font = GameFont.Small;
		}

		protected Rect GetMainRect(Rect rect, float extraTopSpace = 0f, bool ignoreTitle = false)
		{
			float num = 0f;
			if (!ignoreTitle)
			{
				num = 45f + extraTopSpace;
			}
			return new Rect(0f, num, rect.width, rect.height - 38f - num - 17f);
		}

		protected void DoBottomButtons(Rect rect, string nextLabel = null, string midLabel = null, Action midAct = null, bool showNext = true, bool doNextOnKeypress = true)
		{
			float num = rect.y + rect.height - 38f;
			Text.Font = GameFont.Small;
			string label = "Back".Translate();
			float x = rect.x;
			float y = num;
			Vector2 bottomButSize = BottomButSize;
			float x2 = bottomButSize.x;
			Vector2 bottomButSize2 = BottomButSize;
			Rect rect2 = new Rect(x, y, x2, bottomButSize2.y);
			if ((Widgets.ButtonText(rect2, label) || KeyBindingDefOf.Cancel.KeyDownEvent) && CanDoBack())
			{
				DoBack();
			}
			if (showNext)
			{
				if (nextLabel.NullOrEmpty())
				{
					nextLabel = "Next".Translate();
				}
				float num2 = rect.x + rect.width;
				Vector2 bottomButSize3 = BottomButSize;
				float x3 = num2 - bottomButSize3.x;
				float y2 = num;
				Vector2 bottomButSize4 = BottomButSize;
				float x4 = bottomButSize4.x;
				Vector2 bottomButSize5 = BottomButSize;
				Rect rect3 = new Rect(x3, y2, x4, bottomButSize5.y);
				if ((Widgets.ButtonText(rect3, nextLabel) || (doNextOnKeypress && KeyBindingDefOf.Accept.KeyDownEvent)) && CanDoNext())
				{
					DoNext();
				}
				UIHighlighter.HighlightOpportunity(rect3, "NextPage");
			}
			if (midAct != null)
			{
				float num3 = rect.x + rect.width / 2f;
				Vector2 bottomButSize6 = BottomButSize;
				float x5 = num3 - bottomButSize6.x / 2f;
				float y3 = num;
				Vector2 bottomButSize7 = BottomButSize;
				float x6 = bottomButSize7.x;
				Vector2 bottomButSize8 = BottomButSize;
				Rect rect4 = new Rect(x5, y3, x6, bottomButSize8.y);
				if (Widgets.ButtonText(rect4, midLabel))
				{
					midAct();
				}
			}
		}

		protected virtual bool CanDoBack()
		{
			return !TutorSystem.TutorialMode || TutorSystem.AllowAction("GotoPrevPage");
		}

		protected virtual bool CanDoNext()
		{
			return !TutorSystem.TutorialMode || TutorSystem.AllowAction("GotoNextPage");
		}

		protected virtual void DoNext()
		{
			if (next != null)
			{
				Find.WindowStack.Add(next);
			}
			if (nextAct != null)
			{
				nextAct();
			}
			TutorSystem.Notify_Event("PageClosed");
			TutorSystem.Notify_Event("GoToNextPage");
			Close();
		}

		protected virtual void DoBack()
		{
			if (prev != null)
			{
				Find.WindowStack.Add(prev);
			}
			TutorSystem.Notify_Event("PageClosed");
			TutorSystem.Notify_Event("GoToPrevPage");
			Close();
		}

		public override void OnCancelKeyPressed()
		{
			if (closeOnCancel && (Find.World == null || !Find.WorldRoutePlanner.Active))
			{
				if (CanDoBack())
				{
					DoBack();
				}
				else
				{
					Close();
				}
				Event.current.Use();
				base.OnCancelKeyPressed();
			}
		}

		public override void OnAcceptKeyPressed()
		{
			if (closeOnAccept && (Find.World == null || !Find.WorldRoutePlanner.Active))
			{
				if (CanDoNext())
				{
					DoNext();
				}
				Event.current.Use();
			}
		}
	}
}
