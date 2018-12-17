using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class GenThing
	{
		private static List<Thing> tmpThings = new List<Thing>();

		private static List<string> tmpThingLabels = new List<string>();

		private static List<Pair<string, int>> tmpThingCounts = new List<Pair<string, int>>();

		public static Vector3 TrueCenter(this Thing t)
		{
			return (t as Pawn)?.Drawer.DrawPos ?? TrueCenter(t.Position, t.Rotation, t.def.size, t.def.Altitude);
		}

		public static Vector3 TrueCenter(IntVec3 loc, Rot4 rotation, IntVec2 thingSize, float altitude)
		{
			Vector3 result = loc.ToVector3ShiftedWithAltitude(altitude);
			if (thingSize.x != 1 || thingSize.z != 1)
			{
				if (rotation.IsHorizontal)
				{
					int x = thingSize.x;
					thingSize.x = thingSize.z;
					thingSize.z = x;
				}
				switch (rotation.AsInt)
				{
				case 0:
					if (thingSize.x % 2 == 0)
					{
						result.x += 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z += 0.5f;
					}
					break;
				case 1:
					if (thingSize.x % 2 == 0)
					{
						result.x += 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z -= 0.5f;
					}
					break;
				case 2:
					if (thingSize.x % 2 == 0)
					{
						result.x -= 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z -= 0.5f;
					}
					break;
				case 3:
					if (thingSize.x % 2 == 0)
					{
						result.x -= 0.5f;
					}
					if (thingSize.z % 2 == 0)
					{
						result.z += 0.5f;
					}
					break;
				}
			}
			return result;
		}

		public static bool TryDropAndSetForbidden(Thing th, IntVec3 pos, Map map, ThingPlaceMode mode, out Thing resultingThing, bool forbidden)
		{
			if (GenDrop.TryDropSpawn(th, pos, map, ThingPlaceMode.Near, out resultingThing))
			{
				if (resultingThing != null)
				{
					resultingThing.SetForbidden(forbidden, warnOnFail: false);
				}
				return true;
			}
			resultingThing = null;
			return false;
		}

		public static string ThingsToCommaList(IList<Thing> things, bool useAnd = false, bool aggregate = true, int maxCount = -1)
		{
			tmpThings.Clear();
			tmpThingLabels.Clear();
			tmpThingCounts.Clear();
			tmpThings.AddRange(things);
			if (tmpThings.Count >= 2)
			{
				tmpThings.SortByDescending((Thing x) => x is Pawn, (Thing x) => x.MarketValue * (float)x.stackCount);
			}
			for (int i = 0; i < tmpThings.Count; i++)
			{
				string labelNoCount = tmpThings[i].LabelNoCount;
				bool flag = false;
				if (aggregate)
				{
					for (int j = 0; j < tmpThingCounts.Count; j++)
					{
						if (tmpThingCounts[j].First == labelNoCount)
						{
							tmpThingCounts[j] = new Pair<string, int>(tmpThingCounts[j].First, tmpThingCounts[j].Second + tmpThings[i].stackCount);
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					tmpThingCounts.Add(new Pair<string, int>(labelNoCount, tmpThings[i].stackCount));
				}
			}
			tmpThings.Clear();
			bool flag2 = false;
			int num = tmpThingCounts.Count;
			if (maxCount >= 0 && num > maxCount)
			{
				num = maxCount;
				flag2 = true;
			}
			for (int k = 0; k < num; k++)
			{
				string text = tmpThingCounts[k].First;
				if (tmpThingCounts[k].Second != 1)
				{
					text = text + " x" + tmpThingCounts[k].Second;
				}
				tmpThingLabels.Add(text);
			}
			string text2 = tmpThingLabels.ToCommaList(useAnd && !flag2);
			if (flag2)
			{
				text2 += "...";
			}
			return text2;
		}

		public static float GetMarketValue(IList<Thing> things)
		{
			float num = 0f;
			for (int i = 0; i < things.Count; i++)
			{
				num += things[i].MarketValue * (float)things[i].stackCount;
			}
			return num;
		}

		public static void TryAppendSingleRewardInfo(ref string text, IList<Thing> rewards)
		{
			if (rewards.Count == 1 || (rewards.Count >= 2 && rewards.All((Thing x) => x.def == rewards[0].def)))
			{
				string text2 = text;
				text = text2 + "\n\n---\n\n" + rewards[0].LabelCapNoCount + ": " + rewards[0].DescriptionFlavor;
			}
		}
	}
}
