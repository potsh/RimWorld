using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldGenStep_Terrain : WorldGenStep
	{
		[Unsaved]
		private ModuleBase noiseElevation;

		[Unsaved]
		private ModuleBase noiseTemperatureOffset;

		[Unsaved]
		private ModuleBase noiseRainfall;

		[Unsaved]
		private ModuleBase noiseSwampiness;

		[Unsaved]
		private ModuleBase noiseMountainLines;

		[Unsaved]
		private ModuleBase noiseHillsPatchesMicro;

		[Unsaved]
		private ModuleBase noiseHillsPatchesMacro;

		private const float ElevationFrequencyMicro = 0.035f;

		private const float ElevationFrequencyMacro = 0.012f;

		private const float ElevationMacroFactorFrequency = 0.12f;

		private const float ElevationContinentsFrequency = 0.01f;

		private const float MountainLinesFrequency = 0.025f;

		private const float MountainLinesHolesFrequency = 0.06f;

		private const float HillsPatchesFrequencyMicro = 0.19f;

		private const float HillsPatchesFrequencyMacro = 0.032f;

		private const float SwampinessFrequencyMacro = 0.025f;

		private const float SwampinessFrequencyMicro = 0.09f;

		private static readonly FloatRange SwampinessMaxElevation = new FloatRange(650f, 750f);

		private static readonly FloatRange SwampinessMinRainfall = new FloatRange(725f, 900f);

		private static readonly FloatRange ElevationRange = new FloatRange(-500f, 5000f);

		private const float TemperatureOffsetFrequency = 0.018f;

		private const float TemperatureOffsetFactor = 4f;

		private static readonly SimpleCurve AvgTempByLatitudeCurve = new SimpleCurve
		{
			new CurvePoint(0f, 30f),
			new CurvePoint(0.1f, 29f),
			new CurvePoint(0.5f, 7f),
			new CurvePoint(1f, -37f)
		};

		private const float ElevationTempReductionStartAlt = 250f;

		private const float ElevationTempReductionEndAlt = 5000f;

		private const float MaxElevationTempReduction = 40f;

		private const float RainfallOffsetFrequency = 0.013f;

		private const float RainfallPower = 1.5f;

		private const float RainfallFactor = 4000f;

		private const float RainfallStartFallAltitude = 500f;

		private const float RainfallFinishFallAltitude = 5000f;

		private const float FertilityTempMinimum = -15f;

		private const float FertilityTempOptimal = 30f;

		private const float FertilityTempMaximum = 50f;

		public override int SeedPart => 83469557;

		private static float FreqMultiplier => 1f;

		public override void GenerateFresh(string seed)
		{
			GenerateGridIntoWorld();
		}

		public override void GenerateFromScribe(string seed)
		{
			Find.World.pathGrid = new WorldPathGrid();
			NoiseDebugUI.ClearPlanetNoises();
		}

		private void GenerateGridIntoWorld()
		{
			Find.World.grid = new WorldGrid();
			Find.World.pathGrid = new WorldPathGrid();
			NoiseDebugUI.ClearPlanetNoises();
			SetupElevationNoise();
			SetupTemperatureOffsetNoise();
			SetupRainfallNoise();
			SetupHillinessNoise();
			SetupSwampinessNoise();
			List<Tile> tiles = Find.WorldGrid.tiles;
			tiles.Clear();
			int tilesCount = Find.WorldGrid.TilesCount;
			for (int i = 0; i < tilesCount; i++)
			{
				Tile item = GenerateTileFor(i);
				tiles.Add(item);
			}
		}

		private void SetupElevationNoise()
		{
			float freqMultiplier = FreqMultiplier;
			ModuleBase lhs = new Perlin((double)(0.035f * freqMultiplier), 2.0, 0.40000000596046448, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase lhs2 = new RidgedMultifractal((double)(0.012f * freqMultiplier), 2.0, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase input = new Perlin((double)(0.12f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase moduleBase = new Perlin((double)(0.01f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.High);
			float num;
			if (Find.World.PlanetCoverage < 0.55f)
			{
				ModuleBase input2 = new DistanceFromPlanetViewCenter(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, invert: true);
				input2 = new ScaleBias(2.0, -1.0, input2);
				moduleBase = new Blend(moduleBase, input2, new Const(0.40000000596046448));
				num = Rand.Range(-0.4f, -0.35f);
			}
			else
			{
				num = Rand.Range(0.15f, 0.25f);
			}
			NoiseDebugUI.StorePlanetNoise(moduleBase, "elevContinents");
			input = new ScaleBias(0.5, 0.5, input);
			lhs2 = new Multiply(lhs2, input);
			float num2 = Rand.Range(0.4f, 0.6f);
			noiseElevation = new Blend(lhs, lhs2, new Const((double)num2));
			noiseElevation = new Blend(noiseElevation, moduleBase, new Const((double)num));
			if (Find.World.PlanetCoverage < 0.9999f)
			{
				noiseElevation = new ConvertToIsland(Find.WorldGrid.viewCenter, Find.WorldGrid.viewAngle, noiseElevation);
			}
			noiseElevation = new ScaleBias(0.5, 0.5, noiseElevation);
			noiseElevation = new Power(noiseElevation, new Const(3.0));
			NoiseDebugUI.StorePlanetNoise(noiseElevation, "noiseElevation");
			double scale = (double)ElevationRange.Span;
			FloatRange elevationRange = ElevationRange;
			noiseElevation = new ScaleBias(scale, (double)elevationRange.min, noiseElevation);
		}

		private void SetupTemperatureOffsetNoise()
		{
			float freqMultiplier = FreqMultiplier;
			noiseTemperatureOffset = new Perlin((double)(0.018f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			noiseTemperatureOffset = new Multiply(noiseTemperatureOffset, new Const(4.0));
		}

		private void SetupRainfallNoise()
		{
			float freqMultiplier = FreqMultiplier;
			ModuleBase input = new Perlin((double)(0.015f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			input = new ScaleBias(0.5, 0.5, input);
			NoiseDebugUI.StorePlanetNoise(input, "basePerlin");
			SimpleCurve simpleCurve = new SimpleCurve();
			simpleCurve.Add(0f, 1.12f);
			simpleCurve.Add(25f, 0.94f);
			simpleCurve.Add(45f, 0.7f);
			simpleCurve.Add(70f, 0.3f);
			simpleCurve.Add(80f, 0.05f);
			simpleCurve.Add(90f, 0.05f);
			ModuleBase moduleBase = new AbsLatitudeCurve(simpleCurve, 100f);
			NoiseDebugUI.StorePlanetNoise(moduleBase, "latCurve");
			noiseRainfall = new Multiply(input, moduleBase);
			float num = 0.000222222225f;
			float num2 = -500f * num;
			ModuleBase input2 = new ScaleBias((double)num, (double)num2, noiseElevation);
			input2 = new ScaleBias(-1.0, 1.0, input2);
			input2 = new Clamp(0.0, 1.0, input2);
			NoiseDebugUI.StorePlanetNoise(input2, "elevationRainfallEffect");
			noiseRainfall = new Multiply(noiseRainfall, input2);
			Func<double, double> processor = delegate(double val)
			{
				if (val < 0.0)
				{
					val = 0.0;
				}
				if (val < 0.12)
				{
					val = (val + 0.12) / 2.0;
					if (val < 0.03)
					{
						val = (val + 0.03) / 2.0;
					}
				}
				return val;
			};
			noiseRainfall = new Arbitrary(noiseRainfall, processor);
			noiseRainfall = new Power(noiseRainfall, new Const(1.5));
			noiseRainfall = new Clamp(0.0, 999.0, noiseRainfall);
			NoiseDebugUI.StorePlanetNoise(noiseRainfall, "noiseRainfall before mm");
			noiseRainfall = new ScaleBias(4000.0, 0.0, noiseRainfall);
			SimpleCurve rainfallCurve = Find.World.info.overallRainfall.GetRainfallCurve();
			if (rainfallCurve != null)
			{
				noiseRainfall = new CurveSimple(noiseRainfall, rainfallCurve);
			}
		}

		private void SetupHillinessNoise()
		{
			float freqMultiplier = FreqMultiplier;
			noiseMountainLines = new Perlin((double)(0.025f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase module = new Perlin((double)(0.06f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
			noiseMountainLines = new Abs(noiseMountainLines);
			noiseMountainLines = new OneMinus(noiseMountainLines);
			module = new Filter(module, -0.3f, 1f);
			noiseMountainLines = new Multiply(noiseMountainLines, module);
			noiseMountainLines = new OneMinus(noiseMountainLines);
			NoiseDebugUI.StorePlanetNoise(noiseMountainLines, "noiseMountainLines");
			noiseHillsPatchesMacro = new Perlin((double)(0.032f * freqMultiplier), 2.0, 0.5, 5, Rand.Range(0, 2147483647), QualityMode.Medium);
			noiseHillsPatchesMicro = new Perlin((double)(0.19f * freqMultiplier), 2.0, 0.5, 6, Rand.Range(0, 2147483647), QualityMode.High);
		}

		private void SetupSwampinessNoise()
		{
			float freqMultiplier = FreqMultiplier;
			ModuleBase input = new Perlin((double)(0.09f * freqMultiplier), 2.0, 0.40000000596046448, 6, Rand.Range(0, 2147483647), QualityMode.High);
			ModuleBase input2 = new RidgedMultifractal((double)(0.025f * freqMultiplier), 2.0, 6, Rand.Range(0, 2147483647), QualityMode.High);
			input = new ScaleBias(0.5, 0.5, input);
			input2 = new ScaleBias(0.5, 0.5, input2);
			noiseSwampiness = new Multiply(input, input2);
			ModuleBase module = noiseElevation;
			FloatRange swampinessMaxElevation = SwampinessMaxElevation;
			float max = swampinessMaxElevation.max;
			FloatRange swampinessMaxElevation2 = SwampinessMaxElevation;
			InverseLerp rhs = new InverseLerp(module, max, swampinessMaxElevation2.min);
			noiseSwampiness = new Multiply(noiseSwampiness, rhs);
			ModuleBase module2 = noiseRainfall;
			FloatRange swampinessMinRainfall = SwampinessMinRainfall;
			float min = swampinessMinRainfall.min;
			FloatRange swampinessMinRainfall2 = SwampinessMinRainfall;
			InverseLerp rhs2 = new InverseLerp(module2, min, swampinessMinRainfall2.max);
			noiseSwampiness = new Multiply(noiseSwampiness, rhs2);
			NoiseDebugUI.StorePlanetNoise(noiseSwampiness, "noiseSwampiness");
		}

		private Tile GenerateTileFor(int tileID)
		{
			Tile tile = new Tile();
			Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tileID);
			tile.elevation = noiseElevation.GetValue(tileCenter);
			float value = noiseMountainLines.GetValue(tileCenter);
			if (value > 0.235f || tile.elevation <= 0f)
			{
				if (tile.elevation > 0f && noiseHillsPatchesMicro.GetValue(tileCenter) > 0.46f && noiseHillsPatchesMacro.GetValue(tileCenter) > -0.3f)
				{
					if (Rand.Bool)
					{
						tile.hilliness = Hilliness.SmallHills;
					}
					else
					{
						tile.hilliness = Hilliness.LargeHills;
					}
				}
				else
				{
					tile.hilliness = Hilliness.Flat;
				}
			}
			else if (value > 0.12f)
			{
				switch (Rand.Range(0, 4))
				{
				case 0:
					tile.hilliness = Hilliness.Flat;
					break;
				case 1:
					tile.hilliness = Hilliness.SmallHills;
					break;
				case 2:
					tile.hilliness = Hilliness.LargeHills;
					break;
				case 3:
					tile.hilliness = Hilliness.Mountainous;
					break;
				}
			}
			else if (value > 0.0363f)
			{
				tile.hilliness = Hilliness.Mountainous;
			}
			else
			{
				tile.hilliness = Hilliness.Impassable;
			}
			Vector2 vector = Find.WorldGrid.LongLatOf(tileID);
			float num = BaseTemperatureAtLatitude(vector.y);
			num -= TemperatureReductionAtElevation(tile.elevation);
			num += noiseTemperatureOffset.GetValue(tileCenter);
			SimpleCurve temperatureCurve = Find.World.info.overallTemperature.GetTemperatureCurve();
			if (temperatureCurve != null)
			{
				num = temperatureCurve.Evaluate(num);
			}
			tile.temperature = num;
			tile.rainfall = noiseRainfall.GetValue(tileCenter);
			if (float.IsNaN(tile.rainfall))
			{
				float value2 = noiseRainfall.GetValue(tileCenter);
				Log.ErrorOnce(value2 + " rain bad at " + tileID, 694822);
			}
			if (tile.hilliness == Hilliness.Flat || tile.hilliness == Hilliness.SmallHills)
			{
				tile.swampiness = noiseSwampiness.GetValue(tileCenter);
			}
			tile.biome = BiomeFrom(tile, tileID);
			return tile;
		}

		private BiomeDef BiomeFrom(Tile ws, int tileID)
		{
			List<BiomeDef> allDefsListForReading = DefDatabase<BiomeDef>.AllDefsListForReading;
			BiomeDef biomeDef = null;
			float num = 0f;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				BiomeDef biomeDef2 = allDefsListForReading[i];
				if (biomeDef2.implemented)
				{
					float score = biomeDef2.Worker.GetScore(ws, tileID);
					if (score > num || biomeDef == null)
					{
						biomeDef = biomeDef2;
						num = score;
					}
				}
			}
			return biomeDef;
		}

		private static float FertilityFactorFromTemperature(float temp)
		{
			if (temp < -15f)
			{
				return 0f;
			}
			if (temp < 30f)
			{
				return Mathf.InverseLerp(-15f, 30f, temp);
			}
			if (temp < 50f)
			{
				return Mathf.InverseLerp(50f, 30f, temp);
			}
			return 0f;
		}

		private static float BaseTemperatureAtLatitude(float lat)
		{
			float x = Mathf.Abs(lat) / 90f;
			return AvgTempByLatitudeCurve.Evaluate(x);
		}

		private static float TemperatureReductionAtElevation(float elev)
		{
			if (elev < 250f)
			{
				return 0f;
			}
			float t = (elev - 250f) / 4750f;
			return Mathf.Lerp(0f, 40f, t);
		}
	}
}
