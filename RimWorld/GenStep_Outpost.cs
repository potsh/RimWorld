using RimWorld.BaseGen;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class GenStep_Outpost : GenStep
	{
		public int size = 16;

		public FloatRange defaultPawnGroupPointsRange = SymbolResolver_Settlement.DefaultPawnsPoints;

		private static List<CellRect> possibleRects = new List<CellRect>();

		public override int SeedPart => 398638181;

		public override void Generate(Map map, GenStepParams parms)
		{
			if (!MapGenerator.TryGetVar("RectOfInterest", out CellRect var))
			{
				var = CellRect.SingleCell(map.Center);
			}
			Faction faction = (map.ParentFaction != null && map.ParentFaction != Faction.OfPlayer) ? map.ParentFaction : Find.FactionManager.RandomEnemyFaction();
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = GetOutpostRect(var, map);
			resolveParams.faction = faction;
			resolveParams.edgeDefenseWidth = 2;
			resolveParams.edgeDefenseTurretsCount = Rand.RangeInclusive(0, 1);
			resolveParams.edgeDefenseMortarsCount = 0;
			if (parms.siteCoreOrPart != null)
			{
				resolveParams.settlementPawnGroupPoints = parms.siteCoreOrPart.parms.threatPoints;
				resolveParams.settlementPawnGroupSeed = OutpostSitePartUtility.GetPawnGroupMakerSeed(parms.siteCoreOrPart.parms);
			}
			else
			{
				resolveParams.settlementPawnGroupPoints = defaultPawnGroupPointsRange.RandomInRange;
			}
			RimWorld.BaseGen.BaseGen.globalSettings.map = map;
			RimWorld.BaseGen.BaseGen.globalSettings.minBuildings = 1;
			RimWorld.BaseGen.BaseGen.globalSettings.minBarracks = 1;
			RimWorld.BaseGen.BaseGen.symbolStack.Push("settlement", resolveParams);
			RimWorld.BaseGen.BaseGen.Generate();
		}

		private CellRect GetOutpostRect(CellRect rectToDefend, Map map)
		{
			List<CellRect> list = possibleRects;
			int minX = rectToDefend.minX - 1 - size;
			IntVec3 centerCell = rectToDefend.CenterCell;
			list.Add(new CellRect(minX, centerCell.z - size / 2, size, size));
			List<CellRect> list2 = possibleRects;
			int minX2 = rectToDefend.maxX + 1;
			IntVec3 centerCell2 = rectToDefend.CenterCell;
			list2.Add(new CellRect(minX2, centerCell2.z - size / 2, size, size));
			List<CellRect> list3 = possibleRects;
			IntVec3 centerCell3 = rectToDefend.CenterCell;
			list3.Add(new CellRect(centerCell3.x - size / 2, rectToDefend.minZ - 1 - size, size, size));
			List<CellRect> list4 = possibleRects;
			IntVec3 centerCell4 = rectToDefend.CenterCell;
			list4.Add(new CellRect(centerCell4.x - size / 2, rectToDefend.maxZ + 1, size, size));
			IntVec3 intVec = map.Size;
			int x2 = intVec.x;
			IntVec3 intVec2 = map.Size;
			CellRect mapRect = new CellRect(0, 0, x2, intVec2.z);
			possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
			if (possibleRects.Any())
			{
				return possibleRects.RandomElement();
			}
			return rectToDefend;
		}
	}
}
