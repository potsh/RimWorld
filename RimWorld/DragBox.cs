using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class DragBox
	{
		public bool active;

		public Vector3 start;

		private const float DragBoxMinDiagonal = 0.5f;

		public float LeftX
		{
			get
			{
				float x = start.x;
				Vector3 vector = UI.MouseMapPosition();
				return Math.Min(x, vector.x);
			}
		}

		public float RightX
		{
			get
			{
				float x = start.x;
				Vector3 vector = UI.MouseMapPosition();
				return Math.Max(x, vector.x);
			}
		}

		public float BotZ
		{
			get
			{
				float z = start.z;
				Vector3 vector = UI.MouseMapPosition();
				return Math.Min(z, vector.z);
			}
		}

		public float TopZ
		{
			get
			{
				float z = start.z;
				Vector3 vector = UI.MouseMapPosition();
				return Math.Max(z, vector.z);
			}
		}

		public Rect ScreenRect
		{
			get
			{
				Vector2 vector = start.MapToUIPosition();
				Vector2 mousePosition = Event.current.mousePosition;
				if (mousePosition.x < vector.x)
				{
					float x = mousePosition.x;
					mousePosition.x = vector.x;
					vector.x = x;
				}
				if (mousePosition.y < vector.y)
				{
					float y = mousePosition.y;
					mousePosition.y = vector.y;
					vector.y = y;
				}
				Rect result = default(Rect);
				result.xMin = vector.x;
				result.xMax = mousePosition.x;
				result.yMin = vector.y;
				result.yMax = mousePosition.y;
				return result;
			}
		}

		public bool IsValid => (start - UI.MouseMapPosition()).magnitude > 0.5f;

		public bool IsValidAndActive => active && IsValid;

		public void DragBoxOnGUI()
		{
			if (IsValidAndActive)
			{
				Widgets.DrawBox(ScreenRect, 2);
			}
		}

		public bool Contains(Thing t)
		{
			if (t is Pawn)
			{
				return Contains((t as Pawn).Drawer.DrawPos);
			}
			CellRect.CellRectIterator iterator = t.OccupiedRect().GetIterator();
			while (!iterator.Done())
			{
				if (Contains(iterator.Current.ToVector3Shifted()))
				{
					return true;
				}
				iterator.MoveNext();
			}
			return false;
		}

		public bool Contains(Vector3 v)
		{
			if (v.x + 0.5f > LeftX && v.x - 0.5f < RightX && v.z + 0.5f > BotZ && v.z - 0.5f < TopZ)
			{
				return true;
			}
			return false;
		}
	}
}
