using UnityEngine;

namespace Verse
{
	public class Graphic_RandomRotated : Graphic
	{
		private Graphic subGraphic;

		private float maxAngle;

		public override Material MatSingle => subGraphic.MatSingle;

		public Graphic_RandomRotated(Graphic subGraphic, float maxAngle)
		{
			this.subGraphic = subGraphic;
			this.maxAngle = maxAngle;
		}

		public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mesh mesh = MeshAt(rot);
			float num = 0f;
			if (thing != null)
			{
				num = 0f - maxAngle + (float)(thing.thingIDNumber * 542) % (maxAngle * 2f);
			}
			num += extraRotation;
			Material matSingle = subGraphic.MatSingle;
			Graphics.DrawMesh(mesh, loc, Quaternion.AngleAxis(num, Vector3.up), matSingle, 0, null, 0);
		}

		public override string ToString()
		{
			return "RandomRotated(subGraphic=" + subGraphic.ToString() + ")";
		}

		public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			Graphic_RandomRotated graphic_RandomRotated = new Graphic_RandomRotated(subGraphic.GetColoredVersion(newShader, newColor, newColorTwo), maxAngle);
			graphic_RandomRotated.data = data;
			return graphic_RandomRotated;
		}
	}
}
