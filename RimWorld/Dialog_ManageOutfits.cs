using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ManageOutfits : Window
	{
		private Vector2 scrollPosition;

		private Outfit selOutfitInt;

		public const float TopAreaHeight = 40f;

		public const float TopButtonHeight = 35f;

		public const float TopButtonWidth = 150f;

		private static ThingFilter apparelGlobalFilter;

		private Outfit SelectedOutfit
		{
			get
			{
				return selOutfitInt;
			}
			set
			{
				CheckSelectedOutfitHasName();
				selOutfitInt = value;
			}
		}

		public override Vector2 InitialSize => new Vector2(700f, 700f);

		public Dialog_ManageOutfits(Outfit selectedOutfit)
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
			if (apparelGlobalFilter == null)
			{
				apparelGlobalFilter = new ThingFilter();
				apparelGlobalFilter.SetAllow(ThingCategoryDefOf.Apparel, allow: true);
			}
			SelectedOutfit = selectedOutfit;
		}

		private void CheckSelectedOutfitHasName()
		{
			if (SelectedOutfit != null && SelectedOutfit.label.NullOrEmpty())
			{
				SelectedOutfit.label = "Unnamed";
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			float num = 0f;
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect, "SelectOutfit".Translate()))
			{
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (Outfit allOutfit in Current.Game.outfitDatabase.AllOutfits)
				{
					Outfit localOut = allOutfit;
					list.Add(new FloatMenuOption(localOut.label, delegate
					{
						SelectedOutfit = localOut;
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			num += 10f;
			Rect rect2 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect2, "NewOutfit".Translate()))
			{
				SelectedOutfit = Current.Game.outfitDatabase.MakeNewOutfit();
			}
			num += 10f;
			Rect rect3 = new Rect(num, 0f, 150f, 35f);
			num += 150f;
			if (Widgets.ButtonText(rect3, "DeleteOutfit".Translate()))
			{
				List<FloatMenuOption> list2 = new List<FloatMenuOption>();
				foreach (Outfit allOutfit2 in Current.Game.outfitDatabase.AllOutfits)
				{
					Outfit localOut2 = allOutfit2;
					list2.Add(new FloatMenuOption(localOut2.label, delegate
					{
						AcceptanceReport acceptanceReport = Current.Game.outfitDatabase.TryDelete(localOut2);
						if (!acceptanceReport.Accepted)
						{
							Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
						}
						else if (localOut2 == SelectedOutfit)
						{
							SelectedOutfit = null;
						}
					}));
				}
				Find.WindowStack.Add(new FloatMenu(list2));
			}
			float width = inRect.width;
			float num2 = inRect.height - 40f;
			Vector2 closeButSize = CloseButSize;
			Rect rect4 = new Rect(0f, 40f, width, num2 - closeButSize.y).ContractedBy(10f);
			if (SelectedOutfit == null)
			{
				GUI.color = Color.grey;
				Text.Anchor = TextAnchor.MiddleCenter;
				Widgets.Label(rect4, "NoOutfitSelected".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
			else
			{
				GUI.BeginGroup(rect4);
				Rect rect5 = new Rect(0f, 0f, 200f, 30f);
				DoNameInputRect(rect5, ref SelectedOutfit.label);
				Rect rect6 = new Rect(0f, 40f, 300f, rect4.height - 45f - 10f);
				Rect rect7 = rect6;
				ref Vector2 reference = ref scrollPosition;
				ThingFilter filter = SelectedOutfit.filter;
				ThingFilter parentFilter = apparelGlobalFilter;
				int openMask = 16;
				IEnumerable<SpecialThingFilterDef> forceHiddenFilters = HiddenSpecialThingFilters();
				ThingFilterUI.DoThingFilterConfigWindow(rect7, ref reference, filter, parentFilter, openMask, null, forceHiddenFilters);
				GUI.EndGroup();
			}
		}

		private IEnumerable<SpecialThingFilterDef> HiddenSpecialThingFilters()
		{
			yield return SpecialThingFilterDefOf.AllowNonDeadmansApparel;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public override void PreClose()
		{
			base.PreClose();
			CheckSelectedOutfitHasName();
		}

		public static void DoNameInputRect(Rect rect, ref string name)
		{
			name = Widgets.TextField(rect, name, 30, Outfit.ValidNameRegex);
		}
	}
}
