using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public static class ReorderableWidget
	{
		private struct ReorderableGroup
		{
			public Action<int, int> reorderedAction;

			public ReorderableDirection direction;

			public float drawLineExactlyBetween_space;

			public Action<int, Vector2> extraDraggedItemOnGUI;

			public bool DrawLineExactlyBetween => drawLineExactlyBetween_space > 0f;
		}

		private struct ReorderableInstance
		{
			public int groupID;

			public Rect rect;

			public Rect absRect;
		}

		private static List<ReorderableGroup> groups = new List<ReorderableGroup>();

		private static List<ReorderableInstance> reorderables = new List<ReorderableInstance>();

		private static int draggingReorderable = -1;

		private static Vector2 dragStartPos;

		private static bool clicked;

		private static bool released;

		private static bool dragBegun;

		private static Vector2 clickedAt;

		private static Rect clickedInRect;

		private static int lastInsertNear = -1;

		private static bool lastInsertNearLeft;

		private static int lastFrameReorderableCount = -1;

		private const float MinMouseMoveToHighlightReorderable = 5f;

		private static readonly Color LineColor = new Color(1f, 1f, 1f, 0.6f);

		private static readonly Color HighlightColor = new Color(1f, 1f, 1f, 0.3f);

		private const float LineWidth = 2f;

		public static void ReorderableWidgetOnGUI_BeforeWindowStack()
		{
			if (dragBegun && draggingReorderable >= 0 && draggingReorderable < reorderables.Count)
			{
				ReorderableInstance reorderableInstance = reorderables[draggingReorderable];
				int groupID = reorderableInstance.groupID;
				if (groupID >= 0 && groupID < groups.Count)
				{
					ReorderableGroup reorderableGroup = groups[groupID];
					if (reorderableGroup.extraDraggedItemOnGUI != null)
					{
						ReorderableGroup reorderableGroup2 = groups[groupID];
						reorderableGroup2.extraDraggedItemOnGUI(GetIndexWithinGroup(draggingReorderable), dragStartPos);
					}
				}
			}
		}

		public static void ReorderableWidgetOnGUI_AfterWindowStack()
		{
			if (Event.current.rawType == EventType.MouseUp)
			{
				released = true;
			}
			if (Event.current.type == EventType.Repaint)
			{
				if (clicked)
				{
					StopDragging();
					for (int i = 0; i < reorderables.Count; i++)
					{
						ReorderableInstance reorderableInstance = reorderables[i];
						if (reorderableInstance.rect == clickedInRect)
						{
							draggingReorderable = i;
							dragStartPos = Event.current.mousePosition;
							break;
						}
					}
					clicked = false;
				}
				if (draggingReorderable >= reorderables.Count)
				{
					StopDragging();
				}
				if (reorderables.Count != lastFrameReorderableCount)
				{
					StopDragging();
				}
				lastInsertNear = CurrentInsertNear(out lastInsertNearLeft);
				if (released)
				{
					released = false;
					if (dragBegun && draggingReorderable >= 0)
					{
						int indexWithinGroup = GetIndexWithinGroup(draggingReorderable);
						int num = (lastInsertNear == draggingReorderable) ? indexWithinGroup : ((!lastInsertNearLeft) ? (GetIndexWithinGroup(lastInsertNear) + 1) : GetIndexWithinGroup(lastInsertNear));
						if (num >= 0 && num != indexWithinGroup && num != indexWithinGroup + 1)
						{
							SoundDefOf.Tick_High.PlayOneShotOnCamera();
							try
							{
								List<ReorderableGroup> list = groups;
								ReorderableInstance reorderableInstance2 = reorderables[draggingReorderable];
								ReorderableGroup reorderableGroup = list[reorderableInstance2.groupID];
								reorderableGroup.reorderedAction(indexWithinGroup, num);
							}
							catch (Exception ex)
							{
								Log.Error("Could not reorder elements (from " + indexWithinGroup + " to " + num + "): " + ex);
							}
						}
					}
					StopDragging();
				}
				lastFrameReorderableCount = reorderables.Count;
				groups.Clear();
				reorderables.Clear();
			}
		}

		public static int NewGroup(Action<int, int> reorderedAction, ReorderableDirection direction, float drawLineExactlyBetween_space = -1f, Action<int, Vector2> extraDraggedItemOnGUI = null)
		{
			if (Event.current.type != EventType.Repaint)
			{
				return -1;
			}
			ReorderableGroup item = default(ReorderableGroup);
			item.reorderedAction = reorderedAction;
			item.direction = direction;
			item.drawLineExactlyBetween_space = drawLineExactlyBetween_space;
			item.extraDraggedItemOnGUI = extraDraggedItemOnGUI;
			groups.Add(item);
			return groups.Count - 1;
		}

		public static bool Reorderable(int groupID, Rect rect, bool useRightButton = false)
		{
			if (Event.current.type == EventType.Repaint)
			{
				ReorderableInstance item = default(ReorderableInstance);
				item.groupID = groupID;
				item.rect = rect;
				item.absRect = new Rect(UI.GUIToScreenPoint(rect.position), rect.size);
				reorderables.Add(item);
				int num = reorderables.Count - 1;
				if (draggingReorderable != -1 && (dragBegun || Vector2.Distance(clickedAt, Event.current.mousePosition) > 5f))
				{
					if (!dragBegun)
					{
						SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
						dragBegun = true;
					}
					if (draggingReorderable == num)
					{
						GUI.color = HighlightColor;
						Widgets.DrawHighlight(rect);
						GUI.color = Color.white;
					}
					if (lastInsertNear == num && groupID >= 0 && groupID < groups.Count)
					{
						ReorderableInstance reorderableInstance = reorderables[lastInsertNear];
						Rect rect2 = reorderableInstance.rect;
						ReorderableGroup reorderableGroup = groups[groupID];
						if (reorderableGroup.DrawLineExactlyBetween)
						{
							if (reorderableGroup.direction == ReorderableDirection.Horizontal)
							{
								rect2.xMin -= reorderableGroup.drawLineExactlyBetween_space / 2f;
								rect2.xMax += reorderableGroup.drawLineExactlyBetween_space / 2f;
							}
							else
							{
								rect2.yMin -= reorderableGroup.drawLineExactlyBetween_space / 2f;
								rect2.yMax += reorderableGroup.drawLineExactlyBetween_space / 2f;
							}
						}
						GUI.color = LineColor;
						if (reorderableGroup.direction == ReorderableDirection.Horizontal)
						{
							if (lastInsertNearLeft)
							{
								Widgets.DrawLine(rect2.position, new Vector2(rect2.x, rect2.yMax), LineColor, 2f);
							}
							else
							{
								Widgets.DrawLine(new Vector2(rect2.xMax, rect2.y), new Vector2(rect2.xMax, rect2.yMax), LineColor, 2f);
							}
						}
						else if (lastInsertNearLeft)
						{
							Widgets.DrawLine(rect2.position, new Vector2(rect2.xMax, rect2.y), LineColor, 2f);
						}
						else
						{
							Widgets.DrawLine(new Vector2(rect2.x, rect2.yMax), new Vector2(rect2.xMax, rect2.yMax), LineColor, 2f);
						}
						GUI.color = Color.white;
					}
				}
				return draggingReorderable == num && dragBegun;
			}
			if (Event.current.rawType == EventType.MouseUp)
			{
				released = true;
			}
			if (Event.current.type == EventType.MouseDown && ((useRightButton && Event.current.button == 1) || (!useRightButton && Event.current.button == 0)) && Mouse.IsOver(rect))
			{
				clicked = true;
				clickedAt = Event.current.mousePosition;
				clickedInRect = rect;
			}
			return false;
		}

		private static int CurrentInsertNear(out bool toTheLeft)
		{
			toTheLeft = false;
			if (draggingReorderable < 0)
			{
				return -1;
			}
			ReorderableInstance reorderableInstance = reorderables[draggingReorderable];
			int groupID = reorderableInstance.groupID;
			if (groupID < 0 || groupID >= groups.Count)
			{
				Log.ErrorOnce("Reorderable used invalid group.", 1968375560);
				return -1;
			}
			int num = -1;
			for (int i = 0; i < reorderables.Count; i++)
			{
				ReorderableInstance reorderableInstance2 = reorderables[i];
				if (reorderableInstance2.groupID == groupID)
				{
					if (num != -1)
					{
						float num2 = Event.current.mousePosition.DistanceToRect(reorderableInstance2.absRect);
						Vector2 mousePosition = Event.current.mousePosition;
						ReorderableInstance reorderableInstance3 = reorderables[num];
						if (!(num2 < mousePosition.DistanceToRect(reorderableInstance3.absRect)))
						{
							continue;
						}
					}
					num = i;
				}
			}
			if (num >= 0)
			{
				ReorderableInstance reorderableInstance4 = reorderables[num];
				ReorderableGroup reorderableGroup = groups[reorderableInstance4.groupID];
				if (reorderableGroup.direction == ReorderableDirection.Horizontal)
				{
					Vector2 mousePosition2 = Event.current.mousePosition;
					float x = mousePosition2.x;
					Vector2 center = reorderableInstance4.absRect.center;
					toTheLeft = (x < center.x);
				}
				else
				{
					Vector2 mousePosition3 = Event.current.mousePosition;
					float y = mousePosition3.y;
					Vector2 center2 = reorderableInstance4.absRect.center;
					toTheLeft = (y < center2.y);
				}
			}
			return num;
		}

		private static int GetIndexWithinGroup(int index)
		{
			if (index < 0 || index >= reorderables.Count)
			{
				return -1;
			}
			int num = -1;
			for (int i = 0; i <= index; i++)
			{
				ReorderableInstance reorderableInstance = reorderables[i];
				int groupID = reorderableInstance.groupID;
				ReorderableInstance reorderableInstance2 = reorderables[index];
				if (groupID == reorderableInstance2.groupID)
				{
					num++;
				}
			}
			return num;
		}

		private static void StopDragging()
		{
			draggingReorderable = -1;
			dragStartPos = default(Vector2);
			lastInsertNear = -1;
			dragBegun = false;
		}
	}
}
