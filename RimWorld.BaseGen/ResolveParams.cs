using System;
using System.Collections.Generic;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public struct ResolveParams
	{
		public CellRect rect;

		public Faction faction;

		private Dictionary<string, object> custom;

		public int? ancientTempleEntranceHeight;

		public PawnGroupMakerParms pawnGroupMakerParams;

		public PawnGroupKindDef pawnGroupKindDef;

		public RoofDef roofDef;

		public bool? noRoof;

		public bool? addRoomCenterToRootsToUnfog;

		public Thing singleThingToSpawn;

		public ThingDef singleThingDef;

		public ThingDef singleThingStuff;

		public int? singleThingStackCount;

		public bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit;

		public bool? spawnBridgeIfTerrainCantSupportThing;

		public Pawn singlePawnToSpawn;

		public PawnKindDef singlePawnKindDef;

		public bool? disableSinglePawn;

		public Lord singlePawnLord;

		public Predicate<IntVec3> singlePawnSpawnCellExtraPredicate;

		public PawnGenerationRequest? singlePawnGenerationRequest;

		public Action<Thing> postThingSpawn;

		public Action<Thing> postThingGenerate;

		public int? mechanoidsCount;

		public int? hivesCount;

		public bool? disableHives;

		public Rot4? thingRot;

		public ThingDef wallStuff;

		public float? chanceToSkipWallBlock;

		public TerrainDef floorDef;

		public float? chanceToSkipFloor;

		public ThingDef filthDef;

		public FloatRange? filthDensity;

		public bool? floorOnlyIfTerrainSupports;

		public bool? clearEdificeOnly;

		public bool? clearFillageOnly;

		public bool? clearRoof;

		public int? ancientCryptosleepCasketGroupID;

		public PodContentsType? podContentsType;

		public ThingSetMakerDef thingSetMakerDef;

		public ThingSetMakerParams? thingSetMakerParams;

		public IList<Thing> stockpileConcreteContents;

		public float? stockpileMarketValue;

		public int? innerStockpileSize;

		public int? edgeDefenseWidth;

		public int? edgeDefenseTurretsCount;

		public int? edgeDefenseMortarsCount;

		public int? edgeDefenseGuardsCount;

		public ThingDef mortarDef;

		public TerrainDef pathwayFloorDef;

		public ThingDef cultivatedPlantDef;

		public int? fillWithThingsPadding;

		public float? settlementPawnGroupPoints;

		public int? settlementPawnGroupSeed;

		public bool? streetHorizontal;

		public bool? edgeThingAvoidOtherEdgeThings;

		public bool? allowPlacementOffEdge;

		public Rot4? thrustAxis;

		public FloatRange? hpPercentRange;

		public void SetCustom<T>(string name, T obj, bool inherit = false)
		{
			if (custom == null)
			{
				custom = new Dictionary<string, object>();
			}
			else
			{
				custom = new Dictionary<string, object>(custom);
			}
			if (!custom.ContainsKey(name))
			{
				custom.Add(name, obj);
			}
			else if (!inherit)
			{
				custom[name] = obj;
			}
		}

		public void RemoveCustom(string name)
		{
			if (custom != null)
			{
				custom = new Dictionary<string, object>(custom);
				custom.Remove(name);
			}
		}

		public bool TryGetCustom<T>(string name, out T obj)
		{
			if (custom == null || !custom.TryGetValue(name, out object value))
			{
				obj = default(T);
				return false;
			}
			obj = (T)value;
			return true;
		}

		public T GetCustom<T>(string name)
		{
			if (custom == null || !custom.TryGetValue(name, out object value))
			{
				return default(T);
			}
			return (T)value;
		}

		public override string ToString()
		{
			object[] obj = new object[116]
			{
				"rect=",
				rect,
				", faction=",
				(faction == null) ? "null" : faction.ToString(),
				", custom=",
				(custom == null) ? "null" : custom.Count.ToString(),
				", ancientTempleEntranceHeight=",
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};
			int? num = ancientTempleEntranceHeight;
			obj[7] = ((!num.HasValue) ? "null" : ancientTempleEntranceHeight.ToString());
			obj[8] = ", pawnGroupMakerParams=";
			obj[9] = ((pawnGroupMakerParams == null) ? "null" : pawnGroupMakerParams.ToString());
			obj[10] = ", pawnGroupKindDef=";
			obj[11] = ((pawnGroupKindDef == null) ? "null" : pawnGroupKindDef.ToString());
			obj[12] = ", roofDef=";
			obj[13] = ((roofDef == null) ? "null" : roofDef.ToString());
			obj[14] = ", noRoof=";
			bool? flag = noRoof;
			obj[15] = ((!flag.HasValue) ? "null" : noRoof.ToString());
			obj[16] = ", addRoomCenterToRootsToUnfog=";
			bool? flag2 = addRoomCenterToRootsToUnfog;
			obj[17] = ((!flag2.HasValue) ? "null" : addRoomCenterToRootsToUnfog.ToString());
			obj[18] = ", singleThingToSpawn=";
			obj[19] = ((singleThingToSpawn == null) ? "null" : singleThingToSpawn.ToString());
			obj[20] = ", singleThingDef=";
			obj[21] = ((singleThingDef == null) ? "null" : singleThingDef.ToString());
			obj[22] = ", singleThingStuff=";
			obj[23] = ((singleThingStuff == null) ? "null" : singleThingStuff.ToString());
			obj[24] = ", singleThingStackCount=";
			int? num2 = singleThingStackCount;
			obj[25] = ((!num2.HasValue) ? "null" : singleThingStackCount.ToString());
			obj[26] = ", skipSingleThingIfHasToWipeBuildingOrDoesntFit=";
			bool? flag3 = skipSingleThingIfHasToWipeBuildingOrDoesntFit;
			obj[27] = ((!flag3.HasValue) ? "null" : skipSingleThingIfHasToWipeBuildingOrDoesntFit.ToString());
			obj[28] = ", spawnBridgeIfTerrainCantSupportThing=";
			bool? flag4 = spawnBridgeIfTerrainCantSupportThing;
			obj[29] = ((!flag4.HasValue) ? "null" : spawnBridgeIfTerrainCantSupportThing.ToString());
			obj[30] = ", singlePawnToSpawn=";
			obj[31] = ((singlePawnToSpawn == null) ? "null" : singlePawnToSpawn.ToString());
			obj[32] = ", singlePawnKindDef=";
			obj[33] = ((singlePawnKindDef == null) ? "null" : singlePawnKindDef.ToString());
			obj[34] = ", disableSinglePawn=";
			bool? flag5 = disableSinglePawn;
			obj[35] = ((!flag5.HasValue) ? "null" : disableSinglePawn.ToString());
			obj[36] = ", singlePawnLord=";
			obj[37] = ((singlePawnLord == null) ? "null" : singlePawnLord.ToString());
			obj[38] = ", singlePawnSpawnCellExtraPredicate=";
			obj[39] = ((singlePawnSpawnCellExtraPredicate == null) ? "null" : singlePawnSpawnCellExtraPredicate.ToString());
			obj[40] = ", singlePawnGenerationRequest=";
			PawnGenerationRequest? pawnGenerationRequest = singlePawnGenerationRequest;
			obj[41] = ((!pawnGenerationRequest.HasValue) ? "null" : singlePawnGenerationRequest.ToString());
			obj[42] = ", postThingSpawn=";
			obj[43] = ((postThingSpawn == null) ? "null" : postThingSpawn.ToString());
			obj[44] = ", postThingGenerate=";
			obj[45] = ((postThingGenerate == null) ? "null" : postThingGenerate.ToString());
			obj[46] = ", mechanoidsCount=";
			int? num3 = mechanoidsCount;
			obj[47] = ((!num3.HasValue) ? "null" : mechanoidsCount.ToString());
			obj[48] = ", hivesCount=";
			int? num4 = hivesCount;
			obj[49] = ((!num4.HasValue) ? "null" : hivesCount.ToString());
			obj[50] = ", disableHives=";
			bool? flag6 = disableHives;
			obj[51] = ((!flag6.HasValue) ? "null" : disableHives.ToString());
			obj[52] = ", thingRot=";
			Rot4? rot = thingRot;
			obj[53] = ((!rot.HasValue) ? "null" : thingRot.ToString());
			obj[54] = ", wallStuff=";
			obj[55] = ((wallStuff == null) ? "null" : wallStuff.ToString());
			obj[56] = ", chanceToSkipWallBlock=";
			float? num5 = chanceToSkipWallBlock;
			obj[57] = ((!num5.HasValue) ? "null" : chanceToSkipWallBlock.ToString());
			obj[58] = ", floorDef=";
			obj[59] = ((floorDef == null) ? "null" : floorDef.ToString());
			obj[60] = ", chanceToSkipFloor=";
			float? num6 = chanceToSkipFloor;
			obj[61] = ((!num6.HasValue) ? "null" : chanceToSkipFloor.ToString());
			obj[62] = ", filthDef=";
			obj[63] = ((filthDef == null) ? "null" : filthDef.ToString());
			obj[64] = ", filthDensity=";
			FloatRange? floatRange = filthDensity;
			obj[65] = ((!floatRange.HasValue) ? "null" : filthDensity.ToString());
			obj[66] = ", floorOnlyIfTerrainSupports=";
			bool? flag7 = floorOnlyIfTerrainSupports;
			obj[67] = ((!flag7.HasValue) ? "null" : floorOnlyIfTerrainSupports.ToString());
			obj[68] = ", clearEdificeOnly=";
			bool? flag8 = clearEdificeOnly;
			obj[69] = ((!flag8.HasValue) ? "null" : clearEdificeOnly.ToString());
			obj[70] = ", clearFillageOnly=";
			bool? flag9 = clearFillageOnly;
			obj[71] = ((!flag9.HasValue) ? "null" : clearFillageOnly.ToString());
			obj[72] = ", clearRoof=";
			bool? flag10 = clearRoof;
			obj[73] = ((!flag10.HasValue) ? "null" : clearRoof.ToString());
			obj[74] = ", ancientCryptosleepCasketGroupID=";
			int? num7 = ancientCryptosleepCasketGroupID;
			obj[75] = ((!num7.HasValue) ? "null" : ancientCryptosleepCasketGroupID.ToString());
			obj[76] = ", podContentsType=";
			PodContentsType? podContentsType = this.podContentsType;
			obj[77] = ((!podContentsType.HasValue) ? "null" : this.podContentsType.ToString());
			obj[78] = ", thingSetMakerDef=";
			obj[79] = ((thingSetMakerDef == null) ? "null" : thingSetMakerDef.ToString());
			obj[80] = ", thingSetMakerParams=";
			ThingSetMakerParams? thingSetMakerParams = this.thingSetMakerParams;
			obj[81] = ((!thingSetMakerParams.HasValue) ? "null" : this.thingSetMakerParams.ToString());
			obj[82] = ", stockpileConcreteContents=";
			obj[83] = ((stockpileConcreteContents == null) ? "null" : stockpileConcreteContents.Count.ToString());
			obj[84] = ", stockpileMarketValue=";
			float? num8 = stockpileMarketValue;
			obj[85] = ((!num8.HasValue) ? "null" : stockpileMarketValue.ToString());
			obj[86] = ", innerStockpileSize=";
			int? num9 = innerStockpileSize;
			obj[87] = ((!num9.HasValue) ? "null" : innerStockpileSize.ToString());
			obj[88] = ", edgeDefenseWidth=";
			int? num10 = edgeDefenseWidth;
			obj[89] = ((!num10.HasValue) ? "null" : edgeDefenseWidth.ToString());
			obj[90] = ", edgeDefenseTurretsCount=";
			int? num11 = edgeDefenseTurretsCount;
			obj[91] = ((!num11.HasValue) ? "null" : edgeDefenseTurretsCount.ToString());
			obj[92] = ", edgeDefenseMortarsCount=";
			int? num12 = edgeDefenseMortarsCount;
			obj[93] = ((!num12.HasValue) ? "null" : edgeDefenseMortarsCount.ToString());
			obj[94] = ", edgeDefenseGuardsCount=";
			int? num13 = edgeDefenseGuardsCount;
			obj[95] = ((!num13.HasValue) ? "null" : edgeDefenseGuardsCount.ToString());
			obj[96] = ", mortarDef=";
			obj[97] = ((mortarDef == null) ? "null" : mortarDef.ToString());
			obj[98] = ", pathwayFloorDef=";
			obj[99] = ((pathwayFloorDef == null) ? "null" : pathwayFloorDef.ToString());
			obj[100] = ", cultivatedPlantDef=";
			obj[101] = ((cultivatedPlantDef == null) ? "null" : cultivatedPlantDef.ToString());
			obj[102] = ", fillWithThingsPadding=";
			int? num14 = fillWithThingsPadding;
			obj[103] = ((!num14.HasValue) ? "null" : fillWithThingsPadding.ToString());
			obj[104] = ", settlementPawnGroupPoints=";
			float? num15 = settlementPawnGroupPoints;
			obj[105] = ((!num15.HasValue) ? "null" : settlementPawnGroupPoints.ToString());
			obj[106] = ", settlementPawnGroupSeed=";
			int? num16 = settlementPawnGroupSeed;
			obj[107] = ((!num16.HasValue) ? "null" : settlementPawnGroupSeed.ToString());
			obj[108] = ", streetHorizontal=";
			bool? flag11 = streetHorizontal;
			obj[109] = ((!flag11.HasValue) ? "null" : streetHorizontal.ToString());
			obj[110] = ", edgeThingAvoidOtherEdgeThings=";
			bool? flag12 = edgeThingAvoidOtherEdgeThings;
			obj[111] = ((!flag12.HasValue) ? "null" : edgeThingAvoidOtherEdgeThings.ToString());
			obj[112] = ", allowPlacementOffEdge=";
			bool? flag13 = allowPlacementOffEdge;
			obj[113] = ((!flag13.HasValue) ? "null" : allowPlacementOffEdge.ToString());
			obj[114] = ", thrustAxis=";
			Rot4? rot2 = thrustAxis;
			obj[115] = ((!rot2.HasValue) ? "null" : thrustAxis.ToString());
			return string.Concat(obj);
		}
	}
}
