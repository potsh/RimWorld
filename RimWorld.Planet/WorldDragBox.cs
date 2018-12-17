using System;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldDragBox
	{
		public bool active;

		public Vector2 start;

		private const float DragBoxMinDiagonal = 7f;

		public float LeftX
		{
			get
			{
				float x = start.x;
				Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
				return Math.Min(x, mousePositionOnUIInverted.x);
			}
		}

		public float RightX
		{
			get
			{
				float x = start.x;
				Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
				return Math.Max(x, mousePositionOnUIInverted.x);
			}
		}

		public float BotZ
		{
			get
			{
				float y = start.y;
				Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
				return Math.Min(y, mousePositionOnUIInverted.y);
			}
		}

		public float TopZ
		{
			get
			{
				float y = start.y;
				Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
				return Math.Max(y, mousePositionOnUIInverted.y);
			}
		}

		public Rect ScreenRect => new Rect(LeftX, BotZ, RightX - LeftX, TopZ - BotZ);

		public float Diagonal
		{
			get
			{
				Vector2 a = start;
				Vector2 mousePositionOnUIInverted = UI.MousePositionOnUIInverted;
				float x = mousePositionOnUIInverted.x;
				Vector2 mousePositionOnUIInverted2 = UI.MousePositionOnUIInverted;
				return (a - new Vector2(x, mousePositionOnUIInverted2.y)).magnitude;
			}
		}

		public bool IsValid => Diagonal > 7f;

		public bool IsValidAndActive => active && IsValid;

		public void DragBoxOnGUI()
		{
			if (IsValidAndActive)
			{
				Widgets.DrawBox(ScreenRect, 2);
			}
		}

		public bool Contains(WorldObject o)
		{
			return Contains(o.ScreenPos());
		}

		public bool Contains(Vector2 screenPoint)
		{
			if (screenPoint.x + 0.5f > LeftX && screenPoint.x - 0.5f < RightX && screenPoint.y + 0.5f > BotZ && screenPoint.y - 0.5f < TopZ)
			{
				return true;
			}
			return false;
		}
	}
}
