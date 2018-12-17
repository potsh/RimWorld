using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class InspectPaneUtility
	{
		private static Dictionary<string, string> truncatedLabelsCached = new Dictionary<string, string>();

		public const float TabWidth = 72f;

		public const float TabHeight = 30f;

		private static readonly Texture2D InspectTabButtonFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.07450981f, 0.08627451f, 0.105882354f, 1f));

		public const float CornerButtonsSize = 24f;

		public const float PaneInnerMargin = 12f;

		public const float PaneHeight = 165f;

		private const int TabMinimum = 6;

		private static List<Thing> selectedThings = new List<Thing>();

		public static void Reset()
		{
			truncatedLabelsCached.Clear();
		}

		public static float PaneWidthFor(IInspectPane pane)
		{
			if (pane == null)
			{
				return 432f;
			}
			int num = 0;
			foreach (InspectTabBase curTab in pane.CurTabs)
			{
				if (curTab.IsVisible)
				{
					num++;
				}
			}
			return 72f * (float)Mathf.Max(6, num);
		}

		public static Vector2 PaneSizeFor(IInspectPane pane)
		{
			return new Vector2(PaneWidthFor(pane), 165f);
		}

		public static bool CanInspectTogether(object A, object B)
		{
			Thing thing = A as Thing;
			Thing thing2 = B as Thing;
			if (thing == null || thing2 == null)
			{
				return false;
			}
			if (thing.def.category == ThingCategory.Pawn)
			{
				return false;
			}
			return thing.def == thing2.def;
		}

		public static string AdjustedLabelFor(IEnumerable<object> selected, Rect rect)
		{
			Zone zone = selected.First() as Zone;
			string str;
			if (zone != null)
			{
				str = zone.label;
			}
			else
			{
				selectedThings.Clear();
				foreach (object item in selected)
				{
					Thing thing = item as Thing;
					if (thing != null)
					{
						selectedThings.Add(thing);
					}
				}
				if (selectedThings.Count == 1)
				{
					str = selectedThings[0].LabelCap;
				}
				else
				{
					IEnumerable<IGrouping<string, Thing>> source = from th in selectedThings
					group th by th.LabelCapNoCount into g
					select (g);
					str = ((source.Count() <= 1) ? selectedThings[0].LabelCapNoCount : "VariousLabel".Translate());
					int num = 0;
					for (int i = 0; i < selectedThings.Count; i++)
					{
						num += selectedThings[i].stackCount;
					}
					str = str + " x" + num;
				}
				selectedThings.Clear();
			}
			Text.Font = GameFont.Medium;
			return str.Truncate(rect.width, truncatedLabelsCached);
		}

		public static void ExtraOnGUI(IInspectPane pane)
		{
			if (pane.AnythingSelected)
			{
				if (KeyBindingDefOf.SelectNextInCell.KeyDownEvent)
				{
					pane.SelectNextInCell();
				}
				if (Current.ProgramState == ProgramState.Playing)
				{
					pane.DrawInspectGizmos();
				}
				DoTabs(pane);
			}
		}

		public static void UpdateTabs(IInspectPane pane)
		{
			bool flag = false;
			foreach (InspectTabBase curTab in pane.CurTabs)
			{
				if (curTab.IsVisible && curTab.GetType() == pane.OpenTabType)
				{
					curTab.TabUpdate();
					flag = true;
				}
			}
			if (!flag)
			{
				pane.CloseOpenTab();
			}
		}

		public static void InspectPaneOnGUI(Rect inRect, IInspectPane pane)
		{
			pane.RecentHeight = 165f;
			if (pane.AnythingSelected)
			{
				try
				{
					Rect rect = inRect.ContractedBy(12f);
					rect.yMin -= 4f;
					rect.yMax += 6f;
					GUI.BeginGroup(rect);
					float lineEndWidth = 0f;
					if (pane.ShouldShowSelectNextInCellButton)
					{
						Rect rect2 = new Rect(rect.width - 24f, 0f, 24f, 24f);
						if (Widgets.ButtonImage(rect2, TexButton.SelectOverlappingNext))
						{
							pane.SelectNextInCell();
						}
						lineEndWidth += 24f;
						TooltipHandler.TipRegion(rect2, "SelectNextInSquareTip".Translate(KeyBindingDefOf.SelectNextInCell.MainKeyLabel));
					}
					pane.DoInspectPaneButtons(rect, ref lineEndWidth);
					Rect rect3 = new Rect(0f, 0f, rect.width - lineEndWidth, 50f);
					string label = pane.GetLabel(rect3);
					rect3.width += 300f;
					Text.Font = GameFont.Medium;
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect3, label);
					if (pane.ShouldShowPaneContents)
					{
						Rect rect4 = rect.AtZero();
						rect4.yMin += 26f;
						pane.DoPaneContents(rect4);
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception doing inspect pane: " + ex.ToString());
				}
				finally
				{
					GUI.EndGroup();
				}
			}
		}

		private static void DoTabs(IInspectPane pane)
		{
			try
			{
				float y = pane.PaneTopY - 30f;
				float num = PaneWidthFor(pane) - 72f;
				float width = 0f;
				bool flag = false;
				foreach (InspectTabBase curTab in pane.CurTabs)
				{
					if (curTab.IsVisible)
					{
						Rect rect = new Rect(num, y, 72f, 30f);
						width = num;
						Text.Font = GameFont.Small;
						if (Widgets.ButtonText(rect, curTab.labelKey.Translate()))
						{
							InterfaceToggleTab(curTab, pane);
						}
						bool flag2 = curTab.GetType() == pane.OpenTabType;
						if (!flag2 && !curTab.TutorHighlightTagClosed.NullOrEmpty())
						{
							UIHighlighter.HighlightOpportunity(rect, curTab.TutorHighlightTagClosed);
						}
						if (flag2)
						{
							curTab.DoTabGUI();
							pane.RecentHeight = 700f;
							flag = true;
						}
						num -= 72f;
					}
				}
				if (flag)
				{
					GUI.DrawTexture(new Rect(0f, y, width, 30f), InspectTabButtonFillTex);
				}
			}
			catch (Exception ex)
			{
				Log.ErrorOnce(ex.ToString(), 742783);
			}
		}

		private static bool IsOpen(InspectTabBase tab, IInspectPane pane)
		{
			return tab.GetType() == pane.OpenTabType;
		}

		private static void ToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (IsOpen(tab, pane) || (tab == null && pane.OpenTabType == null))
			{
				pane.OpenTabType = null;
				SoundDefOf.TabClose.PlayOneShotOnCamera();
			}
			else
			{
				tab.OnOpen();
				pane.OpenTabType = tab.GetType();
				SoundDefOf.TabOpen.PlayOneShotOnCamera();
			}
		}

		public static InspectTabBase OpenTab(Type inspectTabType)
		{
			MainTabWindow_Inspect mainTabWindow_Inspect = (MainTabWindow_Inspect)MainButtonDefOf.Inspect.TabWindow;
			InspectTabBase inspectTabBase = (from t in mainTabWindow_Inspect.CurTabs
			where inspectTabType.IsAssignableFrom(t.GetType())
			select t).FirstOrDefault();
			if (inspectTabBase != null)
			{
				if (Find.MainTabsRoot.OpenTab != MainButtonDefOf.Inspect)
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Inspect);
				}
				if (!IsOpen(inspectTabBase, mainTabWindow_Inspect))
				{
					ToggleTab(inspectTabBase, mainTabWindow_Inspect);
				}
			}
			return inspectTabBase;
		}

		private static void InterfaceToggleTab(InspectTabBase tab, IInspectPane pane)
		{
			if (!TutorSystem.TutorialMode || IsOpen(tab, pane) || TutorSystem.AllowAction("ITab-" + tab.tutorTag + "-Open"))
			{
				ToggleTab(tab, pane);
			}
		}
	}
}
