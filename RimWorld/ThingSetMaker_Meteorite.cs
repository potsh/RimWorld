using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ThingSetMaker_Meteorite : ThingSetMaker
	{
		public static List<ThingDef> nonSmoothedMineables = new List<ThingDef>();

		public static readonly IntRange MineablesCountRange = new IntRange(8, 20);

		private const float PreciousMineableMarketValue = 5f;

		public static void Reset()
		{
			nonSmoothedMineables.Clear();
			nonSmoothedMineables.AddRange(from x in DefDatabase<ThingDef>.AllDefsListForReading
			where x.mineable && x != ThingDefOf.CollapsedRocks && !x.IsSmoothed
			select x);
		}

		protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
		{
			IntRange? countRange = parms.countRange;
			int randomInRange = ((!countRange.HasValue) ? MineablesCountRange : countRange.Value).RandomInRange;
			ThingDef def = FindRandomMineableDef();
			for (int i = 0; i < randomInRange; i++)
			{
				Building building = (Building)ThingMaker.MakeThing(def);
				building.canChangeTerrainOnDestroyed = false;
				outThings.Add(building);
			}
		}

		private ThingDef FindRandomMineableDef()
		{
			float value = Rand.Value;
			if (value < 0.4f)
			{
				return (from x in nonSmoothedMineables
				where !x.building.isResourceRock
				select x).RandomElement();
			}
			if (value < 0.75f)
			{
				return (from x in nonSmoothedMineables
				where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue < 5f
				select x).RandomElement();
			}
			return (from x in nonSmoothedMineables
			where x.building.isResourceRock && x.building.mineableThing.BaseMarketValue >= 5f
			select x).RandomElement();
		}

		protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
		{
			return nonSmoothedMineables;
		}
	}
}
