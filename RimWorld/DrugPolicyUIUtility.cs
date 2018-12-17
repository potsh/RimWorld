using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DrugPolicyUIUtility
	{
		public const string AssigningDrugsTutorHighlightTag = "ButtonAssignDrugs";

		[CompilerGenerated]
		private static Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>>> _003C_003Ef__mg_0024cache0;

		public static void DoAssignDrugPolicyButtons(Rect rect, Pawn pawn)
		{
			int num = Mathf.FloorToInt((rect.width - 4f) * 0.714285731f);
			int num2 = Mathf.FloorToInt((rect.width - 4f) * 0.2857143f);
			float x = rect.x;
			Rect rect2 = new Rect(x, rect.y + 2f, (float)num, rect.height - 4f);
			string text = pawn.drugs.CurrentPolicy.label;
			if (pawn.story != null && pawn.story.traits != null)
			{
				Trait trait = pawn.story.traits.GetTrait(TraitDefOf.DrugDesire);
				if (trait != null)
				{
					text = text + " (" + trait.Label + ")";
				}
			}
			Rect rect3 = rect2;
			Func<Pawn, DrugPolicy> getPayload = (Pawn p) => p.drugs.CurrentPolicy;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>>> menuGenerator = Button_GenerateMenu;
			string buttonLabel = text.Truncate(rect2.width);
			string label = pawn.drugs.CurrentPolicy.label;
			Widgets.Dropdown(rect3, pawn, getPayload, menuGenerator, buttonLabel, null, label, null, delegate
			{
				PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.DrugPolicies, KnowledgeAmount.Total);
			}, paintable: true);
			x += (float)num;
			x += 4f;
			Rect rect4 = new Rect(x, rect.y + 2f, (float)num2, rect.height - 4f);
			if (Widgets.ButtonText(rect4, "AssignTabEdit".Translate()))
			{
				Find.WindowStack.Add(new Dialog_ManageDrugPolicies(pawn.drugs.CurrentPolicy));
			}
			UIHighlighter.HighlightOpportunity(rect2, "ButtonAssignDrugs");
			UIHighlighter.HighlightOpportunity(rect4, "ButtonAssignDrugs");
			x += (float)num2;
		}

		private static IEnumerable<Widgets.DropdownMenuElement<DrugPolicy>> Button_GenerateMenu(Pawn pawn)
		{
			using (List<DrugPolicy>.Enumerator enumerator = Current.Game.drugPolicyDatabase.AllPolicies.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					DrugPolicy assignedDrugs = enumerator.Current;
					yield return new Widgets.DropdownMenuElement<DrugPolicy>
					{
						option = new FloatMenuOption(assignedDrugs.label, delegate
						{
							pawn.drugs.CurrentPolicy = assignedDrugs;
						}),
						payload = assignedDrugs
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0141:
			/*Error near IL_0142: Unexpected return in MoveNext()*/;
		}
	}
}
