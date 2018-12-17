using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class PawnRenderer
	{
		private Pawn pawn;

		public PawnGraphicSet graphics;

		public PawnDownedWiggler wiggler;

		private PawnHeadOverlays statusOverlays;

		private PawnStatusEffecters effecters;

		private PawnWoundDrawer woundOverlays;

		private Graphic_Shadow shadowGraphic;

		private const float CarriedThingDrawAngle = 16f;

		private const float SubInterval = 0.00390625f;

		private const float YOffset_PrimaryEquipmentUnder = 0f;

		private const float YOffset_Behind = 0.00390625f;

		private const float YOffset_Body = 0.0078125f;

		private const float YOffsetInterval_Clothes = 0.00390625f;

		private const float YOffset_Wounds = 0.01953125f;

		private const float YOffset_Shell = 0.0234375f;

		private const float YOffset_Head = 0.02734375f;

		private const float YOffset_OnHead = 0.03125f;

		private const float YOffset_PostHead = 0.03515625f;

		private const float YOffset_CarriedThing = 0.0390625f;

		private const float YOffset_PrimaryEquipmentOver = 0.0390625f;

		private const float YOffset_Status = 0.04296875f;

		private RotDrawMode CurRotDrawMode
		{
			get
			{
				if (pawn.Dead && pawn.Corpse != null)
				{
					return pawn.Corpse.CurRotDrawMode;
				}
				return RotDrawMode.Fresh;
			}
		}

		public PawnRenderer(Pawn pawn)
		{
			this.pawn = pawn;
			wiggler = new PawnDownedWiggler(pawn);
			statusOverlays = new PawnHeadOverlays(pawn);
			woundOverlays = new PawnWoundDrawer(pawn);
			graphics = new PawnGraphicSet(pawn);
			effecters = new PawnStatusEffecters(pawn);
		}

		public void RenderPawnAt(Vector3 drawLoc)
		{
			RenderPawnAt(drawLoc, CurRotDrawMode, !pawn.health.hediffSet.HasHead);
		}

		public void RenderPawnAt(Vector3 drawLoc, RotDrawMode bodyDrawType, bool headStump)
		{
			if (!graphics.AllResolved)
			{
				graphics.ResolveAllGraphics();
			}
			if (pawn.GetPosture() == PawnPosture.Standing)
			{
				RenderPawnInternal(drawLoc, 0f, renderBody: true, bodyDrawType, headStump);
				if (pawn.carryTracker != null)
				{
					Thing carriedThing = pawn.carryTracker.CarriedThing;
					if (carriedThing != null)
					{
						Vector3 drawPos = drawLoc;
						bool behind = false;
						bool flip = false;
						if (pawn.CurJob == null || !pawn.jobs.curDriver.ModifyCarriedThingDrawPos(ref drawPos, ref behind, ref flip))
						{
							drawPos = ((!(carriedThing is Pawn) && !(carriedThing is Corpse)) ? (drawPos + new Vector3(0.18f, 0f, 0.05f)) : (drawPos + new Vector3(0.44f, 0f, 0f)));
						}
						if (behind)
						{
							drawPos.y -= 0.0390625f;
						}
						else
						{
							drawPos.y += 0.0390625f;
						}
						carriedThing.DrawAt(drawPos, flip);
					}
				}
				if (pawn.def.race.specialShadowData != null)
				{
					if (shadowGraphic == null)
					{
						shadowGraphic = new Graphic_Shadow(pawn.def.race.specialShadowData);
					}
					shadowGraphic.Draw(drawLoc, Rot4.North, pawn);
				}
				if (graphics.nakedGraphic != null && graphics.nakedGraphic.ShadowGraphic != null)
				{
					graphics.nakedGraphic.ShadowGraphic.Draw(drawLoc, Rot4.North, pawn);
				}
			}
			else
			{
				Rot4 rot = LayingFacing();
				Building_Bed building_Bed = pawn.CurrentBed();
				bool renderBody;
				float angle;
				Vector3 rootLoc;
				if (building_Bed != null && pawn.RaceProps.Humanlike)
				{
					renderBody = building_Bed.def.building.bed_showSleeperBody;
					Rot4 rotation = building_Bed.Rotation;
					rotation.AsInt += 2;
					angle = rotation.AsAngle;
					AltitudeLayer altLayer = (AltitudeLayer)Mathf.Max((int)building_Bed.def.altitudeLayer, 15);
					Vector3 vector = pawn.Position.ToVector3ShiftedWithAltitude(altLayer);
					Vector3 vector2 = vector;
					vector2.y += 0.02734375f;
					Vector3 vector3 = BaseHeadOffsetAt(Rot4.South);
					float d = 0f - vector3.z;
					Vector3 a = rotation.FacingCell.ToVector3();
					rootLoc = vector + a * d;
					rootLoc.y += 0.0078125f;
				}
				else
				{
					renderBody = true;
					rootLoc = drawLoc;
					if (!pawn.Dead && pawn.CarriedBy == null)
					{
						rootLoc.y = AltitudeLayer.LayingPawn.AltitudeFor() + 0.0078125f;
					}
					if (pawn.Downed || pawn.Dead)
					{
						angle = wiggler.downedAngle;
					}
					else if (pawn.RaceProps.Humanlike)
					{
						angle = rot.AsAngle;
					}
					else
					{
						Rot4 rot2 = Rot4.West;
						switch (pawn.thingIDNumber % 2)
						{
						case 0:
							rot2 = Rot4.West;
							break;
						case 1:
							rot2 = Rot4.East;
							break;
						}
						angle = rot2.AsAngle;
					}
				}
				RenderPawnInternal(rootLoc, angle, renderBody, rot, rot, bodyDrawType, portrait: false, headStump);
			}
			if (pawn.Spawned && !pawn.Dead)
			{
				pawn.stances.StanceTrackerDraw();
				pawn.pather.PatherDraw();
			}
			DrawDebug();
		}

		public void RenderPortrait()
		{
			Vector3 zero = Vector3.zero;
			float angle;
			if (pawn.Dead || pawn.Downed)
			{
				angle = 85f;
				zero.x -= 0.18f;
				zero.z -= 0.18f;
			}
			else
			{
				angle = 0f;
			}
			RenderPawnInternal(zero, angle, renderBody: true, Rot4.South, Rot4.South, CurRotDrawMode, portrait: true, !pawn.health.hediffSet.HasHead);
		}

		private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, RotDrawMode draw, bool headStump)
		{
			RenderPawnInternal(rootLoc, angle, renderBody, pawn.Rotation, pawn.Rotation, draw, portrait: false, headStump);
		}

		private void RenderPawnInternal(Vector3 rootLoc, float angle, bool renderBody, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, bool portrait, bool headStump)
		{
			if (!graphics.AllResolved)
			{
				graphics.ResolveAllGraphics();
			}
			Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
			Mesh mesh = null;
			if (renderBody)
			{
				Vector3 loc = rootLoc;
				loc.y += 0.0078125f;
				if (bodyDrawType == RotDrawMode.Dessicated && !pawn.RaceProps.Humanlike && graphics.dessicatedGraphic != null && !portrait)
				{
					graphics.dessicatedGraphic.Draw(loc, bodyFacing, pawn, angle);
				}
				else
				{
					mesh = ((!pawn.RaceProps.Humanlike) ? graphics.nakedGraphic.MeshAt(bodyFacing) : MeshPool.humanlikeBodySet.MeshAt(bodyFacing));
					List<Material> list = graphics.MatsBodyBaseAt(bodyFacing, bodyDrawType);
					for (int i = 0; i < list.Count; i++)
					{
						Material damagedMat = graphics.flasher.GetDamagedMat(list[i]);
						GenDraw.DrawMeshNowOrLater(mesh, loc, quaternion, damagedMat, portrait);
						loc.y += 0.00390625f;
					}
					if (bodyDrawType == RotDrawMode.Fresh)
					{
						Vector3 drawLoc = rootLoc;
						drawLoc.y += 0.01953125f;
						woundOverlays.RenderOverBody(drawLoc, mesh, quaternion, portrait);
					}
				}
			}
			Vector3 vector = rootLoc;
			Vector3 a = rootLoc;
			if (bodyFacing != Rot4.North)
			{
				a.y += 0.02734375f;
				vector.y += 0.0234375f;
			}
			else
			{
				a.y += 0.0234375f;
				vector.y += 0.02734375f;
			}
			if (graphics.headGraphic != null)
			{
				Vector3 b = quaternion * BaseHeadOffsetAt(headFacing);
				Material material = graphics.HeadMatAt(headFacing, bodyDrawType, headStump);
				if (material != null)
				{
					Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(headFacing);
					GenDraw.DrawMeshNowOrLater(mesh2, a + b, quaternion, material, portrait);
				}
				Vector3 loc2 = rootLoc + b;
				loc2.y += 0.03125f;
				bool flag = false;
				if (!portrait || !Prefs.HatsOnlyOnMap)
				{
					Mesh mesh3 = graphics.HairMeshSet.MeshAt(headFacing);
					List<ApparelGraphicRecord> apparelGraphics = graphics.apparelGraphics;
					for (int j = 0; j < apparelGraphics.Count; j++)
					{
						ApparelGraphicRecord apparelGraphicRecord = apparelGraphics[j];
						if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
						{
							ApparelGraphicRecord apparelGraphicRecord2 = apparelGraphics[j];
							if (!apparelGraphicRecord2.sourceApparel.def.apparel.hatRenderedFrontOfFace)
							{
								flag = true;
								ApparelGraphicRecord apparelGraphicRecord3 = apparelGraphics[j];
								Material baseMat = apparelGraphicRecord3.graphic.MatAt(bodyFacing);
								baseMat = graphics.flasher.GetDamagedMat(baseMat);
								GenDraw.DrawMeshNowOrLater(mesh3, loc2, quaternion, baseMat, portrait);
							}
							else
							{
								ApparelGraphicRecord apparelGraphicRecord4 = apparelGraphics[j];
								Material baseMat2 = apparelGraphicRecord4.graphic.MatAt(bodyFacing);
								baseMat2 = graphics.flasher.GetDamagedMat(baseMat2);
								Vector3 loc3 = rootLoc + b;
								loc3.y += ((!(bodyFacing == Rot4.North)) ? 0.03515625f : 0.00390625f);
								GenDraw.DrawMeshNowOrLater(mesh3, loc3, quaternion, baseMat2, portrait);
							}
						}
					}
				}
				if (!flag && bodyDrawType != RotDrawMode.Dessicated && !headStump)
				{
					Mesh mesh4 = graphics.HairMeshSet.MeshAt(headFacing);
					Material mat = graphics.HairMatAt(headFacing);
					GenDraw.DrawMeshNowOrLater(mesh4, loc2, quaternion, mat, portrait);
				}
			}
			if (renderBody)
			{
				for (int k = 0; k < graphics.apparelGraphics.Count; k++)
				{
					ApparelGraphicRecord apparelGraphicRecord5 = graphics.apparelGraphics[k];
					if (apparelGraphicRecord5.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
					{
						Material baseMat3 = apparelGraphicRecord5.graphic.MatAt(bodyFacing);
						baseMat3 = graphics.flasher.GetDamagedMat(baseMat3);
						GenDraw.DrawMeshNowOrLater(mesh, vector, quaternion, baseMat3, portrait);
					}
				}
			}
			if (!portrait && pawn.RaceProps.Animal && pawn.inventory != null && pawn.inventory.innerContainer.Count > 0 && graphics.packGraphic != null)
			{
				Graphics.DrawMesh(mesh, vector, quaternion, graphics.packGraphic.MatAt(bodyFacing), 0);
			}
			if (!portrait)
			{
				DrawEquipment(rootLoc);
				if (pawn.apparel != null)
				{
					List<Apparel> wornApparel = pawn.apparel.WornApparel;
					for (int l = 0; l < wornApparel.Count; l++)
					{
						wornApparel[l].DrawWornExtras();
					}
				}
				Vector3 bodyLoc = rootLoc;
				bodyLoc.y += 0.04296875f;
				statusOverlays.RenderStatusOverlays(bodyLoc, quaternion, MeshPool.humanlikeHeadSet.MeshAt(headFacing));
			}
		}

		private void DrawEquipment(Vector3 rootLoc)
		{
			if (!pawn.Dead && pawn.Spawned && pawn.equipment != null && pawn.equipment.Primary != null && (pawn.CurJob == null || !pawn.CurJob.def.neverShowWeapon))
			{
				Stance_Busy stance_Busy = pawn.stances.curStance as Stance_Busy;
				if (stance_Busy != null && !stance_Busy.neverAimWeapon && stance_Busy.focusTarg.IsValid)
				{
					Vector3 a = (!stance_Busy.focusTarg.HasThing) ? stance_Busy.focusTarg.Cell.ToVector3Shifted() : stance_Busy.focusTarg.Thing.DrawPos;
					float num = 0f;
					if ((a - pawn.DrawPos).MagnitudeHorizontalSquared() > 0.001f)
					{
						num = (a - pawn.DrawPos).AngleFlat();
					}
					Vector3 drawLoc = rootLoc + new Vector3(0f, 0f, 0.4f).RotatedBy(num);
					drawLoc.y += 0.0390625f;
					DrawEquipmentAiming(pawn.equipment.Primary, drawLoc, num);
				}
				else if (CarryWeaponOpenly())
				{
					if (pawn.Rotation == Rot4.South)
					{
						Vector3 drawLoc2 = rootLoc + new Vector3(0f, 0f, -0.22f);
						drawLoc2.y += 0.0390625f;
						DrawEquipmentAiming(pawn.equipment.Primary, drawLoc2, 143f);
					}
					else if (pawn.Rotation == Rot4.North)
					{
						Vector3 drawLoc3 = rootLoc + new Vector3(0f, 0f, -0.11f);
						drawLoc3.y = drawLoc3.y;
						DrawEquipmentAiming(pawn.equipment.Primary, drawLoc3, 143f);
					}
					else if (pawn.Rotation == Rot4.East)
					{
						Vector3 drawLoc4 = rootLoc + new Vector3(0.2f, 0f, -0.22f);
						drawLoc4.y += 0.0390625f;
						DrawEquipmentAiming(pawn.equipment.Primary, drawLoc4, 143f);
					}
					else if (pawn.Rotation == Rot4.West)
					{
						Vector3 drawLoc5 = rootLoc + new Vector3(-0.2f, 0f, -0.22f);
						drawLoc5.y += 0.0390625f;
						DrawEquipmentAiming(pawn.equipment.Primary, drawLoc5, 217f);
					}
				}
			}
		}

		public void DrawEquipmentAiming(Thing eq, Vector3 drawLoc, float aimAngle)
		{
			Mesh mesh = null;
			float num = aimAngle - 90f;
			if (aimAngle > 20f && aimAngle < 160f)
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			else if (aimAngle > 200f && aimAngle < 340f)
			{
				mesh = MeshPool.plane10Flip;
				num -= 180f;
				num -= eq.def.equippedAngleOffset;
			}
			else
			{
				mesh = MeshPool.plane10;
				num += eq.def.equippedAngleOffset;
			}
			num %= 360f;
			Material material = null;
			Graphic_StackCount graphic_StackCount = eq.Graphic as Graphic_StackCount;
			Graphics.DrawMesh(material: (graphic_StackCount == null) ? eq.Graphic.MatSingle : graphic_StackCount.SubGraphicForStackCount(1, eq.def).MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);
		}

		private bool CarryWeaponOpenly()
		{
			if (pawn.carryTracker != null && pawn.carryTracker.CarriedThing != null)
			{
				return false;
			}
			if (pawn.Drafted)
			{
				return true;
			}
			if (pawn.CurJob != null && pawn.CurJob.def.alwaysShowWeapon)
			{
				return true;
			}
			if (pawn.mindState.duty != null && pawn.mindState.duty.def.alwaysShowWeapon)
			{
				return true;
			}
			return false;
		}

		private Rot4 LayingFacing()
		{
			if (pawn.GetPosture() == PawnPosture.LayingOnGroundFaceUp)
			{
				return Rot4.South;
			}
			if (!pawn.RaceProps.Humanlike)
			{
				switch (pawn.thingIDNumber % 4)
				{
				case 0:
					return Rot4.South;
				case 1:
					return Rot4.East;
				case 2:
					return Rot4.West;
				case 3:
					return Rot4.West;
				}
			}
			else
			{
				switch (pawn.thingIDNumber % 4)
				{
				case 0:
					return Rot4.South;
				case 1:
					return Rot4.South;
				case 2:
					return Rot4.East;
				case 3:
					return Rot4.West;
				}
			}
			return Rot4.Random;
		}

		public Vector3 BaseHeadOffsetAt(Rot4 rotation)
		{
			Vector2 headOffset = pawn.story.bodyType.headOffset;
			switch (rotation.AsInt)
			{
			case 0:
				return new Vector3(0f, 0f, headOffset.y);
			case 1:
				return new Vector3(headOffset.x, 0f, headOffset.y);
			case 2:
				return new Vector3(0f, 0f, headOffset.y);
			case 3:
				return new Vector3(0f - headOffset.x, 0f, headOffset.y);
			default:
				Log.Error("BaseHeadOffsetAt error in " + pawn);
				return Vector3.zero;
			}
		}

		public void Notify_DamageApplied(DamageInfo dam)
		{
			graphics.flasher.Notify_DamageApplied(dam);
			wiggler.Notify_DamageApplied(dam);
		}

		public void RendererTick()
		{
			wiggler.WigglerTick();
			effecters.EffectersTick();
		}

		private void DrawDebug()
		{
			if (DebugViewSettings.drawDuties && Find.Selector.IsSelected(pawn) && pawn.mindState != null && pawn.mindState.duty != null)
			{
				pawn.mindState.duty.DrawDebug(pawn);
			}
		}
	}
}
