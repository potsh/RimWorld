using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class TransferableOneWayWidget
	{
		private struct Section
		{
			public string title;

			public IEnumerable<TransferableOneWay> transferables;

			public List<TransferableOneWay> cachedTransferables;
		}

		private List<Section> sections = new List<Section>();

		private string sourceLabel;

		private string destinationLabel;

		private string sourceCountDesc;

		private bool drawMass;

		private IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore;

		private bool includePawnsMassInMassUsage;

		private Func<float> availableMassGetter;

		private float extraHeaderSpace;

		private bool ignoreSpawnedCorpseGearAndInventoryMass;

		private int tile;

		private bool drawMarketValue;

		private bool drawEquippedWeapon;

		private bool drawNutritionEatenPerDay;

		private bool drawItemNutrition;

		private bool drawForagedFoodPerDay;

		private bool drawDaysUntilRot;

		private bool playerPawnsReadOnly;

		private bool transferablesCached;

		private Vector2 scrollPosition;

		private TransferableSorterDef sorter1;

		private TransferableSorterDef sorter2;

		private Dictionary<TransferableOneWay, int> cachedTicksUntilRot = new Dictionary<TransferableOneWay, int>();

		private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

		private const float TopAreaHeight = 37f;

		protected readonly Vector2 AcceptButtonSize = new Vector2(160f, 40f);

		protected readonly Vector2 OtherBottomButtonSize = new Vector2(160f, 40f);

		private const float ColumnWidth = 120f;

		private const float FirstTransferableY = 6f;

		private const float RowInterval = 30f;

		public const float CountColumnWidth = 75f;

		public const float AdjustColumnWidth = 240f;

		public const float MassColumnWidth = 100f;

		public static readonly Color ItemMassColor = new Color(0.7f, 0.7f, 0.7f);

		private const float MarketValueColumnWidth = 100f;

		private const float ExtraSpaceAfterSectionTitle = 5f;

		private const float DaysUntilRotColumnWidth = 75f;

		private const float NutritionEatenPerDayColumnWidth = 75f;

		private const float ItemNutritionColumnWidth = 75f;

		private const float ForagedFoodPerDayColumnWidth = 75f;

		private const float GrazeabilityInnerColumnWidth = 40f;

		private const float EquippedWeaponIconSize = 30f;

		public const float TopAreaWidth = 515f;

		private static readonly Texture2D CanGrazeIcon = ContentFinder<Texture2D>.Get("UI/Icons/CanGraze");

		[CompilerGenerated]
		private static Func<TransferableOneWay, float> _003C_003Ef__mg_0024cache0;

		public float TotalNumbersColumnsWidths
		{
			get
			{
				float num = 315f;
				if (drawMass)
				{
					num += 100f;
				}
				if (drawMarketValue)
				{
					num += 100f;
				}
				if (drawDaysUntilRot)
				{
					num += 75f;
				}
				if (drawItemNutrition)
				{
					num += 75f;
				}
				if (drawNutritionEatenPerDay)
				{
					num += 75f;
				}
				if (drawForagedFoodPerDay)
				{
					num += 75f;
				}
				return num;
			}
		}

		private bool AnyTransferable
		{
			get
			{
				if (!transferablesCached)
				{
					CacheTransferables();
				}
				for (int i = 0; i < sections.Count; i++)
				{
					Section section = sections[i];
					if (section.cachedTransferables.Any())
					{
						return true;
					}
				}
				return false;
			}
		}

		public TransferableOneWayWidget(IEnumerable<TransferableOneWay> transferables, string sourceLabel, string destinationLabel, string sourceCountDesc, bool drawMass = false, IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.DontIgnore, bool includePawnsMassInMassUsage = false, Func<float> availableMassGetter = null, float extraHeaderSpace = 0f, bool ignoreSpawnedCorpseGearAndInventoryMass = false, int tile = -1, bool drawMarketValue = false, bool drawEquippedWeapon = false, bool drawNutritionEatenPerDay = false, bool drawItemNutrition = false, bool drawForagedFoodPerDay = false, bool drawDaysUntilRot = false, bool playerPawnsReadOnly = false)
		{
			if (transferables != null)
			{
				AddSection(null, transferables);
			}
			this.sourceLabel = sourceLabel;
			this.destinationLabel = destinationLabel;
			this.sourceCountDesc = sourceCountDesc;
			this.drawMass = drawMass;
			this.ignorePawnInventoryMass = ignorePawnInventoryMass;
			this.includePawnsMassInMassUsage = includePawnsMassInMassUsage;
			this.availableMassGetter = availableMassGetter;
			this.extraHeaderSpace = extraHeaderSpace;
			this.ignoreSpawnedCorpseGearAndInventoryMass = ignoreSpawnedCorpseGearAndInventoryMass;
			this.tile = tile;
			this.drawMarketValue = drawMarketValue;
			this.drawEquippedWeapon = drawEquippedWeapon;
			this.drawNutritionEatenPerDay = drawNutritionEatenPerDay;
			this.drawItemNutrition = drawItemNutrition;
			this.drawForagedFoodPerDay = drawForagedFoodPerDay;
			this.drawDaysUntilRot = drawDaysUntilRot;
			this.playerPawnsReadOnly = playerPawnsReadOnly;
			sorter1 = TransferableSorterDefOf.Category;
			sorter2 = TransferableSorterDefOf.MarketValue;
		}

		public void AddSection(string title, IEnumerable<TransferableOneWay> transferables)
		{
			Section item = default(Section);
			item.title = title;
			item.transferables = transferables;
			item.cachedTransferables = new List<TransferableOneWay>();
			sections.Add(item);
			transferablesCached = false;
		}

		private void CacheTransferables()
		{
			transferablesCached = true;
			for (int i = 0; i < sections.Count; i++)
			{
				Section section = sections[i];
				List<TransferableOneWay> cachedTransferables = section.cachedTransferables;
				cachedTransferables.Clear();
				List<TransferableOneWay> list = cachedTransferables;
				Section section2 = sections[i];
				list.AddRange(section2.transferables.OrderBy((TransferableOneWay tr) => tr, sorter1.Comparer).ThenBy((TransferableOneWay tr) => tr, sorter2.Comparer).ThenBy(TransferableUIUtility.DefaultListOrderPriority)
					.ToList());
			}
		}

		public void OnGUI(Rect inRect)
		{
			OnGUI(inRect, out bool _);
		}

		public void OnGUI(Rect inRect, out bool anythingChanged)
		{
			if (!transferablesCached)
			{
				CacheTransferables();
			}
			TransferableUIUtility.DoTransferableSorters(sorter1, sorter2, delegate(TransferableSorterDef x)
			{
				sorter1 = x;
				CacheTransferables();
			}, delegate(TransferableSorterDef x)
			{
				sorter2 = x;
				CacheTransferables();
			});
			if (!sourceLabel.NullOrEmpty() || !destinationLabel.NullOrEmpty())
			{
				float num = inRect.width - 515f;
				Rect position = new Rect(inRect.x + num, inRect.y, inRect.width - num, 37f);
				GUI.BeginGroup(position);
				Text.Font = GameFont.Medium;
				if (!sourceLabel.NullOrEmpty())
				{
					Rect rect = new Rect(0f, 0f, position.width / 2f, position.height);
					Text.Anchor = TextAnchor.UpperLeft;
					Widgets.Label(rect, sourceLabel);
				}
				if (!destinationLabel.NullOrEmpty())
				{
					Rect rect2 = new Rect(position.width / 2f, 0f, position.width / 2f, position.height);
					Text.Anchor = TextAnchor.UpperRight;
					Widgets.Label(rect2, destinationLabel);
				}
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.EndGroup();
			}
			Rect mainRect = new Rect(inRect.x, inRect.y + 37f + extraHeaderSpace, inRect.width, inRect.height - 37f - extraHeaderSpace);
			FillMainRect(mainRect, out anythingChanged);
		}

		private void FillMainRect(Rect mainRect, out bool anythingChanged)
		{
			anythingChanged = false;
			Text.Font = GameFont.Small;
			if (AnyTransferable)
			{
				float num = 6f;
				for (int i = 0; i < sections.Count; i++)
				{
					float num2 = num;
					Section section = sections[i];
					num = num2 + (float)section.cachedTransferables.Count * 30f;
					Section section2 = sections[i];
					if (section2.title != null)
					{
						num += 30f;
					}
				}
				float curY = 6f;
				float availableMass = (availableMassGetter == null) ? 3.40282347E+38f : availableMassGetter();
				Rect viewRect = new Rect(0f, 0f, mainRect.width - 16f, num);
				Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
				float num3 = scrollPosition.y - 30f;
				float num4 = scrollPosition.y + mainRect.height;
				for (int j = 0; j < sections.Count; j++)
				{
					Section section3 = sections[j];
					List<TransferableOneWay> cachedTransferables = section3.cachedTransferables;
					if (cachedTransferables.Any())
					{
						Section section4 = sections[j];
						if (section4.title != null)
						{
							float width = viewRect.width;
							Section section5 = sections[j];
							Widgets.ListSeparator(ref curY, width, section5.title);
							curY += 5f;
						}
						for (int k = 0; k < cachedTransferables.Count; k++)
						{
							if (curY > num3 && curY < num4)
							{
								Rect rect = new Rect(0f, curY, viewRect.width, 30f);
								int countToTransfer = cachedTransferables[k].CountToTransfer;
								DoRow(rect, cachedTransferables[k], k, availableMass);
								if (countToTransfer != cachedTransferables[k].CountToTransfer)
								{
									anythingChanged = true;
								}
							}
							curY += 30f;
						}
					}
				}
				Widgets.EndScrollView();
			}
			else
			{
				GUI.color = Color.gray;
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(mainRect, "NoneBrackets".Translate());
				Text.Anchor = TextAnchor.UpperLeft;
				GUI.color = Color.white;
			}
		}

		private void DoRow(Rect rect, TransferableOneWay trad, int index, float availableMass)
		{
			if (index % 2 == 1)
			{
				Widgets.DrawLightHighlight(rect);
			}
			Text.Font = GameFont.Small;
			GUI.BeginGroup(rect);
			float width = rect.width;
			int maxCount = trad.MaxCount;
			Rect rect2 = new Rect(width - 240f, 0f, 240f, rect.height);
			stoppingPoints.Clear();
			if (availableMassGetter != null && (!(trad.AnyThing is Pawn) || includePawnsMassInMassUsage))
			{
				float num = availableMass + GetMass(trad.AnyThing) * (float)trad.CountToTransfer;
				int threshold = (!(num <= 0f)) ? Mathf.FloorToInt(num / GetMass(trad.AnyThing)) : 0;
				stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(threshold, "M<", ">M"));
			}
			Pawn pawn = trad.AnyThing as Pawn;
			bool flag = pawn != null && (pawn.IsColonist || pawn.IsPrisonerOfColony);
			Rect rect3 = rect2;
			int min = 0;
			int max = maxCount;
			List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = stoppingPoints;
			TransferableUIUtility.DoCountAdjustInterface(rect3, trad, index, min, max, flash: false, extraStoppingPoints, playerPawnsReadOnly && flag);
			width -= 240f;
			if (drawMarketValue)
			{
				Rect rect4 = new Rect(width - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				DrawMarketValue(rect4, trad);
				width -= 100f;
			}
			if (drawMass)
			{
				Rect rect5 = new Rect(width - 100f, 0f, 100f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				DrawMass(rect5, trad, availableMass);
				width -= 100f;
			}
			if (drawDaysUntilRot)
			{
				Rect rect6 = new Rect(width - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				DrawDaysUntilRot(rect6, trad);
				width -= 75f;
			}
			if (drawItemNutrition)
			{
				Rect rect7 = new Rect(width - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				DrawItemNutrition(rect7, trad);
				width -= 75f;
			}
			if (drawForagedFoodPerDay)
			{
				Rect rect8 = new Rect(width - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				if (!DrawGrazeability(rect8, trad))
				{
					DrawForagedFoodPerDay(rect8, trad);
				}
				width -= 75f;
			}
			if (drawNutritionEatenPerDay)
			{
				Rect rect9 = new Rect(width - 75f, 0f, 75f, rect.height);
				Text.Anchor = TextAnchor.MiddleLeft;
				DrawNutritionEatenPerDay(rect9, trad);
				width -= 75f;
			}
			if (ShouldShowCount(trad))
			{
				Rect rect10 = new Rect(width - 75f, 0f, 75f, rect.height);
				Widgets.DrawHighlightIfMouseover(rect10);
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect11 = rect10;
				rect11.xMin += 5f;
				rect11.xMax -= 5f;
				Widgets.Label(rect11, maxCount.ToStringCached());
				TooltipHandler.TipRegion(rect10, sourceCountDesc);
			}
			width -= 75f;
			if (drawEquippedWeapon)
			{
				Rect rect12 = new Rect(width - 30f, 0f, 30f, rect.height);
				Rect iconRect = new Rect(width - 30f, (rect.height - 30f) / 2f, 30f, 30f);
				DrawEquippedWeapon(rect12, iconRect, trad);
				width -= 30f;
			}
			TransferableUIUtility.DoExtraAnimalIcons(trad, rect, ref width);
			Rect idRect = new Rect(0f, 0f, width, rect.height);
			TransferableUIUtility.DrawTransferableInfo(trad, idRect, Color.white);
			GenUI.ResetLabelAlign();
			GUI.EndGroup();
		}

		private bool ShouldShowCount(TransferableOneWay trad)
		{
			if (!trad.HasAnyThing)
			{
				return true;
			}
			Pawn pawn = trad.AnyThing as Pawn;
			return pawn == null || !pawn.RaceProps.Humanlike || trad.MaxCount != 1;
		}

		private void DrawDaysUntilRot(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing && trad.ThingDef.IsNutritionGivingIngestible)
			{
				if (!cachedTicksUntilRot.TryGetValue(trad, out int value))
				{
					value = 2147483647;
					for (int i = 0; i < trad.things.Count; i++)
					{
						CompRottable compRottable = trad.things[i].TryGetComp<CompRottable>();
						if (compRottable != null)
						{
							value = Mathf.Min(value, DaysUntilRotCalculator.ApproxTicksUntilRot_AssumeTimePassesBy(compRottable, tile));
						}
					}
					cachedTicksUntilRot.Add(trad, value);
				}
				if (value < 36000000 && !((float)value >= 3.6E+07f))
				{
					Widgets.DrawHighlightIfMouseover(rect);
					float num = (float)value / 60000f;
					GUI.color = Color.yellow;
					Widgets.Label(rect, num.ToString("0.#"));
					GUI.color = Color.white;
					TooltipHandler.TipRegion(rect, "DaysUntilRotTip".Translate());
				}
			}
		}

		private void DrawItemNutrition(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing && trad.ThingDef.IsNutritionGivingIngestible)
			{
				Widgets.DrawHighlightIfMouseover(rect);
				GUI.color = Color.green;
				Widgets.Label(rect, trad.ThingDef.GetStatValueAbstract(StatDefOf.Nutrition).ToString("0.##"));
				GUI.color = Color.white;
				TooltipHandler.TipRegion(rect, "ItemNutritionTip".Translate((1.6f * ThingDefOf.Human.race.baseHungerRate).ToString("0.##")));
			}
		}

		private bool DrawGrazeability(Rect rect, TransferableOneWay trad)
		{
			if (!trad.HasAnyThing)
			{
				return false;
			}
			Pawn pawn = trad.AnyThing as Pawn;
			if (pawn == null || !VirtualPlantsUtility.CanEverEatVirtualPlants(pawn))
			{
				return false;
			}
			rect.width = 40f;
			Rect position = new Rect(rect.x + (float)(int)((rect.width - 28f) / 2f), rect.y + (float)(int)((rect.height - 28f) / 2f), 28f, 28f);
			Widgets.DrawHighlightIfMouseover(rect);
			GUI.DrawTexture(position, CanGrazeIcon);
			TooltipHandler.TipRegion(rect, delegate
			{
				string text = "AnimalCanGrazeTip".Translate();
				if (tile != -1)
				{
					text = text + "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(tile, Find.TickManager.TicksAbs);
				}
				return text;
			}, trad.GetHashCode() ^ 0x7424D7F2);
			return true;
		}

		private void DrawForagedFoodPerDay(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn p = trad.AnyThing as Pawn;
				if (p != null)
				{
					bool skip;
					float foragedNutritionPerDay = ForagedFoodPerDayCalculator.GetBaseForagedNutritionPerDay(p, out skip);
					if (!skip)
					{
						Widgets.DrawHighlightIfMouseover(rect);
						GUI.color = ((foragedNutritionPerDay != 0f) ? Color.green : Color.gray);
						Widgets.Label(rect, "+" + foragedNutritionPerDay.ToString("0.##"));
						GUI.color = Color.white;
						TooltipHandler.TipRegion(rect, () => "NutritionForagedPerDayTip".Translate(StatDefOf.ForagedNutritionPerDay.Worker.GetExplanationFull(StatRequest.For(p), StatDefOf.ForagedNutritionPerDay.toStringNumberSense, foragedNutritionPerDay)), trad.GetHashCode() ^ 0x74BEF43E);
					}
				}
			}
		}

		private void DrawNutritionEatenPerDay(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn p = trad.AnyThing as Pawn;
				if (p != null && p.RaceProps.EatsFood && !p.Dead && p.needs.food != null)
				{
					Widgets.DrawHighlightIfMouseover(rect);
					string text = (p.needs.food.FoodFallPerTick * 60000f).ToString("0.##");
					DietCategory resolvedDietCategory = p.RaceProps.ResolvedDietCategory;
					if (resolvedDietCategory != DietCategory.Omnivorous)
					{
						text = text + " (" + resolvedDietCategory.ToStringHumanShort() + ")";
					}
					GUI.color = new Color(1f, 0.5f, 0f);
					Widgets.Label(rect, text);
					GUI.color = Color.white;
					TooltipHandler.TipRegion(rect, delegate
					{
						StringBuilder stringBuilder = new StringBuilder();
						stringBuilder.Append("NoDietCategoryLetter".Translate() + " - " + DietCategory.Omnivorous.ToStringHuman());
						DietCategory[] array = (DietCategory[])Enum.GetValues(typeof(DietCategory));
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] != 0 && array[i] != DietCategory.Omnivorous)
							{
								stringBuilder.AppendLine();
								stringBuilder.Append(array[i].ToStringHumanShort() + " - " + array[i].ToStringHuman());
							}
						}
						return "NutritionEatenPerDayTip".Translate(ThingDefOf.MealSimple.GetStatValueAbstract(StatDefOf.Nutrition).ToString("0.##"), stringBuilder.ToString(), p.RaceProps.foodType.ToHumanString());
					}, trad.GetHashCode() ^ 0x17016B3E);
				}
			}
		}

		private void DrawMarketValue(Rect rect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Widgets.DrawHighlightIfMouseover(rect);
				Widgets.Label(rect, trad.AnyThing.MarketValue.ToStringMoney("F2"));
				TooltipHandler.TipRegion(rect, "MarketValueTip".Translate());
			}
		}

		private void DrawMass(Rect rect, TransferableOneWay trad, float availableMass)
		{
			if (trad.HasAnyThing)
			{
				Thing anyThing = trad.AnyThing;
				Pawn pawn = anyThing as Pawn;
				if (pawn == null || includePawnsMassInMassUsage || MassUtility.CanEverCarryAnything(pawn))
				{
					Widgets.DrawHighlightIfMouseover(rect);
					if (pawn == null || includePawnsMassInMassUsage)
					{
						float mass = GetMass(anyThing);
						if (pawn != null)
						{
							float gearMass = 0f;
							float invMass = 0f;
							gearMass = MassUtility.GearMass(pawn);
							if (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass))
							{
								invMass = MassUtility.InventoryMass(pawn);
							}
							TooltipHandler.TipRegion(rect, () => GetPawnMassTip(trad, 0f, mass - gearMass - invMass, gearMass, invMass), trad.GetHashCode() * 59);
						}
						else
						{
							TooltipHandler.TipRegion(rect, "ItemWeightTip".Translate());
						}
						if (mass > availableMass)
						{
							GUI.color = Color.red;
						}
						else
						{
							GUI.color = ItemMassColor;
						}
						Widgets.Label(rect, mass.ToStringMass());
					}
					else
					{
						float cap = MassUtility.Capacity(pawn);
						float gearMass2 = MassUtility.GearMass(pawn);
						float invMass2 = (!InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass)) ? MassUtility.InventoryMass(pawn) : 0f;
						float num = cap - gearMass2 - invMass2;
						if (num > 0f)
						{
							GUI.color = Color.green;
						}
						else if (num < 0f)
						{
							GUI.color = Color.red;
						}
						else
						{
							GUI.color = Color.gray;
						}
						Widgets.Label(rect, num.ToStringMassOffset());
						TooltipHandler.TipRegion(rect, () => GetPawnMassTip(trad, cap, 0f, gearMass2, invMass2), trad.GetHashCode() * 59);
					}
					GUI.color = Color.white;
				}
			}
		}

		private void DrawEquippedWeapon(Rect rect, Rect iconRect, TransferableOneWay trad)
		{
			if (trad.HasAnyThing)
			{
				Pawn pawn = trad.AnyThing as Pawn;
				if (pawn != null && pawn.equipment != null && pawn.equipment.Primary != null)
				{
					ThingWithComps primary = pawn.equipment.Primary;
					Widgets.DrawHighlightIfMouseover(rect);
					Widgets.ThingIcon(iconRect, primary);
					TooltipHandler.TipRegion(rect, primary.LabelCap);
				}
			}
		}

		private string GetPawnMassTip(TransferableOneWay trad, float capacity, float pawnMass, float gearMass, float invMass)
		{
			if (!trad.HasAnyThing)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			if (capacity != 0f)
			{
				stringBuilder.Append("MassCapacity".Translate() + ": " + capacity.ToStringMass());
			}
			else
			{
				stringBuilder.Append("Mass".Translate() + ": " + pawnMass.ToStringMass());
			}
			if (gearMass != 0f)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("EquipmentAndApparelMass".Translate() + ": " + gearMass.ToStringMass());
			}
			if (invMass != 0f)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("InventoryMass".Translate() + ": " + invMass.ToStringMass());
			}
			return stringBuilder.ToString();
		}

		private float GetMass(Thing thing)
		{
			if (thing == null)
			{
				return 0f;
			}
			float num = thing.GetStatValue(StatDefOf.Mass);
			Pawn pawn = thing as Pawn;
			if (pawn != null)
			{
				if (InventoryCalculatorsUtility.ShouldIgnoreInventoryOf(pawn, ignorePawnInventoryMass))
				{
					num -= MassUtility.InventoryMass(pawn);
				}
			}
			else if (ignoreSpawnedCorpseGearAndInventoryMass)
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null && corpse.Spawned)
				{
					num -= MassUtility.GearAndInventoryMass(corpse.InnerPawn);
				}
			}
			return num;
		}
	}
}
