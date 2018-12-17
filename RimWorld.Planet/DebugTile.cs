using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class DebugTile
	{
		public int tile;

		public string displayString;

		public float colorPct;

		public int ticksLeft;

		public Material customMat;

		private Mesh mesh;

		private static List<Vector3> tmpVerts = new List<Vector3>();

		private static List<int> tmpIndices = new List<int>();

		private Vector2 ScreenPos
		{
			get
			{
				Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
				return GenWorldUI.WorldToUIPosition(tileCenter);
			}
		}

		private bool VisibleForCamera => new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight).Contains(ScreenPos);

		public float DistanceToCamera
		{
			get
			{
				Vector3 tileCenter = Find.WorldGrid.GetTileCenter(tile);
				return Vector3.Distance(Find.WorldCamera.transform.position, tileCenter);
			}
		}

		public void Draw()
		{
			if (VisibleForCamera)
			{
				if (mesh == null)
				{
					Find.WorldGrid.GetTileVertices(tile, tmpVerts);
					for (int i = 0; i < tmpVerts.Count; i++)
					{
						Vector3 a = tmpVerts[i];
						tmpVerts[i] = a + a.normalized * 0.012f;
					}
					mesh = new Mesh();
					mesh.name = "DebugTile";
					mesh.SetVertices(tmpVerts);
					tmpIndices.Clear();
					for (int j = 0; j < tmpVerts.Count - 2; j++)
					{
						tmpIndices.Add(j + 2);
						tmpIndices.Add(j + 1);
						tmpIndices.Add(0);
					}
					mesh.SetTriangles(tmpIndices, 0);
				}
				Material material;
				if (customMat != null)
				{
					material = customMat;
				}
				else
				{
					int num = Mathf.RoundToInt(colorPct * 100f);
					num %= 100;
					material = WorldDebugMatsSpectrum.Mat(num);
				}
				Graphics.DrawMesh(mesh, Vector3.zero, Quaternion.identity, material, WorldCameraManager.WorldLayer);
			}
		}

		public void OnGUI()
		{
			if (VisibleForCamera)
			{
				Vector2 screenPos = ScreenPos;
				Rect rect = new Rect(screenPos.x - 20f, screenPos.y - 20f, 40f, 40f);
				if (displayString != null)
				{
					Widgets.Label(rect, displayString);
				}
			}
		}
	}
}
