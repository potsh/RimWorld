using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class MouseoverReadout
	{
		private TerrainDef cachedTerrain;

		private string cachedTerrainString;

		private string[] glowStrings;

		private const float YInterval = 19f;

		private static readonly Vector2 BotLeft = new Vector2(15f, 65f);

		public MouseoverReadout()
		{
			MakePermaCache();
		}

		private void MakePermaCache()
		{
			glowStrings = new string[101];
			for (int i = 0; i <= 100; i++)
			{
				glowStrings[i] = GlowGrid.PsychGlowAtGlow((float)i / 100f).GetLabel() + " (" + ((float)i / 100f).ToStringPercent() + ")";
			}
		}

		public void MouseoverReadoutOnGUI()
		{
			if (Event.current.type == EventType.Repaint && Find.MainTabsRoot.OpenTab == null)
			{
				GenUI.DrawTextWinterShadow(new Rect(256f, (float)(UI.screenHeight - 256), -256f, 256f));
				Text.Font = GameFont.Small;
				GUI.color = new Color(1f, 1f, 1f, 0.8f);
				IntVec3 c = UI.MouseCell();
				if (c.InBounds(Find.CurrentMap))
				{
					float num = 0f;
					Rect rect = default(Rect);
					if (c.Fogged(Find.CurrentMap))
					{
						Vector2 botLeft = BotLeft;
						float x = botLeft.x;
						float num2 = (float)UI.screenHeight;
						Vector2 botLeft2 = BotLeft;
						rect = new Rect(x, num2 - botLeft2.y - num, 999f, 999f);
						Widgets.Label(rect, "Undiscovered".Translate());
						GUI.color = Color.white;
					}
					else
					{
						Vector2 botLeft3 = BotLeft;
						float x2 = botLeft3.x;
						float num3 = (float)UI.screenHeight;
						Vector2 botLeft4 = BotLeft;
						rect = new Rect(x2, num3 - botLeft4.y - num, 999f, 999f);
						int num4 = Mathf.RoundToInt(Find.CurrentMap.glowGrid.GameGlowAt(c) * 100f);
						Widgets.Label(rect, glowStrings[num4]);
						num += 19f;
						Vector2 botLeft5 = BotLeft;
						float x3 = botLeft5.x;
						float num5 = (float)UI.screenHeight;
						Vector2 botLeft6 = BotLeft;
						rect = new Rect(x3, num5 - botLeft6.y - num, 999f, 999f);
						TerrainDef terrain = c.GetTerrain(Find.CurrentMap);
						if (terrain != cachedTerrain)
						{
							string str = (!((double)terrain.fertility > 0.0001)) ? string.Empty : (" " + "FertShort".Translate() + " " + terrain.fertility.ToStringPercent());
							cachedTerrainString = terrain.LabelCap + ((terrain.passability == Traversability.Impassable) ? null : (" (" + "WalkSpeed".Translate(SpeedPercentString((float)terrain.pathCost)) + str + ")"));
							cachedTerrain = terrain;
						}
						Widgets.Label(rect, cachedTerrainString);
						num += 19f;
						Zone zone = c.GetZone(Find.CurrentMap);
						if (zone != null)
						{
							Vector2 botLeft7 = BotLeft;
							float x4 = botLeft7.x;
							float num6 = (float)UI.screenHeight;
							Vector2 botLeft8 = BotLeft;
							rect = new Rect(x4, num6 - botLeft8.y - num, 999f, 999f);
							string label = zone.label;
							Widgets.Label(rect, label);
							num += 19f;
						}
						float depth = Find.CurrentMap.snowGrid.GetDepth(c);
						if (depth > 0.03f)
						{
							Vector2 botLeft9 = BotLeft;
							float x5 = botLeft9.x;
							float num7 = (float)UI.screenHeight;
							Vector2 botLeft10 = BotLeft;
							rect = new Rect(x5, num7 - botLeft10.y - num, 999f, 999f);
							SnowCategory snowCategory = SnowUtility.GetSnowCategory(depth);
							string label2 = SnowUtility.GetDescription(snowCategory) + " (" + "WalkSpeed".Translate(SpeedPercentString((float)SnowUtility.MovementTicksAddOn(snowCategory))) + ")";
							Widgets.Label(rect, label2);
							num += 19f;
						}
						List<Thing> thingList = c.GetThingList(Find.CurrentMap);
						for (int i = 0; i < thingList.Count; i++)
						{
							Thing thing = thingList[i];
							if (thing.def.category != ThingCategory.Mote)
							{
								Vector2 botLeft11 = BotLeft;
								float x6 = botLeft11.x;
								float num8 = (float)UI.screenHeight;
								Vector2 botLeft12 = BotLeft;
								rect = new Rect(x6, num8 - botLeft12.y - num, 999f, 999f);
								string labelMouseover = thing.LabelMouseover;
								Widgets.Label(rect, labelMouseover);
								num += 19f;
							}
						}
						RoofDef roof = c.GetRoof(Find.CurrentMap);
						if (roof != null)
						{
							Vector2 botLeft13 = BotLeft;
							float x7 = botLeft13.x;
							float num9 = (float)UI.screenHeight;
							Vector2 botLeft14 = BotLeft;
							rect = new Rect(x7, num9 - botLeft14.y - num, 999f, 999f);
							Widgets.Label(rect, roof.LabelCap);
							num += 19f;
						}
						GUI.color = Color.white;
					}
				}
			}
		}

		private string SpeedPercentString(float extraPathTicks)
		{
			float f = 13f / (extraPathTicks + 13f);
			return f.ToStringPercent();
		}
	}
}
