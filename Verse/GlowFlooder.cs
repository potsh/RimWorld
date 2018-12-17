using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GlowFlooder
	{
		private struct GlowFloodCell
		{
			public int intDist;

			public uint status;
		}

		private class CompareGlowFlooderLightSquares : IComparer<int>
		{
			private GlowFloodCell[] grid;

			public CompareGlowFlooderLightSquares(GlowFloodCell[] grid)
			{
				this.grid = grid;
			}

			public int Compare(int a, int b)
			{
				return grid[a].intDist.CompareTo(grid[b].intDist);
			}
		}

		private Map map;

		private GlowFloodCell[] calcGrid;

		private FastPriorityQueue<int> openSet;

		private uint statusUnseenValue;

		private uint statusOpenValue = 1u;

		private uint statusFinalizedValue = 2u;

		private int mapSizeX;

		private int mapSizeZ;

		private CompGlower glower;

		private CellIndices cellIndices;

		private Color32[] glowGrid;

		private float attenLinearSlope;

		private Thing[] blockers = new Thing[8];

		private static readonly sbyte[,] Directions = new sbyte[8, 2]
		{
			{
				0,
				-1
			},
			{
				1,
				0
			},
			{
				0,
				1
			},
			{
				-1,
				0
			},
			{
				1,
				-1
			},
			{
				1,
				1
			},
			{
				-1,
				1
			},
			{
				-1,
				-1
			}
		};

		public GlowFlooder(Map map)
		{
			this.map = map;
			IntVec3 size = map.Size;
			mapSizeX = size.x;
			IntVec3 size2 = map.Size;
			mapSizeZ = size2.z;
			calcGrid = new GlowFloodCell[mapSizeX * mapSizeZ];
			openSet = new FastPriorityQueue<int>(new CompareGlowFlooderLightSquares(calcGrid));
		}

		public void AddFloodGlowFor(CompGlower theGlower, Color32[] glowGrid)
		{
			cellIndices = map.cellIndices;
			this.glowGrid = glowGrid;
			glower = theGlower;
			attenLinearSlope = -1f / theGlower.Props.glowRadius;
			Building[] innerArray = map.edificeGrid.InnerArray;
			IntVec3 position = theGlower.parent.Position;
			int num = Mathf.RoundToInt(glower.Props.glowRadius * 100f);
			int curIndex = cellIndices.CellToIndex(position);
			int num2 = 0;
			bool flag = UnityData.isDebugBuild && DebugViewSettings.drawGlow;
			InitStatusesAndPushStartNode(ref curIndex, position);
			while (openSet.Count != 0)
			{
				curIndex = openSet.Pop();
				IntVec3 c = cellIndices.IndexToCell(curIndex);
				calcGrid[curIndex].status = statusFinalizedValue;
				SetGlowGridFromDist(curIndex);
				if (flag)
				{
					map.debugDrawer.FlashCell(c, (float)calcGrid[curIndex].intDist / 10f, calcGrid[curIndex].intDist.ToString("F2"));
					num2++;
				}
				for (int i = 0; i < 8; i++)
				{
					uint num3 = (uint)(c.x + Directions[i, 0]);
					uint num4 = (uint)(c.z + Directions[i, 1]);
					if (num3 < mapSizeX && num4 < mapSizeZ)
					{
						int x = (int)num3;
						int z = (int)num4;
						int num5 = cellIndices.CellToIndex(x, z);
						if (calcGrid[num5].status != statusFinalizedValue)
						{
							blockers[i] = innerArray[num5];
							if (blockers[i] != null)
							{
								if (blockers[i].def.blockLight)
								{
									continue;
								}
								blockers[i] = null;
							}
							int num6 = (i >= 4) ? 141 : 100;
							int num7 = calcGrid[curIndex].intDist + num6;
							if (num7 <= num)
							{
								switch (i)
								{
								case 4:
									if (blockers[0] != null && blockers[1] != null)
									{
										break;
									}
									goto default;
								case 5:
									if (blockers[1] != null && blockers[2] != null)
									{
										break;
									}
									goto default;
								case 6:
									if (blockers[2] != null && blockers[3] != null)
									{
										break;
									}
									goto default;
								case 7:
									if (blockers[0] != null && blockers[3] != null)
									{
										break;
									}
									goto default;
								default:
									if (calcGrid[num5].status <= statusUnseenValue)
									{
										calcGrid[num5].intDist = 999999;
										calcGrid[num5].status = statusOpenValue;
									}
									if (num7 < calcGrid[num5].intDist)
									{
										calcGrid[num5].intDist = num7;
										calcGrid[num5].status = statusOpenValue;
										openSet.Push(num5);
									}
									break;
								}
							}
						}
					}
				}
			}
		}

		private void InitStatusesAndPushStartNode(ref int curIndex, IntVec3 start)
		{
			statusUnseenValue += 3u;
			statusOpenValue += 3u;
			statusFinalizedValue += 3u;
			curIndex = cellIndices.CellToIndex(start);
			openSet.Clear();
			calcGrid[curIndex].intDist = 100;
			openSet.Clear();
			openSet.Push(curIndex);
		}

		private void SetGlowGridFromDist(int index)
		{
			float num = (float)calcGrid[index].intDist / 100f;
			ColorInt colB = default(ColorInt);
			if (num <= glower.Props.glowRadius)
			{
				float b = 1f / (num * num);
				float a = 1f + attenLinearSlope * num;
				float b2 = Mathf.Lerp(a, b, 0.4f);
				colB = glower.Props.glowColor * b2;
			}
			if (colB.r > 0 || colB.g > 0 || colB.b > 0)
			{
				colB.ClampToNonNegative();
				ColorInt colA = glowGrid[index].AsColorInt();
				colA += colB;
				if (num < glower.Props.overlightRadius)
				{
					colA.a = 1;
				}
				Color32 toColor = colA.ToColor32;
				glowGrid[index] = toColor;
			}
		}
	}
}
