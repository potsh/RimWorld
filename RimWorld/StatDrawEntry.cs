using System;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class StatDrawEntry
	{
		public StatCategoryDef category;

		private int displayOrderWithinCategory;

		public StatDef stat;

		private float value;

		public StatRequest optionalReq;

		public bool hasOptionalReq;

		private string labelInt;

		private string valueStringInt;

		public string overrideReportText;

		private ToStringNumberSense numberSense;

		public bool ShouldDisplay
		{
			get
			{
				if (stat != null)
				{
					return !Mathf.Approximately(value, stat.hideAtValue);
				}
				return true;
			}
		}

		public string LabelCap
		{
			get
			{
				if (labelInt != null)
				{
					return labelInt.CapitalizeFirst();
				}
				return stat.LabelCap;
			}
		}

		public string ValueString
		{
			get
			{
				if (numberSense == ToStringNumberSense.Factor)
				{
					return value.ToStringByStyle(ToStringStyle.PercentZero);
				}
				if (valueStringInt == null)
				{
					return stat.Worker.GetStatDrawEntryLabel(stat, value, numberSense, optionalReq);
				}
				return valueStringInt;
			}
		}

		public int DisplayPriorityWithinCategory
		{
			get
			{
				if (stat != null)
				{
					return stat.displayPriorityInCategory;
				}
				return displayOrderWithinCategory;
			}
		}

		public StatDrawEntry(StatCategoryDef category, StatDef stat, float value, StatRequest optionalReq, ToStringNumberSense numberSense = ToStringNumberSense.Undefined)
		{
			this.category = category;
			this.stat = stat;
			labelInt = null;
			this.value = value;
			valueStringInt = null;
			displayOrderWithinCategory = 0;
			this.optionalReq = optionalReq;
			hasOptionalReq = true;
			if (numberSense == ToStringNumberSense.Undefined)
			{
				this.numberSense = stat.toStringNumberSense;
			}
			else
			{
				this.numberSense = numberSense;
			}
		}

		public StatDrawEntry(StatCategoryDef category, string label, string valueString, int displayPriorityWithinCategory = 0, string overrideReportText = "")
		{
			this.category = category;
			stat = null;
			labelInt = label;
			value = 0f;
			valueStringInt = valueString;
			displayOrderWithinCategory = displayPriorityWithinCategory;
			numberSense = ToStringNumberSense.Absolute;
			this.overrideReportText = overrideReportText;
		}

		public StatDrawEntry(StatCategoryDef category, StatDef stat)
		{
			this.category = category;
			this.stat = stat;
			labelInt = null;
			value = 0f;
			valueStringInt = "-";
			displayOrderWithinCategory = 0;
			numberSense = ToStringNumberSense.Undefined;
		}

		public string GetExplanationText(StatRequest optionalReq)
		{
			if (!overrideReportText.NullOrEmpty())
			{
				return overrideReportText;
			}
			if (stat == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(stat.LabelCap);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(stat.description);
			stringBuilder.AppendLine();
			if (!optionalReq.Empty)
			{
				stringBuilder.AppendLine(stat.Worker.GetExplanationFull(optionalReq, numberSense, value));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public float Draw(float x, float y, float width, bool selected, Action clickedCallback, Action mousedOverCallback, Vector2 scrollPosition, Rect scrollOutRect)
		{
			float num = width * 0.45f;
			Rect rect = new Rect(8f, y, width, Text.CalcHeight(ValueString, num));
			if (!(y - scrollPosition.y + rect.height < 0f) && !(y - scrollPosition.y > scrollOutRect.height))
			{
				if (selected)
				{
					Widgets.DrawHighlightSelected(rect);
				}
				else if (Mouse.IsOver(rect))
				{
					Widgets.DrawHighlight(rect);
				}
				Rect rect2 = rect;
				rect2.width -= num;
				Widgets.Label(rect2, LabelCap);
				Rect rect3 = rect;
				rect3.x = rect2.xMax;
				rect3.width = num;
				Widgets.Label(rect3, ValueString);
				if (stat != null)
				{
					StatDef localStat = stat;
					TooltipHandler.TipRegion(rect, new TipSignal(() => localStat.LabelCap + ": " + localStat.description, stat.GetHashCode()));
				}
				if (Widgets.ButtonInvisible(rect))
				{
					clickedCallback();
				}
				if (Mouse.IsOver(rect))
				{
					mousedOverCallback();
				}
			}
			return rect.height;
		}

		public override string ToString()
		{
			return "(" + LabelCap + ": " + ValueString + ")";
		}
	}
}
