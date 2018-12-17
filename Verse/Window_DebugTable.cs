using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class Window_DebugTable : Window
	{
		private enum SortMode
		{
			Off,
			Ascending,
			Descending
		}

		private string[,] tableRaw;

		private Vector2 scrollPosition = Vector2.zero;

		private string[,] tableSorted;

		private List<float> colWidths = new List<float>();

		private List<float> rowHeights = new List<float>();

		private int sortColumn = -1;

		private SortMode sortMode;

		private bool[] colVisible;

		private const float ColExtraWidth = 2f;

		private const float RowExtraHeight = 2f;

		private const float HiddenColumnWidth = 10f;

		public override Vector2 InitialSize => new Vector2((float)UI.screenWidth, (float)UI.screenHeight);

		public Window_DebugTable(string[,] tables)
		{
			tableRaw = tables;
			colVisible = new bool[tableRaw.GetLength(0)];
			for (int i = 0; i < colVisible.Length; i++)
			{
				colVisible[i] = true;
			}
			doCloseButton = true;
			doCloseX = true;
			Text.Font = GameFont.Tiny;
			BuildTableSorted();
		}

		private void BuildTableSorted()
		{
			if (sortMode == SortMode.Off)
			{
				tableSorted = tableRaw;
			}
			else
			{
				List<List<string>> list = new List<List<string>>();
				for (int i = 1; i < tableRaw.GetLength(1); i++)
				{
					list.Add(new List<string>());
					for (int j = 0; j < tableRaw.GetLength(0); j++)
					{
						list[i - 1].Add(tableRaw[j, i]);
					}
				}
				NumericStringComparer comparer = new NumericStringComparer();
				switch (sortMode)
				{
				case SortMode.Ascending:
					list = list.OrderBy((List<string> x) => x[sortColumn], comparer).ToList();
					break;
				case SortMode.Descending:
					list = list.OrderByDescending((List<string> x) => x[sortColumn], comparer).ToList();
					break;
				case SortMode.Off:
					throw new Exception();
				}
				tableSorted = new string[tableRaw.GetLength(0), tableRaw.GetLength(1)];
				for (int k = 0; k < tableRaw.GetLength(1); k++)
				{
					for (int l = 0; l < tableRaw.GetLength(0); l++)
					{
						if (k == 0)
						{
							string[,] array = tableSorted;
							int num = l;
							int num2 = k;
							string text = tableRaw[l, k];
							array[num, num2] = text;
						}
						else
						{
							string[,] array2 = tableSorted;
							int num3 = l;
							int num4 = k;
							string text2 = list[k - 1][l];
							array2[num3, num4] = text2;
						}
					}
				}
			}
			colWidths.Clear();
			for (int m = 0; m < tableRaw.GetLength(0); m++)
			{
				float item;
				if (colVisible[m])
				{
					float num5 = 0f;
					for (int n = 0; n < tableRaw.GetLength(1); n++)
					{
						string text3 = tableRaw[m, n];
						Vector2 vector = Text.CalcSize(text3);
						float x2 = vector.x;
						if (x2 > num5)
						{
							num5 = x2;
						}
					}
					item = num5 + 2f;
				}
				else
				{
					item = 10f;
				}
				colWidths.Add(item);
			}
			rowHeights.Clear();
			for (int num6 = 0; num6 < tableSorted.GetLength(1); num6++)
			{
				float num7 = 0f;
				for (int num8 = 0; num8 < tableSorted.GetLength(0); num8++)
				{
					string text4 = tableSorted[num8, num6];
					Vector2 vector2 = Text.CalcSize(text4);
					float y = vector2.y;
					if (y > num7)
					{
						num7 = y;
					}
				}
				rowHeights.Add(num7 + 2f);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Tiny;
			inRect.yMax -= 40f;
			Widgets.BeginScrollView(viewRect: new Rect(0f, 0f, colWidths.Sum(), rowHeights.Sum()), outRect: inRect, scrollPosition: ref scrollPosition);
			float num = 0f;
			for (int i = 0; i < tableSorted.GetLength(0); i++)
			{
				float num2 = 0f;
				for (int j = 0; j < tableSorted.GetLength(1); j++)
				{
					Rect rect = new Rect(num, num2, colWidths[i], rowHeights[j]);
					Rect rect2 = rect;
					rect2.xMin -= 999f;
					rect2.xMax += 999f;
					if (Mouse.IsOver(rect2) || i % 2 == 0)
					{
						Widgets.DrawHighlight(rect);
					}
					if (j == 0 && Mouse.IsOver(rect))
					{
						rect.x += 2f;
						rect.y += 2f;
					}
					if (i == 0 || colVisible[i])
					{
						Widgets.Label(rect, tableSorted[i, j]);
					}
					if (j == 0)
					{
						MouseoverSounds.DoRegion(rect);
						if (Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown)
						{
							if (Event.current.button == 0)
							{
								if (i != sortColumn)
								{
									sortMode = SortMode.Off;
								}
								switch (sortMode)
								{
								case SortMode.Off:
									sortMode = SortMode.Descending;
									sortColumn = i;
									SoundDefOf.Tick_High.PlayOneShotOnCamera();
									break;
								case SortMode.Descending:
									sortMode = SortMode.Ascending;
									sortColumn = i;
									SoundDefOf.Tick_Low.PlayOneShotOnCamera();
									break;
								case SortMode.Ascending:
									sortMode = SortMode.Off;
									sortColumn = -1;
									SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
									break;
								}
								BuildTableSorted();
							}
							else if (Event.current.button == 1)
							{
								colVisible[i] = !colVisible[i];
								SoundDefOf.Crunch.PlayOneShotOnCamera();
								BuildTableSorted();
							}
							Event.current.Use();
						}
					}
					num2 += rowHeights[j];
				}
				num += colWidths[i];
			}
			Widgets.EndScrollView();
			Rect butRect = new Rect(inRect.x + inRect.width - 44f, inRect.y + 4f, 18f, 24f);
			if (Widgets.ButtonImage(butRect, TexButton.Copy))
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int k = 0; k < tableSorted.GetLength(1); k++)
				{
					for (int l = 0; l < tableSorted.GetLength(0); l++)
					{
						if (l != 0)
						{
							stringBuilder.Append(",");
						}
						stringBuilder.Append(tableSorted[l, k]);
					}
					stringBuilder.Append("\n");
				}
				GUIUtility.systemCopyBuffer = stringBuilder.ToString();
			}
		}
	}
}
