using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public sealed class Region
	{
		public RegionType type = RegionType.Normal;

		public int id = -1;

		public sbyte mapIndex = -1;

		private Room roomInt;

		public List<RegionLink> links = new List<RegionLink>();

		public CellRect extentsClose;

		public CellRect extentsLimit;

		public Building_Door door;

		private int precalculatedHashCode;

		public bool touchesMapEdge;

		private int cachedCellCount = -1;

		public bool valid = true;

		private ListerThings listerThings = new ListerThings(ListerThingsUse.Region);

		public uint[] closedIndex = new uint[RegionTraverser.NumWorkers];

		public uint reachedIndex;

		public int newRegionGroupIndex = -1;

		private Dictionary<Area, AreaOverlap> cachedAreaOverlaps;

		public int mark;

		private List<KeyValuePair<Pawn, Danger>> cachedDangers = new List<KeyValuePair<Pawn, Danger>>();

		private int cachedDangersForFrame;

		private float cachedBaseDesiredPlantsCount;

		private int cachedBaseDesiredPlantsCountForTick = -999999;

		private int debug_makeTick = -1000;

		private int debug_lastTraverseTick = -1000;

		private static int nextId = 1;

		public const int GridSize = 12;

		public Map Map => (mapIndex >= 0) ? Find.Maps[mapIndex] : null;

		public IEnumerable<IntVec3> Cells
		{
			get
			{
				RegionGrid regions = Map.regionGrid;
				for (int z = extentsClose.minZ; z <= extentsClose.maxZ; z++)
				{
					for (int x = extentsClose.minX; x <= extentsClose.maxX; x++)
					{
						IntVec3 c = new IntVec3(x, 0, z);
						if (regions.GetRegionAt_NoRebuild_InvalidAllowed(c) == this)
						{
							yield return c;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
		}

		public int CellCount
		{
			get
			{
				if (cachedCellCount == -1)
				{
					cachedCellCount = Cells.Count();
				}
				return cachedCellCount;
			}
		}

		public IEnumerable<Region> Neighbors
		{
			get
			{
				for (int li = 0; li < links.Count; li++)
				{
					RegionLink link = links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].valid)
						{
							yield return link.regions[ri];
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
		}

		public IEnumerable<Region> NeighborsOfSameType
		{
			get
			{
				for (int li = 0; li < links.Count; li++)
				{
					RegionLink link = links[li];
					for (int ri = 0; ri < 2; ri++)
					{
						if (link.regions[ri] != null && link.regions[ri] != this && link.regions[ri].type == type && link.regions[ri].valid)
						{
							yield return link.regions[ri];
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
		}

		public Room Room
		{
			get
			{
				return roomInt;
			}
			set
			{
				if (value != roomInt)
				{
					if (roomInt != null)
					{
						roomInt.RemoveRegion(this);
					}
					roomInt = value;
					if (roomInt != null)
					{
						roomInt.AddRegion(this);
					}
				}
			}
		}

		public IntVec3 RandomCell
		{
			get
			{
				Map map = Map;
				CellIndices cellIndices = map.cellIndices;
				Region[] directGrid = map.regionGrid.DirectGrid;
				for (int i = 0; i < 1000; i++)
				{
					IntVec3 randomCell = extentsClose.RandomCell;
					if (directGrid[cellIndices.CellToIndex(randomCell)] == this)
					{
						return randomCell;
					}
				}
				return AnyCell;
			}
		}

		public IntVec3 AnyCell
		{
			get
			{
				Map map = Map;
				CellIndices cellIndices = map.cellIndices;
				Region[] directGrid = map.regionGrid.DirectGrid;
				CellRect.CellRectIterator iterator = extentsClose.GetIterator();
				while (!iterator.Done())
				{
					IntVec3 current = iterator.Current;
					if (directGrid[cellIndices.CellToIndex(current)] == this)
					{
						return current;
					}
					iterator.MoveNext();
				}
				Log.Error("Couldn't find any cell in region " + ToString());
				return extentsClose.RandomCell;
			}
		}

		public string DebugString
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("id: " + id);
				stringBuilder.AppendLine("mapIndex: " + mapIndex);
				stringBuilder.AppendLine("links count: " + links.Count);
				foreach (RegionLink link in links)
				{
					stringBuilder.AppendLine("  --" + link.ToString());
				}
				stringBuilder.AppendLine("valid: " + valid.ToString());
				stringBuilder.AppendLine("makeTick: " + debug_makeTick);
				stringBuilder.AppendLine("roomID: " + ((Room == null) ? "null room!" : Room.ID.ToString()));
				stringBuilder.AppendLine("extentsClose: " + extentsClose);
				stringBuilder.AppendLine("extentsLimit: " + extentsLimit);
				stringBuilder.AppendLine("ListerThings:");
				if (listerThings.AllThings != null)
				{
					for (int i = 0; i < listerThings.AllThings.Count; i++)
					{
						stringBuilder.AppendLine("  --" + listerThings.AllThings[i]);
					}
				}
				return stringBuilder.ToString();
			}
		}

		public bool DebugIsNew => debug_makeTick > Find.TickManager.TicksGame - 60;

		public ListerThings ListerThings => listerThings;

		public bool IsDoorway => door != null;

		private Region()
		{
		}

		public static Region MakeNewUnfilled(IntVec3 root, Map map)
		{
			Region region = new Region();
			region.debug_makeTick = Find.TickManager.TicksGame;
			region.id = nextId;
			nextId++;
			region.mapIndex = (sbyte)map.Index;
			region.precalculatedHashCode = Gen.HashCombineInt(region.id, 1295813358);
			region.extentsClose.minX = root.x;
			region.extentsClose.maxX = root.x;
			region.extentsClose.minZ = root.z;
			region.extentsClose.maxZ = root.z;
			region.extentsLimit.minX = root.x - root.x % 12;
			region.extentsLimit.maxX = root.x + 12 - (root.x + 12) % 12 - 1;
			region.extentsLimit.minZ = root.z - root.z % 12;
			region.extentsLimit.maxZ = root.z + 12 - (root.z + 12) % 12 - 1;
			region.extentsLimit.ClipInsideMap(map);
			return region;
		}

		public bool Allows(TraverseParms tp, bool isDestination)
		{
			if (tp.mode == TraverseMode.PassAllDestroyableThings || tp.mode == TraverseMode.PassAllDestroyableThingsNotWater || type.Passable())
			{
				if ((int)tp.maxDanger < 3 && tp.pawn != null)
				{
					Danger danger = DangerFor(tp.pawn);
					if (isDestination || danger == Danger.Deadly)
					{
						Region region = tp.pawn.GetRegion(RegionType.Set_All);
						if ((region == null || (int)danger > (int)region.DangerFor(tp.pawn)) && (int)danger > (int)tp.maxDanger)
						{
							return false;
						}
					}
				}
				switch (tp.mode)
				{
				case TraverseMode.ByPawn:
					if (door != null)
					{
						ByteGrid avoidGrid = tp.pawn.GetAvoidGrid();
						if (avoidGrid != null && avoidGrid[door.Position] == 255)
						{
							return false;
						}
						if (tp.pawn.HostileTo(door))
						{
							return door.CanPhysicallyPass(tp.pawn) || tp.canBash;
						}
						return door.CanPhysicallyPass(tp.pawn) && !door.IsForbiddenToPass(tp.pawn);
					}
					return true;
				case TraverseMode.NoPassClosedDoors:
					return door == null || door.FreePassage;
				case TraverseMode.PassDoors:
					return true;
				case TraverseMode.PassAllDestroyableThings:
					return true;
				case TraverseMode.NoPassClosedDoorsOrWater:
					return door == null || door.FreePassage;
				case TraverseMode.PassAllDestroyableThingsNotWater:
					return true;
				default:
					throw new NotImplementedException();
				}
			}
			return false;
		}

		public Danger DangerFor(Pawn p)
		{
			if (Current.ProgramState == ProgramState.Playing)
			{
				if (cachedDangersForFrame != Time.frameCount)
				{
					cachedDangers.Clear();
					cachedDangersForFrame = Time.frameCount;
				}
				else
				{
					for (int i = 0; i < cachedDangers.Count; i++)
					{
						if (cachedDangers[i].Key == p)
						{
							return cachedDangers[i].Value;
						}
					}
				}
			}
			Room room = Room;
			float temperature = room.Temperature;
			FloatRange floatRange = p.SafeTemperatureRange();
			Danger danger = floatRange.Includes(temperature) ? Danger.None : ((!floatRange.ExpandedBy(80f).Includes(temperature)) ? Danger.Deadly : Danger.Some);
			if (Current.ProgramState == ProgramState.Playing)
			{
				cachedDangers.Add(new KeyValuePair<Pawn, Danger>(p, danger));
			}
			return danger;
		}

		public float GetBaseDesiredPlantsCount(bool allowCache = true)
		{
			int ticksGame = Find.TickManager.TicksGame;
			if (allowCache && ticksGame - cachedBaseDesiredPlantsCountForTick < 2500)
			{
				return cachedBaseDesiredPlantsCount;
			}
			cachedBaseDesiredPlantsCount = 0f;
			Map map = Map;
			foreach (IntVec3 cell in Cells)
			{
				cachedBaseDesiredPlantsCount += map.wildPlantSpawner.GetBaseDesiredPlantsCountAt(cell);
			}
			cachedBaseDesiredPlantsCountForTick = ticksGame;
			return cachedBaseDesiredPlantsCount;
		}

		public AreaOverlap OverlapWith(Area a)
		{
			if (a.TrueCount == 0)
			{
				return AreaOverlap.None;
			}
			if (Map != a.Map)
			{
				return AreaOverlap.None;
			}
			if (cachedAreaOverlaps == null)
			{
				cachedAreaOverlaps = new Dictionary<Area, AreaOverlap>();
			}
			if (!cachedAreaOverlaps.TryGetValue(a, out AreaOverlap value))
			{
				int num = 0;
				int num2 = 0;
				foreach (IntVec3 cell in Cells)
				{
					num2++;
					if (a[cell])
					{
						num++;
					}
				}
				value = ((num != 0) ? ((num == num2) ? AreaOverlap.Entire : AreaOverlap.Partial) : AreaOverlap.None);
				cachedAreaOverlaps.Add(a, value);
			}
			return value;
		}

		public void Notify_AreaChanged(Area a)
		{
			if (cachedAreaOverlaps != null && cachedAreaOverlaps.ContainsKey(a))
			{
				cachedAreaOverlaps.Remove(a);
			}
		}

		public void DecrementMapIndex()
		{
			if (mapIndex <= 0)
			{
				Log.Warning("Tried to decrement map index for region " + id + ", but mapIndex=" + mapIndex);
			}
			else
			{
				mapIndex = (sbyte)(mapIndex - 1);
			}
		}

		public void Notify_MyMapRemoved()
		{
			listerThings.Clear();
			mapIndex = -1;
		}

		public override string ToString()
		{
			string str = (door == null) ? "null" : door.ToString();
			return "Region(id=" + id + ", mapIndex=" + mapIndex + ", center=" + extentsClose.CenterCell + ", links=" + links.Count + ", cells=" + CellCount + ((door == null) ? null : (", portal=" + str)) + ")";
		}

		public void DebugDraw()
		{
			if (DebugViewSettings.drawRegionTraversal && Find.TickManager.TicksGame < debug_lastTraverseTick + 60)
			{
				float a = 1f - (float)(Find.TickManager.TicksGame - debug_lastTraverseTick) / 60f;
				GenDraw.DrawFieldEdges(Cells.ToList(), new Color(0f, 0f, 1f, a));
			}
		}

		public void DebugDrawMouseover()
		{
			int num = Mathf.RoundToInt(Time.realtimeSinceStartup * 2f) % 2;
			if (DebugViewSettings.drawRegions)
			{
				GenDraw.DrawFieldEdges(color: (!valid) ? Color.red : ((!DebugIsNew) ? Color.green : Color.yellow), cells: Cells.ToList());
				foreach (Region neighbor in Neighbors)
				{
					GenDraw.DrawFieldEdges(neighbor.Cells.ToList(), Color.grey);
				}
			}
			if (DebugViewSettings.drawRegionLinks)
			{
				foreach (RegionLink link in links)
				{
					if (num == 1)
					{
						foreach (IntVec3 cell in link.span.Cells)
						{
							CellRenderer.RenderCell(cell, DebugSolidColorMats.MaterialOf(Color.magenta));
						}
					}
				}
			}
			if (DebugViewSettings.drawRegionThings)
			{
				foreach (Thing allThing in listerThings.AllThings)
				{
					CellRenderer.RenderSpot(allThing.TrueCenter(), (float)(allThing.thingIDNumber % 256) / 256f);
				}
			}
		}

		public void Debug_Notify_Traversed()
		{
			debug_lastTraverseTick = Find.TickManager.TicksGame;
		}

		public override int GetHashCode()
		{
			return precalculatedHashCode;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			Region region = obj as Region;
			if (region == null)
			{
				return false;
			}
			return region.id == id;
		}
	}
}
