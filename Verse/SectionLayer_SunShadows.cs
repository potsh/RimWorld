using UnityEngine;

namespace Verse
{
	internal class SectionLayer_SunShadows : SectionLayer
	{
		private static readonly Color32 LowVertexColor = new Color32(0, 0, 0, 0);

		public override bool Visible => DebugViewSettings.drawShadows;

		public SectionLayer_SunShadows(Section section)
			: base(section)
		{
			relevantChangeTypes = MapMeshFlag.Buildings;
		}

		public override void Regenerate()
		{
			if (MatBases.SunShadow.shader.isSupported)
			{
				Building[] innerArray = base.Map.edificeGrid.InnerArray;
				float y = AltitudeLayer.Shadows.AltitudeFor();
				CellRect cellRect = new CellRect(section.botLeft.x, section.botLeft.z, 17, 17);
				cellRect.ClipInsideMap(base.Map);
				LayerSubMesh subMesh = GetSubMesh(MatBases.SunShadow);
				subMesh.Clear(MeshParts.All);
				subMesh.verts.Capacity = cellRect.Area * 2;
				subMesh.tris.Capacity = cellRect.Area * 4;
				subMesh.colors.Capacity = cellRect.Area * 2;
				CellIndices cellIndices = base.Map.cellIndices;
				for (int i = cellRect.minX; i <= cellRect.maxX; i++)
				{
					for (int j = cellRect.minZ; j <= cellRect.maxZ; j++)
					{
						Thing thing = innerArray[cellIndices.CellToIndex(i, j)];
						if (thing != null && thing.def.staticSunShadowHeight > 0f)
						{
							float staticSunShadowHeight = thing.def.staticSunShadowHeight;
							Color32 item = new Color32(0, 0, 0, (byte)(255f * staticSunShadowHeight));
							int count = subMesh.verts.Count;
							subMesh.verts.Add(new Vector3((float)i, y, (float)j));
							subMesh.verts.Add(new Vector3((float)i, y, (float)(j + 1)));
							subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)(j + 1)));
							subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)j));
							subMesh.colors.Add(LowVertexColor);
							subMesh.colors.Add(LowVertexColor);
							subMesh.colors.Add(LowVertexColor);
							subMesh.colors.Add(LowVertexColor);
							int count2 = subMesh.verts.Count;
							subMesh.tris.Add(count2 - 4);
							subMesh.tris.Add(count2 - 3);
							subMesh.tris.Add(count2 - 2);
							subMesh.tris.Add(count2 - 4);
							subMesh.tris.Add(count2 - 2);
							subMesh.tris.Add(count2 - 1);
							if (i > 0)
							{
								thing = innerArray[cellIndices.CellToIndex(i - 1, j)];
								if (thing == null || thing.def.staticSunShadowHeight < staticSunShadowHeight)
								{
									int count3 = subMesh.verts.Count;
									subMesh.verts.Add(new Vector3((float)i, y, (float)j));
									subMesh.verts.Add(new Vector3((float)i, y, (float)(j + 1)));
									subMesh.colors.Add(item);
									subMesh.colors.Add(item);
									subMesh.tris.Add(count + 1);
									subMesh.tris.Add(count);
									subMesh.tris.Add(count3);
									subMesh.tris.Add(count3);
									subMesh.tris.Add(count3 + 1);
									subMesh.tris.Add(count + 1);
								}
							}
							int num = i;
							IntVec3 size = base.Map.Size;
							if (num < size.x - 1)
							{
								thing = innerArray[cellIndices.CellToIndex(i + 1, j)];
								if (thing == null || thing.def.staticSunShadowHeight < staticSunShadowHeight)
								{
									int count4 = subMesh.verts.Count;
									subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)(j + 1)));
									subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)j));
									subMesh.colors.Add(item);
									subMesh.colors.Add(item);
									subMesh.tris.Add(count + 2);
									subMesh.tris.Add(count4);
									subMesh.tris.Add(count4 + 1);
									subMesh.tris.Add(count4 + 1);
									subMesh.tris.Add(count + 3);
									subMesh.tris.Add(count + 2);
								}
							}
							if (j > 0)
							{
								thing = innerArray[cellIndices.CellToIndex(i, j - 1)];
								if (thing == null || thing.def.staticSunShadowHeight < staticSunShadowHeight)
								{
									int count5 = subMesh.verts.Count;
									subMesh.verts.Add(new Vector3((float)i, y, (float)j));
									subMesh.verts.Add(new Vector3((float)(i + 1), y, (float)j));
									subMesh.colors.Add(item);
									subMesh.colors.Add(item);
									subMesh.tris.Add(count);
									subMesh.tris.Add(count + 3);
									subMesh.tris.Add(count5);
									subMesh.tris.Add(count + 3);
									subMesh.tris.Add(count5 + 1);
									subMesh.tris.Add(count5);
								}
							}
						}
					}
				}
				if (subMesh.verts.Count > 0)
				{
					subMesh.FinalizeMesh(MeshParts.Verts | MeshParts.Tris | MeshParts.Colors);
					float num2 = Mathf.Max(15f, 15f);
					Vector3 size2 = subMesh.mesh.bounds.size;
					size2.x += 2f * num2 + 2f;
					size2.z += 2f * num2 + 2f;
					subMesh.mesh.bounds = new Bounds(subMesh.mesh.bounds.center, size2);
				}
			}
		}
	}
}
