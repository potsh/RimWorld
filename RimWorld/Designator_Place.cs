using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public abstract class Designator_Place : Designator
	{
		protected Rot4 placingRot = Rot4.North;

		protected static float middleMouseDownTime;

		private const float RotButSize = 64f;

		private const float RotButSpacing = 10f;

		public static readonly Color CanPlaceColor = new Color(0.5f, 1f, 0.6f, 0.4f);

		public static readonly Color CannotPlaceColor = new Color(1f, 0f, 0f, 0.4f);

		public abstract BuildableDef PlacingDef
		{
			get;
		}

		public Designator_Place()
		{
			soundDragSustain = SoundDefOf.Designate_DragBuilding;
			soundDragChanged = null;
			soundSucceeded = SoundDefOf.Designate_PlaceBuilding;
		}

		public override void DoExtraGuiControls(float leftX, float bottomY)
		{
			ThingDef thingDef = PlacingDef as ThingDef;
			if (thingDef != null && thingDef.rotatable)
			{
				Rect winRect = new Rect(leftX, bottomY - 90f, 200f, 90f);
				Find.WindowStack.ImmediateWindow(73095, winRect, WindowLayer.GameUI, delegate
				{
					RotationDirection rotationDirection = RotationDirection.None;
					Text.Anchor = TextAnchor.MiddleCenter;
					Text.Font = GameFont.Medium;
					Rect rect = new Rect(winRect.width / 2f - 64f - 5f, 15f, 64f, 64f);
					if (Widgets.ButtonImage(rect, TexUI.RotLeftTex))
					{
						SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
						rotationDirection = RotationDirection.Counterclockwise;
						Event.current.Use();
					}
					Widgets.Label(rect, KeyBindingDefOf.Designator_RotateLeft.MainKeyLabel);
					Rect rect2 = new Rect(winRect.width / 2f + 5f, 15f, 64f, 64f);
					if (Widgets.ButtonImage(rect2, TexUI.RotRightTex))
					{
						SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
						rotationDirection = RotationDirection.Clockwise;
						Event.current.Use();
					}
					Widgets.Label(rect2, KeyBindingDefOf.Designator_RotateRight.MainKeyLabel);
					if (rotationDirection != 0)
					{
						placingRot.Rotate(rotationDirection);
					}
					Text.Anchor = TextAnchor.UpperLeft;
					Text.Font = GameFont.Small;
				});
			}
		}

		public override void SelectedProcessInput(Event ev)
		{
			base.SelectedProcessInput(ev);
			ThingDef thingDef = PlacingDef as ThingDef;
			if (thingDef != null && thingDef.rotatable)
			{
				HandleRotationShortcuts();
			}
		}

		public override void SelectedUpdate()
		{
			GenDraw.DrawNoBuildEdgeLines();
			if (!ArchitectCategoryTab.InfoRect.Contains(UI.MousePositionOnUIInverted))
			{
				IntVec3 intVec = UI.MouseCell();
				if (PlacingDef is TerrainDef)
				{
					GenUI.RenderMouseoverBracket();
				}
				else
				{
					Color ghostCol = (!CanDesignateCell(intVec).Accepted) ? CannotPlaceColor : CanPlaceColor;
					DrawGhost(ghostCol);
					if (CanDesignateCell(intVec).Accepted && PlacingDef.specialDisplayRadius > 0.01f)
					{
						GenDraw.DrawRadiusRing(UI.MouseCell(), PlacingDef.specialDisplayRadius);
					}
					GenDraw.DrawInteractionCell((ThingDef)PlacingDef, intVec, placingRot);
				}
			}
		}

		protected virtual void DrawGhost(Color ghostCol)
		{
			GhostDrawer.DrawGhostThing(UI.MouseCell(), placingRot, (ThingDef)PlacingDef, null, ghostCol, AltitudeLayer.Blueprint);
		}

		private void HandleRotationShortcuts()
		{
			RotationDirection rotationDirection = RotationDirection.None;
			if (Event.current.button == 2)
			{
				if (Event.current.type == EventType.MouseDown)
				{
					Event.current.Use();
					middleMouseDownTime = Time.realtimeSinceStartup;
				}
				if (Event.current.type == EventType.MouseUp && Time.realtimeSinceStartup - middleMouseDownTime < 0.15f)
				{
					rotationDirection = RotationDirection.Clockwise;
				}
			}
			if (KeyBindingDefOf.Designator_RotateRight.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Clockwise;
			}
			if (KeyBindingDefOf.Designator_RotateLeft.KeyDownEvent)
			{
				rotationDirection = RotationDirection.Counterclockwise;
			}
			if (rotationDirection == RotationDirection.Clockwise)
			{
				SoundDefOf.AmountIncrement.PlayOneShotOnCamera();
				placingRot.Rotate(RotationDirection.Clockwise);
			}
			if (rotationDirection == RotationDirection.Counterclockwise)
			{
				SoundDefOf.AmountDecrement.PlayOneShotOnCamera();
				placingRot.Rotate(RotationDirection.Counterclockwise);
			}
		}

		public override void Selected()
		{
			placingRot = PlacingDef.defaultPlacingRot;
		}
	}
}
