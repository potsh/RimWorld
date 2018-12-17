using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_BillConfig : Window
	{
		private IntVec3 billGiverPos;

		private Bill_Production bill;

		private Vector2 thingFilterScrollPosition;

		private string repeatCountEditBuffer;

		private string targetCountEditBuffer;

		private string unpauseCountEditBuffer;

		[TweakValue("Interface", 0f, 400f)]
		private static int RepeatModeSubdialogHeight = 324;

		[TweakValue("Interface", 0f, 400f)]
		private static int StoreModeSubdialogHeight = 30;

		[TweakValue("Interface", 0f, 400f)]
		private static int WorkerSelectionSubdialogHeight = 85;

		[TweakValue("Interface", 0f, 400f)]
		private static int IngredientRadiusSubdialogHeight = 50;

		public override Vector2 InitialSize => new Vector2(800f, 634f);

		public Dialog_BillConfig(Bill_Production bill, IntVec3 billGiverPos)
		{
			this.billGiverPos = billGiverPos;
			this.bill = bill;
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			absorbInputAroundWindow = true;
			closeOnClickedOutside = true;
		}

		private void AdjustCount(int offset)
		{
			if (offset > 0)
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
			}
			else
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
			}
			bill.repeatCount += offset;
			if (bill.repeatCount < 1)
			{
				bill.repeatCount = 1;
			}
		}

		public override void WindowUpdate()
		{
			bill.TryDrawIngredientSearchRadiusOnMap(billGiverPos);
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Medium;
			Rect rect = new Rect(0f, 0f, 400f, 50f);
			Widgets.Label(rect, bill.LabelCap);
			float num = (float)(int)((inRect.width - 34f) / 3f);
			Rect rect2 = new Rect(0f, 80f, num, inRect.height - 80f);
			float x = rect2.xMax + 17f;
			float width = num;
			float num2 = inRect.height - 50f;
			Vector2 closeButSize = CloseButSize;
			Rect rect3 = new Rect(x, 50f, width, num2 - closeButSize.y);
			float x2 = rect3.xMax + 17f;
			float num3 = inRect.height - 50f;
			Vector2 closeButSize2 = CloseButSize;
			Rect rect4 = new Rect(x2, 50f, 0f, num3 - closeButSize2.y);
			rect4.xMax = inRect.xMax;
			Text.Font = GameFont.Small;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect3);
			Listing_Standard listing_Standard2 = listing_Standard.BeginSection((float)RepeatModeSubdialogHeight);
			if (listing_Standard2.ButtonText(bill.repeatMode.LabelCap))
			{
				BillRepeatModeUtility.MakeConfigFloatMenu(bill);
			}
			listing_Standard2.Gap();
			if (bill.repeatMode == BillRepeatModeDefOf.RepeatCount)
			{
				listing_Standard2.Label("RepeatCount".Translate(bill.repeatCount));
				listing_Standard2.IntEntry(ref bill.repeatCount, ref repeatCountEditBuffer);
			}
			else if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				string arg = "CurrentlyHave".Translate() + ": ";
				arg += bill.recipe.WorkerCounter.CountProducts(bill);
				arg += " / ";
				arg += ((bill.targetCount >= 999999) ? "Infinite".Translate().ToLower() : bill.targetCount.ToString());
				string str = bill.recipe.WorkerCounter.ProductsDescription(bill);
				if (!str.NullOrEmpty())
				{
					string text = arg;
					arg = text + "\n" + "CountingProducts".Translate() + ": " + str.CapitalizeFirst();
				}
				listing_Standard2.Label(arg);
				int targetCount = bill.targetCount;
				listing_Standard2.IntEntry(ref bill.targetCount, ref targetCountEditBuffer, bill.recipe.targetCountAdjustment);
				bill.unpauseWhenYouHave = Mathf.Max(0, bill.unpauseWhenYouHave + (bill.targetCount - targetCount));
				ThingDef producedThingDef = bill.recipe.ProducedThingDef;
				if (producedThingDef != null)
				{
					if (producedThingDef.IsWeapon || producedThingDef.IsApparel)
					{
						listing_Standard2.CheckboxLabeled("IncludeEquipped".Translate(), ref bill.includeEquipped);
					}
					if (producedThingDef.IsApparel && producedThingDef.apparel.careIfWornByCorpse)
					{
						listing_Standard2.CheckboxLabeled("IncludeTainted".Translate(), ref bill.includeTainted);
					}
					Widgets.Dropdown(listing_Standard2.GetRect(30f), bill, (Bill_Production b) => b.includeFromZone, (Bill_Production b) => GenerateStockpileInclusion(), (bill.includeFromZone != null) ? "IncludeSpecific".Translate(bill.includeFromZone.label) : "IncludeFromAll".Translate());
					Widgets.FloatRange(listing_Standard2.GetRect(28f), 975643279, ref bill.hpRange, 0f, 1f, "HitPoints", ToStringStyle.PercentZero);
					if (producedThingDef.HasComp(typeof(CompQuality)))
					{
						Widgets.QualityRange(listing_Standard2.GetRect(28f), 1098906561, ref bill.qualityRange);
					}
					if (producedThingDef.MadeFromStuff)
					{
						listing_Standard2.CheckboxLabeled("LimitToAllowedStuff".Translate(), ref bill.limitToAllowedStuff);
					}
				}
			}
			if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
			{
				listing_Standard2.CheckboxLabeled("PauseWhenSatisfied".Translate(), ref bill.pauseWhenSatisfied);
				if (bill.pauseWhenSatisfied)
				{
					listing_Standard2.Label("UnpauseWhenYouHave".Translate() + ": " + bill.unpauseWhenYouHave.ToString("F0"));
					listing_Standard2.IntEntry(ref bill.unpauseWhenYouHave, ref unpauseCountEditBuffer, bill.recipe.targetCountAdjustment);
					if (bill.unpauseWhenYouHave >= bill.targetCount)
					{
						bill.unpauseWhenYouHave = bill.targetCount - 1;
						unpauseCountEditBuffer = bill.unpauseWhenYouHave.ToStringCached();
					}
				}
			}
			listing_Standard.EndSection(listing_Standard2);
			listing_Standard.Gap();
			Listing_Standard listing_Standard3 = listing_Standard.BeginSection((float)StoreModeSubdialogHeight);
			string text2 = string.Format(bill.GetStoreMode().LabelCap, (bill.GetStoreZone() == null) ? string.Empty : bill.GetStoreZone().SlotYielderLabel());
			if (bill.GetStoreZone() != null && !bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone()))
			{
				text2 += string.Format(" ({0})", "IncompatibleLower".Translate());
				Text.Font = GameFont.Tiny;
			}
			if (listing_Standard3.ButtonText(text2))
			{
				Text.Font = GameFont.Small;
				List<FloatMenuOption> list = new List<FloatMenuOption>();
				foreach (BillStoreModeDef item in from bsm in DefDatabase<BillStoreModeDef>.AllDefs
				orderby bsm.listOrder
				select bsm)
				{
					if (item == BillStoreModeDefOf.SpecificStockpile)
					{
						List<SlotGroup> allGroupsListInPriorityOrder = bill.billStack.billGiver.Map.haulDestinationManager.AllGroupsListInPriorityOrder;
						int count = allGroupsListInPriorityOrder.Count;
						for (int i = 0; i < count; i++)
						{
							SlotGroup group = allGroupsListInPriorityOrder[i];
							Zone_Stockpile zone_Stockpile = group.parent as Zone_Stockpile;
							if (zone_Stockpile != null)
							{
								if (!bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, zone_Stockpile))
								{
									list.Add(new FloatMenuOption(string.Format("{0} ({1})", string.Format(item.LabelCap, group.parent.SlotYielderLabel()), "IncompatibleLower".Translate()), null));
								}
								else
								{
									list.Add(new FloatMenuOption(string.Format(item.LabelCap, group.parent.SlotYielderLabel()), delegate
									{
										bill.SetStoreMode(BillStoreModeDefOf.SpecificStockpile, (Zone_Stockpile)group.parent);
									}));
								}
							}
						}
					}
					else
					{
						BillStoreModeDef smLocal = item;
						list.Add(new FloatMenuOption(smLocal.LabelCap, delegate
						{
							bill.SetStoreMode(smLocal);
						}));
					}
				}
				Find.WindowStack.Add(new FloatMenu(list));
			}
			Text.Font = GameFont.Small;
			listing_Standard.EndSection(listing_Standard3);
			listing_Standard.Gap();
			Listing_Standard listing_Standard4 = listing_Standard.BeginSection((float)WorkerSelectionSubdialogHeight);
			Widgets.Dropdown(listing_Standard4.GetRect(30f), bill, (Bill_Production b) => b.pawnRestriction, (Bill_Production b) => GeneratePawnRestrictionOptions(), (bill.pawnRestriction != null) ? bill.pawnRestriction.LabelShortCap : "AnyWorker".Translate());
			if (bill.pawnRestriction == null && bill.recipe.workSkill != null)
			{
				listing_Standard4.Label("AllowedSkillRange".Translate(bill.recipe.workSkill.label));
				listing_Standard4.IntRange(ref bill.allowedSkillRange, 0, 20);
			}
			listing_Standard.EndSection(listing_Standard4);
			listing_Standard.End();
			Rect rect5 = rect4;
			rect5.yMin = rect5.yMax - (float)IngredientRadiusSubdialogHeight;
			rect4.yMax = rect5.yMin - 17f;
			bool flag = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
			Rect rect6 = rect4;
			ref Vector2 scrollPosition = ref thingFilterScrollPosition;
			ThingFilter ingredientFilter = bill.ingredientFilter;
			ThingFilter fixedIngredientFilter = bill.recipe.fixedIngredientFilter;
			int openMask = 4;
			IEnumerable<ThingDef> forceHiddenDefs = null;
			List<SpecialThingFilterDef> forceHiddenSpecialFilters = bill.recipe.forceHiddenSpecialFilters;
			List<ThingDef> premultipliedSmallIngredients = bill.recipe.GetPremultipliedSmallIngredients();
			ThingFilterUI.DoThingFilterConfigWindow(rect6, ref scrollPosition, ingredientFilter, fixedIngredientFilter, openMask, forceHiddenDefs, forceHiddenSpecialFilters, forceHideHitPointsConfig: false, premultipliedSmallIngredients, bill.Map);
			bool flag2 = bill.GetStoreZone() == null || bill.recipe.WorkerCounter.CanPossiblyStoreInStockpile(bill, bill.GetStoreZone());
			if (flag && !flag2)
			{
				Messages.Message("MessageBillValidationStoreZoneInsufficient".Translate(bill.LabelCap, bill.billStack.billGiver.LabelShort.CapitalizeFirst(), bill.GetStoreZone().label), bill.billStack.billGiver as Thing, MessageTypeDefOf.RejectInput, historical: false);
			}
			Listing_Standard listing_Standard5 = new Listing_Standard();
			listing_Standard5.Begin(rect5);
			string str2 = "IngredientSearchRadius".Translate().Truncate(rect5.width * 0.6f);
			string str3 = (bill.ingredientSearchRadius != 999f) ? bill.ingredientSearchRadius.ToString("F0") : "Unlimited".Translate().Truncate(rect5.width * 0.3f);
			listing_Standard5.Label(str2 + ": " + str3);
			bill.ingredientSearchRadius = listing_Standard5.Slider(bill.ingredientSearchRadius, 3f, 100f);
			if (bill.ingredientSearchRadius >= 100f)
			{
				bill.ingredientSearchRadius = 999f;
			}
			listing_Standard5.End();
			Listing_Standard listing_Standard6 = new Listing_Standard();
			listing_Standard6.Begin(rect2);
			if (bill.suspended)
			{
				if (listing_Standard6.ButtonText("Suspended".Translate()))
				{
					bill.suspended = false;
					SoundDefOf.Click.PlayOneShotOnCamera();
				}
			}
			else if (listing_Standard6.ButtonText("NotSuspended".Translate()))
			{
				bill.suspended = true;
				SoundDefOf.Click.PlayOneShotOnCamera();
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (bill.recipe.description != null)
			{
				stringBuilder.AppendLine(bill.recipe.description);
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine("WorkAmount".Translate() + ": " + bill.recipe.WorkAmountTotal(null).ToStringWorkAmount());
			for (int j = 0; j < bill.recipe.ingredients.Count; j++)
			{
				IngredientCount ingredientCount = bill.recipe.ingredients[j];
				if (!ingredientCount.filter.Summary.NullOrEmpty())
				{
					stringBuilder.AppendLine(bill.recipe.IngredientValueGetter.BillRequirementsDescription(bill.recipe, ingredientCount));
				}
			}
			stringBuilder.AppendLine();
			string text3 = bill.recipe.IngredientValueGetter.ExtraDescriptionLine(bill.recipe);
			if (text3 != null)
			{
				stringBuilder.AppendLine(text3);
				stringBuilder.AppendLine();
			}
			if (!bill.recipe.skillRequirements.NullOrEmpty())
			{
				stringBuilder.AppendLine("MinimumSkills".Translate());
				stringBuilder.AppendLine(bill.recipe.MinSkillString);
			}
			Text.Font = GameFont.Small;
			string text4 = stringBuilder.ToString();
			if (Text.CalcHeight(text4, rect2.width) > rect2.height)
			{
				Text.Font = GameFont.Tiny;
			}
			listing_Standard6.Label(text4);
			Text.Font = GameFont.Small;
			listing_Standard6.End();
			if (bill.recipe.products.Count == 1)
			{
				ThingDef thingDef = bill.recipe.products[0].thingDef;
				Widgets.InfoCardButton(rect2.x, rect4.y, thingDef, GenStuff.DefaultStuffFor(thingDef));
			}
		}

		private IEnumerable<Widgets.DropdownMenuElement<Pawn>> GeneratePawnRestrictionOptions()
		{
			_003CGeneratePawnRestrictionOptions_003Ec__Iterator0 _003CGeneratePawnRestrictionOptions_003Ec__Iterator = (_003CGeneratePawnRestrictionOptions_003Ec__Iterator0)/*Error near IL_0048: stateMachine*/;
			yield return new Widgets.DropdownMenuElement<Pawn>
			{
				option = new FloatMenuOption("AnyWorker".Translate(), delegate
				{
					_003CGeneratePawnRestrictionOptions_003Ec__Iterator._0024this.bill.pawnRestriction = null;
				}),
				payload = null
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private IEnumerable<Widgets.DropdownMenuElement<Zone_Stockpile>> GenerateStockpileInclusion()
		{
			yield return new Widgets.DropdownMenuElement<Zone_Stockpile>
			{
				option = new FloatMenuOption("IncludeFromAll".Translate(), delegate
				{
					((_003CGenerateStockpileInclusion_003Ec__Iterator1)/*Error near IL_003e: stateMachine*/)._0024this.bill.includeFromZone = null;
				}),
				payload = null
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
