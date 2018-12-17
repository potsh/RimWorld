using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public abstract class WorldLayer_SingleTile : WorldLayer
	{
		private int lastDrawnTile = -1;

		private List<Vector3> verts = new List<Vector3>();

		protected abstract int Tile
		{
			get;
		}

		protected abstract Material Material
		{
			get;
		}

		public override bool ShouldRegenerate => base.ShouldRegenerate || Tile != lastDrawnTile;

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
			int tile = Tile;
			if (tile >= 0)
			{
				LayerSubMesh subMesh = GetSubMesh(Material);
				Find.WorldGrid.GetTileVertices(tile, verts);
				int count = subMesh.verts.Count;
				int i = 0;
				for (int count2 = verts.Count; i < count2; i++)
				{
					subMesh.verts.Add(verts[i] + verts[i].normalized * 0.012f);
					subMesh.uvs.Add((GenGeo.RegularPolygonVertexPosition(count2, i) + Vector2.one) / 2f);
					if (i < count2 - 2)
					{
						subMesh.tris.Add(count + i + 2);
						subMesh.tris.Add(count + i + 1);
						subMesh.tris.Add(count);
					}
				}
				FinalizeMesh(MeshParts.All);
			}
			lastDrawnTile = tile;
			yield break;
			IL_021a:
			/*Error near IL_021b: Unexpected return in MoveNext()*/;
		}
	}
}
