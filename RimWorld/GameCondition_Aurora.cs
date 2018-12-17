using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition_Aurora : GameCondition
	{
		private int curColorIndex = -1;

		private int prevColorIndex = -1;

		private float curColorTransition;

		private const int LerpTicks = 200;

		public const float MaxSunGlow = 0.5f;

		private const float Glow = 0.25f;

		private const float SkyColorStrength = 0.075f;

		private const float OverlayColorStrength = 0.025f;

		private const float BaseBrightness = 0.73f;

		private const int TransitionDurationTicks_NotPermanent = 280;

		private const int TransitionDurationTicks_Permanent = 3750;

		private static readonly Color[] Colors = new Color[8]
		{
			new Color(0f, 1f, 0f),
			new Color(0.3f, 1f, 0f),
			new Color(0f, 1f, 0.7f),
			new Color(0.3f, 1f, 0.7f),
			new Color(0f, 0.5f, 1f),
			new Color(0f, 0f, 1f),
			new Color(0.87f, 0f, 1f),
			new Color(0.75f, 0f, 1f)
		};

		public Color CurrentColor => Color.Lerp(Colors[prevColorIndex], Colors[curColorIndex], curColorTransition);

		private int TransitionDurationTicks => (!base.Permanent) ? 280 : 3750;

		private bool BrightInAllMaps
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					if (GenCelestial.CurCelestialSunGlow(maps[i]) <= 0.5f)
					{
						return false;
					}
				}
				return true;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref curColorIndex, "curColorIndex", 0);
			Scribe_Values.Look(ref prevColorIndex, "prevColorIndex", 0);
			Scribe_Values.Look(ref curColorTransition, "curColorTransition", 0f);
		}

		public override void Init()
		{
			base.Init();
			curColorIndex = Rand.Range(0, Colors.Length);
			prevColorIndex = curColorIndex;
			curColorTransition = 1f;
		}

		public override float SkyGazeChanceFactor(Map map)
		{
			return 8f;
		}

		public override float SkyGazeJoyGainFactor(Map map)
		{
			return 5f;
		}

		public override float SkyTargetLerpFactor(Map map)
		{
			return GameConditionUtility.LerpInOutValue(this, 200f);
		}

		public override SkyTarget? SkyTarget(Map map)
		{
			Color currentColor = CurrentColor;
			SkyColorSet colorSet = new SkyColorSet(Color.Lerp(Color.white, currentColor, 0.075f) * Brightness(map), new Color(0.92f, 0.92f, 0.92f), Color.Lerp(Color.white, currentColor, 0.025f) * Brightness(map), 1f);
			float glow = Mathf.Max(GenCelestial.CurCelestialSunGlow(map), 0.25f);
			return new SkyTarget(glow, colorSet, 1f, 1f);
		}

		private float Brightness(Map map)
		{
			return Mathf.Max(0.73f, GenCelestial.CurCelestialSunGlow(map));
		}

		public override void GameConditionTick()
		{
			curColorTransition += 1f / (float)TransitionDurationTicks;
			if (curColorTransition >= 1f)
			{
				prevColorIndex = curColorIndex;
				curColorIndex = GetNewColorIndex();
				curColorTransition = 0f;
			}
			if (!base.Permanent && base.TicksLeft > 200 && BrightInAllMaps)
			{
				base.TicksLeft = 200;
			}
		}

		private int GetNewColorIndex()
		{
			return (from x in Enumerable.Range(0, Colors.Length)
			where x != curColorIndex
			select x).RandomElement();
		}
	}
}
