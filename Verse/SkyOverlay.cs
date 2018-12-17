using UnityEngine;

namespace Verse
{
	public abstract class SkyOverlay
	{
		public Material worldOverlayMat;

		public Material screenOverlayMat;

		protected float worldOverlayPanSpeed1;

		protected float worldOverlayPanSpeed2;

		protected Vector2 worldPanDir1;

		protected Vector2 worldPanDir2;

		public Color OverlayColor
		{
			set
			{
				if (worldOverlayMat != null)
				{
					worldOverlayMat.color = value;
				}
				if (screenOverlayMat != null)
				{
					screenOverlayMat.color = value;
				}
			}
		}

		public SkyOverlay()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				OverlayColor = Color.clear;
			});
		}

		public virtual void TickOverlay(Map map)
		{
			if (worldOverlayMat != null)
			{
				Material material = worldOverlayMat;
				Vector2 a = (float)Find.TickManager.TicksGame * worldPanDir1 * -1f * worldOverlayPanSpeed1;
				Vector2 textureScale = worldOverlayMat.GetTextureScale("_MainTex");
				material.SetTextureOffset("_MainTex", a * textureScale.x);
				if (worldOverlayMat.HasProperty("_MainTex2"))
				{
					Material material2 = worldOverlayMat;
					Vector2 a2 = (float)Find.TickManager.TicksGame * worldPanDir2 * -1f * worldOverlayPanSpeed2;
					Vector2 textureScale2 = worldOverlayMat.GetTextureScale("_MainTex2");
					material2.SetTextureOffset("_MainTex2", a2 * textureScale2.x);
				}
			}
		}

		public void DrawOverlay(Map map)
		{
			if (worldOverlayMat != null)
			{
				Vector3 position = map.Center.ToVector3ShiftedWithAltitude(AltitudeLayer.Weather);
				Graphics.DrawMesh(MeshPool.wholeMapPlane, position, Quaternion.identity, worldOverlayMat, 0);
			}
			if (screenOverlayMat != null)
			{
				float num = Find.Camera.orthographicSize * 2f;
				Vector3 s = new Vector3(num * Find.Camera.aspect, 1f, num);
				Vector3 position2 = Find.Camera.transform.position;
				position2.y = AltitudeLayer.Weather.AltitudeFor() + 0.046875f;
				Matrix4x4 matrix = default(Matrix4x4);
				matrix.SetTRS(position2, Quaternion.identity, s);
				Graphics.DrawMesh(MeshPool.plane10, matrix, screenOverlayMat, 0);
			}
		}

		public override string ToString()
		{
			if (worldOverlayMat != null)
			{
				return worldOverlayMat.name;
			}
			if (screenOverlayMat != null)
			{
				return screenOverlayMat.name;
			}
			return "NoOverlayOverlay";
		}
	}
}
