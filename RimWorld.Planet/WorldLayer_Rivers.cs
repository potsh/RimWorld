using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace RimWorld.Planet
{
	public class WorldLayer_Rivers : WorldLayer_Paths
	{
		private Color32 riverColor = new Color32(73, 82, 100, byte.MaxValue);

		private const float PerlinFrequency = 0.6f;

		private const float PerlinMagnitude = 0.1f;

		private ModuleBase riverDisplacementX = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 84905524, QualityMode.Medium);

		private ModuleBase riverDisplacementY = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 37971116, QualityMode.Medium);

		private ModuleBase riverDisplacementZ = new Perlin(0.60000002384185791, 2.0, 0.5, 3, 91572032, QualityMode.Medium);

		public WorldLayer_Rivers()
		{
			pointyEnds = true;
		}

		public override IEnumerable Regenerate()
		{
			IEnumerator enumerator = base.Regenerate().GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					object result = enumerator.Current;
					yield return result;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = disposable = (enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			LayerSubMesh subMesh = GetSubMesh(WorldMaterials.Rivers);
			LayerSubMesh subMeshBorder = GetSubMesh(WorldMaterials.RiversBorder);
			WorldGrid grid = Find.WorldGrid;
			List<OutputDirection> outputs = new List<OutputDirection>();
			List<OutputDirection> outputsBorder = new List<OutputDirection>();
			for (int i = 0; i < grid.TilesCount; i++)
			{
				if (i % 1000 == 0)
				{
					yield return (object)null;
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (subMesh.verts.Count > 60000)
				{
					subMesh = GetSubMesh(WorldMaterials.Rivers);
					subMeshBorder = GetSubMesh(WorldMaterials.RiversBorder);
				}
				Tile tile = grid[i];
				if (tile.potentialRivers != null)
				{
					outputs.Clear();
					outputsBorder.Clear();
					for (int j = 0; j < tile.potentialRivers.Count; j++)
					{
						List<OutputDirection> list = outputs;
						OutputDirection item = default(OutputDirection);
						Tile.RiverLink riverLink = tile.potentialRivers[j];
						item.neighbor = riverLink.neighbor;
						Tile.RiverLink riverLink2 = tile.potentialRivers[j];
						item.width = riverLink2.river.widthOnWorld - 0.2f;
						list.Add(item);
						List<OutputDirection> list2 = outputsBorder;
						OutputDirection item2 = default(OutputDirection);
						Tile.RiverLink riverLink3 = tile.potentialRivers[j];
						item2.neighbor = riverLink3.neighbor;
						Tile.RiverLink riverLink4 = tile.potentialRivers[j];
						item2.width = riverLink4.river.widthOnWorld;
						list2.Add(item2);
					}
					GeneratePaths(subMesh, i, outputs, riverColor, allowSmoothTransition: true);
					GeneratePaths(subMeshBorder, i, outputsBorder, riverColor, allowSmoothTransition: true);
				}
			}
			FinalizeMesh(MeshParts.All);
			yield break;
			IL_0335:
			/*Error near IL_0336: Unexpected return in MoveNext()*/;
		}

		public override Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity)
		{
			float magnitude = inp.magnitude;
			inp = (inp + new Vector3(riverDisplacementX.GetValue(inp), riverDisplacementY.GetValue(inp), riverDisplacementZ.GetValue(inp)) * 0.1f).normalized * magnitude;
			return inp + inp.normalized * 0.008f;
		}
	}
}
