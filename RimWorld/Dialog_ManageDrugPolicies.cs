using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class Dialog_ManageDrugPolicies : Window
	{
		private Vector2 scrollPosition;

		private DrugPolicy selPolicy;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		private const float DrugEntryRowHeight = 35f;

		private const float BottomButtonsAreaHeight = 50f;

		private const float AddEntryButtonHeight = 35f;

		private const float AddEntryButtonWidth = 150f;

		private const float CellsPadding = 4f;

		private static readonly Texture2D IconForAddiction = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/ForAddiction");

		private static readonly Texture2D IconForJoy = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/ForJoy");

		private static readonly Texture2D IconScheduled = ContentFinder<Texture2D>.Get("UI/Icons/DrugPolicy/Scheduled");

		private static readonly Regex ValidNameRegex = Outfit.ValidNameRegex;

		private const float UsageSpacing = 12f;

		private DrugPolicy SelectedPolicy
		{
			get
			{
				return selPolicy;
			}
			set
			{
				CheckSelectedPolicyHasName();
				selPolicy = value;
			}
		}

		public override Vector2 InitialSize => new Vector2(900f, 700f);

		public Dialog_ManageDrugPolicies(DrugPolicy selectedAssignedDrugs)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			SelectedPolicy = selectedAssignedDrugs;
		}

		private void CheckSelectedPolicyHasName()
		{
			if (SelectedPolicy != null && SelectedPolicy.label.NullOrEmpty())
			{
				SelectedPolicy.label = "Unnamed";
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectDrugPolicy".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (DrugPolicy allPolicy in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs = allPolicy;
					list.Add(new FloatMenuOption(localAssignedDrugs.label, delegate
					{
						SelectedPolicy = localAssignedDrugs;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewDrugPolicy".Translate()))
			{
				SelectedPolicy = Current.Game.drugPolicyDatabase.MakeNewDrugPolicy();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteDrugPolicy".Translate()))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (DrugPolicy allPolicy2 in Current.Game.drugPolicyDatabase.AllPolicies)
				{
					DrugPolicy localAssignedDrugs2 = allPolicy2;
					list2.Add(new FloatMenuOption(localAssignedDrugs2.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.drugPolicyDatabase.TryDelete(localAssignedDrugs2);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
						}
						else if (localAssignedDrugs2 == SelectedPolicy)
						{
							SelectedPolicy = null;
						}
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			float width = inRect.width;
			float num2 = inRect.height - 40f;
			Vector2 closeButSize = CloseButSize;
			Rect rect4 = new Rect(0f, 40f, width, num2 - closeButSize.y).ContractedBy(10f);
			if (SelectedPolicy == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoDrugPolicySelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				GUI.BeginGroup(rect4);
				Rect rect5 = new Rect(0f, 0f, 200f, 30f);
				DoNameInputRect(rect5, ref SelectedPolicy.label);
				Rect rect6 = new Rect(0f, 40f, rect4.width, rect4.height - 45f - 10f);
				DoPolicyConfigArea(rect6);
				GUI.EndGroup();
			}
		}

		public override void PostOpen()
		{
			base.PostOpen();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
		}

		public override void PreClose()
		{
			base.PreClose();
			CheckSelectedPolicyHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, ValidNameRegex);
		}

		private void DoPolicyConfigArea(Rect rect)
		{
			Rect rect2 = rect;
			rect2.height = 54f;
			Rect rect3 = rect;
			rect3.yMin = rect2.yMax;
			rect3.height -= 50f;
			Rect rect4 = rect;
			rect4.yMin = rect4.yMax - 50f;
			DoColumnLabels(rect2);
			Widgets.DrawMenuSection(rect3);
			if (SelectedPolicy.Count == 0)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect3, "NoDrugs".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				float height = (float)SelectedPolicy.Count * 35f;
				Rect viewRect = new Rect(0f, 0f, rect3.width - 16f, height);
				Widgets.BeginScrollView(rect3, ref scrollPosition, viewRect);
				DrugPolicy selectedPolicy = SelectedPolicy;
				for (int i = 0; i < selectedPolicy.Count; i++)
				{
					Rect rect5 = new Rect(0f, (float)i * 35f, viewRect.width, 35f);
					DoEntryRow(rect5, selectedPolicy[i]);
				}
				Widgets.EndScrollView();
			}
		}

		private void CalculateColumnsWidths(Rect rect, out float addictionWidth, out float allowJoyWidth, out float scheduledWidth, out float drugNameWidth, out float frequencyWidth, out float moodThresholdWidth, out float joyThresholdWidth, out float takeToInventoryWidth)
		{
			float num = rect.width - 108f;
			drugNameWidth = num * 0.2f;
			addictionWidth = 36f;
			allowJoyWidth = 36f;
			scheduledWidth = 36f;
			frequencyWidth = num * 0.35f;
			moodThresholdWidth = num * 0.15f;
			joyThresholdWidth = num * 0.15f;
			takeToInventoryWidth = num * 0.15f;
		}

		private void DoColumnLabels(Rect rect)
		{
			rect.width -= 16f;
			CalculateColumnsWidths(rect, out float addictionWidth, out float allowJoyWidth, out float scheduledWidth, out float drugNameWidth, out float frequencyWidth, out float moodThresholdWidth, out float joyThresholdWidth, out float takeToInventoryWidth);
			float x = rect.x;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect2 = new Rect(x + 4f, rect.y, drugNameWidth, rect.height);
			Widgets.Label(rect2, "DrugColumnLabel".Translate());
			TooltipHandler.TipRegion(rect2, "DrugNameColumnDesc".Translate());
			x += drugNameWidth;
			Text.Anchor = TextAnchor.UpperCenter;
			Rect rect3 = new Rect(x, rect.y, allowJoyWidth + allowJoyWidth, rect.height / 2f);
			Widgets.Label(rect3, "DrugUsageColumnLabel".Translate());
			TooltipHandler.TipRegion(rect3, "DrugUsageColumnDesc".Translate());
			Rect rect4 = new Rect(x, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect4, IconForAddiction);
			TooltipHandler.TipRegion(rect4, "DrugUsageTipForAddiction".Translate());
			x += addictionWidth;
			Rect rect5 = new Rect(x, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect5, IconForJoy);
			TooltipHandler.TipRegion(rect5, "DrugUsageTipForJoy".Translate());
			x += allowJoyWidth;
			Rect rect6 = new Rect(x, rect.yMax - 24f, 24f, 24f);
			GUI.DrawTexture(rect6, IconScheduled);
			TooltipHandler.TipRegion(rect6, "DrugUsageTipScheduled".Translate());
			x += scheduledWidth;
			Text.Anchor = TextAnchor.LowerCenter;
			Rect rect7 = new Rect(x, rect.y, frequencyWidth, rect.height);
			Widgets.Label(rect7, "FrequencyColumnLabel".Translate());
			TooltipHandler.TipRegion(rect7, "FrequencyColumnDesc".Translate());
			x += frequencyWidth;
			Rect rect8 = new Rect(x, rect.y, moodThresholdWidth, rect.height);
			Widgets.Label(rect8, "MoodThresholdColumnLabel".Translate());
			TooltipHandler.TipRegion(rect8, "MoodThresholdColumnDesc".Translate());
			x += moodThresholdWidth;
			Rect rect9 = new Rect(x, rect.y, joyThresholdWidth, rect.height);
			Widgets.Label(rect9, "JoyThresholdColumnLabel".Translate());
			TooltipHandler.TipRegion(rect9, "JoyThresholdColumnDesc".Translate());
			x += joyThresholdWidth;
			Rect rect10 = new Rect(x, rect.y, takeToInventoryWidth, rect.height);
			Widgets.Label(rect10, "TakeToInventoryColumnLabel".Translate());
			TooltipHandler.TipRegion(rect10, "TakeToInventoryColumnDesc".Translate());
			x += takeToInventoryWidth;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		private void DoEntryRow(Rect rect, DrugPolicyEntry entry)
		{
			CalculateColumnsWidths(rect, out float addictionWidth, out float allowJoyWidth, out float scheduledWidth, out float drugNameWidth, out float frequencyWidth, out float moodThresholdWidth, out float joyThresholdWidth, out float takeToInventoryWidth);
			Text.Anchor = TextAnchor.MiddleLeft;
			float x = rect.x;
			Widgets.Label(new Rect(x, rect.y, drugNameWidth, rect.height).ContractedBy(4f), entry.drug.LabelCap);
			float num = x;
			Vector2 vector = Text.CalcSize(entry.drug.LabelCap);
			Widgets.InfoCardButton(num + vector.x + 5f, rect.y + (rect.height - 24f) / 2f, entry.drug);
			x += drugNameWidth;
			if (entry.drug.IsAddictiveDrug)
			{
				Widgets.Checkbox(x, rect.y, ref entry.allowedForAddiction, 24f, disabled: false, paintable: true);
			}
			x += addictionWidth;
			if (entry.drug.IsPleasureDrug)
			{
				Widgets.Checkbox(x, rect.y, ref entry.allowedForJoy, 24f, disabled: false, paintable: true);
			}
			x += allowJoyWidth;
			Widgets.Checkbox(x, rect.y, ref entry.allowScheduled, 24f, disabled: false, paintable: true);
			x += scheduledWidth;
			if (entry.allowScheduled)
			{
				entry.daysFrequency = Widgets.FrequencyHorizontalSlider(new Rect(x, rect.y, frequencyWidth, rect.height).ContractedBy(4f), entry.daysFrequency, 0.1f, 25f, roundToInt: true);
				x += frequencyWidth;
				entry.onlyIfMoodBelow = Widgets.HorizontalSlider(label: (!(entry.onlyIfMoodBelow < 1f)) ? "NoDrugUseRequirement".Translate() : entry.onlyIfMoodBelow.ToStringPercent(), rect: new Rect(x, rect.y, moodThresholdWidth, rect.height).ContractedBy(4f), value: entry.onlyIfMoodBelow, leftValue: 0.01f, rightValue: 1f, middleAlignment: true);
				x += moodThresholdWidth;
				entry.onlyIfJoyBelow = Widgets.HorizontalSlider(label: (!(entry.onlyIfJoyBelow < 1f)) ? "NoDrugUseRequirement".Translate() : entry.onlyIfJoyBelow.ToStringPercent(), rect: new Rect(x, rect.y, joyThresholdWidth, rect.height).ContractedBy(4f), value: entry.onlyIfJoyBelow, leftValue: 0.01f, rightValue: 1f, middleAlignment: true);
				x += joyThresholdWidth;
				Widgets.TextFieldNumeric(new Rect(x, rect.y, takeToInventoryWidth, rect.height).ContractedBy(4f), ref entry.takeToInventory, ref entry.takeToInventoryTempBuffer, 0f, 15f);
				x += takeToInventoryWidth;
			}
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
