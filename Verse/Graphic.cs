using RimWorld;
using UnityEngine;

namespace Verse
{
	public class Graphic
	{
		public GraphicData data;

		public string path;

		public Color color = Color.white;

		public Color colorTwo = Color.white;

		public Vector2 drawSize = Vector2.one;

		private Graphic_Shadow cachedShadowGraphicInt;

		private Graphic cachedShadowlessGraphicInt;

		public Shader Shader
		{
			get
			{
				Material matSingle = MatSingle;
				if (matSingle != null)
				{
					return matSingle.shader;
				}
				return ShaderDatabase.Cutout;
			}
		}

		public Graphic_Shadow ShadowGraphic
		{
			get
			{
				if (cachedShadowGraphicInt == null && data != null && data.shadowData != null)
				{
					cachedShadowGraphicInt = new Graphic_Shadow(data.shadowData);
				}
				return cachedShadowGraphicInt;
			}
		}

		public Color Color => color;

		public Color ColorTwo => colorTwo;

		public virtual Material MatSingle => BaseContent.BadMat;

		public virtual Material MatWest => MatSingle;

		public virtual Material MatSouth => MatSingle;

		public virtual Material MatEast => MatSingle;

		public virtual Material MatNorth => MatSingle;

		public virtual bool WestFlipped => DataAllowsFlip && !ShouldDrawRotated;

		public virtual bool EastFlipped => false;

		public virtual bool ShouldDrawRotated => false;

		public virtual float DrawRotatedExtraAngleOffset => 0f;

		protected bool DataAllowsFlip => data == null || data.allowFlip;

		public virtual void Init(GraphicRequest req)
		{
			Log.ErrorOnce("Cannot init Graphic of class " + GetType().ToString(), 658928);
		}

		public virtual Material MatAt(Rot4 rot, Thing thing = null)
		{
			switch (rot.AsInt)
			{
			case 0:
				return MatNorth;
			case 1:
				return MatEast;
			case 2:
				return MatSouth;
			case 3:
				return MatWest;
			default:
				return BaseContent.BadMat;
			}
		}

		public virtual Mesh MeshAt(Rot4 rot)
		{
			Vector2 vector = drawSize;
			if (rot.IsHorizontal && !ShouldDrawRotated)
			{
				vector = vector.Rotated();
			}
			if ((rot == Rot4.West && WestFlipped) || (rot == Rot4.East && EastFlipped))
			{
				return MeshPool.GridPlaneFlip(vector);
			}
			return MeshPool.GridPlane(vector);
		}

		public virtual Material MatSingleFor(Thing thing)
		{
			return MatSingle;
		}

		public void Draw(Vector3 loc, Rot4 rot, Thing thing, float extraRotation = 0f)
		{
			DrawWorker(loc, rot, thing.def, thing, extraRotation);
		}

		public void DrawFromDef(Vector3 loc, Rot4 rot, ThingDef thingDef, float extraRotation = 0f)
		{
			DrawWorker(loc, rot, thingDef, null, extraRotation);
		}

		public virtual void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
		{
			Mesh mesh = MeshAt(rot);
			Quaternion quaternion = QuatFromRot(rot);
			if (extraRotation != 0f)
			{
				quaternion *= Quaternion.Euler(Vector3.up * extraRotation);
			}
			Material material = MatAt(rot, thing);
			Graphics.DrawMesh(mesh, loc, quaternion, material, 0);
			if (ShadowGraphic != null)
			{
				ShadowGraphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
			}
		}

		public virtual void Print(SectionLayer layer, Thing thing)
		{
			Vector2 size;
			bool flag;
			if (ShouldDrawRotated)
			{
				size = drawSize;
				flag = false;
			}
			else
			{
				size = (thing.Rotation.IsHorizontal ? drawSize.Rotated() : drawSize);
				flag = ((thing.Rotation == Rot4.West && WestFlipped) || (thing.Rotation == Rot4.East && EastFlipped));
			}
			float num = AngleFromRot(thing.Rotation);
			if (flag && data != null)
			{
				num += data.flipExtraRotation;
			}
			Printer_Plane.PrintPlane(layer, thing.TrueCenter(), size, MatAt(thing.Rotation, thing), num, flag);
			if (ShadowGraphic != null && thing != null)
			{
				ShadowGraphic.Print(layer, thing);
			}
		}

		public virtual Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
		{
			Log.ErrorOnce("CloneColored not implemented on this subclass of Graphic: " + GetType().ToString(), 66300);
			return BaseContent.BadGraphic;
		}

		public virtual Graphic GetCopy(Vector2 newDrawSize)
		{
			return GraphicDatabase.Get(GetType(), path, Shader, newDrawSize, color, colorTwo);
		}

		public virtual Graphic GetShadowlessGraphic()
		{
			if (data == null || data.shadowData == null)
			{
				return this;
			}
			if (cachedShadowlessGraphicInt == null)
			{
				GraphicData graphicData = new GraphicData();
				graphicData.CopyFrom(data);
				graphicData.shadowData = null;
				cachedShadowlessGraphicInt = graphicData.Graphic;
			}
			return cachedShadowlessGraphicInt;
		}

		protected float AngleFromRot(Rot4 rot)
		{
			if (ShouldDrawRotated)
			{
				float asAngle = rot.AsAngle;
				asAngle += DrawRotatedExtraAngleOffset;
				if ((rot == Rot4.West && WestFlipped) || (rot == Rot4.East && EastFlipped))
				{
					asAngle += 180f;
				}
				return asAngle;
			}
			return 0f;
		}

		protected Quaternion QuatFromRot(Rot4 rot)
		{
			float num = AngleFromRot(rot);
			if (num == 0f)
			{
				return Quaternion.identity;
			}
			return Quaternion.AngleAxis(num, Vector3.up);
		}
	}
}
