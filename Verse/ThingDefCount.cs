using System;

namespace Verse
{
	public struct ThingDefCount : IEquatable<ThingDefCount>, IExposable
	{
		private ThingDef thingDef;

		private int count;

		public ThingDef ThingDef => thingDef;

		public int Count => count;

		public ThingDefCount(ThingDef thingDef, int count)
		{
			if (count < 0)
			{
				Log.Warning("Tried to set ThingDefCount count to " + count + ". thingDef=" + thingDef);
				count = 0;
			}
			this.thingDef = thingDef;
			this.count = count;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref count, "count", 1);
		}

		public ThingDefCount WithCount(int newCount)
		{
			return new ThingDefCount(thingDef, newCount);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is ThingDefCount))
			{
				return false;
			}
			return Equals((ThingDefCount)obj);
		}

		public bool Equals(ThingDefCount other)
		{
			return this == other;
		}

		public static bool operator ==(ThingDefCount a, ThingDefCount b)
		{
			return a.thingDef == b.thingDef && a.count == b.count;
		}

		public static bool operator !=(ThingDefCount a, ThingDefCount b)
		{
			return !(a == b);
		}

		public override int GetHashCode()
		{
			return Gen.HashCombine(count, thingDef);
		}

		public override string ToString()
		{
			return "(" + count + "x " + ((thingDef == null) ? "null" : thingDef.defName) + ")";
		}

		public static implicit operator ThingDefCount(ThingDefCountClass t)
		{
			if (t == null)
			{
				return new ThingDefCount(null, 0);
			}
			return new ThingDefCount(t.thingDef, t.count);
		}
	}
}
