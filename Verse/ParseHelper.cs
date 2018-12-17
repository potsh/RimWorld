using RimWorld;
using Steamworks;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public static class ParseHelper
	{
		public static object FromString(string str, Type itemType)
		{
			try
			{
				itemType = (Nullable.GetUnderlyingType(itemType) ?? itemType);
				if (itemType == typeof(string))
				{
					str = str.Replace("\\n", "\n");
					return str;
				}
				if (itemType == typeof(int))
				{
					return ParseIntPermissive(str);
				}
				if (itemType == typeof(float))
				{
					return float.Parse(str, CultureInfo.InvariantCulture);
				}
				if (itemType == typeof(bool))
				{
					return bool.Parse(str);
				}
				if (itemType == typeof(long))
				{
					return long.Parse(str, CultureInfo.InvariantCulture);
				}
				if (itemType == typeof(double))
				{
					return double.Parse(str, CultureInfo.InvariantCulture);
				}
				if (itemType == typeof(sbyte))
				{
					return sbyte.Parse(str, CultureInfo.InvariantCulture);
				}
				if (itemType.IsEnum)
				{
					try
					{
						object obj = BackCompatibility.BackCompatibleEnum(itemType, str);
						if (obj != null)
						{
							return obj;
						}
						return Enum.Parse(itemType, str);
					}
					catch (ArgumentException innerException)
					{
						string str2 = "'" + str + "' is not a valid value for " + itemType + ". Valid values are: \n";
						str2 += GenText.StringFromEnumerable(Enum.GetValues(itemType));
						ArgumentException ex = new ArgumentException(str2, innerException);
						throw ex;
					}
				}
				if (itemType == typeof(Type))
				{
					if (str == "null" || str == "Null")
					{
						return null;
					}
					Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(str);
					if (typeInAnyAssembly == null)
					{
						Log.Error("Could not find a type named " + str);
					}
					return typeInAnyAssembly;
				}
				if (itemType == typeof(Action))
				{
					string[] array = str.Split('.');
					string methodName = array[array.Length - 1];
					string empty = string.Empty;
					empty = ((array.Length != 3) ? array[0] : (array[0] + "." + array[1]));
					Type typeInAnyAssembly2 = GenTypes.GetTypeInAnyAssembly(empty);
					MethodInfo method = typeInAnyAssembly2.GetMethods().First((MethodInfo m) => m.Name == methodName);
					return (Action)Delegate.CreateDelegate(typeof(Action), method);
				}
				if (itemType == typeof(Vector3))
				{
					return FromStringVector3(str);
				}
				if (itemType == typeof(Vector2))
				{
					return FromStringVector2(str);
				}
				if (itemType == typeof(Rect))
				{
					return FromStringRect(str);
				}
				if (itemType == typeof(Color))
				{
					str = str.TrimStart('(', 'R', 'G', 'B', 'A');
					str = str.TrimEnd(')');
					string[] array2 = str.Split(',');
					float num = (float)FromString(array2[0], typeof(float));
					float num2 = (float)FromString(array2[1], typeof(float));
					float num3 = (float)FromString(array2[2], typeof(float));
					bool flag = num > 1f || num3 > 1f || num2 > 1f;
					float num4 = (float)((!flag) ? 1 : 255);
					if (array2.Length == 4)
					{
						num4 = (float)FromString(array2[3], typeof(float));
					}
					Color color = default(Color);
					if (!flag)
					{
						color.r = num;
						color.g = num2;
						color.b = num3;
						color.a = num4;
					}
					else
					{
						color = GenColor.FromBytes(Mathf.RoundToInt(num), Mathf.RoundToInt(num2), Mathf.RoundToInt(num3), Mathf.RoundToInt(num4));
					}
					return color;
				}
				if (itemType == typeof(PublishedFileId_t))
				{
					return new PublishedFileId_t(ulong.Parse(str));
				}
				if (itemType == typeof(IntVec2))
				{
					return IntVec2.FromString(str);
				}
				if (itemType == typeof(IntVec3))
				{
					return IntVec3.FromString(str);
				}
				if (itemType == typeof(Rot4))
				{
					return Rot4.FromString(str);
				}
				if (itemType == typeof(CellRect))
				{
					return CellRect.FromString(str);
				}
				if (itemType != typeof(CurvePoint))
				{
					if (itemType == typeof(NameTriple))
					{
						NameTriple nameTriple = NameTriple.FromString(str);
						nameTriple.ResolveMissingPieces();
					}
					else
					{
						if (itemType == typeof(FloatRange))
						{
							return FloatRange.FromString(str);
						}
						if (itemType == typeof(IntRange))
						{
							return IntRange.FromString(str);
						}
						if (itemType == typeof(QualityRange))
						{
							return QualityRange.FromString(str);
						}
						if (itemType == typeof(ColorInt))
						{
							str = str.TrimStart('(', 'R', 'G', 'B', 'A');
							str = str.TrimEnd(')');
							string[] array3 = str.Split(',');
							ColorInt colorInt = new ColorInt(255, 255, 255, 255);
							colorInt.r = (int)FromString(array3[0], typeof(int));
							colorInt.g = (int)FromString(array3[1], typeof(int));
							colorInt.b = (int)FromString(array3[2], typeof(int));
							if (array3.Length == 4)
							{
								colorInt.a = (int)FromString(array3[3], typeof(int));
							}
							else
							{
								colorInt.a = 255;
							}
							return colorInt;
						}
					}
					throw new ArgumentException("Trying to parse to unknown data type " + itemType.Name + ". Content is '" + str + "'.");
				}
				return CurvePoint.FromString(str);
			}
			catch (Exception innerException2)
			{
				ArgumentException ex2 = new ArgumentException("Exception parsing " + itemType + " from \"" + str + "\"", innerException2);
				throw ex2;
			}
		}

		public static bool HandlesType(Type type)
		{
			return type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(IntVec3) || type == typeof(IntVec2) || type == typeof(Type) || type == typeof(Action) || type == typeof(Vector3) || type == typeof(Vector2) || type == typeof(Rect) || type == typeof(Color) || type == typeof(PublishedFileId_t) || type == typeof(Rot4) || type == typeof(CellRect) || type == typeof(CurvePoint) || type == typeof(NameTriple) || type == typeof(FloatRange) || type == typeof(IntRange) || type == typeof(QualityRange) || type == typeof(ColorInt);
		}

		private static int ParseIntPermissive(string str)
		{
			if (!int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out int result))
			{
				result = (int)float.Parse(str, CultureInfo.InvariantCulture);
				Log.Warning("Parsed " + str + " as int.");
			}
			return result;
		}

		private static Vector3 FromStringVector3(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float z = Convert.ToSingle(array[2]);
			return new Vector3(x, y, z);
		}

		private static Vector2 FromStringVector2(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			float x;
			float y;
			if (array.Length == 1)
			{
				x = (y = Convert.ToSingle(array[0]));
			}
			else
			{
				if (array.Length != 2)
				{
					throw new InvalidOperationException();
				}
				x = Convert.ToSingle(array[0]);
				y = Convert.ToSingle(array[1]);
			}
			return new Vector2(x, y);
		}

		public static Vector4 FromStringVector4Adaptive(string Str)
		{
			Str = Str.TrimStart('(');
			Str = Str.TrimEnd(')');
			string[] array = Str.Split(',');
			float x = 0f;
			float y = 0f;
			float z = 0f;
			float w = 0f;
			if (array.Length >= 1)
			{
				x = Convert.ToSingle(array[0]);
			}
			if (array.Length >= 2)
			{
				y = Convert.ToSingle(array[1]);
			}
			if (array.Length >= 3)
			{
				z = Convert.ToSingle(array[2]);
			}
			if (array.Length >= 4)
			{
				w = Convert.ToSingle(array[3]);
			}
			if (array.Length >= 5)
			{
				Log.ErrorOnce($"Too many elements in vector {Str}", 16139142);
			}
			return new Vector4(x, y, z, w);
		}

		public static Rect FromStringRect(string str)
		{
			str = str.TrimStart('(');
			str = str.TrimEnd(')');
			string[] array = str.Split(',');
			float x = Convert.ToSingle(array[0]);
			float y = Convert.ToSingle(array[1]);
			float width = Convert.ToSingle(array[2]);
			float height = Convert.ToSingle(array[3]);
			return new Rect(x, y, width, height);
		}
	}
}
