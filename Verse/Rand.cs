using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	public static class Rand
	{
		private static Stack<ulong> stateStack;

		private static RandomNumberGenerator random;

		private static uint iterations;

		private static List<int> tmpRange;

		public static int Seed
		{
			set
			{
				if (stateStack.Count == 0)
				{
					Log.ErrorOnce("Modifying the initial rand seed. Call PushState() first. The initial rand seed should always be based on the startup time and set only once.", 825343540);
				}
				random.seed = (uint)value;
				iterations = 0u;
			}
		}

		public static float Value => random.GetFloat(iterations++);

		public static bool Bool => Value < 0.5f;

		public static int Sign => Bool ? 1 : (-1);

		public static int Int => random.GetInt(iterations++);

		public static Vector3 UnitVector3 => new Vector3(Gaussian(), Gaussian(), Gaussian()).normalized;

		public static Vector2 UnitVector2 => new Vector2(Gaussian(), Gaussian()).normalized;

		public static Vector2 InsideUnitCircle
		{
			get
			{
				Vector2 result;
				do
				{
					result = new Vector2(Value - 0.5f, Value - 0.5f) * 2f;
				}
				while (!(result.sqrMagnitude <= 1f));
				return result;
			}
		}

		public static Vector3 InsideUnitCircleVec3
		{
			get
			{
				Vector2 insideUnitCircle = InsideUnitCircle;
				return new Vector3(insideUnitCircle.x, 0f, insideUnitCircle.y);
			}
		}

		private static ulong StateCompressed
		{
			get
			{
				return random.seed | ((ulong)iterations << 32);
			}
			set
			{
				random.seed = (uint)(value & uint.MaxValue);
				iterations = (uint)((value >> 32) & uint.MaxValue);
			}
		}

		static Rand()
		{
			stateStack = new Stack<ulong>();
			random = new RandomNumberGenerator_BasicHash();
			iterations = 0u;
			tmpRange = new List<int>();
			random.seed = (uint)DateTime.Now.GetHashCode();
		}

		public static void EnsureStateStackEmpty()
		{
			if (stateStack.Count > 0)
			{
				Log.Warning("Random state stack is not empty. There were more calls to PushState than PopState. Fixing.");
				while (stateStack.Any())
				{
					PopState();
				}
			}
		}

		public static float Gaussian(float centerX = 0f, float widthFactor = 1f)
		{
			float value = Value;
			float value2 = Value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin(6.28318548f * value2);
			return num * widthFactor + centerX;
		}

		public static float GaussianAsymmetric(float centerX = 0f, float lowerWidthFactor = 1f, float upperWidthFactor = 1f)
		{
			float value = Value;
			float value2 = Value;
			float num = Mathf.Sqrt(-2f * Mathf.Log(value)) * Mathf.Sin(6.28318548f * value2);
			if (num <= 0f)
			{
				return num * lowerWidthFactor + centerX;
			}
			return num * upperWidthFactor + centerX;
		}

		public static int Range(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return min + Mathf.Abs(Int % (max - min));
		}

		public static int RangeInclusive(int min, int max)
		{
			if (max <= min)
			{
				return min;
			}
			return Range(min, max + 1);
		}

		public static float Range(float min, float max)
		{
			if (max <= min)
			{
				return min;
			}
			return Value * (max - min) + min;
		}

		public static bool Chance(float chance)
		{
			if (chance <= 0f)
			{
				return false;
			}
			if (chance >= 1f)
			{
				return true;
			}
			return Value < chance;
		}

		public static bool ChanceSeeded(float chance, int specialSeed)
		{
			PushState(specialSeed);
			bool result = Chance(chance);
			PopState();
			return result;
		}

		public static float ValueSeeded(int specialSeed)
		{
			PushState(specialSeed);
			float value = Value;
			PopState();
			return value;
		}

		public static float RangeSeeded(float min, float max, int specialSeed)
		{
			PushState(specialSeed);
			float result = Range(min, max);
			PopState();
			return result;
		}

		public static int RangeSeeded(int min, int max, int specialSeed)
		{
			PushState(specialSeed);
			int result = Range(min, max);
			PopState();
			return result;
		}

		public static int RangeInclusiveSeeded(int min, int max, int specialSeed)
		{
			PushState(specialSeed);
			int result = RangeInclusive(min, max);
			PopState();
			return result;
		}

		public static T Element<T>(T a, T b)
		{
			return (!Bool) ? b : a;
		}

		public static T Element<T>(T a, T b, T c)
		{
			float value = Value;
			if (value < 0.33333f)
			{
				return a;
			}
			if (value < 0.66666f)
			{
				return b;
			}
			return c;
		}

		public static T Element<T>(T a, T b, T c, T d)
		{
			float value = Value;
			if (value < 0.25f)
			{
				return a;
			}
			if (value < 0.5f)
			{
				return b;
			}
			if (value < 0.75f)
			{
				return c;
			}
			return d;
		}

		public static T Element<T>(T a, T b, T c, T d, T e)
		{
			float value = Value;
			if (value < 0.2f)
			{
				return a;
			}
			if (value < 0.4f)
			{
				return b;
			}
			if (value < 0.6f)
			{
				return c;
			}
			if (value < 0.8f)
			{
				return d;
			}
			return e;
		}

		public static T Element<T>(T a, T b, T c, T d, T e, T f)
		{
			float value = Value;
			if (value < 0.16666f)
			{
				return a;
			}
			if (value < 0.33333f)
			{
				return b;
			}
			if (value < 0.5f)
			{
				return c;
			}
			if (value < 0.66666f)
			{
				return d;
			}
			if (value < 0.83333f)
			{
				return e;
			}
			return f;
		}

		public static void PushState()
		{
			stateStack.Push(StateCompressed);
		}

		public static void PushState(int replacementSeed)
		{
			PushState();
			Seed = replacementSeed;
		}

		public static void PopState()
		{
			StateCompressed = stateStack.Pop();
		}

		public static float ByCurve(SimpleCurve curve)
		{
			if (curve.PointsCount < 3)
			{
				throw new ArgumentException("curve has < 3 points");
			}
			if (curve[0].y != 0f || curve[curve.PointsCount - 1].y != 0f)
			{
				throw new ArgumentException("curve has start/end point with y != 0");
			}
			float num = 0f;
			for (int i = 0; i < curve.PointsCount - 1; i++)
			{
				if (curve[i].y < 0f)
				{
					throw new ArgumentException("curve has point with y < 0");
				}
				num += (curve[i + 1].x - curve[i].x) * (curve[i].y + curve[i + 1].y);
			}
			float num2 = Range(0f, num);
			for (int j = 0; j < curve.PointsCount - 1; j++)
			{
				float num3 = (curve[j + 1].x - curve[j].x) * (curve[j].y + curve[j + 1].y);
				if (!(num3 < num2))
				{
					float num4 = curve[j + 1].x - curve[j].x;
					float y = curve[j].y;
					float y2 = curve[j + 1].y;
					float num5 = num2 / (y + y2);
					float num6 = Range(0f, (y + y2) / 2f);
					if (num6 > Mathf.Lerp(y, y2, num5 / num4))
					{
						num5 = num4 - num5;
					}
					return num5 + curve[j].x;
				}
				num2 -= num3;
			}
			throw new Exception("Reached end of Rand.ByCurve without choosing a point.");
		}

		public static float ByCurveAverage(SimpleCurve curve)
		{
			float num = 0f;
			float num2 = 0f;
			for (int i = 0; i < curve.PointsCount - 1; i++)
			{
				num += (curve[i + 1].x - curve[i].x) * (curve[i].y + curve[i + 1].y);
				num2 += (curve[i + 1].x - curve[i].x) * (curve[i].x * (2f * curve[i].y + curve[i + 1].y) + curve[i + 1].x * (curve[i].y + 2f * curve[i + 1].y));
			}
			return num2 / num / 3f;
		}

		public static bool MTBEventOccurs(float mtb, float mtbUnit, float checkDuration)
		{
			if (mtb == float.PositiveInfinity)
			{
				return false;
			}
			if (mtb <= 0f)
			{
				Log.Error("MTBEventOccurs with mtb=" + mtb);
				return true;
			}
			if (mtbUnit <= 0f)
			{
				Log.Error("MTBEventOccurs with mtbUnit=" + mtbUnit);
				return false;
			}
			if (checkDuration <= 0f)
			{
				Log.Error("MTBEventOccurs with checkDuration=" + checkDuration);
				return false;
			}
			double num = (double)checkDuration / ((double)mtb * (double)mtbUnit);
			if (num <= 0.0)
			{
				Log.Error("chancePerCheck is " + num + ". mtb=" + mtb + ", mtbUnit=" + mtbUnit + ", checkDuration=" + checkDuration);
				return false;
			}
			double num2 = 1.0;
			if (num < 0.0001)
			{
				while (num < 0.0001)
				{
					num *= 8.0;
					num2 /= 8.0;
				}
				if ((double)Value > num2)
				{
					return false;
				}
			}
			return (double)Value < num;
		}

		[DebugOutput]
		[Category("System")]
		internal static void RandTests()
		{
			StringBuilder stringBuilder = new StringBuilder();
			int @int = Int;
			stringBuilder.AppendLine("Repeating single ValueSeeded with seed " + @int + ". This should give the same result:");
			for (int i = 0; i < 4; i++)
			{
				stringBuilder.AppendLine("   " + ValueSeeded(@int));
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Long-term tests");
			for (int j = 0; j < 3; j++)
			{
				int num = 0;
				for (int k = 0; k < 5000000; k++)
				{
					if (MTBEventOccurs(250f, 60000f, 60f))
					{
						num++;
					}
				}
				string value = "MTB=" + 250 + " days, MTBUnit=" + 60000 + ", check duration=" + 60 + " Simulated " + 5000 + " days (" + 5000000 + " tests). Got " + num + " events.";
				stringBuilder.AppendLine(value);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Short-term tests");
			for (int l = 0; l < 5; l++)
			{
				int num2 = 0;
				for (int m = 0; m < 10000; m++)
				{
					if (MTBEventOccurs(1f, 24000f, 12000f))
					{
						num2++;
					}
				}
				string value2 = "MTB=" + 1f + " days, MTBUnit=" + 24000f + ", check duration=" + 12000f + ", " + 10000 + " tests got " + num2 + " events.";
				stringBuilder.AppendLine(value2);
			}
			for (int n = 0; n < 5; n++)
			{
				int num3 = 0;
				for (int num4 = 0; num4 < 10000; num4++)
				{
					if (MTBEventOccurs(2f, 24000f, 6000f))
					{
						num3++;
					}
				}
				string value3 = "MTB=" + 2f + " days, MTBUnit=" + 24000f + ", check duration=" + 6000f + ", " + 10000 + " tests got " + num3 + " events.";
				stringBuilder.AppendLine(value3);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Near seed tests");
			DebugHistogram debugHistogram = new DebugHistogram(new float[11]
			{
				0f,
				0.1f,
				0.2f,
				0.3f,
				0.4f,
				0.5f,
				0.6f,
				0.7f,
				0.8f,
				0.9f,
				1f
			});
			PushState();
			for (int num5 = 0; num5 < 1000; num5++)
			{
				Seed = num5;
				debugHistogram.Add(Value);
			}
			PopState();
			debugHistogram.Display(stringBuilder);
			Log.Message(stringBuilder.ToString());
		}

		public static int RandSeedForHour(this Thing t, int salt)
		{
			int seed = t.HashOffset();
			seed = Gen.HashCombineInt(seed, Find.TickManager.TicksAbs / 2500);
			return Gen.HashCombineInt(seed, salt);
		}

		public static bool TryRangeInclusiveWhere(int from, int to, Predicate<int> predicate, out int value)
		{
			int num = to - from + 1;
			if (num <= 0)
			{
				value = 0;
				return false;
			}
			int num2 = Mathf.Max(Mathf.RoundToInt(Mathf.Sqrt((float)num)), 5);
			for (int i = 0; i < num2; i++)
			{
				int num3 = RangeInclusive(from, to);
				if (predicate(num3))
				{
					value = num3;
					return true;
				}
			}
			tmpRange.Clear();
			for (int j = from; j <= to; j++)
			{
				tmpRange.Add(j);
			}
			tmpRange.Shuffle();
			int k = 0;
			for (int count = tmpRange.Count; k < count; k++)
			{
				if (predicate(tmpRange[k]))
				{
					value = tmpRange[k];
					return true;
				}
			}
			value = 0;
			return false;
		}

		public static Vector3 PointOnSphereCap(Vector3 center, float angle)
		{
			if (angle <= 0f)
			{
				return center;
			}
			if (angle >= 180f)
			{
				return UnitVector3;
			}
			float num = Range(Mathf.Cos(angle * 0.0174532924f), 1f);
			float f = Range(0f, 6.28318548f);
			Vector3 point = new Vector3(Mathf.Sqrt(1f - num * num) * Mathf.Cos(f), Mathf.Sqrt(1f - num * num) * Mathf.Sin(f), num);
			return Quaternion.FromToRotation(Vector3.forward, center) * point;
		}
	}
}
