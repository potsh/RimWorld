using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class MainButtonsRoot
	{
		public MainTabsRoot tabs = new MainTabsRoot();

		private List<MainButtonDef> allButtonsInOrder;

		private int VisibleButtonsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < allButtonsInOrder.Count; i++)
				{
					if (allButtonsInOrder[i].buttonVisible)
					{
						num++;
					}
				}
				return num;
			}
		}

		public MainButtonsRoot()
		{
			allButtonsInOrder = (from x in DefDatabase<MainButtonDef>.AllDefs
			orderby x.order
			select x).ToList();
		}

		public void MainButtonsOnGUI()
		{
			if (Event.current.type != EventType.Layout)
			{
				DoButtons();
				int num = 0;
				while (true)
				{
					if (num >= allButtonsInOrder.Count)
					{
						return;
					}
					if (!allButtonsInOrder[num].Worker.Disabled && allButtonsInOrder[num].hotKey != null && allButtonsInOrder[num].hotKey.KeyDownEvent)
					{
						break;
					}
					num++;
				}
				Event.current.Use();
				allButtonsInOrder[num].Worker.InterfaceTryActivate();
			}
		}

		public void HandleLowPriorityShortcuts()
		{
			tabs.HandleLowPriorityShortcuts();
			if (WorldRendererUtility.WorldRenderedNow && Current.ProgramState == ProgramState.Playing && Find.CurrentMap != null && KeyBindingDefOf.Cancel.KeyDownEvent)
			{
				Event.current.Use();
				Find.World.renderer.wantedMode = WorldRenderMode.None;
			}
		}

		private void DoButtons()
		{
			GUI.color = Color.white;
			int visibleButtonsCount = VisibleButtonsCount;
			int num = (int)((float)UI.screenWidth / (float)visibleButtonsCount);
			int num2 = allButtonsInOrder.FindLastIndex((MainButtonDef x) => x.buttonVisible);
			int num3 = 0;
			for (int i = 0; i < allButtonsInOrder.Count; i++)
			{
				if (allButtonsInOrder[i].buttonVisible)
				{
					int num4 = num;
					if (i == num2)
					{
						num4 = UI.screenWidth - num3;
					}
					Rect rect = new Rect((float)num3, (float)(UI.screenHeight - 35), (float)num4, 35f);
					allButtonsInOrder[i].Worker.DoButton(rect);
					num3 += num;
				}
			}
		}
	}
}
