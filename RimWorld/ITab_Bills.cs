using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class ITab_Bills : ITab
	{
		private float viewHeight = 1000f;

		private Vector2 scrollPosition = default(Vector2);

		private Bill mouseoverBill;

		private static readonly Vector2 WinSize = new Vector2(420f, 480f);

		[TweakValue("Interface", 0f, 128f)]
		private static float PasteX = 48f;

		[TweakValue("Interface", 0f, 128f)]
		private static float PasteY = 3f;

		[TweakValue("Interface", 0f, 32f)]
		private static float PasteSize = 24f;

		protected Building_WorkTable SelTable => (Building_WorkTable)base.SelThing;

		public ITab_Bills()
		{
			size = WinSize;
			labelKey = "TabBills";
			tutorTag = "Bills";
		}

		protected override void FillTab()
		{
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.BillsTab, KnowledgeAmount.FrameDisplayed);
			Vector2 winSize = WinSize;
			Rect rect2 = new Rect(winSize.x - PasteX, PasteY, PasteSize, PasteSize);
			if (BillUtility.Clipboard == null)
			{
				GUI.color = Color.gray;
				Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
				GUI.color = Color.white;
				TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate());
			}
			else if (!SelTable.def.AllRecipes.Contains(BillUtility.Clipboard.recipe) || !BillUtility.Clipboard.recipe.AvailableNow)
			{
				GUI.color = Color.gray;
				Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
				GUI.color = Color.white;
				TooltipHandler.TipRegion(rect2, "ClipboardBillNotAvailableHere".Translate());
			}
			else if (SelTable.billStack.Count >= 15)
			{
				GUI.color = Color.gray;
				Widgets.DrawTextureFitted(rect2, TexButton.Paste, 1f);
				GUI.color = Color.white;
				TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate() + " (" + "PasteBillTip_LimitReached".Translate() + ")");
			}
			else
			{
				if (Widgets.ButtonImageFitted(rect2, TexButton.Paste, Color.white))
				{
					Bill bill = BillUtility.Clipboard.Clone();
					bill.InitializeAfterClone();
					SelTable.billStack.AddBill(bill);
					SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				}
				TooltipHandler.TipRegion(rect2, "PasteBillTip".Translate());
			}
			Vector2 winSize2 = WinSize;
			float x = winSize2.x;
			Vector2 winSize3 = WinSize;
			Rect rect3 = new Rect(0f, 0f, x, winSize3.y).ContractedBy(10f);
			Func<List<FloatMenuOption>> recipeOptionsMaker = delegate
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				for (int i = 0; i < SelTable.def.AllRecipes.Count; i++)
				{
					ITab_Bills tab_Bills = this;
					if (SelTable.def.AllRecipes[i].AvailableNow)
					{
						RecipeDef recipe = SelTable.def.AllRecipes[i];
						list.Add(new FloatMenuOption(recipe.LabelCap, delegate
						{
							if (!tab_Bills.SelTable.Map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
							{
								Bill.CreateNoPawnsWithSkillDialog(recipe);
							}
							Bill bill2 = recipe.MakeNewBill();
							tab_Bills.SelTable.billStack.AddBill(bill2);
							if (recipe.conceptLearned != null)
							{
								PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
							}
							if (TutorSystem.TutorialMode)
							{
								TutorSystem.Notify_Event("AddBill-" + recipe.LabelCap);
							}
						}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, recipe)));
					}
				}
				if (!list.Any())
				{
					list.Add(new FloatMenuOption("NoneBrackets".Translate(), null));
				}
				return list;
			};
			mouseoverBill = SelTable.billStack.DoListing(rect3, recipeOptionsMaker, ref scrollPosition, ref viewHeight);
		}

		public override void TabUpdate()
		{
			if (mouseoverBill != null)
			{
				mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelTable.Position);
				mouseoverBill = null;
			}
		}
	}
}
