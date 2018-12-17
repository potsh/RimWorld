using RimWorld;
using UnityEngine;

namespace Verse
{
	public sealed class MapDrawer
	{
		private Map map;

		private Section[,] sections;

		private IntVec2 SectionCount
		{
			get
			{
				IntVec2 result = default(IntVec2);
				IntVec3 size = map.Size;
				result.x = Mathf.CeilToInt((float)size.x / 17f);
				IntVec3 size2 = map.Size;
				result.z = Mathf.CeilToInt((float)size2.z / 17f);
				return result;
			}
		}

		private CellRect VisibleSections
		{
			get
			{
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				CellRect sunShadowsViewRect = GetSunShadowsViewRect(currentViewRect);
				sunShadowsViewRect.ClipInsideMap(map);
				IntVec2 intVec = SectionCoordsAt(sunShadowsViewRect.BottomLeft);
				IntVec2 intVec2 = SectionCoordsAt(sunShadowsViewRect.TopRight);
				if (intVec2.x < intVec.x || intVec2.z < intVec.z)
				{
					return CellRect.Empty;
				}
				return CellRect.FromLimits(intVec.x, intVec.z, intVec2.x, intVec2.z);
			}
		}

		public MapDrawer(Map map)
		{
			this.map = map;
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags)
		{
			bool regenAdjacentCells = (dirtyFlags & (MapMeshFlag.FogOfWar | MapMeshFlag.Buildings)) != MapMeshFlag.None;
			bool regenAdjacentSections = (dirtyFlags & MapMeshFlag.GroundGlow) != MapMeshFlag.None;
			MapMeshDirty(loc, dirtyFlags, regenAdjacentCells, regenAdjacentSections);
		}

		public void MapMeshDirty(IntVec3 loc, MapMeshFlag dirtyFlags, bool regenAdjacentCells, bool regenAdjacentSections)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				Section section = SectionAt(loc);
				section.dirtyFlags |= dirtyFlags;
				if (regenAdjacentCells)
				{
					for (int i = 0; i < 8; i++)
					{
						IntVec3 intVec = loc + GenAdj.AdjacentCells[i];
						if (intVec.InBounds(map))
						{
							SectionAt(intVec).dirtyFlags |= dirtyFlags;
						}
					}
				}
				if (regenAdjacentSections)
				{
					IntVec2 a = SectionCoordsAt(loc);
					for (int j = 0; j < 8; j++)
					{
						IntVec3 intVec2 = GenAdj.AdjacentCells[j];
						IntVec2 intVec3 = a + new IntVec2(intVec2.x, intVec2.z);
						IntVec2 sectionCount = SectionCount;
						if (intVec3.x >= 0 && intVec3.z >= 0 && intVec3.x <= sectionCount.x - 1 && intVec3.z <= sectionCount.z - 1)
						{
							Section section2 = sections[intVec3.x, intVec3.z];
							section2.dirtyFlags |= dirtyFlags;
						}
					}
				}
			}
		}

		public void MapMeshDrawerUpdate_First()
		{
			CellRect visibleSections = VisibleSections;
			bool flag = false;
			CellRect.CellRectIterator iterator = visibleSections.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				Section sect = sections[current.x, current.z];
				if (TryUpdateSection(sect))
				{
					flag = true;
				}
				iterator.MoveNext();
			}
			if (!flag)
			{
				int num = 0;
				while (true)
				{
					int num2 = num;
					IntVec2 sectionCount = SectionCount;
					if (num2 >= sectionCount.x)
					{
						break;
					}
					int num3 = 0;
					while (true)
					{
						int num4 = num3;
						IntVec2 sectionCount2 = SectionCount;
						if (num4 >= sectionCount2.z)
						{
							break;
						}
						if (TryUpdateSection(sections[num, num3]))
						{
							return;
						}
						num3++;
					}
					num++;
				}
			}
		}

		private bool TryUpdateSection(Section sect)
		{
			if (sect.dirtyFlags == MapMeshFlag.None)
			{
				return false;
			}
			for (int i = 0; i < MapMeshFlagUtility.allFlags.Count; i++)
			{
				MapMeshFlag mapMeshFlag = MapMeshFlagUtility.allFlags[i];
				if ((sect.dirtyFlags & mapMeshFlag) != 0)
				{
					sect.RegenerateLayers(mapMeshFlag);
				}
			}
			sect.dirtyFlags = MapMeshFlag.None;
			return true;
		}

		public void DrawMapMesh()
		{
			CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
			currentViewRect.minX -= 17;
			currentViewRect.minZ -= 17;
			CellRect.CellRectIterator iterator = VisibleSections.GetIterator();
			while (!iterator.Done())
			{
				IntVec3 current = iterator.Current;
				Section section = sections[current.x, current.z];
				section.DrawSection(!currentViewRect.Contains(section.botLeft));
				iterator.MoveNext();
			}
		}

		private IntVec2 SectionCoordsAt(IntVec3 loc)
		{
			return new IntVec2(Mathf.FloorToInt((float)(loc.x / 17)), Mathf.FloorToInt((float)(loc.z / 17)));
		}

		public Section SectionAt(IntVec3 loc)
		{
			IntVec2 intVec = SectionCoordsAt(loc);
			return sections[intVec.x, intVec.z];
		}

		public void RegenerateEverythingNow()
		{
			if (sections == null)
			{
				IntVec2 sectionCount = SectionCount;
				int x = sectionCount.x;
				IntVec2 sectionCount2 = SectionCount;
				sections = new Section[x, sectionCount2.z];
			}
			int num = 0;
			while (true)
			{
				int num2 = num;
				IntVec2 sectionCount3 = SectionCount;
				if (num2 >= sectionCount3.x)
				{
					break;
				}
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					IntVec2 sectionCount4 = SectionCount;
					if (num4 >= sectionCount4.z)
					{
						break;
					}
					if (sections[num, num3] == null)
					{
						Section[,] array = sections;
						int num5 = num;
						int num6 = num3;
						Section section = new Section(new IntVec3(num, 0, num3), map);
						array[num5, num6] = section;
					}
					sections[num, num3].RegenerateAllLayers();
					num3++;
				}
				num++;
			}
		}

		public void WholeMapChanged(MapMeshFlag change)
		{
			int num = 0;
			while (true)
			{
				int num2 = num;
				IntVec2 sectionCount = SectionCount;
				if (num2 >= sectionCount.x)
				{
					break;
				}
				int num3 = 0;
				while (true)
				{
					int num4 = num3;
					IntVec2 sectionCount2 = SectionCount;
					if (num4 >= sectionCount2.z)
					{
						break;
					}
					sections[num, num3].dirtyFlags |= change;
					num3++;
				}
				num++;
			}
		}

		private CellRect GetSunShadowsViewRect(CellRect rect)
		{
			GenCelestial.LightInfo lightSourceInfo = GenCelestial.GetLightSourceInfo(map, GenCelestial.LightType.Shadow);
			if (lightSourceInfo.vector.x < 0f)
			{
				rect.maxX -= Mathf.FloorToInt(lightSourceInfo.vector.x);
			}
			else
			{
				rect.minX -= Mathf.CeilToInt(lightSourceInfo.vector.x);
			}
			if (lightSourceInfo.vector.y < 0f)
			{
				rect.maxZ -= Mathf.FloorToInt(lightSourceInfo.vector.y);
			}
			else
			{
				rect.minZ -= Mathf.CeilToInt(lightSourceInfo.vector.y);
			}
			return rect;
		}
	}
}
