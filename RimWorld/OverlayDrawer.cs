using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class OverlayDrawer
	{
		private Dictionary<Thing, OverlayTypes> overlaysToDraw = new Dictionary<Thing, OverlayTypes>();

		private Vector3 curOffset;

		private static readonly Material ForbiddenMat;

		private static readonly Material NeedsPowerMat;

		private static readonly Material PowerOffMat;

		private static readonly Material QuestionMarkMat;

		private static readonly Material BrokenDownMat;

		private static readonly Material OutOfFuelMat;

		private static readonly Material WickMaterialA;

		private static readonly Material WickMaterialB;

		private const int AltitudeIndex_Forbidden = 4;

		private const int AltitudeIndex_BurningWick = 5;

		private const int AltitudeIndex_QuestionMark = 6;

		private static float SingleCellForbiddenOffset;

		private const float PulseFrequency = 4f;

		private const float PulseAmplitude = 0.7f;

		private static readonly float BaseAlt;

		private const float StackOffsetMultipiler = 0.25f;

		static OverlayDrawer()
		{
			ForbiddenMat = MaterialPool.MatFrom("Things/Special/ForbiddenOverlay", ShaderDatabase.MetaOverlay);
			NeedsPowerMat = MaterialPool.MatFrom("UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay);
			PowerOffMat = MaterialPool.MatFrom("UI/Overlays/PowerOff", ShaderDatabase.MetaOverlay);
			QuestionMarkMat = MaterialPool.MatFrom("UI/Overlays/QuestionMark", ShaderDatabase.MetaOverlay);
			BrokenDownMat = MaterialPool.MatFrom("UI/Overlays/BrokenDown", ShaderDatabase.MetaOverlay);
			OutOfFuelMat = MaterialPool.MatFrom("UI/Overlays/OutOfFuel", ShaderDatabase.MetaOverlay);
			WickMaterialA = MaterialPool.MatFrom("Things/Special/BurningWickA", ShaderDatabase.MetaOverlay);
			WickMaterialB = MaterialPool.MatFrom("Things/Special/BurningWickB", ShaderDatabase.MetaOverlay);
			SingleCellForbiddenOffset = 0.3f;
			BaseAlt = AltitudeLayer.MetaOverlays.AltitudeFor();
		}

		public void DrawOverlay(Thing t, OverlayTypes overlayType)
		{
			if (overlaysToDraw.ContainsKey(t))
			{
				Dictionary<Thing, OverlayTypes> dictionary;
				Thing key;
				(dictionary = overlaysToDraw)[key = t] = (dictionary[key] | overlayType);
			}
			else
			{
				overlaysToDraw.Add(t, overlayType);
			}
		}

		public void DrawAllOverlays()
		{
			foreach (KeyValuePair<Thing, OverlayTypes> item in overlaysToDraw)
			{
				curOffset = Vector3.zero;
				Thing key = item.Key;
				OverlayTypes value = item.Value;
				if ((value & OverlayTypes.BurningWick) != 0)
				{
					RenderBurningWick(key);
				}
				else
				{
					OverlayTypes overlayTypes = OverlayTypes.NeedsPower | OverlayTypes.PowerOff;
					int bitCountOf = Gen.GetBitCountOf((long)(value & overlayTypes));
					float num = StackOffsetFor(item.Key);
					switch (bitCountOf)
					{
					case 1:
						curOffset = Vector3.zero;
						break;
					case 2:
						curOffset = new Vector3(-0.5f * num, 0f, 0f);
						break;
					case 3:
						curOffset = new Vector3(-1.5f * num, 0f, 0f);
						break;
					}
					if ((value & OverlayTypes.NeedsPower) != 0)
					{
						RenderNeedsPowerOverlay(key);
					}
					if ((value & OverlayTypes.PowerOff) != 0)
					{
						RenderPowerOffOverlay(key);
					}
					if ((value & OverlayTypes.BrokenDown) != 0)
					{
						RenderBrokenDownOverlay(key);
					}
					if ((value & OverlayTypes.OutOfFuel) != 0)
					{
						RenderOutOfFuelOverlay(key);
					}
				}
				if ((value & OverlayTypes.ForbiddenBig) != 0)
				{
					RenderForbiddenBigOverlay(key);
				}
				if ((value & OverlayTypes.Forbidden) != 0)
				{
					RenderForbiddenOverlay(key);
				}
				if ((value & OverlayTypes.QuestionMark) != 0)
				{
					RenderQuestionMarkOverlay(key);
				}
			}
			overlaysToDraw.Clear();
		}

		private float StackOffsetFor(Thing t)
		{
			IntVec2 rotatedSize = t.RotatedSize;
			return (float)rotatedSize.x * 0.25f;
		}

		private void RenderNeedsPowerOverlay(Thing t)
		{
			RenderPulsingOverlay(t, NeedsPowerMat, 2);
		}

		private void RenderPowerOffOverlay(Thing t)
		{
			RenderPulsingOverlay(t, PowerOffMat, 3);
		}

		private void RenderBrokenDownOverlay(Thing t)
		{
			RenderPulsingOverlay(t, BrokenDownMat, 4);
		}

		private void RenderOutOfFuelOverlay(Thing t)
		{
			CompRefuelable compRefuelable = t.TryGetComp<CompRefuelable>();
			Material mat = MaterialPool.MatFrom((compRefuelable == null) ? ThingDefOf.Chemfuel.uiIcon : compRefuelable.Props.FuelIcon, ShaderDatabase.MetaOverlay, Color.white);
			RenderPulsingOverlay(t, mat, 5, incrementOffset: false);
			RenderPulsingOverlay(t, OutOfFuelMat, 6);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, bool incrementOffset = true)
		{
			Mesh plane = MeshPool.plane08;
			RenderPulsingOverlay(thing, mat, altInd, plane, incrementOffset);
		}

		private void RenderPulsingOverlay(Thing thing, Material mat, int altInd, Mesh mesh, bool incrementOffset = true)
		{
			Vector3 vector = thing.TrueCenter();
			vector.y = BaseAlt + 0.046875f * (float)altInd;
			vector += curOffset;
			if (incrementOffset)
			{
				curOffset.x += StackOffsetFor(thing);
			}
			RenderPulsingOverlayInternal(thing, mat, vector, mesh);
		}

		private void RenderPulsingOverlayInternal(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
		{
			float num = (Time.realtimeSinceStartup + 397f * (float)(thing.thingIDNumber % 571)) * 4f;
			float num2 = ((float)Math.Sin((double)num) + 1f) * 0.5f;
			num2 = 0.3f + num2 * 0.7f;
			Material material = FadedMaterialPool.FadedVersionOf(mat, num2);
			Graphics.DrawMesh(mesh, drawPos, Quaternion.identity, material, 0);
		}

		private void RenderForbiddenOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			IntVec2 rotatedSize = t.RotatedSize;
			if (rotatedSize.z == 1)
			{
				drawPos.z -= SingleCellForbiddenOffset;
			}
			else
			{
				float z = drawPos.z;
				IntVec2 rotatedSize2 = t.RotatedSize;
				drawPos.z = z - (float)rotatedSize2.z * 0.3f;
			}
			drawPos.y = BaseAlt + 0.1875f;
			Graphics.DrawMesh(MeshPool.plane05, drawPos, Quaternion.identity, ForbiddenMat, 0);
		}

		private void RenderForbiddenBigOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = BaseAlt + 0.1875f;
			Graphics.DrawMesh(MeshPool.plane10, drawPos, Quaternion.identity, ForbiddenMat, 0);
		}

		private void RenderBurningWick(Thing parent)
		{
			Material material = ((parent.thingIDNumber + Find.TickManager.TicksGame) % 6 >= 3) ? WickMaterialB : WickMaterialA;
			Vector3 drawPos = parent.DrawPos;
			drawPos.y = BaseAlt + 0.234375f;
			Graphics.DrawMesh(MeshPool.plane20, drawPos, Quaternion.identity, material, 0);
		}

		private void RenderQuestionMarkOverlay(Thing t)
		{
			Vector3 drawPos = t.DrawPos;
			drawPos.y = BaseAlt + 0.28125f;
			if (t is Pawn)
			{
				drawPos.x += (float)t.def.size.x - 0.52f;
				drawPos.z += (float)t.def.size.z - 0.45f;
			}
			RenderPulsingOverlayInternal(t, QuestionMarkMat, drawPos, MeshPool.plane05);
		}
	}
}
