using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class DebugThingPlaceHelper
	{
		public static bool IsDebugSpawnable(ThingDef def, bool allowPlayerBuildable = false)
		{
			if (def.forceDebugSpawnable)
			{
				return true;
			}
			if (def.thingClass == typeof(Corpse) || def.IsBlueprint || def.IsFrame || def == ThingDefOf.ActiveDropPod || def.thingClass == typeof(MinifiedThing) || def.thingClass == typeof(UnfinishedThing) || def.destroyOnDrop)
			{
				return false;
			}
			if (def.category == ThingCategory.Filth || def.category == ThingCategory.Item || def.category == ThingCategory.Plant || def.category == ThingCategory.Ethereal)
			{
				return true;
			}
			if (def.category == ThingCategory.Building && def.building.isNaturalRock)
			{
				return true;
			}
			if (def.category == ThingCategory.Building && !def.BuildableByPlayer)
			{
				return true;
			}
			if (def.category == ThingCategory.Building && def.BuildableByPlayer && allowPlayerBuildable)
			{
				return true;
			}
			return false;
		}

		public static void DebugSpawn(ThingDef def, IntVec3 c, int stackCount = -1, bool direct = false)
		{
			if (stackCount <= 0)
			{
				stackCount = def.stackLimit;
			}
			ThingDef stuff = GenStuff.RandomStuffFor(def);
			Thing thing = ThingMaker.MakeThing(def, stuff);
			thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
			if (thing.def.Minifiable)
			{
				thing = thing.MakeMinified();
			}
			thing.stackCount = stackCount;
			if (direct)
			{
				GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Direct);
			}
			else
			{
				GenPlace.TryPlaceThing(thing, c, Find.CurrentMap, ThingPlaceMode.Near);
			}
		}

		public static List<DebugMenuOption> TryPlaceOptionsForStackCount(int stackCount, bool direct)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
			where IsDebugSpawnable(def) && def.stackLimit >= stackCount
			select def;
			foreach (ThingDef item in enumerable)
			{
				ThingDef localDef = item;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					DebugSpawn(localDef, UI.MouseCell(), stackCount, direct);
				}));
			}
			if (stackCount == 1)
			{
				foreach (ThingDef item2 in from def in DefDatabase<ThingDef>.AllDefs
				where def.Minifiable
				select def)
				{
					ThingDef localDef2 = item2;
					list.Add(new DebugMenuOption(localDef2.LabelCap + " (minified)", DebugMenuOptionMode.Tool, delegate
					{
						DebugSpawn(localDef2, UI.MouseCell(), stackCount, direct);
					}));
				}
				return list;
			}
			return list;
		}

		public static List<DebugMenuOption> SpawnOptions(WipeMode wipeMode)
		{
			List<DebugMenuOption> list = new List<DebugMenuOption>();
			IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
			where IsDebugSpawnable(def, allowPlayerBuildable: true)
			select def;
			foreach (ThingDef item in enumerable)
			{
				ThingDef localDef = item;
				list.Add(new DebugMenuOption(localDef.LabelCap, DebugMenuOptionMode.Tool, delegate
				{
					Thing thing = ThingMaker.MakeThing(localDef, GenStuff.RandomStuffFor(localDef));
					thing.TryGetComp<CompQuality>()?.SetQuality(QualityUtility.GenerateQualityRandomEqualChance(), ArtGenerationContext.Colony);
					GenSpawn.Spawn(thing, UI.MouseCell(), Find.CurrentMap, wipeMode);
				}));
			}
			return list;
		}
	}
}
