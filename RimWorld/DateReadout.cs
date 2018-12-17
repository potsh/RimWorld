using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DateReadout
	{
		private static string dateString;

		private static int dateStringDay;

		private static Season dateStringSeason;

		private static Quadrum dateStringQuadrum;

		private static int dateStringYear;

		private static readonly List<string> fastHourStrings;

		private const float DateRightPadding = 7f;

		public static float Height => (float)(48 + (SeasonLabelVisible ? 26 : 0));

		private static bool SeasonLabelVisible => !WorldRendererUtility.WorldRenderedNow && Find.CurrentMap != null;

		static DateReadout()
		{
			dateStringDay = -1;
			dateStringSeason = Season.Undefined;
			dateStringQuadrum = Quadrum.Undefined;
			dateStringYear = -1;
			fastHourStrings = new List<string>();
			Reset();
		}

		public static void Reset()
		{
			dateString = null;
			dateStringDay = -1;
			dateStringSeason = Season.Undefined;
			dateStringQuadrum = Quadrum.Undefined;
			dateStringYear = -1;
			fastHourStrings.Clear();
			for (int i = 0; i < 24; i++)
			{
				fastHourStrings.Add(i + "LetterHour".Translate());
			}
		}

		public static void DateOnGUI(Rect dateRect)
		{
			Vector2 location;
			if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.selectedTile >= 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.selectedTile);
			}
			else if (WorldRendererUtility.WorldRenderedNow && Find.WorldSelector.NumSelectedObjects > 0)
			{
				location = Find.WorldGrid.LongLatOf(Find.WorldSelector.FirstSelectedObject.Tile);
			}
			else
			{
				if (Find.CurrentMap == null)
				{
					return;
				}
				location = Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
			}
			int index = GenDate.HourInteger(Find.TickManager.TicksAbs, location.x);
			int num = GenDate.DayOfTwelfth(Find.TickManager.TicksAbs, location.x);
			Season season = GenDate.Season(Find.TickManager.TicksAbs, location);
			Quadrum quadrum = GenDate.Quadrum(Find.TickManager.TicksAbs, location.x);
			int num2 = GenDate.Year(Find.TickManager.TicksAbs, location.x);
			string text = (!SeasonLabelVisible) ? string.Empty : season.LabelCap();
			if (num != dateStringDay || season != dateStringSeason || quadrum != dateStringQuadrum || num2 != dateStringYear)
			{
				dateString = GenDate.DateReadoutStringAt(Find.TickManager.TicksAbs, location);
				dateStringDay = num;
				dateStringSeason = season;
				dateStringQuadrum = quadrum;
				dateStringYear = num2;
			}
			Text.Font = GameFont.Small;
			Vector2 vector = Text.CalcSize(fastHourStrings[index]);
			float x = vector.x;
			Vector2 vector2 = Text.CalcSize(dateString);
			float a = Mathf.Max(x, vector2.x);
			Vector2 vector3 = Text.CalcSize(text);
			float num3 = Mathf.Max(a, vector3.x) + 7f;
			dateRect.xMin = dateRect.xMax - num3;
			if (Mouse.IsOver(dateRect))
			{
				Widgets.DrawHighlight(dateRect);
			}
			GUI.BeginGroup(dateRect);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperRight;
			Rect rect = dateRect.AtZero();
			rect.xMax -= 7f;
			Widgets.Label(rect, fastHourStrings[index]);
			rect.yMin += 26f;
			Widgets.Label(rect, dateString);
			rect.yMin += 26f;
			if (!text.NullOrEmpty())
			{
				Widgets.Label(rect, text);
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.EndGroup();
			TooltipHandler.TipRegion(dateRect, new TipSignal(delegate
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < 4; i++)
				{
					Quadrum quadrum2 = (Quadrum)i;
					stringBuilder.AppendLine(quadrum2.Label() + " - " + quadrum2.GetSeason(location.y).LabelCap());
				}
				return "DateReadoutTip".Translate(GenDate.DaysPassed, 15, season.LabelCap(), 15, GenDate.Quadrum(GenTicks.TicksAbs, location.x).Label(), stringBuilder.ToString());
			}, 86423));
		}
	}
}
