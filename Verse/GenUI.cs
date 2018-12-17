using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	[StaticConstructorOnStartup]
	public static class GenUI
	{
		public const float Pad = 10f;

		public const float GapTiny = 4f;

		public const float GapSmall = 10f;

		public const float Gap = 17f;

		public const float GapWide = 26f;

		public const float ListSpacing = 28f;

		public const float MouseAttachIconSize = 32f;

		public const float MouseAttachIconOffset = 8f;

		public const float ScrollBarWidth = 16f;

		public const float HorizontalSliderHeight = 16f;

		public static readonly Vector2 TradeableDrawSize = new Vector2(150f, 45f);

		public static readonly Color MouseoverColor = new Color(0.3f, 0.7f, 0.9f);

		public static readonly Color SubtleMouseoverColor = new Color(0.7f, 0.7f, 0.7f);

		public static readonly Vector2 MaxWinSize = new Vector2(1010f, 754f);

		public const float SmallIconSize = 24f;

		public const int RootGUIDepth = 50;

		public const int CameraGUIDepth = 100;

		private const float MouseIconSize = 32f;

		private const float MouseIconOffset = 12f;

		private static readonly Material MouseoverBracketMaterial = MaterialPool.MatFrom("UI/Overlays/MouseoverBracketTex", ShaderDatabase.MetaOverlay);

		private static readonly Texture2D UnderShadowTex = ContentFinder<Texture2D>.Get("UI/Misc/ScreenCornerShadow");

		private static readonly Texture2D UIFlash = ContentFinder<Texture2D>.Get("UI/Misc/Flash");

		private static Dictionary<string, float> labelWidthCache = new Dictionary<string, float>();

		private static readonly Vector2 PieceBarSize = new Vector2(100f, 17f);

		public const float PawnDirectClickRadius = 0.4f;

		private static List<Pawn> clickedPawns = new List<Pawn>();

		[CompilerGenerated]
		private static Comparison<Pawn> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Comparison<Thing> _003C_003Ef__mg_0024cache1;

		[CompilerGenerated]
		private static Comparison<Pawn> _003C_003Ef__mg_0024cache2;

		public static void SetLabelAlign(TextAnchor a)
		{
			Text.Anchor = a;
		}

		public static void ResetLabelAlign()
		{
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public static float BackgroundDarkAlphaForText()
		{
			if (Find.CurrentMap == null)
			{
				return 0f;
			}
			float num = GenCelestial.CurCelestialSunGlow(Find.CurrentMap);
			float num2 = (Find.CurrentMap.Biome != BiomeDefOf.IceSheet) ? Mathf.Clamp01(Find.CurrentMap.snowGrid.TotalDepth / 1000f) : 1f;
			return num * num2 * 0.41f;
		}

		public static void DrawTextWinterShadow(Rect rect)
		{
			float num = BackgroundDarkAlphaForText();
			if (num > 0.001f)
			{
				GUI.color = new Color(1f, 1f, 1f, num);
				GUI.DrawTexture(rect, UnderShadowTex);
				GUI.color = Color.white;
			}
		}

		public static void DrawTextureWithMaterial(Rect rect, Texture texture, Material material, Rect texCoords = default(Rect))
		{
			if (texCoords == default(Rect))
			{
				if (material == null)
				{
					GUI.DrawTexture(rect, texture);
				}
				else if (Event.current.type == EventType.Repaint)
				{
					Rect sourceRect = new Rect(0f, 0f, 1f, 1f);
					Color color = GUI.color;
					float r = color.r * 0.5f;
					Color color2 = GUI.color;
					float g = color2.g * 0.5f;
					Color color3 = GUI.color;
					Graphics.DrawTexture(rect, texture, sourceRect, 0, 0, 0, 0, new Color(r, g, color3.b * 0.5f, 0.5f), material);
				}
			}
			else if (material == null)
			{
				GUI.DrawTextureWithTexCoords(rect, texture, texCoords);
			}
			else if (Event.current.type == EventType.Repaint)
			{
				Color color4 = GUI.color;
				float r2 = color4.r * 0.5f;
				Color color5 = GUI.color;
				float g2 = color5.g * 0.5f;
				Color color6 = GUI.color;
				Graphics.DrawTexture(rect, texture, texCoords, 0, 0, 0, 0, new Color(r2, g2, color6.b * 0.5f, 0.5f), material);
			}
		}

		public static float IconDrawScale(ThingDef tDef)
		{
			float num = tDef.uiIconScale;
			if (tDef.uiIconPath.NullOrEmpty() && tDef.graphicData != null)
			{
				IntVec2 intVec = tDef.defaultPlacingRot.IsHorizontal ? tDef.Size.Rotated() : tDef.Size;
				num *= Mathf.Min(tDef.graphicData.drawSize.x / (float)intVec.x, tDef.graphicData.drawSize.y / (float)intVec.z);
			}
			return num;
		}

		public static void ErrorDialog(string message)
		{
			if (Find.WindowStack != null)
			{
				Find.WindowStack.Add(new Dialog_MessageBox(message));
			}
		}

		public static void DrawFlash(float centerX, float centerY, float size, float alpha, Color color)
		{
			Rect position = new Rect(centerX - size / 2f, centerY - size / 2f, size, size);
			Color color2 = color;
			color2.a = alpha;
			GUI.color = color2;
			GUI.DrawTexture(position, UIFlash);
			GUI.color = Color.white;
		}

		public static float GetWidthCached(this string s)
		{
			if (labelWidthCache.Count > 2000 || (Time.frameCount % 40000 == 0 && labelWidthCache.Count > 100))
			{
				labelWidthCache.Clear();
			}
			if (labelWidthCache.TryGetValue(s, out float value))
			{
				return value;
			}
			Vector2 vector = Text.CalcSize(s);
			value = vector.x;
			labelWidthCache.Add(s, value);
			return value;
		}

		public static Rect Rounded(this Rect r)
		{
			return new Rect((float)(int)r.x, (float)(int)r.y, (float)(int)r.width, (float)(int)r.height);
		}

		public static Vector2 Rounded(this Vector2 v)
		{
			return new Vector2((float)(int)v.x, (float)(int)v.y);
		}

		public static float DistFromRect(Rect r, Vector2 p)
		{
			float x = p.x;
			Vector2 center = r.center;
			float num = Mathf.Abs(x - center.x) - r.width / 2f;
			if (num < 0f)
			{
				num = 0f;
			}
			float y = p.y;
			Vector2 center2 = r.center;
			float num2 = Mathf.Abs(y - center2.y) - r.height / 2f;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			return Mathf.Sqrt(num * num + num2 * num2);
		}

		public static void DrawMouseAttachment(Texture iconTex, string text = "", float angle = 0f, Vector2 offset = default(Vector2), Rect? customRect = default(Rect?))
		{
			Vector2 mousePosition = Event.current.mousePosition;
			float num = mousePosition.y + 12f;
			if (iconTex != null)
			{
				Rect mouseRect;
				if (customRect.HasValue)
				{
					mouseRect = customRect.Value;
				}
				else
				{
					mouseRect = new Rect(mousePosition.x + 8f, num + 8f, 32f, 32f);
				}
				Find.WindowStack.ImmediateWindow(34003428, mouseRect, WindowLayer.Super, delegate
				{
					Rect rect = mouseRect.AtZero();
					Vector2 position = rect.position;
					float x = offset.x;
					Vector2 size = rect.size;
					float x2 = x * size.x;
					float y = offset.y;
					Vector2 size2 = rect.size;
					rect.position = position + new Vector2(x2, y * size2.y);
					Widgets.DrawTextureRotated(rect, iconTex, angle);
				}, doBackground: false, absorbInputAroundWindow: false, 0f);
				num += mouseRect.height;
			}
			if (text != string.Empty)
			{
				Rect textRect = new Rect(mousePosition.x + 12f, num, 200f, 9999f);
				Find.WindowStack.ImmediateWindow(34003429, textRect, WindowLayer.Super, delegate
				{
					Widgets.Label(textRect.AtZero(), text);
				}, doBackground: false, absorbInputAroundWindow: false, 0f);
			}
		}

		public static void DrawMouseAttachment(Texture2D icon)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			Rect mouseRect = new Rect(mousePosition.x + 8f, mousePosition.y + 8f, 32f, 32f);
			Find.WindowStack.ImmediateWindow(34003428, mouseRect, WindowLayer.Super, delegate
			{
				GUI.DrawTexture(mouseRect.AtZero(), icon);
			}, doBackground: false, absorbInputAroundWindow: false, 0f);
		}

		public static void RenderMouseoverBracket()
		{
			Vector3 position = UI.MouseCell().ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays);
			Graphics.DrawMesh(MeshPool.plane10, position, Quaternion.identity, MouseoverBracketMaterial, 0);
		}

		public static void DrawStatusLevel(Need status, Rect rect)
		{
			GUI.BeginGroup(rect);
			Rect rect2 = new Rect(0f, 2f, rect.width, 25f);
			Widgets.Label(rect2, status.LabelCap);
			Vector2 pieceBarSize = PieceBarSize;
			float x = pieceBarSize.x;
			Vector2 pieceBarSize2 = PieceBarSize;
			Rect rect3 = new Rect(100f, 3f, x, pieceBarSize2.y);
			Widgets.FillableBar(rect3, status.CurLevelPercentage);
			Widgets.FillableBarChangeArrows(rect3, status.GUIChangeArrow);
			GUI.EndGroup();
			TooltipHandler.TipRegion(rect, status.GetTipString());
			if (Mouse.IsOver(rect))
			{
				GUI.DrawTexture(rect, TexUI.HighlightTex);
			}
		}

		public static IEnumerable<LocalTargetInfo> TargetsAtMouse(TargetingParameters clickParams, bool thingsOnly = false)
		{
			return TargetsAt(UI.MouseMapPosition(), clickParams, thingsOnly);
		}

		public static IEnumerable<LocalTargetInfo> TargetsAt(Vector3 clickPos, TargetingParameters clickParams, bool thingsOnly = false)
		{
			List<Thing> clickableList = ThingsUnderMouse(clickPos, 0.8f, clickParams);
			int i = 0;
			if (i < clickableList.Count)
			{
				yield return (LocalTargetInfo)clickableList[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!thingsOnly)
			{
				IntVec3 cellTarg = UI.MouseCell();
				if (cellTarg.InBounds(Find.CurrentMap) && clickParams.CanTarget(new TargetInfo(cellTarg, Find.CurrentMap)))
				{
					yield return (LocalTargetInfo)cellTarg;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static List<Thing> ThingsUnderMouse(Vector3 clickPos, float pawnWideClickRadius, TargetingParameters clickParams)
		{
			IntVec3 c = IntVec3.FromVector3(clickPos);
			List<Thing> list = new List<Thing>();
			clickedPawns.Clear();
			List<Pawn> allPawnsSpawned = Find.CurrentMap.mapPawns.AllPawnsSpawned;
			for (int i = 0; i < allPawnsSpawned.Count; i++)
			{
				Pawn pawn = allPawnsSpawned[i];
				if ((pawn.DrawPos - clickPos).MagnitudeHorizontal() < 0.4f && clickParams.CanTarget(pawn))
				{
					clickedPawns.Add(pawn);
				}
			}
			clickedPawns.Sort(CompareThingsByDistanceToMousePointer);
			for (int j = 0; j < clickedPawns.Count; j++)
			{
				list.Add(clickedPawns[j]);
			}
			List<Thing> list2 = new List<Thing>();
			foreach (Thing item in Find.CurrentMap.thingGrid.ThingsAt(c))
			{
				if (!list.Contains(item) && clickParams.CanTarget(item))
				{
					list2.Add(item);
				}
			}
			list2.Sort(CompareThingsByDrawAltitude);
			list.AddRange(list2);
			clickedPawns.Clear();
			List<Pawn> allPawnsSpawned2 = Find.CurrentMap.mapPawns.AllPawnsSpawned;
			for (int k = 0; k < allPawnsSpawned2.Count; k++)
			{
				Pawn pawn2 = allPawnsSpawned2[k];
				if ((pawn2.DrawPos - clickPos).MagnitudeHorizontal() < pawnWideClickRadius && clickParams.CanTarget(pawn2))
				{
					clickedPawns.Add(pawn2);
				}
			}
			clickedPawns.Sort(CompareThingsByDistanceToMousePointer);
			for (int l = 0; l < clickedPawns.Count; l++)
			{
				if (!list.Contains(clickedPawns[l]))
				{
					list.Add(clickedPawns[l]);
				}
			}
			list.RemoveAll((Thing t) => !t.Spawned);
			clickedPawns.Clear();
			return list;
		}

		private static int CompareThingsByDistanceToMousePointer(Thing a, Thing b)
		{
			Vector3 b2 = UI.MouseMapPosition();
			float num = (a.DrawPos - b2).MagnitudeHorizontalSquared();
			float num2 = (b.DrawPos - b2).MagnitudeHorizontalSquared();
			if (num < num2)
			{
				return -1;
			}
			if (num == num2)
			{
				return 0;
			}
			return 1;
		}

		private static int CompareThingsByDrawAltitude(Thing A, Thing B)
		{
			if (A.def.Altitude < B.def.Altitude)
			{
				return 1;
			}
			if (A.def.Altitude == B.def.Altitude)
			{
				return 0;
			}
			return -1;
		}

		public static int CurrentAdjustmentMultiplier()
		{
			if (KeyBindingDefOf.ModifierIncrement_10x.IsDownEvent && KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent)
			{
				return 1000;
			}
			if (KeyBindingDefOf.ModifierIncrement_100x.IsDownEvent)
			{
				return 100;
			}
			if (KeyBindingDefOf.ModifierIncrement_10x.IsDownEvent)
			{
				return 10;
			}
			return 1;
		}

		public static Rect GetInnerRect(this Rect rect)
		{
			return rect.ContractedBy(17f);
		}

		public static Rect ExpandedBy(this Rect rect, float margin)
		{
			return new Rect(rect.x - margin, rect.y - margin, rect.width + margin * 2f, rect.height + margin * 2f);
		}

		public static Rect ContractedBy(this Rect rect, float margin)
		{
			return new Rect(rect.x + margin, rect.y + margin, rect.width - margin * 2f, rect.height - margin * 2f);
		}

		public static Rect ScaledBy(this Rect rect, float scale)
		{
			rect.x -= rect.width * (scale - 1f) / 2f;
			rect.y -= rect.height * (scale - 1f) / 2f;
			rect.width *= scale;
			rect.height *= scale;
			return rect;
		}

		public static Rect CenteredOnXIn(this Rect rect, Rect otherRect)
		{
			return new Rect(otherRect.x + (otherRect.width - rect.width) / 2f, rect.y, rect.width, rect.height);
		}

		public static Rect CenteredOnYIn(this Rect rect, Rect otherRect)
		{
			return new Rect(rect.x, otherRect.y + (otherRect.height - rect.height) / 2f, rect.width, rect.height);
		}

		public static Rect AtZero(this Rect rect)
		{
			return new Rect(0f, 0f, rect.width, rect.height);
		}

		public static void AbsorbClicksInRect(Rect r)
		{
			if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
			{
				Event.current.Use();
			}
		}

		public static Rect LeftHalf(this Rect rect)
		{
			return new Rect(rect.x, rect.y, rect.width / 2f, rect.height);
		}

		public static Rect LeftPart(this Rect rect, float pct)
		{
			return new Rect(rect.x, rect.y, rect.width * pct, rect.height);
		}

		public static Rect LeftPartPixels(this Rect rect, float width)
		{
			return new Rect(rect.x, rect.y, width, rect.height);
		}

		public static Rect RightHalf(this Rect rect)
		{
			return new Rect(rect.x + rect.width / 2f, rect.y, rect.width / 2f, rect.height);
		}

		public static Rect RightPart(this Rect rect, float pct)
		{
			return new Rect(rect.x + rect.width * (1f - pct), rect.y, rect.width * pct, rect.height);
		}

		public static Rect RightPartPixels(this Rect rect, float width)
		{
			return new Rect(rect.x + rect.width - width, rect.y, width, rect.height);
		}

		public static Rect TopHalf(this Rect rect)
		{
			return new Rect(rect.x, rect.y, rect.width, rect.height / 2f);
		}

		public static Rect TopPart(this Rect rect, float pct)
		{
			return new Rect(rect.x, rect.y, rect.width, rect.height * pct);
		}

		public static Rect TopPartPixels(this Rect rect, float height)
		{
			return new Rect(rect.x, rect.y, rect.width, height);
		}

		public static Rect BottomHalf(this Rect rect)
		{
			return new Rect(rect.x, rect.y + rect.height / 2f, rect.width, rect.height / 2f);
		}

		public static Rect BottomPart(this Rect rect, float pct)
		{
			return new Rect(rect.x, rect.y + rect.height * (1f - pct), rect.width, rect.height * pct);
		}

		public static Rect BottomPartPixels(this Rect rect, float height)
		{
			return new Rect(rect.x, rect.y + rect.height - height, rect.width, height);
		}

		public static Color LerpColor(List<Pair<float, Color>> colors, float value)
		{
			if (colors.Count == 0)
			{
				return Color.white;
			}
			for (int i = 0; i < colors.Count; i++)
			{
				if (value < colors[i].First)
				{
					if (i == 0)
					{
						return colors[i].Second;
					}
					return Color.Lerp(colors[i - 1].Second, colors[i].Second, Mathf.InverseLerp(colors[i - 1].First, colors[i].First, value));
				}
			}
			return colors.Last().Second;
		}

		public static Vector2 GetMouseAttachedWindowPos(float width, float height)
		{
			Vector2 mousePosition = Event.current.mousePosition;
			float num = 0f;
			num = ((mousePosition.y + 14f + height < (float)UI.screenHeight) ? (mousePosition.y + 14f) : ((!(mousePosition.y - 5f - height >= 0f)) ? 0f : (mousePosition.y - 5f - height)));
			float num2 = 0f;
			num2 = ((!(mousePosition.x + 16f + width < (float)UI.screenWidth)) ? (mousePosition.x - 4f - width) : (mousePosition.x + 16f));
			return new Vector2(num2, num);
		}

		public static float GetCenteredButtonPos(int buttonIndex, int buttonsCount, float totalWidth, float buttonWidth, float pad = 10f)
		{
			float num = (float)buttonsCount * buttonWidth + (float)(buttonsCount - 1) * pad;
			return Mathf.Floor((totalWidth - num) / 2f + (float)buttonIndex * (buttonWidth + pad));
		}
	}
}
