using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Instruction_BuildRoomDoor : Lesson_Instruction
	{
		private List<IntVec3> allowedPlaceCells;

		private CellRect RoomRect => Find.TutorialState.roomRect;

		public override void OnActivated()
		{
			base.OnActivated();
			allowedPlaceCells = RoomRect.EdgeCells.ToList();
			allowedPlaceCells.RemoveAll(delegate(IntVec3 c)
			{
				int x = c.x;
				CellRect roomRect = RoomRect;
				if (x == roomRect.minX)
				{
					int z = c.z;
					CellRect roomRect2 = RoomRect;
					if (z == roomRect2.minZ)
					{
						goto IL_00d6;
					}
				}
				int x2 = c.x;
				CellRect roomRect3 = RoomRect;
				if (x2 == roomRect3.minX)
				{
					int z2 = c.z;
					CellRect roomRect4 = RoomRect;
					if (z2 == roomRect4.maxZ)
					{
						goto IL_00d6;
					}
				}
				int x3 = c.x;
				CellRect roomRect5 = RoomRect;
				if (x3 == roomRect5.maxX)
				{
					int z3 = c.z;
					CellRect roomRect6 = RoomRect;
					if (z3 == roomRect6.minZ)
					{
						goto IL_00d6;
					}
				}
				int x4 = c.x;
				CellRect roomRect7 = RoomRect;
				int result;
				if (x4 == roomRect7.maxX)
				{
					int z4 = c.z;
					CellRect roomRect8 = RoomRect;
					result = ((z4 == roomRect8.maxZ) ? 1 : 0);
				}
				else
				{
					result = 0;
				}
				goto IL_00d7;
				IL_00d7:
				return (byte)result != 0;
				IL_00d6:
				result = 1;
				goto IL_00d7;
			});
		}

		public override void LessonOnGUI()
		{
			TutorUtility.DrawCellRectOnGUI(RoomRect, def.onMapInstruction);
			base.LessonOnGUI();
		}

		public override void LessonUpdate()
		{
			GenDraw.DrawArrowPointingAt(RoomRect.CenterVector3);
		}

		public override AcceptanceReport AllowAction(EventPack ep)
		{
			if (ep.Tag == "Designate-Door")
			{
				return TutorUtility.EventCellsAreWithin(ep, allowedPlaceCells);
			}
			return base.AllowAction(ep);
		}

		public override void Notify_Event(EventPack ep)
		{
			if (ep.Tag == "Designate-Door")
			{
				Find.ActiveLesson.Deactivate();
			}
		}
	}
}
