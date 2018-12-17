using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnColumnWorker_Outfit : PawnColumnWorker
	{
		public const int TopAreaHeight = 65;

		public const int ManageOutfitsButtonHeight = 32;

		public override void DoHeader(Rect rect, PawnTable table)
		{
			base.DoHeader(rect, table);
			Rect rect2 = new Rect(rect.x, rect.y + (rect.height - 65f), Mathf.Min(rect.width, 360f), 32f);
			if (Widgets.ButtonText(rect2, "ManageOutfits".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageOutfits(null));
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Outfits, KnowledgeAmount.Total);
			}
			UIHighlighter.HighlightOpportunity(rect2, "ManageOutfits");
		}

		public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
		{
			if (pawn.outfits != null)
			{
				int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
				int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
				float x = rect.x;
				bool somethingIsForced = pawn.outfits.forcedHandler.SomethingIsForced;
				Rect rect2 = new Rect(x, rect.y + 2f, (float)num, rect.height - 4f);
				if (somethingIsForced)
				{
					rect2.width -= 4f + (float)num2;
				}
				Rect rect3 = rect2;
				Pawn target = pawn;
				Func<Pawn, Outfit> getPayload = (Pawn p) => p.outfits.CurrentOutfit;
				Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Outfit>>> menuGenerator = Button_GenerateMenu;
				string buttonLabel = pawn.outfits.CurrentOutfit.label.Truncate(rect2.width);
				string label = pawn.outfits.CurrentOutfit.label;
				Widgets.Dropdown(rect3, target, getPayload, menuGenerator, buttonLabel, null, label, null, null, paintable: true);
				x += rect2.width;
				x += 4f;
				Rect rect4 = new Rect(x, rect.y + 2f, (float)num2, rect.height - 4f);
				if (somethingIsForced)
				{
					if (Widgets.ButtonText(rect4, "ClearForcedApparel".Translate()))
					{
						pawn.outfits.forcedHandler.Reset();
					}
					TooltipHandler.TipRegion(rect4, new TipSignal(delegate
					{
						string text = "ForcedApparel".Translate() + ":\n";
						foreach (Apparel item in pawn.outfits.forcedHandler.ForcedApparel)
						{
							text = text + "\n   " + item.LabelCap;
						}
						return text;
					}, pawn.GetHashCode() * 612));
					x += (float)num2;
					x += 4f;
				}
				Rect rect5 = new Rect(x, rect.y + 2f, (float)num2, rect.height - 4f);
				if (Widgets.ButtonText(rect5, "AssignTabEdit".Translate()))
				{
					Find.WindowStack.Add(new Dialog_ManageOutfits(pawn.outfits.CurrentOutfit));
				}
				x += (float)num2;
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<Outfit>> Button_GenerateMenu(Pawn pawn)
		{
			using (List<Outfit>.Enumerator enumerator = Current.Game.outfitDatabase.AllOutfits.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Outfit outfit = enumerator.Current;
					yield return new Widgets.DropdownMenuElement<Outfit>
					{
						option = new FloatMenuOption(outfit.label, delegate
						{
							pawn.outfits.CurrentOutfit = outfit;
						}),
						payload = outfit
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
			return (pawn.outfits != null && pawn.outfits.CurrentOutfit != null) ? pawn.outfits.CurrentOutfit.uniqueId : (-2147483648);
		}
	}
}
