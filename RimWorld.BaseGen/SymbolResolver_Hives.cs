using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Hives : SymbolResolver
	{
		private static readonly IntRange DefaultHivesCountRange = new IntRange(1, 3);

		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (!TryFindFirstHivePos(rp.rect, out IntVec3 _))
			{
				return false;
			}
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			if (TryFindFirstHivePos(rp.rect, out IntVec3 pos))
			{
				int? hivesCount = rp.hivesCount;
				int num = (!hivesCount.HasValue) ? DefaultHivesCountRange.RandomInRange : hivesCount.Value;
				Hive hive = (Hive)ThingMaker.MakeThing(ThingDefOf.Hive);
				hive.SetFaction(Faction.OfInsects);
				if (rp.disableHives.HasValue && rp.disableHives.Value)
				{
					hive.active = false;
				}
				hive = (Hive)GenSpawn.Spawn(hive, pos, BaseGen.globalSettings.map);
				for (int i = 0; i < num - 1; i++)
				{
					if (hive.GetComp<CompSpawnerHives>().TrySpawnChildHive(ignoreRoofedRequirement: true, out Hive newHive))
					{
						hive = newHive;
					}
				}
			}
		}

		private bool TryFindFirstHivePos(CellRect rect, out IntVec3 pos)
		{
			Map map = BaseGen.globalSettings.map;
			return (from mc in rect.Cells
			where mc.Standable(map)
			select mc).TryRandomElement(out pos);
		}
	}
}
