using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Verse
{
	[HasDebugOutput]
	internal static class MaterialAllocator
	{
		private struct MaterialInfo
		{
			public string stackTrace;
		}

		private static Dictionary<Material, MaterialInfo> references = new Dictionary<Material, MaterialInfo>();

		public static int nextWarningThreshold;

		private static Dictionary<string, int> snapshot = new Dictionary<string, int>();

		[CompilerGenerated]
		private static Func<IGrouping<string, KeyValuePair<Material, MaterialInfo>>, int> _003C_003Ef__mg_0024cache0;

		public static Material Create(Material material)
		{
			Material material2 = new Material(material);
			references[material2] = new MaterialInfo
			{
				stackTrace = ((!Prefs.DevMode) ? "(unavailable)" : Environment.StackTrace)
			};
			TryReport();
			return material2;
		}

		public static Material Create(Shader shader)
		{
			Material material = new Material(shader);
			references[material] = new MaterialInfo
			{
				stackTrace = ((!Prefs.DevMode) ? "(unavailable)" : Environment.StackTrace)
			};
			TryReport();
			return material;
		}

		public static void Destroy(Material material)
		{
			if (!references.ContainsKey(material))
			{
				Log.Error($"Destroying material {material}, but that material was not created through the MaterialTracker");
			}
			references.Remove(material);
			UnityEngine.Object.Destroy(material);
		}

		public static void TryReport()
		{
			if (MaterialWarningThreshold() > nextWarningThreshold)
			{
				nextWarningThreshold = MaterialWarningThreshold();
			}
			if (references.Count > nextWarningThreshold)
			{
				Log.Error($"Material allocator has allocated {references.Count} materials; this may be a sign of a material leak");
				if (Prefs.DevMode)
				{
					MaterialReport();
				}
				nextWarningThreshold *= 2;
			}
		}

		public static int MaterialWarningThreshold()
		{
			return 2147483647;
		}

		[DebugOutput]
		[Category("System")]
		public static void MaterialReport()
		{
			foreach (string item in references.GroupBy(delegate(KeyValuePair<Material, MaterialInfo> kvp)
			{
				MaterialInfo value2 = kvp.Value;
				return value2.stackTrace;
			}).OrderByDescending(Enumerable.Count<KeyValuePair<Material, MaterialInfo>>).Select(delegate(IGrouping<string, KeyValuePair<Material, MaterialInfo>> g)
			{
				object arg = g.Count();
				MaterialInfo value = g.FirstOrDefault().Value;
				return $"{arg}: {value.stackTrace}";
			})
				.Take(20))
			{
				Log.Error(item);
			}
		}

		[DebugOutput]
		[Category("System")]
		public static void MaterialSnapshot()
		{
			snapshot = new Dictionary<string, int>();
			foreach (IGrouping<string, KeyValuePair<Material, MaterialInfo>> item in references.GroupBy(delegate(KeyValuePair<Material, MaterialInfo> kvp)
			{
				MaterialInfo value = kvp.Value;
				return value.stackTrace;
			}))
			{
				snapshot[item.Key] = item.Count();
			}
		}

		[DebugOutput]
		[Category("System")]
		public static void MaterialDelta()
		{
			IEnumerable<string> source = (from v in references.Values
			select v.stackTrace).Concat(snapshot.Keys).Distinct();
			Dictionary<string, int> currentSnapshot = new Dictionary<string, int>();
			foreach (IGrouping<string, KeyValuePair<Material, MaterialInfo>> item in references.GroupBy(delegate(KeyValuePair<Material, MaterialInfo> kvp)
			{
				MaterialInfo value = kvp.Value;
				return value.stackTrace;
			}))
			{
				currentSnapshot[item.Key] = item.Count();
			}
			IEnumerable<KeyValuePair<string, int>> source2 = from k in source
			select new KeyValuePair<string, int>(k, currentSnapshot.TryGetValue(k, 0) - snapshot.TryGetValue(k, 0));
			foreach (string item2 in (from kvp in source2
			orderby kvp.Value descending
			select kvp into g
			select $"{g.Value}: {g.Key}").Take(20))
			{
				Log.Error(item2);
			}
		}
	}
}
