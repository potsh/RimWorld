using System;
using UnityEngine;

namespace Verse
{
	public struct FloatRange : IEquatable<FloatRange>
	{
		public float min;

		public float max;

		public static FloatRange Zero => new FloatRange(0f, 0f);

		public static FloatRange One => new FloatRange(1f, 1f);

		public static FloatRange ZeroToOne => new FloatRange(0f, 1f);

		public float Average => (min + max) / 2f;

		public float RandomInRange => Rand.Range(min, max);

		public float TrueMin => Mathf.Min(min, max);

		public float TrueMax => Mathf.Max(min, max);

		public float Span => TrueMax - TrueMin;

		public FloatRange(float min, float max)
		{
			this.min = min;
			this.max = max;
		}

		public float LerpThroughRange(float lerpPct)
		{
			return Mathf.Lerp(min, max, lerpPct);
		}

		public float InverseLerpThroughRange(float f)
		{
			return Mathf.InverseLerp(min, max, f);
		}

		public bool Includes(float f)
		{
			return f >= min && f <= max;
		}

		public bool IncludesEpsilon(float f)
		{
			return f >= min - 1E-05f && f <= max + 1E-05f;
		}

		public FloatRange ExpandedBy(float f)
		{
			return new FloatRange(min - f, max + f);
		}

		public static bool operator ==(FloatRange a, FloatRange b)
		{
			return a.min == b.min && a.max == b.max;
		}

		public static bool operator !=(FloatRange a, FloatRange b)
		{
			return a.min != b.min || a.max != b.max;
		}

		public static FloatRange operator *(FloatRange r, float val)
		{
			return new FloatRange(r.min * val, r.max * val);
		}

		public static FloatRange FromString(string s)
		{
			string[] array = s.Split('~');
			if (array.Length == 1)
			{
				float num = Convert.ToSingle(array[0]);
				return new FloatRange(num, num);
			}
			return new FloatRange(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]));
		}

		public override string ToString()
		{
			return min + "~" + max;
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(min.GetHashCode(), max);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is FloatRange))
			{
				return false;
			}
			return Equals((FloatRange)obj);
		}

		public bool Equals(FloatRange other)
		{
			return other.min == min && other.max == max;
		}
	}
}
