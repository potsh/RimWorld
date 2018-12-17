using System;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public class ActiveTip
	{
		public TipSignal signal;

		public double firstTriggerTime;

		public int lastTriggerFrame;

		private const int TipMargin = 4;

		private const float MaxWidth = 260f;

		public static readonly Texture2D TooltipBGAtlas = ContentFinder<Texture2D>.Get("UI/Widgets/TooltipBG");

		private string FinalText
		{
			get
			{
				string text;
				if (signal.textGetter != null)
				{
					try
					{
						text = signal.textGetter();
					}
					catch (Exception ex)
					{
						Log.Error(ex.ToString());
						text = "Error getting tip text.";
					}
				}
				else
				{
					text = signal.text;
				}
				return text.TrimEnd();
			}
		}

		public Rect TipRect
		{
			get
			{
				string finalText = FinalText;
				Vector2 vector = Text.CalcSize(finalText);
				if (vector.x > 260f)
				{
					vector.x = 260f;
					vector.y = Text.CalcHeight(finalText, vector.x);
				}
				Rect rect = new Rect(0f, 0f, vector.x, vector.y);
				return rect.ContractedBy(-4f);
			}
		}

		public ActiveTip(TipSignal signal)
		{
			this.signal = signal;
		}

		public ActiveTip(ActiveTip cloneSource)
		{
			signal = cloneSource.signal;
			firstTriggerTime = cloneSource.firstTriggerTime;
			lastTriggerFrame = cloneSource.lastTriggerFrame;
		}

		public float DrawTooltip(Vector2 pos)
		{
			Text.Font = GameFont.Small;
			string finalText = FinalText;
			Rect bgRect = TipRect;
			bgRect.position = pos;
			Find.WindowStack.ImmediateWindow(153 * signal.uniqueId + 62346, bgRect, WindowLayer.Super, delegate
			{
				Rect rect = bgRect.AtZero();
				Widgets.DrawAtlas(rect, TooltipBGAtlas);
				Text.Font = GameFont.Small;
				Widgets.Label(rect.ContractedBy(4f), finalText);
			}, doBackground: false);
			return bgRect.height;
		}
	}
}
