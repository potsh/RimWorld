using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class FloatMenuOption
	{
		private string labelInt;

		public Action action;

		private MenuOptionPriority priorityInt = MenuOptionPriority.Default;

		public bool autoTakeable;

		public float autoTakeablePriority;

		public Action mouseoverGuiAction;

		public Thing revalidateClickTarget;

		public WorldObject revalidateWorldClickTarget;

		public float extraPartWidth;

		public Func<Rect, bool> extraPartOnGUI;

		public string tutorTag;

		private FloatMenuSizeMode sizeMode;

		private float cachedRequiredHeight;

		private float cachedRequiredWidth;

		public const float MaxWidth = 300f;

		private const float NormalVerticalMargin = 4f;

		private const float TinyVerticalMargin = 1f;

		private const float NormalHorizontalMargin = 6f;

		private const float TinyHorizontalMargin = 3f;

		private const float MouseOverLabelShift = 4f;

		private static readonly Color ColorBGActive = new ColorInt(21, 25, 29).ToColor;

		private static readonly Color ColorBGActiveMouseover = new ColorInt(29, 45, 50).ToColor;

		private static readonly Color ColorBGDisabled = new ColorInt(40, 40, 40).ToColor;

		private static readonly Color ColorTextActive = Color.white;

		private static readonly Color ColorTextDisabled = new Color(0.9f, 0.9f, 0.9f);

		public const float ExtraPartHeight = 30f;

		public string Label
		{
			get
			{
				return labelInt;
			}
			set
			{
				if (value.NullOrEmpty())
				{
					value = "(missing label)";
				}
				labelInt = value.TrimEnd();
				SetSizeMode(sizeMode);
			}
		}

		private float VerticalMargin => (sizeMode != FloatMenuSizeMode.Normal) ? 1f : 4f;

		private float HorizontalMargin => (sizeMode != FloatMenuSizeMode.Normal) ? 3f : 6f;

		private GameFont CurrentFont => (sizeMode == FloatMenuSizeMode.Normal) ? GameFont.Small : GameFont.Tiny;

		public bool Disabled
		{
			get
			{
				return action == null;
			}
			set
			{
				if (value)
				{
					action = null;
				}
			}
		}

		public float RequiredHeight => cachedRequiredHeight;

		public float RequiredWidth => cachedRequiredWidth;

		public MenuOptionPriority Priority
		{
			get
			{
				if (Disabled)
				{
					return MenuOptionPriority.DisabledOption;
				}
				return priorityInt;
			}
			set
			{
				if (Disabled)
				{
					Log.Error("Setting priority on disabled FloatMenuOption: " + Label);
				}
				priorityInt = value;
			}
		}

		public FloatMenuOption(string label, Action action, MenuOptionPriority priority = MenuOptionPriority.Default, Action mouseoverGuiAction = null, Thing revalidateClickTarget = null, float extraPartWidth = 0f, Func<Rect, bool> extraPartOnGUI = null, WorldObject revalidateWorldClickTarget = null)
		{
			Label = label;
			this.action = action;
			priorityInt = priority;
			this.revalidateClickTarget = revalidateClickTarget;
			this.mouseoverGuiAction = mouseoverGuiAction;
			this.extraPartWidth = extraPartWidth;
			this.extraPartOnGUI = extraPartOnGUI;
			this.revalidateWorldClickTarget = revalidateWorldClickTarget;
		}

		public void SetSizeMode(FloatMenuSizeMode newSizeMode)
		{
			sizeMode = newSizeMode;
			GameFont font = Text.Font;
			Text.Font = CurrentFont;
			float width = 300f - (2f * HorizontalMargin + 4f + extraPartWidth);
			cachedRequiredHeight = 2f * VerticalMargin + Text.CalcHeight(Label, width);
			float num = HorizontalMargin + 4f;
			Vector2 vector = Text.CalcSize(Label);
			cachedRequiredWidth = num + vector.x + extraPartWidth + HorizontalMargin;
			Text.Font = font;
		}

		public void Chosen(bool colonistOrdering, FloatMenu floatMenu)
		{
			floatMenu?.PreOptionChosen(this);
			if (!Disabled)
			{
				if (action != null)
				{
					if (colonistOrdering)
					{
						SoundDefOf.ColonistOrdered.PlayOneShotOnCamera();
					}
					action();
				}
			}
			else
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera();
			}
		}

		public virtual bool DoGUI(Rect rect, bool colonistOrdering, FloatMenu floatMenu)
		{
			Rect rect2 = rect;
			rect2.height -= 1f;
			bool flag = !Disabled && Mouse.IsOver(rect2);
			bool flag2 = false;
			Text.Font = CurrentFont;
			Rect rect3 = rect;
			rect3.xMin += HorizontalMargin;
			rect3.xMax -= HorizontalMargin;
			rect3.xMax -= 4f;
			rect3.xMax -= extraPartWidth;
			if (flag)
			{
				rect3.x += 4f;
			}
			Rect rect4 = default(Rect);
			if (extraPartWidth != 0f)
			{
				Vector2 vector = Text.CalcSize(Label);
				float num = Mathf.Min(vector.x, rect3.width - 4f);
				rect4 = new Rect(rect3.xMin + num, rect3.yMin, extraPartWidth, 30f);
				flag2 = Mouse.IsOver(rect4);
			}
			if (!Disabled)
			{
				MouseoverSounds.DoRegion(rect2);
			}
			Color color = GUI.color;
			if (Disabled)
			{
				GUI.color = ColorBGDisabled * color;
			}
			else if (flag && !flag2)
			{
				GUI.color = ColorBGActiveMouseover * color;
			}
			else
			{
				GUI.color = ColorBGActive * color;
			}
			GUI.DrawTexture(rect, BaseContent.WhiteTex);
			GUI.color = (Disabled ? ColorTextDisabled : ColorTextActive) * color;
			if (sizeMode == FloatMenuSizeMode.Tiny)
			{
				rect3.y += 1f;
			}
			Widgets.DrawAtlas(rect, TexUI.FloatMenuOptionBG);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect3, Label);
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = color;
			if (extraPartOnGUI != null)
			{
				bool flag3 = extraPartOnGUI(rect4);
				GUI.color = color;
				if (flag3)
				{
					return true;
				}
			}
			if (flag && mouseoverGuiAction != null)
			{
				mouseoverGuiAction();
			}
			if (tutorTag != null)
			{
				UIHighlighter.HighlightOpportunity(rect, tutorTag);
			}
			if (Widgets.ButtonInvisible(rect2))
			{
				if (tutorTag != null && !TutorSystem.AllowAction(tutorTag))
				{
					return false;
				}
				Chosen(colonistOrdering, floatMenu);
				if (tutorTag != null)
				{
					TutorSystem.Notify_Event(tutorTag);
				}
				return true;
			}
			return false;
		}

		public override string ToString()
		{
			return "FloatMenuOption(" + Label + ", " + ((!Disabled) ? "enabled" : "disabled") + ")";
		}
	}
}
