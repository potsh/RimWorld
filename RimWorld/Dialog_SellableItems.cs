using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_SellableItems : Window
	{
		private ThingCategoryDef currentCategory;

		private bool pawnsTabOpen;

		private List<ThingDef> sellableItems = new List<ThingDef>();

		private List<TabRecord> tabs = new List<TabRecord>();

		private Vector2 scrollPosition;

		private List<ThingDef> cachedSellablePawns;

		private Dictionary<ThingCategoryDef, List<ThingDef>> cachedSellableItemsByCategory = new Dictionary<ThingCategoryDef, List<ThingDef>>();

		private const float RowHeight = 24f;

		private const float IconMargin = 4f;

		private const float IconSize = 20f;

		private const float TitleRectHeight = 60f;

		private const float BottomAreaHeight = 55f;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		public override Vector2 InitialSize => new Vector2(650f, (float)Mathf.Min(UI.screenHeight, 1000));

		protected override float Margin => 0f;

		public Dialog_SellableItems(TraderKindDef trader)
		{
			forcePause = true;
			absorbInputAroundWindow = true;
			CalculateSellableItems(trader);
			CalculateTabs();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 60f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "SellableItemsTitle".Translate().CapitalizeFirst());
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			inRect.yMin += 124f;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, tabs, 2);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			DoBottomButtons(rect2);
			Rect outRect = rect2;
			outRect.yMax -= 65f;
			List<ThingDef> sellableItemsInCategory = GetSellableItemsInCategory(currentCategory, pawnsTabOpen);
			if (sellableItemsInCategory.Any())
			{
				float height = (float)sellableItemsInCategory.Count * 24f;
				float num = 0f;
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, height);
				Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
				float num2 = scrollPosition.y - 24f;
				float num3 = scrollPosition.y + outRect.height;
				for (int i = 0; i < sellableItemsInCategory.Count; i++)
				{
					if (num > num2 && num < num3)
					{
						Rect rect3 = new Rect(0f, num, viewRect.width, 24f);
						DoRow(rect3, sellableItemsInCategory[i], i);
					}
					num += 24f;
				}
				Widgets.EndScrollView();
			}
			else
			{
				Widgets.NoneLabel(0f, outRect.width);
			}
			GUI.EndGroup();
		}

		private void DoRow(Rect rect, ThingDef thingDef, int index)
		{
			Widgets.DrawHighlightIfMouseover(rect);
			TooltipHandler.TipRegion(rect, thingDef.description);
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(4f, (rect.height - 20f) / 2f, 20f, 20f);
			Widgets.ThingIcon(rect2, thingDef);
			Rect rect3 = new Rect(rect2.xMax + 4f, 0f, rect.width, 24f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;
			Widgets.Label(rect3, thingDef.LabelCap);
			Text.Anchor = TextAnchor.UpperLeft;
			Text.WordWrap = true;
			GUI.EndGroup();
		}

		private void DoBottomButtons(Rect rect)
		{
			float num = rect.width / 2f;
			Vector2 bottomButtonSize = BottomButtonSize;
			float x = num - bottomButtonSize.x / 2f;
			float y = rect.height - 55f;
			Vector2 bottomButtonSize2 = BottomButtonSize;
			float x2 = bottomButtonSize2.x;
			Vector2 bottomButtonSize3 = BottomButtonSize;
			Rect rect2 = new Rect(x, y, x2, bottomButtonSize3.y);
			if (Widgets.ButtonText(rect2, "CloseButton".Translate()))
			{
				Close();
			}
		}

		private void CalculateSellableItems(TraderKindDef trader)
		{
			sellableItems.Clear();
			cachedSellableItemsByCategory.Clear();
			cachedSellablePawns = null;
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].PlayerAcquirable && !allDefsListForReading[i].IsCorpse && !typeof(MinifiedThing).IsAssignableFrom(allDefsListForReading[i].thingClass) && trader.WillTrade(allDefsListForReading[i]) && TradeUtility.EverPlayerSellable(allDefsListForReading[i]))
				{
					sellableItems.Add(allDefsListForReading[i]);
				}
			}
			sellableItems.SortBy((ThingDef x) => x.label);
		}

		private void CalculateTabs()
		{
			tabs.Clear();
			List<ThingCategoryDef> allDefsListForReading = DefDatabase<ThingCategoryDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				ThingCategoryDef category = allDefsListForReading[i];
				if (category.parent == ThingCategoryDefOf.Root && AnyTraderWillEverTrade(category))
				{
					if (currentCategory == null)
					{
						currentCategory = category;
					}
					tabs.Add(new TabRecord(category.LabelCap, delegate
					{
						currentCategory = category;
						pawnsTabOpen = false;
					}, () => currentCategory == category));
				}
			}
			tabs.Add(new TabRecord("PawnsTabShort".Translate(), delegate
			{
				currentCategory = null;
				pawnsTabOpen = true;
			}, () => pawnsTabOpen));
		}

		private List<ThingDef> GetSellableItemsInCategory(ThingCategoryDef category, bool pawns)
		{
			if (pawns)
			{
				if (cachedSellablePawns == null)
				{
					cachedSellablePawns = new List<ThingDef>();
					for (int i = 0; i < sellableItems.Count; i++)
					{
						if (sellableItems[i].category == ThingCategory.Pawn)
						{
							cachedSellablePawns.Add(sellableItems[i]);
						}
					}
				}
				return cachedSellablePawns;
			}
			if (cachedSellableItemsByCategory.TryGetValue(category, out List<ThingDef> value))
			{
				return value;
			}
			value = new List<ThingDef>();
			for (int j = 0; j < sellableItems.Count; j++)
			{
				if (sellableItems[j].IsWithinCategory(category))
				{
					value.Add(sellableItems[j]);
				}
			}
			cachedSellableItemsByCategory.Add(category, value);
			return value;
		}

		private bool AnyTraderWillEverTrade(ThingCategoryDef thingCategory)
		{
			List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].IsWithinCategory(thingCategory))
				{
					List<TraderKindDef> allDefsListForReading2 = DefDatabase<TraderKindDef>.AllDefsListForReading;
					for (int j = 0; j < allDefsListForReading2.Count; j++)
					{
						if (allDefsListForReading2[j].WillTrade(allDefsListForReading[i]))
						{
							return true;
						}
					}
				}
			}
			return false;
		}
	}
}
