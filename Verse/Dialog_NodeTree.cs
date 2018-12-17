using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_NodeTree : Window
	{
		private Vector2 scrollPosition;

		protected string title;

		protected DiaNode curNode;

		public Action closeAction;

		private float makeInteractiveAtTime;

		public Color screenFillColor = Color.clear;

		protected float minOptionsAreaHeight;

		private const float InteractivityDelay = 0.5f;

		private const float TitleHeight = 36f;

		protected const float OptHorMargin = 15f;

		protected const float OptVerticalSpace = 7f;

		private float optTotalHeight;

		public override Vector2 InitialSize => new Vector2(620f, 480f);

		private bool InteractiveNow => Time.realtimeSinceStartup >= makeInteractiveAtTime;

		public Dialog_NodeTree(DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false, string title = null)
		{
			this.title = title;
			GotoNode(nodeRoot);
			forcePause = true;
			absorbInputAroundWindow = true;
			closeOnAccept = false;
			closeOnCancel = false;
			if (delayInteractivity)
			{
				makeInteractiveAtTime = Time.realtimeSinceStartup + 0.5f;
			}
			soundAppear = SoundDefOf.CommsWindow_Open;
			soundClose = SoundDefOf.CommsWindow_Close;
			if (radioMode)
			{
				soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
		}

		public override void PreClose()
		{
			base.PreClose();
			curNode.PreClose();
		}

		public override void PostClose()
		{
			base.PostClose();
			if (closeAction != null)
			{
				closeAction();
			}
		}

		public override void WindowOnGUI()
		{
			if (screenFillColor != Color.clear)
			{
				GUI.color = screenFillColor;
				GUI.DrawTexture(new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight), BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = inRect.AtZero();
			if (title != null)
			{
				Text.Font = GameFont.Small;
				Rect rect2 = rect;
				rect2.height = 36f;
				rect.yMin += 53f;
				Widgets.DrawTitleBG(rect2);
				rect2.xMin += 9f;
				rect2.yMin += 5f;
				Widgets.Label(rect2, title);
			}
			DrawNode(rect);
		}

		protected void DrawNode(Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height - Mathf.Max(optTotalHeight, minOptionsAreaHeight));
			float width = rect.width - 16f;
			Rect rect2 = new Rect(0f, 0f, width, Text.CalcHeight(curNode.text, width));
			Widgets.BeginScrollView(outRect, ref scrollPosition, rect2);
			Widgets.Label(rect2, curNode.text);
			Widgets.EndScrollView();
			float num = rect.height - optTotalHeight;
			float num2 = 0f;
			for (int i = 0; i < curNode.options.Count; i++)
			{
				Rect rect3 = new Rect(15f, num, rect.width - 30f, 999f);
				float num3 = curNode.options[i].OptOnGUI(rect3, InteractiveNow);
				num += num3 + 7f;
				num2 += num3 + 7f;
			}
			if (Event.current.type == EventType.Layout)
			{
				optTotalHeight = num2;
			}
			GUI.EndGroup();
		}

		public void GotoNode(DiaNode node)
		{
			foreach (DiaOption option in node.options)
			{
				option.dialog = this;
			}
			curNode = node;
		}
	}
}
