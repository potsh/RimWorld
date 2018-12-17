using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Verse
{
	public class EditWindow_TweakValues : EditWindow
	{
		private struct TweakInfo
		{
			public FieldInfo field;

			public TweakValue tweakValue;

			public float initial;
		}

		[TweakValue("TweakValue", 0f, 300f)]
		public static float CategoryWidth = 180f;

		[TweakValue("TweakValue", 0f, 300f)]
		public static float TitleWidth = 300f;

		[TweakValue("TweakValue", 0f, 300f)]
		public static float NumberWidth = 140f;

		private Vector2 scrollPosition;

		private static List<TweakInfo> tweakValueFields;

		public override Vector2 InitialSize => new Vector2(1000f, 600f);

		public override bool IsDebug => true;

		public EditWindow_TweakValues()
		{
			optionalTitle = "TweakValues";
			if (tweakValueFields == null)
			{
				tweakValueFields = (from ti in FindAllTweakables().Select(delegate(FieldInfo field)
				{
					TweakInfo result = default(TweakInfo);
					result.field = field;
					result.tweakValue = field.TryGetAttribute<TweakValue>();
					result.initial = GetAsFloat(field);
					return result;
				})
				orderby $"{ti.tweakValue.category}.{ti.field.DeclaringType.Name}"
				select ti).ToList();
			}
		}

		private IEnumerable<FieldInfo> FindAllTweakables()
		{
			foreach (Type allType in GenTypes.AllTypes)
			{
				FieldInfo[] fields = allType.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo field in fields)
				{
					TweakValue tv = field.TryGetAttribute<TweakValue>();
					if (tv != null)
					{
						if (!field.IsStatic)
						{
							Log.Error($"Field {field.DeclaringType.FullName}.{field.Name} is marked with TweakValue, but isn't static; TweakValue won't work");
						}
						else if (field.IsLiteral)
						{
							Log.Error($"Field {field.DeclaringType.FullName}.{field.Name} is marked with TweakValue, but is const; TweakValue won't work");
						}
						else
						{
							if (!field.IsInitOnly)
							{
								yield return field;
								/*Error: Unable to find new state assignment for yield return*/;
							}
							Log.Error($"Field {field.DeclaringType.FullName}.{field.Name} is marked with TweakValue, but is readonly; TweakValue won't work");
						}
					}
				}
			}
			yield break;
			IL_01e2:
			/*Error near IL_01e3: Unexpected return in MoveNext()*/;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			Rect rect = inRect.ContractedBy(4f);
			Rect rect2 = rect;
			rect2.xMax -= 33f;
			Rect rect3 = new Rect(0f, 0f, CategoryWidth, Text.CalcHeight("test", 1000f));
			Rect rect4 = new Rect(rect3.xMax, 0f, TitleWidth, rect3.height);
			Rect rect5 = new Rect(rect4.xMax, 0f, NumberWidth, rect3.height);
			Rect rect6 = new Rect(rect5.xMax, 0f, rect2.width - rect5.xMax, rect3.height);
			Widgets.BeginScrollView(rect, ref scrollPosition, new Rect(0f, 0f, rect2.width, rect3.height * (float)tweakValueFields.Count));
			foreach (TweakInfo tweakValueField in tweakValueFields)
			{
				TweakInfo current = tweakValueField;
				Widgets.Label(rect3, current.tweakValue.category);
				Widgets.Label(rect4, $"{current.field.DeclaringType.Name}.{current.field.Name}");
				float num;
				bool flag;
				if (current.field.FieldType == typeof(float) || current.field.FieldType == typeof(int) || current.field.FieldType == typeof(ushort))
				{
					float asFloat = GetAsFloat(current.field);
					num = Widgets.HorizontalSlider(rect6, GetAsFloat(current.field), current.tweakValue.min, current.tweakValue.max);
					SetFromFloat(current.field, num);
					flag = (asFloat != num);
				}
				else if (current.field.FieldType == typeof(bool))
				{
					bool flag2 = (bool)current.field.GetValue(null);
					bool checkOn = flag2;
					Widgets.Checkbox(rect6.xMin, rect6.yMin, ref checkOn);
					current.field.SetValue(null, checkOn);
					num = (float)(checkOn ? 1 : 0);
					flag = (flag2 != checkOn);
				}
				else
				{
					Log.ErrorOnce($"Attempted to tweakvalue unknown field type {current.field.FieldType}", 83944645);
					flag = false;
					num = current.initial;
				}
				if (num != current.initial)
				{
					GUI.color = Color.red;
					Widgets.Label(rect5, $"{current.initial} -> {num}");
					GUI.color = Color.white;
					if (Widgets.ButtonInvisible(rect5))
					{
						flag = true;
						if (current.field.FieldType == typeof(float) || current.field.FieldType == typeof(int) || current.field.FieldType == typeof(ushort))
						{
							SetFromFloat(current.field, current.initial);
						}
						else if (current.field.FieldType == typeof(bool))
						{
							current.field.SetValue(null, current.initial != 0f);
						}
						else
						{
							Log.ErrorOnce($"Attempted to tweakvalue unknown field type {current.field.FieldType}", 83944646);
						}
					}
				}
				else
				{
					Widgets.Label(rect5, $"{current.initial}");
				}
				if (flag)
				{
					current.field.DeclaringType.GetMethod(current.field.Name + "_Changed", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, null);
				}
				rect3.y += rect3.height;
				rect4.y += rect3.height;
				rect5.y += rect3.height;
				rect6.y += rect3.height;
			}
			Widgets.EndScrollView();
		}

		private float GetAsFloat(FieldInfo field)
		{
			if (field.FieldType == typeof(float))
			{
				return (float)field.GetValue(null);
			}
			if (field.FieldType == typeof(bool))
			{
				return (float)(((bool)field.GetValue(null)) ? 1 : 0);
			}
			if (field.FieldType == typeof(int))
			{
				return (float)(int)field.GetValue(null);
			}
			if (field.FieldType == typeof(ushort))
			{
				return (float)(int)(ushort)field.GetValue(null);
			}
			Log.ErrorOnce($"Attempted to return unknown field type {field.FieldType} as a float", 83944644);
			return 0f;
		}

		private void SetFromFloat(FieldInfo field, float input)
		{
			if (field.FieldType == typeof(float))
			{
				field.SetValue(null, input);
			}
			else if (field.FieldType == typeof(bool))
			{
				field.SetValue(null, input != 0f);
			}
			else if (field.FieldType == typeof(int))
			{
				field.SetValue(field, (int)input);
			}
			else if (field.FieldType == typeof(ushort))
			{
				field.SetValue(field, (ushort)input);
			}
			else
			{
				Log.ErrorOnce($"Attempted to set unknown field type {field.FieldType} from a float", 83944645);
			}
		}
	}
}
