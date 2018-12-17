using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_Paths : WorldLayer
	{
		public struct OutputDirection
		{
			public int neighbor;

			public float width;

			public float distortionFrequency;

			public float distortionIntensity;
		}

		protected bool pointyEnds;

		private List<Vector3> tmpVerts = new List<Vector3>();

		private List<Vector3> tmpHexVerts = new List<Vector3>();

		private List<int> tmpNeighbors = new List<int>();

		private static List<int> lhsID = new List<int>();

		private static List<int> rhsID = new List<int>();

		public void GeneratePaths(LayerSubMesh subMesh, int tileID, List<OutputDirection> nodes, Color32 color, bool allowSmoothTransition)
		{
			WorldGrid worldGrid = Find.WorldGrid;
			worldGrid.GetTileVertices(tileID, tmpVerts);
			worldGrid.GetTileNeighbors(tileID, tmpNeighbors);
			if (nodes.Count == 1 && pointyEnds)
			{
				int count = subMesh.verts.Count;
				List<Vector3> verts = tmpVerts;
				List<int> list = tmpNeighbors;
				OutputDirection outputDirection = nodes[0];
				AddPathEndpoint(subMesh, verts, list.IndexOf(outputDirection.neighbor), color, tileID, nodes[0]);
				List<Vector3> verts2 = subMesh.verts;
				Vector3 tileCenter = worldGrid.GetTileCenter(tileID);
				OutputDirection outputDirection2 = nodes[0];
				float distortionFrequency = outputDirection2.distortionFrequency;
				OutputDirection outputDirection3 = nodes[0];
				verts2.Add(FinalizePoint(tileCenter, distortionFrequency, outputDirection3.distortionIntensity));
				subMesh.colors.Add(color.MutateAlpha(0));
				subMesh.tris.Add(count);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 1);
				subMesh.tris.Add(count + 3);
				subMesh.tris.Add(count + 2);
			}
			else
			{
				if (nodes.Count == 2)
				{
					int count2 = subMesh.verts.Count;
					List<int> list2 = tmpNeighbors;
					OutputDirection outputDirection4 = nodes[0];
					int num = list2.IndexOf(outputDirection4.neighbor);
					List<int> list3 = tmpNeighbors;
					OutputDirection outputDirection5 = nodes[1];
					int num2 = list3.IndexOf(outputDirection5.neighbor);
					if (allowSmoothTransition && Mathf.Abs(num - num2) > 1 && Mathf.Abs((num - num2 + tmpVerts.Count) % tmpVerts.Count) > 1)
					{
						AddPathEndpoint(subMesh, tmpVerts, num, color, tileID, nodes[0]);
						AddPathEndpoint(subMesh, tmpVerts, num2, color, tileID, nodes[1]);
						subMesh.tris.Add(count2);
						subMesh.tris.Add(count2 + 5);
						subMesh.tris.Add(count2 + 1);
						subMesh.tris.Add(count2 + 5);
						subMesh.tris.Add(count2 + 4);
						subMesh.tris.Add(count2 + 1);
						subMesh.tris.Add(count2 + 1);
						subMesh.tris.Add(count2 + 4);
						subMesh.tris.Add(count2 + 2);
						subMesh.tris.Add(count2 + 4);
						subMesh.tris.Add(count2 + 3);
						subMesh.tris.Add(count2 + 2);
						return;
					}
				}
				float num3 = 0f;
				for (int i = 0; i < nodes.Count; i++)
				{
					float a = num3;
					OutputDirection outputDirection6 = nodes[i];
					num3 = Mathf.Max(a, outputDirection6.width);
				}
				Vector3 tileCenter2 = worldGrid.GetTileCenter(tileID);
				tmpHexVerts.Clear();
				for (int j = 0; j < tmpVerts.Count; j++)
				{
					tmpHexVerts.Add(FinalizePoint(Vector3.LerpUnclamped(tileCenter2, tmpVerts[j], num3 * 0.5f * 2f), 0f, 0f));
				}
				tileCenter2 = FinalizePoint(tileCenter2, 0f, 0f);
				int count3 = subMesh.verts.Count;
				subMesh.verts.Add(tileCenter2);
				subMesh.colors.Add(color);
				int count4 = subMesh.verts.Count;
				for (int k = 0; k < tmpHexVerts.Count; k++)
				{
					subMesh.verts.Add(tmpHexVerts[k]);
					subMesh.colors.Add(color.MutateAlpha(0));
					subMesh.tris.Add(count3);
					subMesh.tris.Add(count4 + (k + 1) % tmpHexVerts.Count);
					subMesh.tris.Add(count4 + k);
				}
				for (int l = 0; l < nodes.Count; l++)
				{
					OutputDirection outputDirection7 = nodes[l];
					if (outputDirection7.width != 0f)
					{
						int count5 = subMesh.verts.Count;
						List<int> list4 = tmpNeighbors;
						OutputDirection outputDirection8 = nodes[l];
						int num4 = list4.IndexOf(outputDirection8.neighbor);
						AddPathEndpoint(subMesh, tmpVerts, num4, color, tileID, nodes[l]);
						subMesh.tris.Add(count5);
						subMesh.tris.Add(count4 + (num4 + tmpHexVerts.Count - 1) % tmpHexVerts.Count);
						subMesh.tris.Add(count3);
						subMesh.tris.Add(count5);
						subMesh.tris.Add(count3);
						subMesh.tris.Add(count5 + 1);
						subMesh.tris.Add(count5 + 1);
						subMesh.tris.Add(count3);
						subMesh.tris.Add(count5 + 2);
						subMesh.tris.Add(count3);
						subMesh.tris.Add(count4 + (num4 + 2) % tmpHexVerts.Count);
						subMesh.tris.Add(count5 + 2);
					}
				}
			}
		}

		private void AddPathEndpoint(LayerSubMesh subMesh, List<Vector3> verts, int index, Color32 color, int tileID, OutputDirection data)
		{
			int index2 = (index + 1) % verts.Count;
			Find.WorldGrid.GetTileNeighbors(tileID, lhsID);
			Find.WorldGrid.GetTileNeighbors(data.neighbor, rhsID);
			float num = (!lhsID.Intersect(rhsID).Any((int id) => Find.WorldGrid[id].WaterCovered)) ? 1f : 0.5f;
			Vector3 a = FinalizePoint(verts[index], data.distortionFrequency, data.distortionIntensity * num);
			Vector3 b = FinalizePoint(verts[index2], data.distortionFrequency, data.distortionIntensity * num);
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f - data.width));
			subMesh.colors.Add(color.MutateAlpha(0));
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f));
			subMesh.colors.Add(color);
			subMesh.verts.Add(Vector3.LerpUnclamped(a, b, 0.5f + data.width));
			subMesh.colors.Add(color.MutateAlpha(0));
		}

		public abstract Vector3 FinalizePoint(Vector3 inp, float distortionFrequency, float distortionIntensity);
	}
}
