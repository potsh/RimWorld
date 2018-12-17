using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class DoorsDebugDrawer
	{
		public static void DrawDebug()
		{
			if (DebugViewSettings.drawDoorsDebug)
			{
				CellRect currentViewRect = Find.CameraDriver.CurrentViewRect;
				List<Thing> list = Find.CurrentMap.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial);
				for (int i = 0; i < list.Count; i++)
				{
					if (currentViewRect.Contains(list[i].Position))
					{
						Building_Door building_Door = list[i] as Building_Door;
						if (building_Door != null)
						{
							Color col = building_Door.FreePassage ? new Color(0f, 1f, 0f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
							CellRenderer.RenderCell(building_Door.Position, SolidColorMaterials.SimpleSolidColorMaterial(col));
						}
					}
				}
			}
		}
	}
}