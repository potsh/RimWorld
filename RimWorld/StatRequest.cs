using System;
using Verse;

namespace RimWorld
{
	public struct StatRequest : IEquatable<StatRequest>
	{
		private Thing thingInt;

		private BuildableDef defInt;

		private ThingDef stuffDefInt;

		private QualityCategory qualityCategoryInt;

		public Thing Thing => thingInt;

		public BuildableDef Def => defInt;

		public ThingDef StuffDef => stuffDefInt;

		public QualityCategory QualityCategory => qualityCategoryInt;

		public bool HasThing => Thing != null;

		public bool Empty => Def == null;

		public static StatRequest For(Thing thing)
		{
			if (thing == null)
			{
				Log.Error("StatRequest for null thing.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = thing;
			result.defInt = thing.def;
			result.stuffDefInt = thing.Stuff;
			thing.TryGetQuality(out result.qualityCategoryInt);
			return result;
		}

		public static StatRequest For(BuildableDef def, ThingDef stuffDef, QualityCategory quality = QualityCategory.Normal)
		{
			if (def == null)
			{
				Log.Error("StatRequest for null def.");
				return ForEmpty();
			}
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.defInt = def;
			result.stuffDefInt = stuffDef;
			result.qualityCategoryInt = quality;
			return result;
		}

		public static StatRequest ForEmpty()
		{
			StatRequest result = default(StatRequest);
			result.thingInt = null;
			result.defInt = null;
			result.stuffDefInt = null;
			result.qualityCategoryInt = QualityCategory.Normal;
			return result;
		}

		public override string ToString()
		{
			if (Thing != null)
			{
				return "(" + Thing + ")";
			}
			return "(" + Thing + ", " + ((StuffDef == null) ? "null" : StuffDef.defName) + ")";
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombineInt(seed, defInt.shortHash);
			if (thingInt != null)
			{
				seed = Gen.HashCombineInt(seed, thingInt.thingIDNumber);
			}
			if (stuffDefInt != null)
			{
				seed = Gen.HashCombineInt(seed, stuffDefInt.shortHash);
			}
			return seed;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is StatRequest))
			{
				return false;
			}
			StatRequest statRequest = (StatRequest)obj;
			return statRequest.defInt == defInt && statRequest.thingInt == thingInt && statRequest.stuffDefInt == stuffDefInt;
		}

		public bool Equals(StatRequest other)
		{
			return other.defInt == defInt && other.thingInt == thingInt && other.stuffDefInt == stuffDefInt;
		}

		public static bool operator ==(StatRequest lhs, StatRequest rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(StatRequest lhs, StatRequest rhs)
		{
			return !(lhs == rhs);
		}
	}
}
