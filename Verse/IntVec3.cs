using System;
using UnityEngine;

namespace Verse
{
	public struct IntVec3 : IEquatable<IntVec3>
	{
		public int x;

		public int y;

		public int z;

		public IntVec2 ToIntVec2 => new IntVec2(x, z);

		public bool IsValid => y >= 0;

		public int LengthHorizontalSquared => x * x + z * z;

		public float LengthHorizontal => GenMath.Sqrt((float)(x * x + z * z));

		public int LengthManhattan => ((x < 0) ? (-x) : x) + ((z < 0) ? (-z) : z);

		public float AngleFlat
		{
			get
			{
				if (x == 0 && z == 0)
				{
					return 0f;
				}
				Vector3 eulerAngles = Quaternion.LookRotation(ToVector3()).eulerAngles;
				return eulerAngles.y;
			}
		}

		public static IntVec3 Zero => new IntVec3(0, 0, 0);

		public static IntVec3 North => new IntVec3(0, 0, 1);

		public static IntVec3 East => new IntVec3(1, 0, 0);

		public static IntVec3 South => new IntVec3(0, 0, -1);

		public static IntVec3 West => new IntVec3(-1, 0, 0);

		public static IntVec3 NorthWest => new IntVec3(-1, 0, 1);

		public static IntVec3 NorthEast => new IntVec3(1, 0, 1);

		public static IntVec3 SouthWest => new IntVec3(-1, 0, -1);

		public static IntVec3 SouthEast => new IntVec3(1, 0, -1);

		public static IntVec3 Invalid => new IntVec3(-1000, -1000, -1000);

		public IntVec3(int newX, int newY, int newZ)
		{
			x = newX;
			y = newY;
			z = newZ;
		}

		public IntVec3(Vector3 v)
		{
			x = (int)v.x;
			y = 0;
			z = (int)v.z;
		}

		public IntVec3(Vector2 v)
		{
			x = (int)v.x;
			y = 0;
			z = (int)v.y;
		}

		public static IntVec3 FromString(string str)
		{
			str = str.TrimStart('(');
			str = str.TrimEnd(')');
			string[] array = str.Split(',');
			try
			{
				int newX = Convert.ToInt32(array[0]);
				int newY = Convert.ToInt32(array[1]);
				int newZ = Convert.ToInt32(array[2]);
				return new IntVec3(newX, newY, newZ);
			}
			catch (Exception arg)
			{
				Log.Warning(str + " is not a valid IntVec3 format. Exception: " + arg);
				return Invalid;
			}
		}

		public Vector3 ToVector3()
		{
			return new Vector3((float)x, (float)y, (float)z);
		}

		public Vector3 ToVector3Shifted()
		{
			return new Vector3((float)x + 0.5f, (float)y, (float)z + 0.5f);
		}

		public Vector3 ToVector3ShiftedWithAltitude(AltitudeLayer AltLayer)
		{
			return ToVector3ShiftedWithAltitude(AltLayer.AltitudeFor());
		}

		public Vector3 ToVector3ShiftedWithAltitude(float AddedAltitude)
		{
			return new Vector3((float)x + 0.5f, (float)y + AddedAltitude, (float)z + 0.5f);
		}

		public bool InHorDistOf(IntVec3 otherLoc, float maxDist)
		{
			float num = (float)(x - otherLoc.x);
			float num2 = (float)(z - otherLoc.z);
			return num * num + num2 * num2 <= maxDist * maxDist;
		}

		public static IntVec3 FromVector3(Vector3 v)
		{
			return FromVector3(v, 0);
		}

		public static IntVec3 FromVector3(Vector3 v, int newY)
		{
			return new IntVec3((int)v.x, newY, (int)v.z);
		}

		public Vector2 ToUIPosition()
		{
			return ToVector3Shifted().MapToUIPosition();
		}

		public bool AdjacentToCardinal(IntVec3 other)
		{
			if (!IsValid)
			{
				return false;
			}
			if (other.z == z && (other.x == x + 1 || other.x == x - 1))
			{
				return true;
			}
			if (other.x == x && (other.z == z + 1 || other.z == z - 1))
			{
				return true;
			}
			return false;
		}

		public bool AdjacentToDiagonal(IntVec3 other)
		{
			if (!IsValid)
			{
				return false;
			}
			return Mathf.Abs(x - other.x) == 1 && Mathf.Abs(z - other.z) == 1;
		}

		public bool AdjacentToCardinal(Room room)
		{
			if (!IsValid)
			{
				return false;
			}
			Map map = room.Map;
			if (this.InBounds(map) && this.GetRoom(map, RegionType.Set_All) == room)
			{
				return true;
			}
			IntVec3[] cardinalDirections = GenAdj.CardinalDirections;
			for (int i = 0; i < cardinalDirections.Length; i++)
			{
				IntVec3 intVec = this + cardinalDirections[i];
				if (intVec.InBounds(map) && intVec.GetRoom(map, RegionType.Set_All) == room)
				{
					return true;
				}
			}
			return false;
		}

		public IntVec3 ClampInsideMap(Map map)
		{
			return ClampInsideRect(CellRect.WholeMap(map));
		}

		public IntVec3 ClampInsideRect(CellRect rect)
		{
			x = Mathf.Clamp(x, rect.minX, rect.maxX);
			y = 0;
			z = Mathf.Clamp(z, rect.minZ, rect.maxZ);
			return this;
		}

		public static IntVec3 operator +(IntVec3 a, IntVec3 b)
		{
			return new IntVec3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static IntVec3 operator -(IntVec3 a, IntVec3 b)
		{
			return new IntVec3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static IntVec3 operator *(IntVec3 a, int i)
		{
			return new IntVec3(a.x * i, a.y * i, a.z * i);
		}

		public static bool operator ==(IntVec3 a, IntVec3 b)
		{
			if (a.x == b.x && a.z == b.z && a.y == b.y)
			{
				return true;
			}
			return false;
		}

		public static bool operator !=(IntVec3 a, IntVec3 b)
		{
			if (a.x != b.x || a.z != b.z || a.y != b.y)
			{
				return true;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			return obj is IntVec3 && Equals((IntVec3)obj);
		}

		public bool Equals(IntVec3 other)
		{
			return x == other.x && z == other.z && y == other.y;
		}

		public override int GetHashCode()
		{
			int seed = 0;
			seed = Gen.HashCombineInt(seed, x);
			seed = Gen.HashCombineInt(seed, y);
			return Gen.HashCombineInt(seed, z);
		}

		public ulong UniqueHashCode()
		{
			ulong num = 0uL;
			num = (ulong)((long)num + (long)x);
			num = (ulong)((long)num + 4096L * (long)z);
			return (ulong)((long)num + 16777216L * (long)y);
		}

		public override string ToString()
		{
			return "(" + x.ToString() + ", " + y.ToString() + ", " + z.ToString() + ")";
		}
	}
}
