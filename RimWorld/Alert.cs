using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Alert
	{
		protected AlertPriority defaultPriority;

		protected string defaultLabel;

		protected string defaultExplanation;

		protected float lastBellTime = -1000f;

		private int jumpToTargetCycleIndex;

		private AlertBounce alertBounce;

		public const float Width = 154f;

		private const float TextWidth = 148f;

		public const float Height = 28f;

		private const float ItemPeekWidth = 30f;

		public const float InfoRectWidth = 330f;

		private static readonly Texture2D AlertBGTex = SolidColorMaterials.NewSolidColorTexture(Color.white);

		private static readonly Texture2D AlertBGTexHighlight = TexUI.HighlightTex;

		private static List<GlobalTargetInfo> tmpTargets = new List<GlobalTargetInfo>();

		public virtual AlertPriority Priority => defaultPriority;

		protected virtual Color BGColor => Color.clear;

		public virtual bool Active
		{
			get
			{
				AlertReport report = GetReport();
				return report.active;
			}
		}

		public abstract AlertReport GetReport();

		public virtual string GetExplanation()
		{
			return defaultExplanation;
		}

		public virtual string GetLabel()
		{
			return defaultLabel;
		}

		public void Notify_Started()
		{
			if ((int)Priority >= 1)
			{
				if (alertBounce == null)
				{
					alertBounce = new AlertBounce();
				}
				alertBounce.DoAlertStartEffect();
				if (Time.timeSinceLevelLoad > 1f && Time.realtimeSinceStartup > lastBellTime + 0.5f)
				{
					SoundDefOf.TinyBell.PlayOneShotOnCamera();
					lastBellTime = Time.realtimeSinceStartup;
				}
			}
		}

		public virtual void AlertActiveUpdate()
		{
		}

		public virtual Rect DrawAt(float topY, bool minimized)
		{
			Text.Font = GameFont.Small;
			string label = GetLabel();
			float height = Text.CalcHeight(label, 148f);
			Rect rect = new Rect((float)UI.screenWidth - 154f, topY, 154f, height);
			if (alertBounce != null)
			{
				rect.x -= alertBounce.CalculateHorizontalOffset();
			}
			GUI.color = BGColor;
			GUI.DrawTexture(rect, AlertBGTex);
			GUI.color = Color.white;
			GUI.BeginGroup(rect);
			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(new Rect(0f, 0f, 148f, height), label);
			GUI.EndGroup();
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, AlertBGTexHighlight);
			}
			if (Widgets.ButtonInvisible(rect))
			{
				AlertReport report = GetReport();
				IEnumerable<GlobalTargetInfo> culprits = report.culprits;
				if (culprits != null)
				{
					tmpTargets.Clear();
					foreach (GlobalTargetInfo item in culprits)
					{
						if (item.IsValid)
						{
							tmpTargets.Add(item);
						}
					}
					if (tmpTargets.Any())
					{
						if (Event.current.button == 1)
						{
							jumpToTargetCycleIndex--;
						}
						else
						{
							jumpToTargetCycleIndex++;
						}
						GlobalTargetInfo target = tmpTargets[GenMath.PositiveMod(jumpToTargetCycleIndex, tmpTargets.Count)];
						CameraJumper.TryJumpAndSelect(target);
						tmpTargets.Clear();
					}
				}
			}
			Text.Anchor = TextAnchor.UpperLeft;
			return rect;
		}

		public void DrawInfoPane()
		{
			if (Event.current.type == EventType.Repaint)
			{
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				string expString = GetExplanation();
				if (GetReport().AnyCulpritValid)
				{
					expString = expString + "\n\n(" + "ClickToJumpToProblem".Translate() + ")";
				}
				float num = Text.CalcHeight(expString, 310f);
				num += 20f;
				float x = (float)UI.screenWidth - 154f - 330f - 8f;
				Vector2 mousePosition = Event.current.mousePosition;
				Rect infoRect = new Rect(x, Mathf.Max(Mathf.Min(mousePosition.y, (float)UI.screenHeight - num), 0f), 330f, num);
				if (infoRect.yMax > (float)UI.screenHeight)
				{
					infoRect.y -= (float)UI.screenHeight - infoRect.yMax;
				}
				if (infoRect.y < 0f)
				{
					infoRect.y = 0f;
				}
				Find.WindowStack.ImmediateWindow(138956, infoRect, WindowLayer.GameUI, delegate
				{
					Text.Font = GameFont.Small;
					Rect rect = infoRect.AtZero();
					Widgets.DrawWindowBackground(rect);
					Rect position = rect.ContractedBy(10f);
					GUI.BeginGroup(position);
					Widgets.Label(new Rect(0f, 0f, position.width, position.height), expString);
					GUI.EndGroup();
				}, doBackground: false);
			}
		}
	}
}
