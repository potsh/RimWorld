using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class GizmoGridDrawer
	{
		public static HashSet<KeyCode> drawnHotKeys = new HashSet<KeyCode>();

		private static float heightDrawn;

		private static int heightDrawnFrame;

		private static readonly Vector2 GizmoSpacing = new Vector2(5f, 14f);

		private static List<List<Gizmo>> gizmoGroups = new List<List<Gizmo>>();

		private static List<Gizmo> firstGizmos = new List<Gizmo>();

		private static List<Gizmo> tmpAllGizmos = new List<Gizmo>();

		public static float HeightDrawnRecently
		{
			get
			{
				if (Time.frameCount > heightDrawnFrame + 2)
				{
					return 0f;
				}
				return heightDrawn;
			}
		}

		public static void DrawGizmoGrid(IEnumerable<Gizmo> gizmos, float startX, out Gizmo mouseoverGizmo)
		{
			tmpAllGizmos.Clear();
			tmpAllGizmos.AddRange(gizmos);
			tmpAllGizmos.SortStable((Gizmo lhs, Gizmo rhs) => lhs.order.CompareTo(rhs.order));
			gizmoGroups.Clear();
			for (int i = 0; i < tmpAllGizmos.Count; i++)
			{
				Gizmo gizmo = tmpAllGizmos[i];
				bool flag = false;
				for (int j = 0; j < gizmoGroups.Count; j++)
				{
					if (gizmoGroups[j][0].GroupsWith(gizmo))
					{
						flag = true;
						gizmoGroups[j].Add(gizmo);
						gizmoGroups[j][0].MergeWith(gizmo);
						break;
					}
				}
				if (!flag)
				{
					List<Gizmo> list = new List<Gizmo>();
					list.Add(gizmo);
					gizmoGroups.Add(list);
				}
			}
			firstGizmos.Clear();
			for (int k = 0; k < gizmoGroups.Count; k++)
			{
				List<Gizmo> list2 = gizmoGroups[k];
				Gizmo gizmo2 = null;
				for (int l = 0; l < list2.Count; l++)
				{
					if (!list2[l].disabled)
					{
						gizmo2 = list2[l];
						break;
					}
				}
				if (gizmo2 == null)
				{
					gizmo2 = list2.FirstOrDefault();
				}
				if (gizmo2 != null)
				{
					firstGizmos.Add(gizmo2);
				}
			}
			drawnHotKeys.Clear();
			float num = (float)(UI.screenWidth - 147);
			float maxWidth = num - startX;
			Text.Font = GameFont.Tiny;
			float num2 = (float)(UI.screenHeight - 35);
			Vector2 gizmoSpacing = GizmoSpacing;
			Vector2 topLeft = new Vector2(startX, num2 - gizmoSpacing.y - 75f);
			mouseoverGizmo = null;
			Gizmo interactedGiz = null;
			Event ev = null;
			Gizmo floatMenuGiz = null;
			for (int m = 0; m < firstGizmos.Count; m++)
			{
				Gizmo gizmo3 = firstGizmos[m];
				if (gizmo3.Visible)
				{
					if (topLeft.x + gizmo3.GetWidth(maxWidth) > num)
					{
						topLeft.x = startX;
						float y = topLeft.y;
						Vector2 gizmoSpacing2 = GizmoSpacing;
						topLeft.y = y - (75f + gizmoSpacing2.x);
					}
					heightDrawnFrame = Time.frameCount;
					heightDrawn = (float)UI.screenHeight - topLeft.y;
					GizmoResult gizmoResult = gizmo3.GizmoOnGUI(topLeft, maxWidth);
					if (gizmoResult.State == GizmoState.Interacted)
					{
						ev = gizmoResult.InteractEvent;
						interactedGiz = gizmo3;
					}
					else if (gizmoResult.State == GizmoState.OpenedFloatMenu)
					{
						floatMenuGiz = gizmo3;
					}
					if ((int)gizmoResult.State >= 1)
					{
						mouseoverGizmo = gizmo3;
					}
					float x2 = topLeft.x;
					float y2 = topLeft.y;
					float width = gizmo3.GetWidth(maxWidth);
					Vector2 gizmoSpacing3 = GizmoSpacing;
					Rect rect = new Rect(x2, y2, width, 75f + gizmoSpacing3.y);
					rect = rect.ContractedBy(-12f);
					GenUI.AbsorbClicksInRect(rect);
					float x3 = topLeft.x;
					float width2 = gizmo3.GetWidth(maxWidth);
					Vector2 gizmoSpacing4 = GizmoSpacing;
					topLeft.x = x3 + (width2 + gizmoSpacing4.x);
				}
			}
			if (interactedGiz != null)
			{
				List<Gizmo> list3 = gizmoGroups.First((List<Gizmo> group) => group.Contains(interactedGiz));
				for (int n = 0; n < list3.Count; n++)
				{
					Gizmo gizmo4 = list3[n];
					if (gizmo4 != interactedGiz && !gizmo4.disabled && interactedGiz.InheritInteractionsFrom(gizmo4))
					{
						gizmo4.ProcessInput(ev);
					}
				}
				interactedGiz.ProcessInput(ev);
				Event.current.Use();
			}
			else if (floatMenuGiz != null)
			{
				List<FloatMenuOption> list4 = new List<FloatMenuOption>();
				foreach (FloatMenuOption rightClickFloatMenuOption in floatMenuGiz.RightClickFloatMenuOptions)
				{
					list4.Add(rightClickFloatMenuOption);
				}
				List<Gizmo> list5 = gizmoGroups.First((List<Gizmo> group) => group.Contains(floatMenuGiz));
				Action prevAction;
				Action localOptionAction;
				for (int num3 = 0; num3 < list5.Count; num3++)
				{
					Gizmo gizmo5 = list5[num3];
					if (gizmo5 != floatMenuGiz && !gizmo5.disabled && floatMenuGiz.InheritFloatMenuInteractionsFrom(gizmo5))
					{
						foreach (FloatMenuOption rightClickFloatMenuOption2 in gizmo5.RightClickFloatMenuOptions)
						{
							FloatMenuOption floatMenuOption = list4.Find((FloatMenuOption x) => x.Label == rightClickFloatMenuOption2.Label);
							if (floatMenuOption == null)
							{
								list4.Add(rightClickFloatMenuOption2);
							}
							else if (!rightClickFloatMenuOption2.Disabled)
							{
								if (!floatMenuOption.Disabled)
								{
									prevAction = floatMenuOption.action;
									localOptionAction = rightClickFloatMenuOption2.action;
									floatMenuOption.action = delegate
									{
										prevAction();
										localOptionAction();
									};
								}
								else if (floatMenuOption.Disabled)
								{
									list4[list4.IndexOf(floatMenuOption)] = rightClickFloatMenuOption2;
								}
							}
						}
					}
				}
				Event.current.Use();
				if (list4.Any())
				{
					Find.WindowStack.Add(new FloatMenu(list4));
				}
			}
			gizmoGroups.Clear();
			firstGizmos.Clear();
			tmpAllGizmos.Clear();
		}
	}
}
