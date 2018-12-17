using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageFoodRestrictions : Window
	{
		private Vector2 scrollPosition;

		private FoodRestriction selFoodRestrictionInt;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		private static ThingFilter foodGlobalFilter;

		private FoodRestriction SelectedFoodRestriction
		{
			get
			{
				return selFoodRestrictionInt;
			}
			set
			{
				CheckSelectedFoodRestrictionHasName();
				selFoodRestrictionInt = value;
			}
		}

		public override Vector2 InitialSize => new Vector2(700f, 700f);

		public Dialog_ManageFoodRestrictions(FoodRestriction selectedFoodRestriction)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			if (foodGlobalFilter == null)
			{
				foodGlobalFilter = new ThingFilter();
				foodGlobalFilter.SetAllow(ThingCategoryDefOf.Foods, allow: true);
				foodGlobalFilter.SetAllow(ThingCategoryDefOf.CorpsesHumanlike, allow: true);
				foodGlobalFilter.SetAllow(ThingCategoryDefOf.CorpsesAnimal, allow: true);
			}
			SelectedFoodRestriction = selectedFoodRestriction;
		}

		private void CheckSelectedFoodRestrictionHasName()
		{
			if (SelectedFoodRestriction != null && SelectedFoodRestriction.label.NullOrEmpty())
			{
				SelectedFoodRestriction.label = "Unnamed";
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectFoodRestriction".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (FoodRestriction allFoodRestriction in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction = allFoodRestriction;
					list.Add(new FloatMenuOption(localRestriction.label, delegate
					{
						SelectedFoodRestriction = localRestriction;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewFoodRestriction".Translate()))
			{
				SelectedFoodRestriction = Current.Game.foodRestrictionDatabase.MakeNewFoodRestriction();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteFoodRestriction".Translate()))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (FoodRestriction allFoodRestriction2 in Current.Game.foodRestrictionDatabase.AllFoodRestrictions)
				{
					FoodRestriction localRestriction2 = allFoodRestriction2;
					list2.Add(new FloatMenuOption(localRestriction2.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.foodRestrictionDatabase.TryDelete(localRestriction2);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
						}
						else if (localRestriction2 == SelectedFoodRestriction)
						{
							SelectedFoodRestriction = null;
						}
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			float width = inRect.width;
			float num2 = inRect.height - 40f;
			Vector2 closeButSize = CloseButSize;
			Rect rect4 = new Rect(0f, 40f, width, num2 - closeButSize.y).ContractedBy(10f);
			if (SelectedFoodRestriction == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoFoodRestrictionSelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				GUI.BeginGroup(rect4);
				Rect rect5 = new Rect(0f, 0f, 200f, 30f);
				DoNameInputRect(rect5, ref SelectedFoodRestriction.label);
				Rect rect6 = new Rect(0f, 40f, 300f, rect4.height - 45f - 10f);
				Rect rect7 = rect6;
				ref Vector2 reference = ref scrollPosition;
				ThingFilter filter = SelectedFoodRestriction.filter;
				ThingFilter parentFilter = foodGlobalFilter;
				IEnumerable<SpecialThingFilterDef> forceHiddenFilters = HiddenSpecialThingFilters();
				ThingFilterUI.DoThingFilterConfigWindow(rect7, ref reference, filter, parentFilter, 1, null, forceHiddenFilters, forceHideHitPointsConfig: true);
				GUI.EndGroup();
			}
		}

		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			yield return SpecialThingFilterDefOf.AllowFresh;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void PreClose()
		{
			base.PreClose();
			CheckSelectedFoodRestrictionHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, Outfit.ValidNameRegex);
		}
	}
}
