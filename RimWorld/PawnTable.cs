using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnTable
	{
		private PawnTableDef def;

		private Func<IEnumerable<Pawn>> pawnsGetter;

		private int minTableWidth;

		private int maxTableWidth;

		private int minTableHeight;

		private int maxTableHeight;

		private Vector2 fixedSize;

		private bool hasFixedSize;

		private bool dirty;

		private List<bool> columnAtMaxWidth = new List<bool>();

		private List<bool> columnAtOptimalWidth = new List<bool>();

		private Vector2 scrollPosition;

		private PawnColumnDef sortByColumn;

		private bool sortDescending;

		private Vector2 cachedSize;

		private List<Pawn> cachedPawns = new List<Pawn>();

		private List<float> cachedColumnWidths = new List<float>();

		private List<float> cachedRowHeights = new List<float>();

		private float cachedHeaderHeight;

		private float cachedHeightNoScrollbar;

		public List<PawnColumnDef> ColumnsListForReading => def.columns;

		public PawnColumnDef SortingBy => sortByColumn;

		public bool SortingDescending => SortingBy != null && sortDescending;

		public Vector2 Size
		{
			get
			{
				RecacheIfDirty();
				return cachedSize;
			}
		}

		public float HeightNoScrollbar
		{
			get
			{
				RecacheIfDirty();
				return cachedHeightNoScrollbar;
			}
		}

		public float HeaderHeight
		{
			get
			{
				RecacheIfDirty();
				return cachedHeaderHeight;
			}
		}

		public List<Pawn> PawnsListForReading
		{
			get
			{
				RecacheIfDirty();
				return cachedPawns;
			}
		}

		public PawnTable(PawnTableDef def, Func<IEnumerable<Pawn>> pawnsGetter, int uiWidth, int uiHeight)
		{
			this.def = def;
			this.pawnsGetter = pawnsGetter;
			SetMinMaxSize(def.minWidth, uiWidth, 0, uiHeight);
			SetDirty();
		}

		public void PawnTableOnGUI(Vector2 position)
		{
			if (Event.current.type != EventType.Layout)
			{
				RecacheIfDirty();
				float num = cachedSize.x - 16f;
				int num2 = 0;
				for (int i = 0; i < def.columns.Count; i++)
				{
					int num3 = (i != def.columns.Count - 1) ? ((int)cachedColumnWidths[i]) : ((int)(num - (float)num2));
					Rect rect = new Rect((float)((int)position.x + num2), (float)(int)position.y, (float)num3, (float)(int)cachedHeaderHeight);
					def.columns[i].Worker.DoHeader(rect, this);
					num2 += num3;
				}
				Rect outRect = new Rect((float)(int)position.x, (float)((int)position.y + (int)cachedHeaderHeight), (float)(int)cachedSize.x, (float)((int)cachedSize.y - (int)cachedHeaderHeight));
				Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, (float)((int)cachedHeightNoScrollbar - (int)cachedHeaderHeight));
				Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
				int num4 = 0;
				for (int j = 0; j < cachedPawns.Count; j++)
				{
					num2 = 0;
					if (!((float)num4 - scrollPosition.y + (float)(int)cachedRowHeights[j] < 0f) && !((float)num4 - scrollPosition.y > outRect.height))
					{
						GUI.color = new Color(1f, 1f, 1f, 0.2f);
						Widgets.DrawLineHorizontal(0f, (float)num4, viewRect.width);
						GUI.color = Color.white;
						Rect rect2 = new Rect(0f, (float)num4, viewRect.width, (float)(int)cachedRowHeights[j]);
						if (Mouse.IsOver(rect2))
						{
							GUI.DrawTexture(rect2, TexUI.HighlightTex);
						}
						for (int k = 0; k < def.columns.Count; k++)
						{
							int num5 = (k != def.columns.Count - 1) ? ((int)cachedColumnWidths[k]) : ((int)(num - (float)num2));
							Rect rect3 = new Rect((float)num2, (float)num4, (float)num5, (float)(int)cachedRowHeights[j]);
							def.columns[k].Worker.DoCell(rect3, cachedPawns[j], this);
							num2 += num5;
						}
						if (cachedPawns[j].Downed)
						{
							GUI.color = new Color(1f, 0f, 0f, 0.5f);
							Vector2 center = rect2.center;
							Widgets.DrawLineHorizontal(0f, center.y, viewRect.width);
							GUI.color = Color.white;
						}
					}
					num4 += (int)cachedRowHeights[j];
				}
				Widgets.EndScrollView();
			}
		}

		public void SetDirty()
		{
			dirty = true;
		}

		public void SetMinMaxSize(int minTableWidth, int maxTableWidth, int minTableHeight, int maxTableHeight)
		{
			this.minTableWidth = minTableWidth;
			this.maxTableWidth = maxTableWidth;
			this.minTableHeight = minTableHeight;
			this.maxTableHeight = maxTableHeight;
			hasFixedSize = false;
			SetDirty();
		}

		public void SetFixedSize(Vector2 size)
		{
			fixedSize = size;
			hasFixedSize = true;
			SetDirty();
		}

		public void SortBy(PawnColumnDef column, bool descending)
		{
			sortByColumn = column;
			sortDescending = descending;
			SetDirty();
		}

		private void RecacheIfDirty()
		{
			if (dirty)
			{
				dirty = false;
				RecachePawns();
				RecacheRowHeights();
				cachedHeaderHeight = CalculateHeaderHeight();
				cachedHeightNoScrollbar = CalculateTotalRequiredHeight();
				RecacheSize();
				RecacheColumnWidths();
			}
		}

		private void RecachePawns()
		{
			cachedPawns.Clear();
			cachedPawns.AddRange(pawnsGetter());
			cachedPawns = LabelSortFunction(cachedPawns).ToList();
			if (sortByColumn != null)
			{
				if (sortDescending)
				{
					List<Pawn> list = cachedPawns;
					PawnColumnWorker worker = sortByColumn.Worker;
					list.SortStable(worker.Compare);
				}
				else
				{
					cachedPawns.SortStable((Pawn a, Pawn b) => sortByColumn.Worker.Compare(b, a));
				}
			}
			cachedPawns = PrimarySortFunction(cachedPawns).ToList();
		}

		protected virtual IEnumerable<Pawn> LabelSortFunction(IEnumerable<Pawn> input)
		{
			return from p in input
			orderby p.Label
			select p;
		}

		protected virtual IEnumerable<Pawn> PrimarySortFunction(IEnumerable<Pawn> input)
		{
			return input;
		}

		private void RecacheColumnWidths()
		{
			float num = cachedSize.x - 16f;
			float minWidthsSum = 0f;
			RecacheColumnWidths_StartWithMinWidths(out minWidthsSum);
			if (minWidthsSum != num)
			{
				if (minWidthsSum > num)
				{
					SubtractProportionally(minWidthsSum - num, minWidthsSum);
				}
				else
				{
					RecacheColumnWidths_DistributeUntilOptimal(num, ref minWidthsSum, out bool noMoreFreeSpace);
					if (!noMoreFreeSpace)
					{
						RecacheColumnWidths_DistributeAboveOptimal(num, ref minWidthsSum);
					}
				}
			}
		}

		private void RecacheColumnWidths_StartWithMinWidths(out float minWidthsSum)
		{
			minWidthsSum = 0f;
			cachedColumnWidths.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				float minWidth = GetMinWidth(def.columns[i]);
				cachedColumnWidths.Add(minWidth);
				minWidthsSum += minWidth;
			}
		}

		private void RecacheColumnWidths_DistributeUntilOptimal(float totalAvailableSpaceForColumns, ref float usedWidth, out bool noMoreFreeSpace)
		{
			columnAtOptimalWidth.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				columnAtOptimalWidth.Add(cachedColumnWidths[i] >= GetOptimalWidth(def.columns[i]));
			}
			int num = 0;
			bool flag;
			bool flag2;
			do
			{
				num++;
				if (num >= 10000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				float num2 = -3.40282347E+38f;
				for (int j = 0; j < def.columns.Count; j++)
				{
					if (!columnAtOptimalWidth[j])
					{
						num2 = Mathf.Max(num2, (float)def.columns[j].widthPriority);
					}
				}
				float num3 = 0f;
				for (int k = 0; k < cachedColumnWidths.Count; k++)
				{
					if (!columnAtOptimalWidth[k] && (float)def.columns[k].widthPriority == num2)
					{
						num3 += GetOptimalWidth(def.columns[k]);
					}
				}
				float num4 = totalAvailableSpaceForColumns - usedWidth;
				flag = false;
				flag2 = false;
				for (int l = 0; l < cachedColumnWidths.Count; l++)
				{
					if (!columnAtOptimalWidth[l])
					{
						if ((float)def.columns[l].widthPriority != num2)
						{
							flag = true;
						}
						else
						{
							float num5 = num4 * GetOptimalWidth(def.columns[l]) / num3;
							float num6 = GetOptimalWidth(def.columns[l]) - cachedColumnWidths[l];
							if (num5 >= num6)
							{
								num5 = num6;
								columnAtOptimalWidth[l] = true;
								flag2 = true;
							}
							else
							{
								flag = true;
							}
							if (num5 > 0f)
							{
								List<float> list;
								int index;
								(list = cachedColumnWidths)[index = l] = list[index] + num5;
								usedWidth += num5;
							}
						}
					}
				}
				if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
				{
					noMoreFreeSpace = true;
					break;
				}
			}
			while (flag && flag2);
			noMoreFreeSpace = false;
		}

		private void RecacheColumnWidths_DistributeAboveOptimal(float totalAvailableSpaceForColumns, ref float usedWidth)
		{
			columnAtMaxWidth.Clear();
			for (int i = 0; i < def.columns.Count; i++)
			{
				columnAtMaxWidth.Add(cachedColumnWidths[i] >= GetMaxWidth(def.columns[i]));
			}
			int num = 0;
			bool flag;
			do
			{
				num++;
				if (num >= 10000)
				{
					Log.Error("Too many iterations.");
					return;
				}
				float num2 = 0f;
				for (int j = 0; j < def.columns.Count; j++)
				{
					if (!columnAtMaxWidth[j])
					{
						num2 += Mathf.Max(GetOptimalWidth(def.columns[j]), 1f);
					}
				}
				float num3 = totalAvailableSpaceForColumns - usedWidth;
				flag = false;
				for (int k = 0; k < def.columns.Count; k++)
				{
					if (!columnAtMaxWidth[k])
					{
						float num4 = num3 * Mathf.Max(GetOptimalWidth(def.columns[k]), 1f) / num2;
						float num5 = GetMaxWidth(def.columns[k]) - cachedColumnWidths[k];
						if (num4 >= num5)
						{
							num4 = num5;
							columnAtMaxWidth[k] = true;
						}
						else
						{
							flag = true;
						}
						if (num4 > 0f)
						{
							List<float> list;
							int index;
							(list = cachedColumnWidths)[index = k] = list[index] + num4;
							usedWidth += num4;
						}
					}
				}
				if (usedWidth >= totalAvailableSpaceForColumns - 0.1f)
				{
					return;
				}
			}
			while (flag);
			DistributeRemainingWidthProportionallyAboveMax(totalAvailableSpaceForColumns - usedWidth);
		}

		private void RecacheRowHeights()
		{
			cachedRowHeights.Clear();
			for (int i = 0; i < cachedPawns.Count; i++)
			{
				cachedRowHeights.Add(CalculateRowHeight(cachedPawns[i]));
			}
		}

		private void RecacheSize()
		{
			if (hasFixedSize)
			{
				cachedSize = fixedSize;
			}
			else
			{
				float num = 0f;
				for (int i = 0; i < def.columns.Count; i++)
				{
					if (!def.columns[i].ignoreWhenCalculatingOptimalTableSize)
					{
						num += GetOptimalWidth(def.columns[i]);
					}
				}
				float a = Mathf.Clamp(num + 16f, (float)minTableWidth, (float)maxTableWidth);
				float a2 = Mathf.Clamp(cachedHeightNoScrollbar, (float)minTableHeight, (float)maxTableHeight);
				a = Mathf.Min(a, (float)UI.screenWidth);
				a2 = Mathf.Min(a2, (float)UI.screenHeight);
				cachedSize = new Vector2(a, a2);
			}
		}

		private void SubtractProportionally(float toSubtract, float totalUsedWidth)
		{
			for (int i = 0; i < cachedColumnWidths.Count; i++)
			{
				List<float> list;
				int index;
				(list = cachedColumnWidths)[index = i] = list[index] - toSubtract * cachedColumnWidths[i] / totalUsedWidth;
			}
		}

		private void DistributeRemainingWidthProportionallyAboveMax(float toDistribute)
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num += Mathf.Max(GetOptimalWidth(def.columns[i]), 1f);
			}
			for (int j = 0; j < def.columns.Count; j++)
			{
				List<float> list;
				int index;
				(list = cachedColumnWidths)[index = j] = list[index] + toDistribute * Mathf.Max(GetOptimalWidth(def.columns[j]), 1f) / num;
			}
		}

		private float GetOptimalWidth(PawnColumnDef column)
		{
			return Mathf.Max((float)column.Worker.GetOptimalWidth(this), 0f);
		}

		private float GetMinWidth(PawnColumnDef column)
		{
			return Mathf.Max((float)column.Worker.GetMinWidth(this), 0f);
		}

		private float GetMaxWidth(PawnColumnDef column)
		{
			return Mathf.Max((float)column.Worker.GetMaxWidth(this), 0f);
		}

		private float CalculateRowHeight(Pawn pawn)
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num = Mathf.Max(num, (float)def.columns[i].Worker.GetMinCellHeight(pawn));
			}
			return num;
		}

		private float CalculateHeaderHeight()
		{
			float num = 0f;
			for (int i = 0; i < def.columns.Count; i++)
			{
				num = Mathf.Max(num, (float)def.columns[i].Worker.GetMinHeaderHeight(this));
			}
			return num;
		}

		private float CalculateTotalRequiredHeight()
		{
			float num = CalculateHeaderHeight();
			for (int i = 0; i < cachedPawns.Count; i++)
			{
				num += CalculateRowHeight(cachedPawns[i]);
			}
			return num;
		}
	}
}
