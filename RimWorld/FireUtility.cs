using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class FireUtility
	{
		public static bool CanEverAttachFire(this Thing t)
		{
			if (t.Destroyed)
			{
				return false;
			}
			if (!t.FlammableNow)
			{
				return false;
			}
			if (t.def.category != ThingCategory.Pawn)
			{
				return false;
			}
			if (t.TryGetComp<CompAttachBase>() == null)
			{
				return false;
			}
			return true;
		}

		public static float ChanceToStartFireIn(IntVec3 c, Map map)
		{
			List<Thing> thingList = c.GetThingList(map);
			float num = (!c.TerrainFlammableNow(map)) ? 0f : c.GetTerrain(map).GetStatValueAbstract(StatDefOf.Flammability);
			for (int i = 0; i < thingList.Count; i++)
			{
				Thing thing = thingList[i];
				if (thing is Fire)
				{
					return 0f;
				}
				if (thing.def.category != ThingCategory.Pawn && thingList[i].FlammableNow)
				{
					num = Mathf.Max(num, thing.GetStatValue(StatDefOf.Flammability));
				}
			}
			if (num > 0f)
			{
				Building edifice = c.GetEdifice(map);
				if (edifice != null && edifice.def.passability == Traversability.Impassable && edifice.OccupiedRect().ContractedBy(1).Contains(c))
				{
					return 0f;
				}
				List<Thing> thingList2 = c.GetThingList(map);
				for (int j = 0; j < thingList2.Count; j++)
				{
					if (thingList2[j].def.category == ThingCategory.Filth && !thingList2[j].def.filth.allowsFire)
					{
						return 0f;
					}
				}
			}
			return num;
		}

		public static bool TryStartFireIn(IntVec3 c, Map map, float fireSize)
		{
			float num = ChanceToStartFireIn(c, map);
			if (num <= 0f)
			{
				return false;
			}
			Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
			fire.fireSize = fireSize;
			GenSpawn.Spawn(fire, c, map, Rot4.North);
			return true;
		}

		public static void TryAttachFire(this Thing t, float fireSize)
		{
			if (t.CanEverAttachFire() && !t.HasAttachment(ThingDefOf.Fire))
			{
				Fire fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
				fire.fireSize = fireSize;
				fire.AttachTo(t);
				GenSpawn.Spawn(fire, t.Position, t.Map, Rot4.North);
				Pawn pawn = t as Pawn;
				if (pawn != null)
				{
					pawn.jobs.StopAll();
					pawn.records.Increment(RecordDefOf.TimesOnFire);
				}
			}
		}

		public static bool IsBurning(this TargetInfo t)
		{
			if (t.HasThing)
			{
				return t.Thing.IsBurning();
			}
			return t.Cell.ContainsStaticFire(t.Map);
		}

		public static bool IsBurning(this Thing t)
		{
			if (t.Destroyed || !t.Spawned)
			{
				return false;
			}
			if (t.def.size == IntVec2.One)
			{
				if (t is Pawn)
				{
					return t.HasAttachment(ThingDefOf.Fire);
				}
				return t.Position.ContainsStaticFire(t.Map);
			}
			CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				if (iterator.Current.ContainsStaticFire(t.Map))
				{
					return true;
				}
				iterator.MoveNext();
			}
			return false;
		}

		public static bool ContainsStaticFire(this IntVec3 c, Map map)
		{
			List<Thing> list = map.thingGrid.ThingsListAt(c);
			for (int i = 0; i < list.Count; i++)
			{
				Fire fire = list[i] as Fire;
				if (fire != null && fire.parent == null)
				{
					return true;
				}
			}
			return false;
		}

		public static bool ContainsTrap(this IntVec3 c, Map map)
		{
			Building edifice = c.GetEdifice(map);
			return edifice != null && edifice is Building_Trap;
		}

		public static bool Flammable(this TerrainDef terrain)
		{
			return terrain.GetStatValueAbstract(StatDefOf.Flammability) > 0.01f;
		}

		public static bool TerrainFlammableNow(this IntVec3 c, Map map)
		{
			TerrainDef terrain = c.GetTerrain(map);
			if (!terrain.Flammable())
			{
				return false;
			}
			List<Thing> thingList = c.GetThingList(map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (thingList[i].FireBulwark)
				{
					return false;
				}
			}
			return true;
		}
	}
}
