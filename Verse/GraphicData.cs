using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class GraphicData
	{
		[NoTranslate]
		public string texPath;

		public Type graphicClass;

		public ShaderTypeDef shaderType;

		public List<ShaderParameter> shaderParameters;

		public Color color = Color.white;

		public Color colorTwo = Color.white;

		public Vector2 drawSize = Vector2.one;

		public float onGroundRandomRotateAngle;

		public bool drawRotated = true;

		public bool allowFlip = true;

		public float flipExtraRotation;

		public ShadowData shadowData;

		public DamageGraphicData damageData;

		public LinkDrawerType linkType;

		public LinkFlags linkFlags;

		[Unsaved]
		private Graphic cachedGraphic;

		public bool Linked => linkType != LinkDrawerType.None;

		public Graphic Graphic
		{
			get
			{
				if (cachedGraphic == null)
				{
					Init();
				}
				return cachedGraphic;
			}
		}

		public void CopyFrom(GraphicData other)
		{
			texPath = other.texPath;
			graphicClass = other.graphicClass;
			shaderType = other.shaderType;
			color = other.color;
			colorTwo = other.colorTwo;
			drawSize = other.drawSize;
			onGroundRandomRotateAngle = other.onGroundRandomRotateAngle;
			drawRotated = other.drawRotated;
			allowFlip = other.allowFlip;
			flipExtraRotation = other.flipExtraRotation;
			shadowData = other.shadowData;
			damageData = other.damageData;
			linkType = other.linkType;
			linkFlags = other.linkFlags;
		}

		private void Init()
		{
			if (graphicClass == null)
			{
				cachedGraphic = null;
			}
			else
			{
				ShaderTypeDef cutout = shaderType;
				if (cutout == null)
				{
					cutout = ShaderTypeDefOf.Cutout;
				}
				Shader shader = cutout.Shader;
				cachedGraphic = GraphicDatabase.Get(graphicClass, texPath, shader, drawSize, color, colorTwo, this, shaderParameters);
				if (onGroundRandomRotateAngle > 0.01f)
				{
					cachedGraphic = new Graphic_RandomRotated(cachedGraphic, onGroundRandomRotateAngle);
				}
				if (Linked)
				{
					cachedGraphic = GraphicUtility.WrapLinked(cachedGraphic, linkType);
				}
			}
		}

		public void ResolveReferencesSpecial()
		{
			if (damageData != null)
			{
				damageData.ResolveReferencesSpecial();
			}
		}

		public Graphic GraphicColoredFor(Thing t)
		{
			if (t.DrawColor.IndistinguishableFrom(Graphic.Color) && t.DrawColorTwo.IndistinguishableFrom(Graphic.ColorTwo))
			{
				return Graphic;
			}
			return Graphic.GetColoredVersion(Graphic.Shader, t.DrawColor, t.DrawColorTwo);
		}

		internal IEnumerable<string> ConfigErrors(ThingDef thingDef)
		{
			if (graphicClass == null)
			{
				yield return "graphicClass is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (texPath.NullOrEmpty())
			{
				yield return "texPath is null or empty";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (thingDef != null && thingDef.drawerType == DrawerType.RealtimeOnly && Linked)
			{
				yield return "does not add to map mesh but has a link drawer. Link drawers can only work on the map mesh.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if ((shaderType == ShaderTypeDefOf.Cutout || shaderType == ShaderTypeDefOf.CutoutComplex) && thingDef.mote != null && (thingDef.mote.fadeInTime > 0f || thingDef.mote.fadeOutTime > 0f))
			{
				yield return "mote fades but uses cutout shader type. It will abruptly disappear when opacity falls under the cutout threshold.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
