using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanUIUtility
	{
		public struct CaravanInfo
		{
			public float massUsage;

			public float massCapacity;

			public string massCapacityExplanation;

			public float tilesPerDay;

			public string tilesPerDayExplanation;

			public Pair<float, float> daysWorthOfFood;

			public Pair<ThingDef, float> foragedFoodPerDay;

			public string foragedFoodPerDayExplanation;

			public float visibility;

			public string visibilityExplanation;

			public float extraMassUsage;

			public float extraMassCapacity;

			public string extraMassCapacityExplanation;

			public CaravanInfo(float massUsage, float massCapacity, string massCapacityExplanation, float tilesPerDay, string tilesPerDayExplanation, Pair<float, float> daysWorthOfFood, Pair<ThingDef, float> foragedFoodPerDay, string foragedFoodPerDayExplanation, float visibility, string visibilityExplanation, float extraMassUsage = -1f, float extraMassCapacity = -1f, string extraMassCapacityExplanation = null)
			{
				this.massUsage = massUsage;
				this.massCapacity = massCapacity;
				this.massCapacityExplanation = massCapacityExplanation;
				this.tilesPerDay = tilesPerDay;
				this.tilesPerDayExplanation = tilesPerDayExplanation;
				this.daysWorthOfFood = daysWorthOfFood;
				this.foragedFoodPerDay = foragedFoodPerDay;
				this.foragedFoodPerDayExplanation = foragedFoodPerDayExplanation;
				this.visibility = visibility;
				this.visibilityExplanation = visibilityExplanation;
				this.extraMassUsage = extraMassUsage;
				this.extraMassCapacity = extraMassCapacity;
				this.extraMassCapacityExplanation = extraMassCapacityExplanation;
			}
		}

		private static readonly List<Pair<float, Color>> MassColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0.37f, Color.green),
			new Pair<float, Color>(0.82f, Color.yellow),
			new Pair<float, Color>(1f, new Color(1f, 0.6f, 0f))
		};

		private static readonly List<Pair<float, Color>> TilesPerDayColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0f, Color.white),
			new Pair<float, Color>(0.001f, Color.red),
			new Pair<float, Color>(1f, Color.yellow),
			new Pair<float, Color>(2f, Color.white)
		};

		private static readonly List<Pair<float, Color>> DaysWorthOfFoodColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(1f, Color.red),
			new Pair<float, Color>(2f, Color.white)
		};

		private static readonly List<Pair<float, Color>> DaysWorthOfFoodKnownRouteColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0.3f, Color.red),
			new Pair<float, Color>(0.9f, Color.yellow),
			new Pair<float, Color>(1.02f, Color.green)
		};

		private static readonly List<Pair<float, Color>> VisibilityColor = new List<Pair<float, Color>>
		{
			new Pair<float, Color>(0f, Color.white),
			new Pair<float, Color>(0.01f, Color.green),
			new Pair<float, Color>(0.2f, Color.green),
			new Pair<float, Color>(1f, Color.white),
			new Pair<float, Color>(1.2f, Color.red)
		};

		private static List<TransferableUIUtility.ExtraInfo> tmpInfo = new List<TransferableUIUtility.ExtraInfo>();

		public static void CreateCaravanTransferableWidgets(List<TransferableOneWay> transferables, out TransferableOneWayWidget pawnsTransfer, out TransferableOneWayWidget itemsTransfer, string thingCountTip, IgnorePawnsInventoryMode ignorePawnInventoryMass, Func<float> availableMassGetter, bool ignoreSpawnedCorpsesGearAndInventoryMass, int tile, bool playerPawnsReadOnly = false)
		{
			IEnumerable<TransferableOneWay> transferables2 = null;
			string sourceLabel = null;
			string destinationLabel = null;
			bool drawMass = true;
			bool includePawnsMassInMassUsage = false;
			bool ignoreSpawnedCorpseGearAndInventoryMass = ignoreSpawnedCorpsesGearAndInventoryMass;
			bool playerPawnsReadOnly2 = playerPawnsReadOnly;
			pawnsTransfer = new TransferableOneWayWidget(transferables2, sourceLabel, destinationLabel, thingCountTip, drawMass, ignorePawnInventoryMass, includePawnsMassInMassUsage, availableMassGetter, 0f, ignoreSpawnedCorpseGearAndInventoryMass, tile, drawMarketValue: true, drawEquippedWeapon: true, drawNutritionEatenPerDay: true, drawItemNutrition: false, drawForagedFoodPerDay: true, drawDaysUntilRot: false, playerPawnsReadOnly2);
			AddPawnsSections(pawnsTransfer, transferables);
			transferables2 = from x in transferables
			where x.ThingDef.category != ThingCategory.Pawn
			select x;
			string sourceLabel2 = null;
			destinationLabel = null;
			playerPawnsReadOnly2 = true;
			ignoreSpawnedCorpseGearAndInventoryMass = false;
			includePawnsMassInMassUsage = ignoreSpawnedCorpsesGearAndInventoryMass;
			itemsTransfer = new TransferableOneWayWidget(transferables2, sourceLabel2, destinationLabel, thingCountTip, playerPawnsReadOnly2, ignorePawnInventoryMass, ignoreSpawnedCorpseGearAndInventoryMass, availableMassGetter, 0f, includePawnsMassInMassUsage, tile, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);
		}

		public static void AddPawnsSections(TransferableOneWayWidget widget, List<TransferableOneWay> transferables)
		{
			IEnumerable<TransferableOneWay> source = from x in transferables
			where x.ThingDef.category == ThingCategory.Pawn
			select x;
			widget.AddSection("ColonistsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).IsFreeColonist
			select x);
			widget.AddSection("PrisonersSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).IsPrisoner
			select x);
			widget.AddSection("CaptureSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).Downed && CaravanUtility.ShouldAutoCapture((Pawn)x.AnyThing, Faction.OfPlayer)
			select x);
			widget.AddSection("AnimalsSection".Translate(), from x in source
			where ((Pawn)x.AnyThing).RaceProps.Animal
			select x);
		}

		private static string GetDaysWorthOfFoodLabel(Pair<float, float> daysWorthOfFood, bool multiline)
		{
			if (daysWorthOfFood.First >= 600f)
			{
				return "InfiniteDaysWorthOfFoodInfo".Translate();
			}
			string text = daysWorthOfFood.First.ToString("0.#");
			string str = (!multiline) ? " " : "\n";
			if (daysWorthOfFood.Second < 600f && daysWorthOfFood.Second < daysWorthOfFood.First)
			{
				text = text + str + "(" + "DaysWorthOfFoodInfoRot".Translate(daysWorthOfFood.Second.ToString("0.#") + ")");
			}
			return text;
		}

		private static Color GetDaysWorthOfFoodColor(Pair<float, float> daysWorthOfFood, int? ticksToArrive)
		{
			if (daysWorthOfFood.First >= 600f)
			{
				return Color.white;
			}
			float num = Mathf.Min(daysWorthOfFood.First, daysWorthOfFood.Second);
			if (ticksToArrive.HasValue)
			{
				return GenUI.LerpColor(DaysWorthOfFoodKnownRouteColor, num / ((float)ticksToArrive.Value / 60000f));
			}
			return GenUI.LerpColor(DaysWorthOfFoodColor, num);
		}

		public static void DrawCaravanInfo(CaravanInfo info, CaravanInfo? info2, int currentTile, int? ticksToArrive, float lastMassFlashTime, Rect rect, bool lerpMassColor = true, string extraDaysWorthOfFoodTipInfo = null, bool multiline = false)
		{
			tmpInfo.Clear();
			string text = info.massUsage.ToStringEnsureThreshold(info.massCapacity, 0) + " / " + info.massCapacity.ToString("F0") + " " + "kg".Translate();
			object obj2;
			if (info2.HasValue)
			{
				string[] obj = new string[5];
				CaravanInfo value = info2.Value;
				float massUsage = value.massUsage;
				CaravanInfo value2 = info2.Value;
				obj[0] = massUsage.ToStringEnsureThreshold(value2.massCapacity, 0);
				obj[1] = " / ";
				CaravanInfo value3 = info2.Value;
				obj[2] = value3.massCapacity.ToString("F0");
				obj[3] = " ";
				obj[4] = "kg".Translate();
				obj2 = string.Concat(obj);
			}
			else
			{
				obj2 = null;
			}
			string text2 = (string)obj2;
			List<TransferableUIUtility.ExtraInfo> list = tmpInfo;
			string key = "Mass".Translate();
			string value4 = text;
			Color massColor = GetMassColor(info.massUsage, info.massCapacity, lerpMassColor);
			float massUsage2 = info.massUsage;
			float massCapacity = info.massCapacity;
			string massCapacityExplanation = info.massCapacityExplanation;
			float? massUsage3;
			if (info2.HasValue)
			{
				CaravanInfo value5 = info2.Value;
				massUsage3 = value5.massUsage;
			}
			else
			{
				massUsage3 = null;
			}
			float? massCapacity2;
			if (info2.HasValue)
			{
				CaravanInfo value6 = info2.Value;
				massCapacity2 = value6.massCapacity;
			}
			else
			{
				massCapacity2 = null;
			}
			object massCapacity2Explanation;
			if (info2.HasValue)
			{
				CaravanInfo value7 = info2.Value;
				massCapacity2Explanation = value7.massCapacityExplanation;
			}
			else
			{
				massCapacity2Explanation = null;
			}
			string massTip = GetMassTip(massUsage2, massCapacity, massCapacityExplanation, massUsage3, massCapacity2, (string)massCapacity2Explanation);
			string secondValue = text2;
			Color secondColor;
			if (info2.HasValue)
			{
				CaravanInfo value8 = info2.Value;
				float massUsage4 = value8.massUsage;
				CaravanInfo value9 = info2.Value;
				secondColor = GetMassColor(massUsage4, value9.massCapacity, lerpMassColor);
			}
			else
			{
				secondColor = Color.white;
			}
			list.Add(new TransferableUIUtility.ExtraInfo(key, value4, massColor, massTip, secondValue, secondColor, lastMassFlashTime));
			if (info.extraMassUsage != -1f)
			{
				string text3 = info.extraMassUsage.ToStringEnsureThreshold(info.extraMassCapacity, 0) + " / " + info.extraMassCapacity.ToString("F0") + " " + "kg".Translate();
				object obj4;
				if (info2.HasValue)
				{
					string[] obj3 = new string[5];
					CaravanInfo value10 = info2.Value;
					float extraMassUsage = value10.extraMassUsage;
					CaravanInfo value11 = info2.Value;
					obj3[0] = extraMassUsage.ToStringEnsureThreshold(value11.extraMassCapacity, 0);
					obj3[1] = " / ";
					CaravanInfo value12 = info2.Value;
					obj3[2] = value12.extraMassCapacity.ToString("F0");
					obj3[3] = " ";
					obj3[4] = "kg".Translate();
					obj4 = string.Concat(obj3);
				}
				else
				{
					obj4 = null;
				}
				string text4 = (string)obj4;
				List<TransferableUIUtility.ExtraInfo> list2 = tmpInfo;
				string key2 = "CaravanMass".Translate();
				string value13 = text3;
				Color massColor2 = GetMassColor(info.extraMassUsage, info.extraMassCapacity, lerpMassColor: true);
				float extraMassUsage2 = info.extraMassUsage;
				float extraMassCapacity = info.extraMassCapacity;
				string extraMassCapacityExplanation = info.extraMassCapacityExplanation;
				float? massUsage5;
				if (info2.HasValue)
				{
					CaravanInfo value14 = info2.Value;
					massUsage5 = value14.extraMassUsage;
				}
				else
				{
					massUsage5 = null;
				}
				float? massCapacity3;
				if (info2.HasValue)
				{
					CaravanInfo value15 = info2.Value;
					massCapacity3 = value15.extraMassCapacity;
				}
				else
				{
					massCapacity3 = null;
				}
				object massCapacity2Explanation2;
				if (info2.HasValue)
				{
					CaravanInfo value16 = info2.Value;
					massCapacity2Explanation2 = value16.extraMassCapacityExplanation;
				}
				else
				{
					massCapacity2Explanation2 = null;
				}
				string massTip2 = GetMassTip(extraMassUsage2, extraMassCapacity, extraMassCapacityExplanation, massUsage5, massCapacity3, (string)massCapacity2Explanation2);
				string secondValue2 = text4;
				Color secondColor2;
				if (info2.HasValue)
				{
					CaravanInfo value17 = info2.Value;
					float extraMassUsage3 = value17.extraMassUsage;
					CaravanInfo value18 = info2.Value;
					secondColor2 = GetMassColor(extraMassUsage3, value18.extraMassCapacity, lerpMassColor: true);
				}
				else
				{
					secondColor2 = Color.white;
				}
				list2.Add(new TransferableUIUtility.ExtraInfo(key2, value13, massColor2, massTip2, secondValue2, secondColor2));
			}
			string text5 = "CaravanMovementSpeedTip".Translate();
			if (!info.tilesPerDayExplanation.NullOrEmpty())
			{
				text5 = text5 + "\n\n" + info.tilesPerDayExplanation;
			}
			if (info2.HasValue)
			{
				CaravanInfo value19 = info2.Value;
				if (!value19.tilesPerDayExplanation.NullOrEmpty())
				{
					string str = text5;
					CaravanInfo value20 = info2.Value;
					text5 = str + "\n\n-----\n\n" + value20.tilesPerDayExplanation;
				}
			}
			List<TransferableUIUtility.ExtraInfo> list3 = tmpInfo;
			string key3 = "CaravanMovementSpeed".Translate();
			string value21 = info.tilesPerDay.ToString("0.#") + " " + "TilesPerDay".Translate();
			Color color = GenUI.LerpColor(TilesPerDayColor, info.tilesPerDay);
			string tip = text5;
			object secondValue3;
			if (info2.HasValue)
			{
				CaravanInfo value22 = info2.Value;
				secondValue3 = value22.tilesPerDay.ToString("0.#") + " " + "TilesPerDay".Translate();
			}
			else
			{
				secondValue3 = null;
			}
			Color secondColor3;
			if (info2.HasValue)
			{
				List<Pair<float, Color>> tilesPerDayColor = TilesPerDayColor;
				CaravanInfo value23 = info2.Value;
				secondColor3 = GenUI.LerpColor(tilesPerDayColor, value23.tilesPerDay);
			}
			else
			{
				secondColor3 = Color.white;
			}
			list3.Add(new TransferableUIUtility.ExtraInfo(key3, value21, color, tip, (string)secondValue3, secondColor3));
			List<TransferableUIUtility.ExtraInfo> list4 = tmpInfo;
			string key4 = "DaysWorthOfFood".Translate();
			string daysWorthOfFoodLabel = GetDaysWorthOfFoodLabel(info.daysWorthOfFood, multiline);
			Color daysWorthOfFoodColor = GetDaysWorthOfFoodColor(info.daysWorthOfFood, ticksToArrive);
			string tip2 = "DaysWorthOfFoodTooltip".Translate() + extraDaysWorthOfFoodTipInfo + "\n\n" + VirtualPlantsUtility.GetVirtualPlantsStatusExplanationAt(currentTile, Find.TickManager.TicksAbs);
			object secondValue4;
			if (info2.HasValue)
			{
				CaravanInfo value24 = info2.Value;
				secondValue4 = GetDaysWorthOfFoodLabel(value24.daysWorthOfFood, multiline);
			}
			else
			{
				secondValue4 = null;
			}
			Color secondColor4;
			if (info2.HasValue)
			{
				CaravanInfo value25 = info2.Value;
				secondColor4 = GetDaysWorthOfFoodColor(value25.daysWorthOfFood, ticksToArrive);
			}
			else
			{
				secondColor4 = Color.white;
			}
			list4.Add(new TransferableUIUtility.ExtraInfo(key4, daysWorthOfFoodLabel, daysWorthOfFoodColor, tip2, (string)secondValue4, secondColor4));
			string text6 = info.foragedFoodPerDay.Second.ToString("0.#");
			object obj5;
			if (info2.HasValue)
			{
				CaravanInfo value26 = info2.Value;
				obj5 = value26.foragedFoodPerDay.Second.ToString("0.#");
			}
			else
			{
				obj5 = null;
			}
			string text7 = (string)obj5;
			string str2 = "ForagedFoodPerDayTip".Translate();
			str2 = str2 + "\n\n" + info.foragedFoodPerDayExplanation;
			if (info2.HasValue)
			{
				string str3 = str2;
				CaravanInfo value27 = info2.Value;
				str2 = str3 + "\n\n-----\n\n" + value27.foragedFoodPerDayExplanation;
			}
			if (info.foragedFoodPerDay.Second > 0f)
			{
				goto IL_06ad;
			}
			if (info2.HasValue)
			{
				CaravanInfo value28 = info2.Value;
				if (value28.foragedFoodPerDay.Second > 0f)
				{
					goto IL_06ad;
				}
			}
			goto IL_076b;
			IL_06ad:
			string text8 = (!multiline) ? " " : "\n";
			if (!info2.HasValue)
			{
				string text9 = text6;
				text6 = text9 + text8 + "(" + info.foragedFoodPerDay.First.label + ")";
			}
			else
			{
				string text9 = text7;
				string[] obj6 = new string[5]
				{
					text9,
					text8,
					"(",
					null,
					null
				};
				CaravanInfo value29 = info2.Value;
				obj6[3] = value29.foragedFoodPerDay.First.label.Truncate(50f);
				obj6[4] = ")";
				text7 = string.Concat(obj6);
			}
			goto IL_076b;
			IL_076b:
			tmpInfo.Add(new TransferableUIUtility.ExtraInfo("ForagedFoodPerDay".Translate(), text6, Color.white, str2, text7, Color.white));
			string text10 = "CaravanVisibilityTip".Translate();
			if (!info.visibilityExplanation.NullOrEmpty())
			{
				text10 = text10 + "\n\n" + info.visibilityExplanation;
			}
			if (info2.HasValue)
			{
				CaravanInfo value30 = info2.Value;
				if (!value30.visibilityExplanation.NullOrEmpty())
				{
					string str4 = text10;
					CaravanInfo value31 = info2.Value;
					text10 = str4 + "\n\n-----\n\n" + value31.visibilityExplanation;
				}
			}
			List<TransferableUIUtility.ExtraInfo> list5 = tmpInfo;
			string key5 = "Visibility".Translate();
			string value32 = info.visibility.ToStringPercent();
			Color color2 = GenUI.LerpColor(VisibilityColor, info.visibility);
			string tip3 = text10;
			object secondValue5;
			if (info2.HasValue)
			{
				CaravanInfo value33 = info2.Value;
				secondValue5 = value33.visibility.ToStringPercent();
			}
			else
			{
				secondValue5 = null;
			}
			Color secondColor5;
			if (info2.HasValue)
			{
				List<Pair<float, Color>> visibilityColor = VisibilityColor;
				CaravanInfo value34 = info2.Value;
				secondColor5 = GenUI.LerpColor(visibilityColor, value34.visibility);
			}
			else
			{
				secondColor5 = Color.white;
			}
			list5.Add(new TransferableUIUtility.ExtraInfo(key5, value32, color2, tip3, (string)secondValue5, secondColor5));
			TransferableUIUtility.DrawExtraInfo(tmpInfo, rect);
		}

		private static Color GetMassColor(float massUsage, float massCapacity, bool lerpMassColor)
		{
			if (massCapacity == 0f)
			{
				return Color.white;
			}
			if (massUsage > massCapacity)
			{
				return Color.red;
			}
			if (lerpMassColor)
			{
				return GenUI.LerpColor(MassColor, massUsage / massCapacity);
			}
			return Color.white;
		}

		private static string GetMassTip(float massUsage, float massCapacity, string massCapacityExplanation, float? massUsage2, float? massCapacity2, string massCapacity2Explanation)
		{
			string text = "MassCarriedSimple".Translate() + ": " + massUsage.ToStringEnsureThreshold(massCapacity, 2) + " " + "kg".Translate() + "\n" + "MassCapacity".Translate() + ": " + massCapacity.ToString("F2") + " " + "kg".Translate();
			if (massUsage2.HasValue)
			{
				string text2 = text;
				text = text2 + "\n <-> \n" + "MassCarriedSimple".Translate() + ": " + massUsage2.Value.ToStringEnsureThreshold(massCapacity2.Value, 2) + " " + "kg".Translate() + "\n" + "MassCapacity".Translate() + ": " + massCapacity2.Value.ToString("F2") + " " + "kg".Translate();
			}
			text = text + "\n\n" + "CaravanMassUsageTooltip".Translate();
			if (!massCapacityExplanation.NullOrEmpty())
			{
				string text2 = text;
				text = text2 + "\n\n" + "MassCapacity".Translate() + ":\n" + massCapacityExplanation;
			}
			if (!massCapacity2Explanation.NullOrEmpty())
			{
				string text2 = text;
				text = text2 + "\n\n-----\n\n" + "MassCapacity".Translate() + ":\n" + massCapacity2Explanation;
			}
			return text;
		}
	}
}
