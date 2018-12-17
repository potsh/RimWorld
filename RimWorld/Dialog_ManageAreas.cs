using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageAreas : Window
	{
		private Map map;

		private static Regex validNameRegex = new Regex("^[\\p{L}0-9 '\\-]*$");

		public override Vector2 InitialSize => new Vector2(450f, 400f);

		public Dialog_ManageAreas(Map map)
		{
			this.map = map;
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = inRect.width;
			listing_Standard.Begin(inRect);
			List<Area> allAreas = map.areaManager.AllAreas;
			int i = 0;
			for (int j = 0; j < allAreas.Count; j++)
			{
				if (allAreas[j].Mutable)
				{
					Rect rect = listing_Standard.GetRect(24f);
					DoAreaRow(rect, allAreas[j]);
					listing_Standard.Gap(6f);
					i++;
				}
			}
			if (map.areaManager.CanMakeNewAllowed())
			{
				for (; i < 9; i++)
				{
					listing_Standard.Gap(30f);
				}
				if (listing_Standard.ButtonText("NewArea".Translate()))
				{
					map.areaManager.TryMakeNewAllowed(out Area_Allowed _);
				}
			}
			listing_Standard.End();
		}

		private static void DoAreaRow(Rect rect, Area area)
		{
			if (Mouse.IsOver(rect))
			{
				area.MarkForDraw();
				GUI.color = area.Color;
				Widgets.DrawHighlight(rect);
				GUI.color = Color.white;
			}
			GUI.BeginGroup(rect);
			WidgetRow widgetRow = new WidgetRow(0f, 0f);
			widgetRow.Icon(area.ColorTexture);
			widgetRow.Gap(4f);
			float num = rect.width - widgetRow.FinalX - 4f;
			Vector2 vector = Text.CalcSize("Rename".Translate());
			float num2 = num - vector.x - 16f - 4f;
			Vector2 vector2 = Text.CalcSize("InvertArea".Translate());
			float width = num2 - vector2.x - 16f - 4f - 24f;
			widgetRow.Label(area.Label, width);
			if (widgetRow.ButtonText("Rename".Translate()))
			{
				Find.WindowStack.Add(new Dialog_RenameArea(area));
			}
			if (widgetRow.ButtonText("InvertArea".Translate()))
			{
				area.Invert();
			}
			WidgetRow widgetRow2 = widgetRow;
			Texture2D deleteX = TexButton.DeleteX;
			Color? mouseoverColor = GenUI.SubtleMouseoverColor;
			if (widgetRow2.ButtonIcon(deleteX, null, mouseoverColor))
			{
				area.Delete();
			}
			GUI.EndGroup();
		}

		public static void DoNameInputRect(Rect rect, ref string name, int maxLength)
		{
			string text = Widgets.TextField(rect, name);
			if (text.Length <= maxLength && validNameRegex.IsMatch(text))
			{
				name = text;
			}
		}
	}
}
