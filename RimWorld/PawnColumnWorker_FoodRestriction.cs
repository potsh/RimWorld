using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_FoodRestriction : PawnColumnWorker
	{
		private const int TopAreaHeight = 65;

		public const int ManageFoodRestrictionsButtonHeight = 32;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
			if (Widgets.ButtonText(rect2, "ManageFoodRestrictions".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(null));
			}
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.foodRestriction != null)
			{
				DoAssignFoodRestrictionButtons(rect, pawn);
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>> Button_GenerateMenu(Pawn pawn)
		{
			using (List<FoodRestriction>.Enumerator enumerator = Current.Game.foodRestrictionDatabase.AllFoodRestrictions.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FoodRestriction foodRestriction = enumerator.Current;
					yield return new Widgets.DropdownMenuElement<FoodRestriction>
					{
						option = new FloatMenuOption(foodRestriction.label, delegate
						{
							pawn.foodRestriction.CurrentFoodRestriction = foodRestriction;
						}),
						payload = foodRestriction
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0141:
			/*Error near IL_0142: Unexpected return in MoveNext()*/;
		}

		public override int GetMinWidth(PawnTable table)
		{
			return Mathf.Max(base.GetMinWidth(table), Mathf.CeilToInt(194f));
		}

		public override int GetOptimalWidth(PawnTable table)
		{
			return Mathf.Clamp(Mathf.CeilToInt(251f), GetMinWidth(table), GetMaxWidth(table));
		}

		public override int GetMinHeaderHeight(PawnTable table)
		{
			return Mathf.Max(base.GetMinHeaderHeight(table), 65);
		}

		public override int Compare(Pawn a, Pawn b)
		{
			return GetValueToCompare(a).CompareTo(GetValueToCompare(b));
		}

		private int GetValueToCompare(Pawn pawn)
		{
			return (pawn.foodRestriction != null && pawn.foodRestriction.CurrentFoodRestriction != null) ? pawn.foodRestriction.CurrentFoodRestriction.id : (-2147483648);
		}

		private void DoAssignFoodRestrictionButtons(Rect rect, Pawn pawn)
		{
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float x = rect.x;
			Rect rect2 = new Rect(x, rect.y + 2f, (float)num, rect.height - 4f);
			Rect rect3 = rect2;
			Func<Pawn, FoodRestriction> getPayload = (Pawn p) => p.foodRestriction.CurrentFoodRestriction;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<FoodRestriction>>> menuGenerator = Button_GenerateMenu;
			string buttonLabel = pawn.foodRestriction.CurrentFoodRestriction.label.Truncate(rect2.width);
			string label = pawn.foodRestriction.CurrentFoodRestriction.label;
			Widgets.Dropdown(rect3, pawn, getPayload, menuGenerator, buttonLabel, null, label, null, null, paintable: true);
			x += (float)num;
			x += 4f;
			Rect rect4 = new Rect(x, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageFoodRestrictions(pawn.foodRestriction.CurrentFoodRestriction));
			}
			x += (float)num2;
		}
	}
}
