using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	public static class TooltipHandler
	{
		private static Dictionary<int, ActiveTip> activeTips = new Dictionary<int, ActiveTip>();

		private static int frame = 0;

		private static List<int> dyingTips = new List<int>(32);

		private static float TooltipDelay = 0.45f;

		private const float SpaceBetweenTooltips = 2f;

		private static List<ActiveTip> drawingTips = new List<ActiveTip>();

		[CompilerGenerated]
		private static Comparison<ActiveTip> _003C_003Ef__mg_0024cache0;

		public static void ClearTooltipsFrom(Rect rect)
		{
			if (Event.current.type == EventType.Repaint && Mouse.IsOver(rect))
			{
				dyingTips.Clear();
				foreach (KeyValuePair<int, ActiveTip> activeTip in activeTips)
				{
					if (activeTip.Value.lastTriggerFrame == frame)
					{
						dyingTips.Add(activeTip.Key);
					}
				}
				for (int i = 0; i < dyingTips.Count; i++)
				{
					activeTips.Remove(dyingTips[i]);
				}
			}
		}

		public static void TipRegion(Rect rect, Func<string> textGetter, int uniqueId)
		{
			TipRegion(rect, new TipSignal(textGetter, uniqueId));
		}

		public static void TipRegion(Rect rect, TipSignal tip)
		{
			if (Event.current.type == EventType.Repaint && (tip.textGetter != null || !tip.text.NullOrEmpty()) && Mouse.IsOver(rect))
			{
				if (DebugViewSettings.drawTooltipEdges)
				{
					Widgets.DrawBox(rect);
				}
				if (!activeTips.ContainsKey(tip.uniqueId))
				{
					ActiveTip value = new ActiveTip(tip);
					activeTips.Add(tip.uniqueId, value);
					activeTips[tip.uniqueId].firstTriggerTime = (double)Time.realtimeSinceStartup;
				}
				activeTips[tip.uniqueId].lastTriggerFrame = frame;
				activeTips[tip.uniqueId].signal.text = tip.text;
				activeTips[tip.uniqueId].signal.textGetter = tip.textGetter;
			}
		}

		public static void DoTooltipGUI()
		{
			DrawActiveTips();
			if (Event.current.type == EventType.Repaint)
			{
				CleanActiveTooltips();
				frame++;
			}
		}

		private static void DrawActiveTips()
		{
			drawingTips.Clear();
			foreach (KeyValuePair<int, ActiveTip> activeTip in activeTips)
			{
				if ((double)Time.realtimeSinceStartup > activeTip.Value.firstTriggerTime + (double)TooltipDelay)
				{
					drawingTips.Add(activeTip.Value);
				}
			}
			drawingTips.Sort(CompareTooltipsByPriority);
			Vector2 pos = CalculateInitialTipPosition(drawingTips);
			for (int i = 0; i < drawingTips.Count; i++)
			{
				pos.y += drawingTips[i].DrawTooltip(pos);
				pos.y += 2f;
			}
			drawingTips.Clear();
		}

		private static void CleanActiveTooltips()
		{
			dyingTips.Clear();
			foreach (KeyValuePair<int, ActiveTip> activeTip in activeTips)
			{
				if (activeTip.Value.lastTriggerFrame != frame)
				{
					dyingTips.Add(activeTip.Key);
				}
			}
			for (int i = 0; i < dyingTips.Count; i++)
			{
				activeTips.Remove(dyingTips[i]);
			}
		}

		private static Vector2 CalculateInitialTipPosition(List<ActiveTip> drawingTips)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < drawingTips.Count; i++)
			{
				Rect tipRect = drawingTips[i].TipRect;
				num += tipRect.height;
				num2 = Mathf.Max(num2, tipRect.width);
				if (i != drawingTips.Count - 1)
				{
					num += 2f;
				}
			}
			return GenUI.GetMouseAttachedWindowPos(num2, num);
		}

		private static int CompareTooltipsByPriority(ActiveTip A, ActiveTip B)
		{
			if (A.signal.priority == B.signal.priority)
			{
				return 0;
			}
			if (A.signal.priority == TooltipPriority.Pawn)
			{
				return -1;
			}
			if (B.signal.priority == TooltipPriority.Pawn)
			{
				return 1;
			}
			return 0;
		}
	}
}
