using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class MainTabWindow_History : MainTabWindow
	{
		private enum HistoryTab : byte
		{
			Graph,
			Messages,
			Statistics
		}

		private HistoryAutoRecorderGroup historyAutoRecorderGroup;

		private FloatRange graphSection;

		private Vector2 messagesScrollPos;

		private float messagesLastHeight;

		private static HistoryTab curTab = HistoryTab.Graph;

		private static bool showLetters = true;

		private static bool showMessages;

		private const float MessagesRowHeight = 30f;

		private const float PinColumnSize = 30f;

		private const float PinSize = 22f;

		private const float IconColumnSize = 30f;

		private const float DateSize = 200f;

		private const float SpaceBetweenColumns = 10f;

		private static readonly Texture2D PinTex = ContentFinder<Texture2D>.Get("UI/Icons/Pin");

		private static List<CurveMark> marks = new List<CurveMark>();

		public override Vector2 RequestedTabSize => new Vector2(1010f, 640f);

		public override void PreOpen()
		{
			base.PreOpen();
			historyAutoRecorderGroup = Find.History.Groups().FirstOrDefault();
			if (historyAutoRecorderGroup != null)
			{
				graphSection = new FloatRange(0f, (float)Find.TickManager.TicksGame / 60000f);
			}
			List<Map> maps = Find.Maps;
			for (int i = 0; i < maps.Count; i++)
			{
				maps[i].wealthWatcher.ForceRecount();
			}
		}

		public override void DoWindowContents(Rect rect)
		{
			base.DoWindowContents(rect);
			Rect rect2 = rect;
			rect2.yMin += 45f;
			List<TabRecord> list = new List<TabRecord>();
			list.Add(new TabRecord("Graph".Translate(), delegate
			{
				curTab = HistoryTab.Graph;
			}, curTab == HistoryTab.Graph));
			list.Add(new TabRecord("Messages".Translate(), delegate
			{
				curTab = HistoryTab.Messages;
			}, curTab == HistoryTab.Messages));
			list.Add(new TabRecord("Statistics".Translate(), delegate
			{
				curTab = HistoryTab.Statistics;
			}, curTab == HistoryTab.Statistics));
			TabDrawer.DrawTabs(rect2, list);
			switch (curTab)
			{
			case HistoryTab.Graph:
				DoGraphPage(rect2);
				break;
			case HistoryTab.Messages:
				DoMessagesPage(rect2);
				break;
			case HistoryTab.Statistics:
				DoStatisticsPage(rect2);
				break;
			}
		}

		private void DoStatisticsPage(Rect rect)
		{
			rect.yMin += 17f;
			GUI.BeginGroup(rect);
			StringBuilder stringBuilder = new StringBuilder();
			TimeSpan timeSpan = new TimeSpan(0, 0, (int)Find.GameInfo.RealPlayTimeInteracting);
			stringBuilder.AppendLine("Playtime".Translate() + ": " + timeSpan.Days + "LetterDay".Translate() + " " + timeSpan.Hours + "LetterHour".Translate() + " " + timeSpan.Minutes + "LetterMinute".Translate() + " " + timeSpan.Seconds + "LetterSecond".Translate());
			stringBuilder.AppendLine("Storyteller".Translate() + ": " + Find.Storyteller.def.LabelCap);
			DifficultyDef difficulty = Find.Storyteller.difficulty;
			stringBuilder.AppendLine("Difficulty".Translate() + ": " + difficulty.LabelCap);
			if (Find.CurrentMap != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("ThisMapColonyWealthTotal".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthTotal.ToString("F0"));
				stringBuilder.AppendLine("ThisMapColonyWealthItems".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthItems.ToString("F0"));
				stringBuilder.AppendLine("ThisMapColonyWealthBuildings".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthBuildings.ToString("F0"));
				stringBuilder.AppendLine("ThisMapColonyWealthColonistsAndTameAnimals".Translate() + ": " + Find.CurrentMap.wealthWatcher.WealthPawns.ToString("F0"));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("NumThreatBigs".Translate() + ": " + Find.StoryWatcher.statsRecord.numThreatBigs);
			stringBuilder.AppendLine("NumEnemyRaids".Translate() + ": " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
			stringBuilder.AppendLine();
			if (Find.CurrentMap != null)
			{
				stringBuilder.AppendLine("ThisMapDamageTaken".Translate() + ": " + Find.CurrentMap.damageWatcher.DamageTakenEver);
			}
			stringBuilder.AppendLine("ColonistsKilled".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsKilled);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("ColonistsLaunched".Translate() + ": " + Find.StoryWatcher.statsRecord.colonistsLaunched);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(0f, 0f, 400f, 400f);
			Widgets.Label(rect2, stringBuilder.ToString());
			GUI.EndGroup();
		}

		private void DoMessagesPage(Rect rect)
		{
			rect.yMin += 10f;
			Widgets.CheckboxLabeled(new Rect(rect.x, rect.y, 200f, 30f), "ShowLetters".Translate(), ref showLetters, disabled: false, null, null, placeCheckboxNearText: true);
			Widgets.CheckboxLabeled(new Rect(rect.x + 200f, rect.y, 200f, 30f), "ShowMessages".Translate(), ref showMessages, disabled: false, null, null, placeCheckboxNearText: true);
			rect.yMin += 40f;
			bool flag = false;
			Rect outRect = rect;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, messagesLastHeight);
			Widgets.BeginScrollView(outRect, ref messagesScrollPos, viewRect);
			float num = 0f;
			List<IArchivable> archivablesListForReading = Find.Archive.ArchivablesListForReading;
			for (int num2 = archivablesListForReading.Count - 1; num2 >= 0; num2--)
			{
				if ((showLetters || (!(archivablesListForReading[num2] is Letter) && !(archivablesListForReading[num2] is ArchivedDialog))) && (showMessages || !(archivablesListForReading[num2] is Message)))
				{
					flag = true;
					if (num + 30f >= messagesScrollPos.y && num <= messagesScrollPos.y + outRect.height)
					{
						DoArchivableRow(new Rect(0f, num, viewRect.width, 30f), archivablesListForReading[num2], num2);
					}
					num += 30f;
				}
			}
			messagesLastHeight = num;
			Widgets.EndScrollView();
			if (!flag)
			{
				Widgets.NoneLabel(rect.yMin + 3f, rect.width, "(" + "NoMessages".Translate() + ")");
			}
		}

		private void DoArchivableRow(Rect rect, IArchivable archivable, int index)
		{
			if (index % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect);
			}
			Widgets.DrawHighlightIfMouseover(rect);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Rect rect2 = rect;
			Rect rect3 = rect2;
			rect3.width = 30f;
			rect2.xMin += 40f;
			float num = Find.Archive.IsPinned(archivable) ? 1f : ((!Mouse.IsOver(rect3)) ? 0f : 0.25f);
			if (num > 0f)
			{
				GUI.color = new Color(1f, 1f, 1f, num);
				GUI.DrawTexture(new Rect(rect3.x + (rect3.width - 22f) / 2f, rect3.y + (rect3.height - 22f) / 2f, 22f, 22f).Rounded(), PinTex);
				GUI.color = Color.white;
			}
			Rect rect4 = rect2;
			Rect outerRect = rect2;
			outerRect.width = 30f;
			rect2.xMin += 40f;
			Texture archivedIcon = archivable.ArchivedIcon;
			if (archivedIcon != null)
			{
				GUI.color = archivable.ArchivedIconColor;
				Widgets.DrawTextureFitted(outerRect, archivedIcon, 0.8f);
				GUI.color = Color.white;
			}
			Rect rect5 = rect2;
			rect5.width = 200f;
			rect2.xMin += 210f;
			Vector2 location = (Find.CurrentMap == null) ? default(Vector2) : Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile);
			GUI.color = new Color(0.75f, 0.75f, 0.75f);
			int num2 = GenDate.TickGameToAbs(archivable.CreatedTicksGame);
			string str = GenDate.DateFullStringAt(num2, location) + ", " + GenDate.HourInteger(num2, location.x) + "LetterHour".Translate();
			Widgets.Label(rect5, str.Truncate(rect5.width));
			GUI.color = Color.white;
			Rect rect6 = rect2;
			Widgets.Label(rect6, archivable.ArchivedLabel.Truncate(rect6.width));
			GenUI.ResetLabelAlign();
			Text.WordWrap = true;
			TooltipHandler.TipRegion(rect3, "PinArchivableTip".Translate(200));
			if (Mouse.IsOver(rect4))
			{
				TooltipHandler.TipRegion(rect4, archivable.ArchivedTooltip);
			}
			if (Widgets.ButtonInvisible(rect3))
			{
				if (Find.Archive.IsPinned(archivable))
				{
					Find.Archive.Unpin(archivable);
					SoundDefOf.Checkbox_TurnedOff.PlayOneShotOnCamera();
				}
				else
				{
					Find.Archive.Pin(archivable);
					SoundDefOf.Checkbox_TurnedOn.PlayOneShotOnCamera();
				}
			}
			if (Widgets.ButtonInvisible(rect4))
			{
				if (Event.current.button == 1)
				{
					LookTargets lookTargets = archivable.LookTargets;
					if (CameraJumper.CanJump(lookTargets.TryGetPrimaryTarget()))
					{
						CameraJumper.TryJumpAndSelect(lookTargets.TryGetPrimaryTarget());
						Find.MainTabsRoot.EscapeCurrentTab();
					}
				}
				else
				{
					archivable.OpenArchived();
				}
			}
		}

		private void DoGraphPage(Rect rect)
		{
			rect.yMin += 17f;
			GUI.BeginGroup(rect);
			Rect graphRect = new Rect(0f, 0f, rect.width, 450f);
			Rect legendRect = new Rect(0f, graphRect.yMax, rect.width / 2f, 40f);
			Rect rect2 = new Rect(0f, legendRect.yMax, rect.width, 40f);
			if (historyAutoRecorderGroup != null)
			{
				marks.Clear();
				List<Tale> allTalesListForReading = Find.TaleManager.AllTalesListForReading;
				for (int i = 0; i < allTalesListForReading.Count; i++)
				{
					Tale tale = allTalesListForReading[i];
					if (tale.def.type == TaleType.PermanentHistorical)
					{
						float x = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
						marks.Add(new CurveMark(x, tale.ShortSummary, tale.def.historyGraphColor));
					}
				}
				historyAutoRecorderGroup.DrawGraph(graphRect, legendRect, graphSection, marks);
			}
			Text.Font = GameFont.Small;
			float num = (float)Find.TickManager.TicksGame / 60000f;
			if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width, legendRect.yMin, 110f, 40f), "Last30Days".Translate()))
			{
				graphSection = new FloatRange(Mathf.Max(0f, num - 30f), num);
			}
			if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 110f + 4f, legendRect.yMin, 110f, 40f), "Last100Days".Translate()))
			{
				graphSection = new FloatRange(Mathf.Max(0f, num - 100f), num);
			}
			if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 228f, legendRect.yMin, 110f, 40f), "Last300Days".Translate()))
			{
				graphSection = new FloatRange(Mathf.Max(0f, num - 300f), num);
			}
			if (Widgets.ButtonText(new Rect(legendRect.xMin + legendRect.width + 342f, legendRect.yMin, 110f, 40f), "AllDays".Translate()))
			{
				graphSection = new FloatRange(0f, num);
			}
			if (Widgets.ButtonText(new Rect(rect2.x, rect2.y, 110f, 40f), "SelectGraph".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				List<HistoryAutoRecorderGroup> list2 = Find.History.Groups();
				for (int j = 0; j < list2.Count; j++)
				{
					HistoryAutoRecorderGroup groupLocal = list2[j];
					if (!groupLocal.def.devModeOnly || Prefs.DevMode)
					{
						list.Add(new FloatMenuOption(groupLocal.def.LabelCap, delegate
						{
							historyAutoRecorderGroup = groupLocal;
						}));
					}
				}
				FloatMenu window = new FloatMenu(list, "SelectGraph".Translate());
				Find.WindowStack.Add(window);
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.HistoryTab, KnowledgeAmount.Total);
			}
			GUI.EndGroup();
		}
	}
}
