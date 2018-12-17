using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ColonistBarDrawLocsFinder
	{
		private List<int> entriesInGroup = new List<int>();

		private List<int> horizontalSlotsPerGroup = new List<int>();

		private const float MarginTop = 21f;

		private ColonistBar ColonistBar => Find.ColonistBar;

		private static float MaxColonistBarWidth => (float)UI.screenWidth - 520f;

		public void CalculateDrawLocs(List<Vector2> outDrawLocs, out float scale)
		{
			if (ColonistBar.Entries.Count == 0)
			{
				outDrawLocs.Clear();
				scale = 1f;
			}
			else
			{
				CalculateColonistsInGroup();
				scale = FindBestScale(out bool onlyOneRow, out int maxPerGlobalRow);
				CalculateDrawLocs(outDrawLocs, scale, onlyOneRow, maxPerGlobalRow);
			}
		}

		private void CalculateColonistsInGroup()
		{
			entriesInGroup.Clear();
			List<ColonistBar.Entry> entries = ColonistBar.Entries;
			int num = CalculateGroupsCount();
			for (int i = 0; i < num; i++)
			{
				entriesInGroup.Add(0);
			}
			for (int j = 0; j < entries.Count; j++)
			{
				List<int> list;
				List<int> list2 = list = entriesInGroup;
				ColonistBar.Entry entry = entries[j];
				int group;
				list2[group = entry.group] = list[group] + 1;
			}
		}

		private int CalculateGroupsCount()
		{
			List<ColonistBar.Entry> entries = ColonistBar.Entries;
			int num = -1;
			int num2 = 0;
			for (int i = 0; i < entries.Count; i++)
			{
				int num3 = num;
				ColonistBar.Entry entry = entries[i];
				if (num3 != entry.group)
				{
					num2++;
					ColonistBar.Entry entry2 = entries[i];
					num = entry2.group;
				}
			}
			return num2;
		}

		private float FindBestScale(out bool onlyOneRow, out int maxPerGlobalRow)
		{
			float num = 1f;
			List<ColonistBar.Entry> entries = ColonistBar.Entries;
			int num2 = CalculateGroupsCount();
			while (true)
			{
				Vector2 baseSize = ColonistBar.BaseSize;
				float num3 = (baseSize.x + 24f) * num;
				float num4 = MaxColonistBarWidth - (float)(num2 - 1) * 25f * num;
				maxPerGlobalRow = Mathf.FloorToInt(num4 / num3);
				onlyOneRow = true;
				if (TryDistributeHorizontalSlotsBetweenGroups(maxPerGlobalRow))
				{
					int allowedRowsCountForScale = GetAllowedRowsCountForScale(num);
					bool flag = true;
					int num5 = -1;
					for (int i = 0; i < entries.Count; i++)
					{
						int num6 = num5;
						ColonistBar.Entry entry = entries[i];
						if (num6 != entry.group)
						{
							ColonistBar.Entry entry2 = entries[i];
							num5 = entry2.group;
							List<int> list = entriesInGroup;
							ColonistBar.Entry entry3 = entries[i];
							float num7 = (float)list[entry3.group];
							List<int> list2 = horizontalSlotsPerGroup;
							ColonistBar.Entry entry4 = entries[i];
							int num8 = Mathf.CeilToInt(num7 / (float)list2[entry4.group]);
							if (num8 > 1)
							{
								onlyOneRow = false;
							}
							if (num8 > allowedRowsCountForScale)
							{
								flag = false;
								break;
							}
						}
					}
					if (flag)
					{
						break;
					}
				}
				num *= 0.95f;
			}
			return num;
		}

		private bool TryDistributeHorizontalSlotsBetweenGroups(int maxPerGlobalRow)
		{
			int num = CalculateGroupsCount();
			horizontalSlotsPerGroup.Clear();
			for (int j = 0; j < num; j++)
			{
				horizontalSlotsPerGroup.Add(0);
			}
			GenMath.DHondtDistribution(horizontalSlotsPerGroup, (int i) => (float)entriesInGroup[i], maxPerGlobalRow);
			for (int k = 0; k < horizontalSlotsPerGroup.Count; k++)
			{
				if (horizontalSlotsPerGroup[k] == 0)
				{
					int num2 = horizontalSlotsPerGroup.Max();
					if (num2 <= 1)
					{
						return false;
					}
					int num3 = horizontalSlotsPerGroup.IndexOf(num2);
					int index;
					List<int> list;
					(list = horizontalSlotsPerGroup)[index = num3] = list[index] - 1;
					int index2;
					(list = horizontalSlotsPerGroup)[index2 = k] = list[index2] + 1;
				}
			}
			return true;
		}

		private static int GetAllowedRowsCountForScale(float scale)
		{
			if (scale > 0.58f)
			{
				return 1;
			}
			if (scale > 0.42f)
			{
				return 2;
			}
			return 3;
		}

		private void CalculateDrawLocs(List<Vector2> outDrawLocs, float scale, bool onlyOneRow, int maxPerGlobalRow)
		{
			outDrawLocs.Clear();
			int num = maxPerGlobalRow;
			if (onlyOneRow)
			{
				for (int i = 0; i < horizontalSlotsPerGroup.Count; i++)
				{
					horizontalSlotsPerGroup[i] = Mathf.Min(horizontalSlotsPerGroup[i], entriesInGroup[i]);
				}
				num = ColonistBar.Entries.Count;
			}
			int num2 = CalculateGroupsCount();
			Vector2 baseSize = ColonistBar.BaseSize;
			float num3 = (baseSize.x + 24f) * scale;
			float num4 = (float)num * num3 + (float)(num2 - 1) * 25f * scale;
			List<ColonistBar.Entry> entries = ColonistBar.Entries;
			int num5 = -1;
			int num6 = -1;
			float num7 = ((float)UI.screenWidth - num4) / 2f;
			for (int j = 0; j < entries.Count; j++)
			{
				int num8 = num5;
				ColonistBar.Entry entry = entries[j];
				if (num8 != entry.group)
				{
					if (num5 >= 0)
					{
						num7 += 25f * scale;
						float num9 = num7;
						float num10 = (float)horizontalSlotsPerGroup[num5] * scale;
						Vector2 baseSize2 = ColonistBar.BaseSize;
						num7 = num9 + num10 * (baseSize2.x + 24f);
					}
					num6 = 0;
					ColonistBar.Entry entry2 = entries[j];
					num5 = entry2.group;
				}
				else
				{
					num6++;
				}
				float groupStartX = num7;
				ColonistBar.Entry entry3 = entries[j];
				Vector2 drawLoc = GetDrawLoc(groupStartX, 21f, entry3.group, num6, scale);
				outDrawLocs.Add(drawLoc);
			}
		}

		private Vector2 GetDrawLoc(float groupStartX, float groupStartY, int group, int numInGroup, float scale)
		{
			float num = (float)(numInGroup % horizontalSlotsPerGroup[group]) * scale;
			Vector2 baseSize = ColonistBar.BaseSize;
			float num2 = groupStartX + num * (baseSize.x + 24f);
			float num3 = (float)(numInGroup / horizontalSlotsPerGroup[group]) * scale;
			Vector2 baseSize2 = ColonistBar.BaseSize;
			float y = groupStartY + num3 * (baseSize2.y + 32f);
			if (numInGroup >= entriesInGroup[group] - entriesInGroup[group] % horizontalSlotsPerGroup[group])
			{
				int num4 = horizontalSlotsPerGroup[group] - entriesInGroup[group] % horizontalSlotsPerGroup[group];
				float num5 = num2;
				float num6 = (float)num4 * scale;
				Vector2 baseSize3 = ColonistBar.BaseSize;
				num2 = num5 + num6 * (baseSize3.x + 24f) * 0.5f;
			}
			return new Vector2(num2, y);
		}
	}
}
