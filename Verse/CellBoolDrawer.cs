using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class CellBoolDrawer
	{
		public ICellBoolGiver giver;

		private bool wantDraw;

		private Material material;

		private bool materialCaresAboutVertexColors;

		private bool dirty = true;

		private List<Mesh> meshes = new List<Mesh>();

		private int mapSizeX;

		private int mapSizeZ;

		private float opacity = 0.33f;

		private static List<Vector3> verts = new List<Vector3>();

		private static List<int> tris = new List<int>();

		private static List<Color> colors = new List<Color>();

		private const float DefaultOpacity = 0.33f;

		private const int MaxCellsPerMesh = 16383;

		public CellBoolDrawer(ICellBoolGiver giver, int mapSizeX, int mapSizeZ, float opacity = 0.33f)
		{
			this.giver = giver;
			this.mapSizeX = mapSizeX;
			this.mapSizeZ = mapSizeZ;
			this.opacity = opacity;
		}

		public void MarkForDraw()
		{
			wantDraw = true;
		}

		public void CellBoolDrawerUpdate()
		{
			if (wantDraw)
			{
				ActuallyDraw();
				wantDraw = false;
			}
		}

		private void ActuallyDraw()
		{
			if (dirty)
			{
				RegenerateMesh();
			}
			for (int i = 0; i < meshes.Count; i++)
			{
				Graphics.DrawMesh(meshes[i], Vector3.zero, Quaternion.identity, material, 0);
			}
		}

		public void SetDirty()
		{
			dirty = true;
		}

		public void RegenerateMesh()
		{
			for (int i = 0; i < meshes.Count; i++)
			{
				meshes[i].Clear();
			}
			int num = 0;
			int num2 = 0;
			if (meshes.Count < num + 1)
			{
				Mesh mesh = new Mesh();
				mesh.name = "CellBoolDrawer";
				meshes.Add(mesh);
			}
			Mesh mesh2 = meshes[num];
			CellRect cellRect = new CellRect(0, 0, mapSizeX, mapSizeZ);
			float y = AltitudeLayer.MapDataOverlay.AltitudeFor();
			bool careAboutVertexColors = false;
			for (int j = cellRect.minX; j <= cellRect.maxX; j++)
			{
				for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
				{
					int index = CellIndicesUtility.CellToIndex(j, k, mapSizeX);
					if (giver.GetCellBool(index))
					{
						verts.Add(new Vector3((float)j, y, (float)k));
						verts.Add(new Vector3((float)j, y, (float)(k + 1)));
						verts.Add(new Vector3((float)(j + 1), y, (float)(k + 1)));
						verts.Add(new Vector3((float)(j + 1), y, (float)k));
						Color cellExtraColor = giver.GetCellExtraColor(index);
						colors.Add(cellExtraColor);
						colors.Add(cellExtraColor);
						colors.Add(cellExtraColor);
						colors.Add(cellExtraColor);
						if (cellExtraColor != Color.white)
						{
							careAboutVertexColors = true;
						}
						int count = verts.Count;
						tris.Add(count - 4);
						tris.Add(count - 3);
						tris.Add(count - 2);
						tris.Add(count - 4);
						tris.Add(count - 2);
						tris.Add(count - 1);
						num2++;
						if (num2 >= 16383)
						{
							FinalizeWorkingDataIntoMesh(mesh2);
							num++;
							if (meshes.Count < num + 1)
							{
								Mesh mesh3 = new Mesh();
								mesh3.name = "CellBoolDrawer";
								meshes.Add(mesh3);
							}
							mesh2 = meshes[num];
							num2 = 0;
						}
					}
				}
			}
			FinalizeWorkingDataIntoMesh(mesh2);
			CreateMaterialIfNeeded(careAboutVertexColors);
			dirty = false;
		}

		private void FinalizeWorkingDataIntoMesh(Mesh mesh)
		{
			if (verts.Count > 0)
			{
				mesh.SetVertices(verts);
				verts.Clear();
				mesh.SetTriangles(tris, 0);
				tris.Clear();
				mesh.SetColors(colors);
				colors.Clear();
			}
		}

		private void CreateMaterialIfNeeded(bool careAboutVertexColors)
		{
			if (material == null || materialCaresAboutVertexColors != careAboutVertexColors)
			{
				Color color = giver.Color;
				float r = color.r;
				Color color2 = giver.Color;
				float g = color2.g;
				Color color3 = giver.Color;
				float b = color3.b;
				float num = opacity;
				Color color4 = giver.Color;
				material = SolidColorMaterials.SimpleSolidColorMaterial(new Color(r, g, b, num * color4.a), careAboutVertexColors);
				materialCaresAboutVertexColors = careAboutVertexColors;
				material.renderQueue = 3600;
			}
		}
	}
}
