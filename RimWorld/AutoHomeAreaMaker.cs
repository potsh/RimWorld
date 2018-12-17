using Verse;

namespace RimWorld
{
	public static class AutoHomeAreaMaker
	{
		private const int BorderWidth = 4;

		private static bool ShouldAdd()
		{
			return Find.PlaySettings.autoHomeArea && Current.ProgramState == ProgramState.Playing;
		}

		public static void Notify_BuildingSpawned(Thing b)
		{
			if (ShouldAdd() && b.def.building.expandHomeArea && b.Faction == Faction.OfPlayer)
			{
				MarkHomeAroundThing(b);
			}
		}

		public static void Notify_BuildingClaimed(Thing b)
		{
			if (ShouldAdd() && b.def.building.expandHomeArea && b.Faction == Faction.OfPlayer)
			{
				MarkHomeAroundThing(b);
			}
		}

		public static void MarkHomeAroundThing(Thing t)
		{
			if (ShouldAdd())
			{
				IntVec3 position = t.Position;
				int x = position.x;
				IntVec2 rotatedSize = t.RotatedSize;
				int minX = x - rotatedSize.x / 2 - 4;
				IntVec3 position2 = t.Position;
				int z = position2.z;
				IntVec2 rotatedSize2 = t.RotatedSize;
				int minZ = z - rotatedSize2.z / 2 - 4;
				IntVec2 rotatedSize3 = t.RotatedSize;
				int width = rotatedSize3.x + 8;
				IntVec2 rotatedSize4 = t.RotatedSize;
				CellRect cellRect = new CellRect(minX, minZ, width, rotatedSize4.z + 8);
				cellRect.ClipInsideMap(t.Map);
				foreach (IntVec3 item in cellRect)
				{
					t.Map.areaManager.Home[item] = true;
				}
			}
		}

		public static void Notify_ZoneCellAdded(IntVec3 c, Zone zone)
		{
			if (ShouldAdd())
			{
				CellRect.CellRectIterator iterator = CellRect.CenteredOn(c, 4).ClipInsideMap(zone.Map).GetIterator();
				while (!iterator.Done())
				{
					zone.Map.areaManager.Home[iterator.Current] = true;
					iterator.MoveNext();
				}
			}
		}
	}
}
