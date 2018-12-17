using System;
using Verse;

namespace RimWorld
{
	public struct QualityRange : IEquatable<QualityRange>
	{
		public QualityCategory min;

		public QualityCategory max;

		public static QualityRange All => new QualityRange(QualityCategory.Awful, QualityCategory.Legendary);

		public QualityRange(QualityCategory min, QualityCategory max)
		{
			this.min = min;
			this.max = max;
		}

		public bool Includes(QualityCategory p)
		{
			return (int)p >= (int)min && (int)p <= (int)max;
		}

		public static bool operator ==(QualityRange a, QualityRange b)
		{
			return a.min == b.min && a.max == b.max;
		}

		public static bool operator !=(QualityRange a, QualityRange b)
		{
			return !(a == b);
		}

		public static QualityRange FromString(string s)
		{
			string[] array = s.Split('~');
			return new QualityRange((QualityCategory)ParseHelper.FromString(array[0], typeof(QualityCategory)), (QualityCategory)ParseHelper.FromString(array[1], typeof(QualityCategory)));
		}

		public override string ToString()
		{
			return min.ToString() + "~" + max.ToString();
		}

		public override int GetHashCode()
		{
			return Gen.HashCombineStruct(min.GetHashCode(), max);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is QualityRange))
			{
				return false;
			}
			QualityRange qualityRange = (QualityRange)obj;
			return qualityRange.min == min && qualityRange.max == max;
		}

		public bool Equals(QualityRange other)
		{
			return other.min == min && other.max == max;
		}
	}
}
