using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class TransferableUIUtility
	{
		public struct ExtraInfo
		{
			public string key;

			public string value;

			public string secondValue;

			public string tip;

			public float lastFlashTime;

			public Color color;

			public Color secondColor;

			public ExtraInfo(string key, string value, Color color, string tip, float lastFlashTime = -9999f)
			{
				this.key = key;
				this.value = value;
				this.color = color;
				this.tip = tip;
				this.lastFlashTime = lastFlashTime;
				secondValue = null;
				secondColor = default(Color);
			}

			public ExtraInfo(string key, string value, Color color, string tip, string secondValue, Color secondColor, float lastFlashTime = -9999f)
			{
				this.key = key;
				this.value = value;
				this.color = color;
				this.tip = tip;
				this.lastFlashTime = lastFlashTime;
				this.secondValue = secondValue;
				this.secondColor = secondColor;
			}
		}

		private static List<TransferableCountToTransferStoppingPoint> stoppingPoints = new List<TransferableCountToTransferStoppingPoint>();

		private const float AmountAreaWidth = 90f;

		private const float AmountAreaHeight = 25f;

		private const float AdjustArrowWidth = 30f;

		public const float ResourceIconSize = 27f;

		public const float SortersHeight = 27f;

		public const float ExtraInfoHeight = 40f;

		public const float ExtraInfoMargin = 12f;

		public static readonly Color ZeroCountColor = new Color(0.5f, 0.5f, 0.5f);

		public static readonly Texture2D FlashTex = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 0f, 0f, 0.4f));

		private static readonly Texture2D TradeArrow = ContentFinder<Texture2D>.Get("UI/Widgets/TradeArrow");

		private static readonly Texture2D DividerTex = ContentFinder<Texture2D>.Get("UI/Widgets/Divider");

		private static readonly Texture2D PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant");

		private static readonly Texture2D BondIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Bond");

		[TweakValue("Interface", 0f, 50f)]
		private static float PregnancyIconWidth = 24f;

		[TweakValue("Interface", 0f, 50f)]
		private static float BondIconWidth = 24f;

		public static void DoCountAdjustInterface(Rect rect, Transferable trad, int index, int min, int max, bool flash = false, List<TransferableCountToTransferStoppingPoint> extraStoppingPoints = null, bool readOnly = false)
		{
			stoppingPoints.Clear();
			if (extraStoppingPoints != null)
			{
				stoppingPoints.AddRange(extraStoppingPoints);
			}
			for (int num = stoppingPoints.Count - 1; num >= 0; num--)
			{
				TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint = stoppingPoints[num];
				if (transferableCountToTransferStoppingPoint.threshold != 0)
				{
					TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint2 = stoppingPoints[num];
					if (transferableCountToTransferStoppingPoint2.threshold > min)
					{
						TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint3 = stoppingPoints[num];
						if (transferableCountToTransferStoppingPoint3.threshold < max)
						{
							continue;
						}
					}
					stoppingPoints.RemoveAt(num);
				}
			}
			bool flag = false;
			for (int i = 0; i < stoppingPoints.Count; i++)
			{
				TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint4 = stoppingPoints[i];
				if (transferableCountToTransferStoppingPoint4.threshold == 0)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				stoppingPoints.Add(new TransferableCountToTransferStoppingPoint(0, "0", "0"));
			}
			DoCountAdjustInterfaceInternal(rect, trad, index, min, max, flash, readOnly);
		}

		private static void DoCountAdjustInterfaceInternal(Rect rect, Transferable trad, int index, int min, int max, bool flash, bool readOnly)
		{
			rect = rect.Rounded();
			Vector2 center = rect.center;
			float x = center.x - 45f;
			Vector2 center2 = rect.center;
			Rect rect2 = new Rect(x, center2.y - 12.5f, 90f, 25f).Rounded();
			if (flash)
			{
				GUI.DrawTexture(rect2, FlashTex);
			}
			TransferableOneWay transferableOneWay = trad as TransferableOneWay;
			bool flag = transferableOneWay != null && transferableOneWay.HasAnyThing && transferableOneWay.AnyThing is Pawn && transferableOneWay.MaxCount == 1;
			if (!trad.Interactive || readOnly)
			{
				if (flag)
				{
					bool checkOn = trad.CountToTransfer != 0;
					Widgets.Checkbox(rect2.position, ref checkOn, 24f, disabled: true);
				}
				else
				{
					GUI.color = ((trad.CountToTransfer != 0) ? Color.white : ZeroCountColor);
					Text.Anchor = TextAnchor.MiddleCenter;
					Widgets.Label(rect2, trad.CountToTransfer.ToStringCached());
				}
			}
			else if (flag)
			{
				bool flag2 = trad.CountToTransfer != 0;
				bool checkOn2 = flag2;
				Widgets.Checkbox(rect2.position, ref checkOn2, 24f, disabled: false, paintable: true);
				if (checkOn2 != flag2)
				{
					if (checkOn2)
					{
						trad.AdjustTo(trad.GetMaximumToTransfer());
					}
					else
					{
						trad.AdjustTo(trad.GetMinimumToTransfer());
					}
				}
			}
			else
			{
				Rect rect3 = rect2.ContractedBy(2f);
				rect3.xMax -= 15f;
				rect3.xMin += 16f;
				int val = trad.CountToTransfer;
				string buffer = trad.EditBuffer;
				Widgets.TextFieldNumeric(rect3, ref val, ref buffer, (float)min, (float)max);
				trad.AdjustTo(val);
				trad.EditBuffer = buffer;
			}
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			if (trad.Interactive && !flag)
			{
				TransferablePositiveCountDirection positiveCountDirection = trad.PositiveCountDirection;
				int num = (positiveCountDirection == TransferablePositiveCountDirection.Source) ? 1 : (-1);
				int num2 = GenUI.CurrentAdjustmentMultiplier();
				bool flag3 = trad.GetRange() == 1;
				if (trad.CanAdjustBy(num * num2).Accepted)
				{
					Rect rect4 = new Rect(rect2.x - 30f, rect.y, 30f, rect.height);
					if (flag3)
					{
						rect4.x -= rect4.width;
						rect4.width += rect4.width;
					}
					if (Widgets.ButtonText(rect4, "<"))
					{
						trad.AdjustBy(num * num2);
						SoundDefOf.Tick_High.PlayOneShotOnCamera();
					}
					if (!flag3)
					{
						string label = "<<";
						int? num3 = null;
						int num4 = 0;
						for (int i = 0; i < stoppingPoints.Count; i++)
						{
							TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint = stoppingPoints[i];
							if (positiveCountDirection == TransferablePositiveCountDirection.Source)
							{
								if (trad.CountToTransfer < transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold < num4 || !num3.HasValue))
								{
									label = transferableCountToTransferStoppingPoint.leftLabel;
									num3 = transferableCountToTransferStoppingPoint.threshold;
								}
							}
							else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint.threshold && (transferableCountToTransferStoppingPoint.threshold > num4 || !num3.HasValue))
							{
								label = transferableCountToTransferStoppingPoint.leftLabel;
								num3 = transferableCountToTransferStoppingPoint.threshold;
							}
						}
						rect4.x -= rect4.width;
						if (Widgets.ButtonText(rect4, label))
						{
							if (num3.HasValue)
							{
								trad.AdjustTo(num3.Value);
							}
							else if (num == 1)
							{
								trad.AdjustTo(trad.GetMaximumToTransfer());
							}
							else
							{
								trad.AdjustTo(trad.GetMinimumToTransfer());
							}
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
						}
					}
				}
				if (trad.CanAdjustBy(-num * num2).Accepted)
				{
					Rect rect5 = new Rect(rect2.xMax, rect.y, 30f, rect.height);
					if (flag3)
					{
						rect5.width += rect5.width;
					}
					if (Widgets.ButtonText(rect5, ">"))
					{
						trad.AdjustBy(-num * num2);
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
					}
					if (!flag3)
					{
						string label2 = ">>";
						int? num5 = null;
						int num6 = 0;
						for (int j = 0; j < stoppingPoints.Count; j++)
						{
							TransferableCountToTransferStoppingPoint transferableCountToTransferStoppingPoint2 = stoppingPoints[j];
							if (positiveCountDirection == TransferablePositiveCountDirection.Destination)
							{
								if (trad.CountToTransfer < transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold < num6 || !num5.HasValue))
								{
									label2 = transferableCountToTransferStoppingPoint2.rightLabel;
									num5 = transferableCountToTransferStoppingPoint2.threshold;
								}
							}
							else if (trad.CountToTransfer > transferableCountToTransferStoppingPoint2.threshold && (transferableCountToTransferStoppingPoint2.threshold > num6 || !num5.HasValue))
							{
								label2 = transferableCountToTransferStoppingPoint2.rightLabel;
								num5 = transferableCountToTransferStoppingPoint2.threshold;
							}
						}
						rect5.x += rect5.width;
						if (Widgets.ButtonText(rect5, label2))
						{
							if (num5.HasValue)
							{
								trad.AdjustTo(num5.Value);
							}
							else if (num == 1)
							{
								trad.AdjustTo(trad.GetMinimumToTransfer());
							}
							else
							{
								trad.AdjustTo(trad.GetMaximumToTransfer());
							}
							SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						}
					}
				}
			}
			if (trad.CountToTransfer != 0)
			{
				Rect position = new Rect(rect2.x + rect2.width / 2f - (float)(TradeArrow.width / 2), rect2.y + rect2.height / 2f - (float)(TradeArrow.height / 2), (float)TradeArrow.width, (float)TradeArrow.height);
				TransferablePositiveCountDirection positiveCountDirection2 = trad.PositiveCountDirection;
				if ((positiveCountDirection2 == TransferablePositiveCountDirection.Source && trad.CountToTransfer > 0) || (positiveCountDirection2 == TransferablePositiveCountDirection.Destination && trad.CountToTransfer < 0))
				{
					position.x += position.width;
					position.width *= -1f;
				}
				GUI.DrawTexture(position, TradeArrow);
			}
		}

		public static void DrawTransferableInfo(Transferable trad, Rect idRect, Color labelColor)
		{
			if (trad.HasAnyThing)
			{
				if (Mouse.IsOver(idRect))
				{
					Widgets.DrawHighlight(idRect);
				}
				Rect rect = new Rect(0f, 0f, 27f, 27f);
				Widgets.ThingIcon(rect, trad.AnyThing);
				Widgets.InfoCardButton(40f, 0f, trad.AnyThing);
				Text.Anchor = TextAnchor.MiddleLeft;
				Rect rect2 = new Rect(80f, 0f, idRect.width - 80f, idRect.height);
				Text.WordWrap = false;
				GUI.color = labelColor;
				Widgets.Label(rect2, trad.LabelCap);
				GUI.color = Color.white;
				Text.WordWrap = true;
				TooltipHandler.TipRegion(idRect, new TipSignal(delegate
				{
					if (!trad.HasAnyThing)
					{
						return string.Empty;
					}
					string text = trad.LabelCap;
					string tipDescription = trad.TipDescription;
					if (!tipDescription.NullOrEmpty())
					{
						text = text + ": " + tipDescription;
					}
					return text;
				}, trad.GetHashCode()));
			}
		}

		public static float DefaultListOrderPriority(Transferable transferable)
		{
			if (!transferable.HasAnyThing)
			{
				return 0f;
			}
			return DefaultListOrderPriority(transferable.ThingDef);
		}

		public static float DefaultListOrderPriority(ThingDef def)
		{
			if (def == ThingDefOf.Silver)
			{
				return 100f;
			}
			if (def == ThingDefOf.Gold)
			{
				return 99f;
			}
			if (def.Minifiable)
			{
				return 90f;
			}
			if (def.IsApparel)
			{
				return 80f;
			}
			if (def.IsRangedWeapon)
			{
				return 70f;
			}
			if (def.IsMeleeWeapon)
			{
				return 60f;
			}
			if (def.isTechHediff)
			{
				return 50f;
			}
			if (def.CountAsResource)
			{
				return -10f;
			}
			return 20f;
		}

		public static void DoTransferableSorters(TransferableSorterDef sorter1, TransferableSorterDef sorter2, Action<TransferableSorterDef> sorter1Setter, Action<TransferableSorterDef> sorter2Setter)
		{
			Rect position = new Rect(0f, 0f, 350f, 27f);
			GUI.BeginGroup(position);
			Text.Font = GameFont.Tiny;
			Rect rect = new Rect(0f, 0f, 60f, 27f);
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "SortBy".Translate());
			Text.Anchor = TextAnchor.UpperLeft;
			Rect rect2 = new Rect(rect.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect2, sorter1.LabelCap))
			{
				OpenSorterChangeFloatMenu(sorter1Setter);
			}
			Rect rect3 = new Rect(rect2.xMax + 10f, 0f, 130f, 27f);
			if (Widgets.ButtonText(rect3, sorter2.LabelCap))
			{
				OpenSorterChangeFloatMenu(sorter2Setter);
			}
			GUI.EndGroup();
		}

		public static void DoExtraAnimalIcons(Transferable trad, Rect rect, ref float curX)
		{
			Pawn pawn = trad.AnyThing as Pawn;
			if (pawn != null && pawn.RaceProps.Animal)
			{
				if (pawn.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Bond) != null)
				{
					Rect rect2 = new Rect(curX - BondIconWidth, (rect.height - BondIconWidth) / 2f, BondIconWidth, BondIconWidth);
					curX -= BondIconWidth;
					GUI.DrawTexture(rect2, BondIcon);
					string iconTooltipText = TrainableUtility.GetIconTooltipText(pawn);
					if (!iconTooltipText.NullOrEmpty())
					{
						TooltipHandler.TipRegion(rect2, iconTooltipText);
					}
				}
				if (pawn.health.hediffSet.HasHediff(HediffDefOf.Pregnant, mustBeVisible: true))
				{
					Rect rect3 = new Rect(curX - PregnancyIconWidth, (rect.height - PregnancyIconWidth) / 2f, PregnancyIconWidth, PregnancyIconWidth);
					curX -= PregnancyIconWidth;
					TooltipHandler.TipRegion(rect3, PawnColumnWorker_Pregnant.GetTooltipText(pawn));
					GUI.DrawTexture(rect3, PregnantIcon);
				}
			}
		}

		private static void OpenSorterChangeFloatMenu(Action<TransferableSorterDef> sorterSetter)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			List<TransferableSorterDef> allDefsListForReading = DefDatabase<TransferableSorterDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				TransferableSorterDef def = allDefsListForReading[i];
				list.Add(new FloatMenuOption(def.LabelCap, delegate
				{
					sorterSetter(def);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DrawExtraInfo(List<ExtraInfo> info, Rect rect)
		{
			if (rect.width > (float)info.Count * 230f)
			{
				rect.x += Mathf.Floor((rect.width - (float)info.Count * 230f) / 2f);
				rect.width = (float)info.Count * 230f;
			}
			GUI.BeginGroup(rect);
			float num = Mathf.Floor(rect.width / (float)info.Count);
			float num2 = 0f;
			for (int i = 0; i < info.Count; i++)
			{
				float num3 = (i != info.Count - 1) ? num : (rect.width - num2);
				Rect rect2 = new Rect(num2, 0f, num3, rect.height);
				Rect rect3 = new Rect(num2, 0f, num3, rect.height / 2f);
				Rect rect4 = new Rect(num2, rect.height / 2f, num3, rect.height / 2f);
				float time = Time.time;
				ExtraInfo extraInfo = info[i];
				if (time - extraInfo.lastFlashTime < 1f)
				{
					GUI.DrawTexture(rect2, FlashTex);
				}
				Text.Anchor = TextAnchor.LowerCenter;
				Text.Font = GameFont.Tiny;
				GUI.color = Color.gray;
				Rect rect5 = new Rect(rect3.x, rect3.y - 2f, rect3.width, rect3.height - -3f);
				ExtraInfo extraInfo2 = info[i];
				Widgets.Label(rect5, extraInfo2.key);
				Rect rect6 = new Rect(rect4.x, rect4.y + -3f + 2f, rect4.width, rect4.height - -3f);
				Text.Font = GameFont.Small;
				ExtraInfo extraInfo3 = info[i];
				if (extraInfo3.secondValue.NullOrEmpty())
				{
					Text.Anchor = TextAnchor.UpperCenter;
					ExtraInfo extraInfo4 = info[i];
					GUI.color = extraInfo4.color;
					Rect rect7 = rect6;
					ExtraInfo extraInfo5 = info[i];
					Widgets.Label(rect7, extraInfo5.value);
				}
				else
				{
					Rect rect8 = rect6;
					rect8.width = Mathf.Floor(rect6.width / 2f - 15f);
					Text.Anchor = TextAnchor.UpperRight;
					ExtraInfo extraInfo6 = info[i];
					GUI.color = extraInfo6.color;
					Rect rect9 = rect8;
					ExtraInfo extraInfo7 = info[i];
					Widgets.Label(rect9, extraInfo7.value);
					Rect rect10 = rect6;
					rect10.xMin += Mathf.Ceil(rect6.width / 2f + 15f);
					Text.Anchor = TextAnchor.UpperLeft;
					ExtraInfo extraInfo8 = info[i];
					GUI.color = extraInfo8.secondColor;
					Rect rect11 = rect10;
					ExtraInfo extraInfo9 = info[i];
					Widgets.Label(rect11, extraInfo9.secondValue);
					Rect position = rect6;
					position.x = Mathf.Floor(rect6.x + rect6.width / 2f - 7.5f);
					position.y += 3f;
					position.width = 15f;
					position.height = 15f;
					GUI.color = Color.white;
					GUI.DrawTexture(position, DividerTex);
				}
				GUI.color = Color.white;
				Widgets.DrawHighlightIfMouseover(rect2);
				Rect rect12 = rect2;
				ExtraInfo extraInfo10 = info[i];
				TooltipHandler.TipRegion(rect12, extraInfo10.tip);
				num2 += num3;
			}
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
		}
	}
}
