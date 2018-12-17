using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class FilthMaker
	{
		private static List<Filth> toBeRemoved = new List<Filth>();

		public static void MakeFilth(IntVec3 c, Map map, ThingDef filthDef, int count = 1)
		{
			for (int i = 0; i < count; i++)
			{
				MakeFilth(c, map, filthDef, null, shouldPropagate: true);
			}
		}

		public static bool MakeFilth(IntVec3 c, Map map, ThingDef filthDef, string source, int count = 1)
		{
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				flag |= MakeFilth(c, map, filthDef, Gen.YieldSingle(source), shouldPropagate: true);
			}
			return flag;
		}

		public static void MakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources)
		{
			MakeFilth(c, map, filthDef, sources, shouldPropagate: true);
		}

		private static bool MakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, bool shouldPropagate)
		{
			Filth filth = (Filth)(from t in c.GetThingList(map)
			where t.def == filthDef
			select t).FirstOrDefault();
			if (!c.Walkable(map) || (filth != null && !filth.CanBeThickened))
			{
				if (shouldPropagate)
				{
					List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
					for (int i = 0; i < 8; i++)
					{
						IntVec3 c2 = c + list[i];
						if (c2.InBounds(map) && MakeFilth(c2, map, filthDef, sources, shouldPropagate: false))
						{
							return true;
						}
					}
				}
				filth?.AddSources(sources);
				return false;
			}
			if (filth != null)
			{
				filth.ThickenFilth();
				filth.AddSources(sources);
			}
			else
			{
				Filth filth2 = (Filth)ThingMaker.MakeThing(filthDef);
				filth2.AddSources(sources);
				GenSpawn.Spawn(filth2, c, map);
			}
			FilthMonitor.Notify_FilthSpawned();
			return true;
		}

		public static void RemoveAllFilth(IntVec3 c, Map map)
		{
			toBeRemoved.Clear();
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				Filth filth = thingList[i] as Filth;
				if (filth != null)
				{
					toBeRemoved.Add(filth);
				}
			}
			for (int j = 0; j < toBeRemoved.Count; j++)
			{
				toBeRemoved[j].Destroy();
			}
			toBeRemoved.Clear();
		}
	}
}
