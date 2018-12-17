using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld
{
	public class GenStep_ElevationFertility : GenStep
	{
		private const float ElevationFreq = 0.021f;

		private const float FertilityFreq = 0.021f;

		private const float EdgeMountainSpan = 0.42f;

		public override int SeedPart => 826504671;

		public override void Generate(Map map, GenStepParams parms)
		{
			IntVec3 size = map.Size;
			int x = size.x;
			IntVec3 size2 = map.Size;
			NoiseRenderer.renderSize = new IntVec2(x, size2.z);
			ModuleBase input = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			input = new ScaleBias(0.5, 0.5, input);
			NoiseDebugUI.StoreNoiseRender(input, "elev base");
			float num = 1f;
			switch (map.TileInfo.hilliness)
			{
			case Hilliness.Flat:
				num = MapGenTuning.ElevationFactorFlat;
				break;
			case Hilliness.SmallHills:
				num = MapGenTuning.ElevationFactorSmallHills;
				break;
			case Hilliness.LargeHills:
				num = MapGenTuning.ElevationFactorLargeHills;
				break;
			case Hilliness.Mountainous:
				num = MapGenTuning.ElevationFactorMountains;
				break;
			case Hilliness.Impassable:
				num = MapGenTuning.ElevationFactorImpassableMountains;
				break;
			}
			input = new Multiply(input, new Const((double)num));
			NoiseDebugUI.StoreNoiseRender(input, "elev world-factored");
			if (map.TileInfo.hilliness == Hilliness.Mountainous || map.TileInfo.hilliness == Hilliness.Impassable)
			{
				IntVec3 size3 = map.Size;
				ModuleBase input2 = new DistFromAxis((float)size3.x * 0.42f);
				input2 = new Clamp(0.0, 1.0, input2);
				input2 = new Invert(input2);
				input2 = new ScaleBias(1.0, 1.0, input2);
				Rot4 random;
				do
				{
					random = Rot4.Random;
				}
				while (random == Find.World.CoastDirectionAt(map.Tile));
				if (random == Rot4.North)
				{
					input2 = new Rotate(0.0, 90.0, 0.0, input2);
					IntVec3 size4 = map.Size;
					input2 = new Translate(0.0, 0.0, (double)(-size4.z), input2);
				}
				else if (random == Rot4.East)
				{
					IntVec3 size5 = map.Size;
					input2 = new Translate((double)(-size5.x), 0.0, 0.0, input2);
				}
				else if (random == Rot4.South)
				{
					input2 = new Rotate(0.0, 90.0, 0.0, input2);
				}
				else if (!(random == Rot4.West))
				{
				}
				NoiseDebugUI.StoreNoiseRender(input2, "mountain");
				input = new Add(input, input2);
				NoiseDebugUI.StoreNoiseRender(input, "elev + mountain");
			}
			float b = (!map.TileInfo.WaterCovered) ? 3.40282347E+38f : 0f;
			MapGenFloatGrid elevation = MapGenerator.Elevation;
			foreach (IntVec3 allCell in map.AllCells)
			{
				elevation[allCell] = Mathf.Min(input.GetValue(allCell), b);
			}
			ModuleBase input3 = new Perlin(0.020999999716877937, 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			input3 = new ScaleBias(0.5, 0.5, input3);
			NoiseDebugUI.StoreNoiseRender(input3, "noiseFert base");
			MapGenFloatGrid fertility = MapGenerator.Fertility;
			foreach (IntVec3 allCell2 in map.AllCells)
			{
				fertility[allCell2] = input3.GetValue(allCell2);
			}
		}
	}
}
