using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class SectionLayer_BuildingsDamage : SectionLayer
	{
		private static List<Vector2> scratches = new List<Vector2>();

		public SectionLayer_BuildingsDamage(Section section)
			: base(section)
		{
			relevantChangeTypes = (MapMeshFlag.Buildings | MapMeshFlag.BuildingsDamage);
		}

		public override void Regenerate()
		{
			ClearSubMeshes(MeshParts.All);
			foreach (IntVec3 item in section.CellRect)
			{
				IntVec3 current = item;
				List<Thing> list = base.Map.thingGrid.ThingsListAt(current);
				int count = list.Count;
				for (int i = 0; i < count; i++)
				{
					Building building = list[i] as Building;
					if (building != null && building.def.useHitPoints && building.HitPoints < building.MaxHitPoints && building.def.drawDamagedOverlay)
					{
						IntVec3 position = building.Position;
						if (position.x == current.x)
						{
							IntVec3 position2 = building.Position;
							if (position2.z == current.z)
							{
								PrintDamageVisualsFrom(building);
							}
						}
					}
				}
			}
			FinalizeMesh(MeshParts.All);
		}

		private void PrintDamageVisualsFrom(Building b)
		{
			if (b.def.graphicData == null || b.def.graphicData.damageData == null || b.def.graphicData.damageData.enabled)
			{
				PrintScratches(b);
				PrintCornersAndEdges(b);
			}
		}

		private void PrintScratches(Building b)
		{
			int num = 0;
			List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
			for (int i = 0; i < overlays.Count; i++)
			{
				if (overlays[i] == DamageOverlay.Scratch)
				{
					num++;
				}
			}
			if (num != 0)
			{
				Rect damageRect = BuildingsDamageSectionLayerUtility.GetDamageRect(b);
				float num2 = Mathf.Min(0.5f * Mathf.Min(damageRect.width, damageRect.height), 1f);
				damageRect = damageRect.ContractedBy(num2 / 2f);
				if (!(damageRect.width <= 0f) && !(damageRect.height <= 0f))
				{
					float minDist = Mathf.Max(damageRect.width, damageRect.height) * 0.7f;
					scratches.Clear();
					Rand.PushState();
					Rand.Seed = b.thingIDNumber * 3697;
					for (int j = 0; j < num; j++)
					{
						AddScratch(b, damageRect.width, damageRect.height, ref minDist);
					}
					Rand.PopState();
					float damageTexturesAltitude = GetDamageTexturesAltitude(b);
					IList<Material> scratchMats = BuildingsDamageSectionLayerUtility.GetScratchMats(b);
					Rand.PushState();
					Rand.Seed = b.thingIDNumber * 7;
					for (int k = 0; k < scratches.Count; k++)
					{
						Vector2 vector = scratches[k];
						float x = vector.x;
						Vector2 vector2 = scratches[k];
						float y = vector2.y;
						float rot = Rand.Range(0f, 360f);
						float num3 = num2;
						if (damageRect.width > 0.95f && damageRect.height > 0.95f)
						{
							num3 *= Rand.Range(0.85f, 1f);
						}
						Vector3 center = new Vector3(damageRect.xMin + x, damageTexturesAltitude, damageRect.yMin + y);
						Printer_Plane.PrintPlane(this, center, new Vector2(num3, num3), scratchMats.RandomElement(), rot, flipUv: false, null, null, 0f);
					}
					Rand.PopState();
				}
			}
		}

		private void AddScratch(Building b, float rectWidth, float rectHeight, ref float minDist)
		{
			bool flag = false;
			float num = 0f;
			float num2 = 0f;
			while (!flag)
			{
				for (int i = 0; i < 5; i++)
				{
					num = Rand.Value * rectWidth;
					num2 = Rand.Value * rectHeight;
					float num3 = 3.40282347E+38f;
					for (int j = 0; j < scratches.Count; j++)
					{
						float num4 = num;
						Vector2 vector = scratches[j];
						float num5 = num4 - vector.x;
						float num6 = num;
						Vector2 vector2 = scratches[j];
						float num7 = num5 * (num6 - vector2.x);
						float num8 = num2;
						Vector2 vector3 = scratches[j];
						float num9 = num8 - vector3.y;
						float num10 = num2;
						Vector2 vector4 = scratches[j];
						float num11 = num7 + num9 * (num10 - vector4.y);
						if (num11 < num3)
						{
							num3 = num11;
						}
					}
					if (num3 >= minDist * minDist)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					minDist *= 0.85f;
					if (minDist < 0.001f)
					{
						break;
					}
				}
			}
			if (flag)
			{
				scratches.Add(new Vector2(num, num2));
			}
		}

		private void PrintCornersAndEdges(Building b)
		{
			Rand.PushState();
			Rand.Seed = b.thingIDNumber * 3;
			if (BuildingsDamageSectionLayerUtility.UsesLinkableCornersAndEdges(b))
			{
				DrawLinkableCornersAndEdges(b);
			}
			else
			{
				DrawFullThingCorners(b);
			}
			Rand.PopState();
		}

		private void DrawLinkableCornersAndEdges(Building b)
		{
			if (b.def.graphicData != null)
			{
				DamageGraphicData damageData = b.def.graphicData.damageData;
				if (damageData != null)
				{
					float damageTexturesAltitude = GetDamageTexturesAltitude(b);
					List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
					IntVec3 position = b.Position;
					Vector3 vector = new Vector3((float)position.x + 0.5f, damageTexturesAltitude, (float)position.z + 0.5f);
					float x = Rand.Range(0.4f, 0.6f);
					float z = Rand.Range(0.4f, 0.6f);
					float x2 = Rand.Range(0.4f, 0.6f);
					float z2 = Rand.Range(0.4f, 0.6f);
					for (int i = 0; i < overlays.Count; i++)
					{
						switch (overlays[i])
						{
						case DamageOverlay.TopEdge:
							Printer_Plane.PrintPlane(this, vector + new Vector3(x, 0f, 0f), Vector2.one, damageData.edgeTopMat, 0f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.RightEdge:
							Printer_Plane.PrintPlane(this, vector + new Vector3(0f, 0f, z), Vector2.one, damageData.edgeRightMat, 90f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.BotEdge:
							Printer_Plane.PrintPlane(this, vector + new Vector3(x2, 0f, 0f), Vector2.one, damageData.edgeBotMat, 180f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.LeftEdge:
							Printer_Plane.PrintPlane(this, vector + new Vector3(0f, 0f, z2), Vector2.one, damageData.edgeLeftMat, 270f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.TopLeftCorner:
							Printer_Plane.PrintPlane(this, vector, Vector2.one, damageData.cornerTLMat, 0f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.TopRightCorner:
							Printer_Plane.PrintPlane(this, vector, Vector2.one, damageData.cornerTRMat, 90f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.BotRightCorner:
							Printer_Plane.PrintPlane(this, vector, Vector2.one, damageData.cornerBRMat, 180f, flipUv: false, null, null, 0f);
							break;
						case DamageOverlay.BotLeftCorner:
							Printer_Plane.PrintPlane(this, vector, Vector2.one, damageData.cornerBLMat, 270f, flipUv: false, null, null, 0f);
							break;
						}
					}
				}
			}
		}

		private void DrawFullThingCorners(Building b)
		{
			if (b.def.graphicData != null)
			{
				DamageGraphicData damageData = b.def.graphicData.damageData;
				if (damageData != null)
				{
					Rect damageRect = BuildingsDamageSectionLayerUtility.GetDamageRect(b);
					float damageTexturesAltitude = GetDamageTexturesAltitude(b);
					float num = Mathf.Min(Mathf.Min(damageRect.width, damageRect.height), 1.5f);
					BuildingsDamageSectionLayerUtility.GetCornerMats(out Material topLeft, out Material topRight, out Material botRight, out Material botLeft, b);
					float num2 = num * Rand.Range(0.9f, 1f);
					float num3 = num * Rand.Range(0.9f, 1f);
					float num4 = num * Rand.Range(0.9f, 1f);
					float num5 = num * Rand.Range(0.9f, 1f);
					List<DamageOverlay> overlays = BuildingsDamageSectionLayerUtility.GetOverlays(b);
					for (int i = 0; i < overlays.Count; i++)
					{
						switch (overlays[i])
						{
						case DamageOverlay.TopLeftCorner:
						{
							Rect rect4 = new Rect(damageRect.xMin, damageRect.yMax - num2, num2, num2);
							Vector2 center7 = rect4.center;
							float x4 = center7.x;
							float y4 = damageTexturesAltitude;
							Vector2 center8 = rect4.center;
							Printer_Plane.PrintPlane(this, new Vector3(x4, y4, center8.y), rect4.size, topLeft, 0f, flipUv: false, null, null, 0f);
							break;
						}
						case DamageOverlay.TopRightCorner:
						{
							Rect rect3 = new Rect(damageRect.xMax - num3, damageRect.yMax - num3, num3, num3);
							Vector2 center5 = rect3.center;
							float x3 = center5.x;
							float y3 = damageTexturesAltitude;
							Vector2 center6 = rect3.center;
							Printer_Plane.PrintPlane(this, new Vector3(x3, y3, center6.y), rect3.size, topRight, 90f, flipUv: false, null, null, 0f);
							break;
						}
						case DamageOverlay.BotRightCorner:
						{
							Rect rect2 = new Rect(damageRect.xMax - num4, damageRect.yMin, num4, num4);
							Vector2 center3 = rect2.center;
							float x2 = center3.x;
							float y2 = damageTexturesAltitude;
							Vector2 center4 = rect2.center;
							Printer_Plane.PrintPlane(this, new Vector3(x2, y2, center4.y), rect2.size, botRight, 180f, flipUv: false, null, null, 0f);
							break;
						}
						case DamageOverlay.BotLeftCorner:
						{
							Rect rect = new Rect(damageRect.xMin, damageRect.yMin, num5, num5);
							Vector2 center = rect.center;
							float x = center.x;
							float y = damageTexturesAltitude;
							Vector2 center2 = rect.center;
							Printer_Plane.PrintPlane(this, new Vector3(x, y, center2.y), rect.size, botLeft, 270f, flipUv: false, null, null, 0f);
							break;
						}
						}
					}
				}
			}
		}

		private float GetDamageTexturesAltitude(Building b)
		{
			return b.def.Altitude + 0.046875f;
		}
	}
}
