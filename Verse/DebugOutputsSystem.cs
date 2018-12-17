using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace Verse
{
	[HasDebugOutput]
	public static class DebugOutputsSystem
	{
		[DebugOutput]
		[Category("System")]
		public static void LoadedAssets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Object[] array = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			stringBuilder.AppendLine("Meshes: " + array.Length + " (" + TotalBytes(array).ToStringBytes() + ")");
			Object[] array2 = Resources.FindObjectsOfTypeAll(typeof(Material));
			stringBuilder.AppendLine("Materials: " + array2.Length + " (" + TotalBytes(array2).ToStringBytes() + ")");
			stringBuilder.AppendLine("   Damaged: " + DamagedMatPool.MatCount);
			stringBuilder.AppendLine("   Faded: " + FadedMaterialPool.TotalMaterialCount + " (" + FadedMaterialPool.TotalMaterialBytes.ToStringBytes() + ")");
			stringBuilder.AppendLine("   SolidColorsSimple: " + SolidColorMaterials.SimpleColorMatCount);
			Object[] array3 = Resources.FindObjectsOfTypeAll(typeof(Texture));
			stringBuilder.AppendLine("Textures: " + array3.Length + " (" + TotalBytes(array3).ToStringBytes() + ")");
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Texture list:");
			Object[] array4 = array3;
			foreach (Object @object in array4)
			{
				string text = ((Texture)@object).name;
				if (text.NullOrEmpty())
				{
					text = "-";
				}
				stringBuilder.AppendLine(text);
			}
			Log.Message(stringBuilder.ToString());
		}

		private static long TotalBytes(Object[] arr)
		{
			long num = 0L;
			foreach (Object o in arr)
			{
				num += Profiler.GetRuntimeMemorySizeLong(o);
			}
			return num;
		}

		[DebugOutput]
		[ModeRestrictionPlay]
		[Category("System")]
		public static void DynamicDrawThingsList()
		{
			Find.CurrentMap.dynamicDrawManager.LogDynamicDrawThings();
		}

		[DebugOutput]
		[Category("System")]
		public static void RandByCurveTests()
		{
			DebugHistogram debugHistogram = new DebugHistogram((from x in Enumerable.Range(0, 30)
			select (float)x).ToArray());
			SimpleCurve simpleCurve = new SimpleCurve();
			simpleCurve.Add(new CurvePoint(0f, 0f));
			simpleCurve.Add(new CurvePoint(10f, 1f));
			simpleCurve.Add(new CurvePoint(15f, 2f));
			simpleCurve.Add(new CurvePoint(20f, 2f));
			simpleCurve.Add(new CurvePoint(21f, 0.5f));
			simpleCurve.Add(new CurvePoint(30f, 0f));
			SimpleCurve curve = simpleCurve;
			float num = 0f;
			for (int i = 0; i < 1000000; i++)
			{
				float num2 = Rand.ByCurve(curve);
				num += num2;
				debugHistogram.Add(num2);
			}
			debugHistogram.Display();
			Log.Message($"Average {num / 1000000f}, calculated as {Rand.ByCurveAverage(curve)}");
		}
	}
}
