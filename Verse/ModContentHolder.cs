using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class ModContentHolder<T> where T : class
	{
		private ModContentPack mod;

		public Dictionary<string, T> contentList = new Dictionary<string, T>();

		public ModContentHolder(ModContentPack mod)
		{
			this.mod = mod;
		}

		public void ClearDestroy()
		{
			if (typeof(Object).IsAssignableFrom(typeof(T)))
			{
				foreach (T value in contentList.Values)
				{
					T localObj = value;
					LongEventHandler.ExecuteWhenFinished(delegate
					{
						Object.Destroy((Object)localObj);
					});
				}
			}
			contentList.Clear();
		}

		public void ReloadAll()
		{
			foreach (LoadedContentItem<T> item in ModContentLoader<T>.LoadAllForMod(mod))
			{
				if (contentList.ContainsKey(item.internalPath))
				{
					Log.Warning("Tried to load duplicate " + typeof(T) + " with path: " + item.internalPath);
				}
				else
				{
					contentList.Add(item.internalPath, item.contentItem);
				}
			}
		}

		public T Get(string path)
		{
			if (contentList.TryGetValue(path, out T value))
			{
				return value;
			}
			return (T)null;
		}

		public IEnumerable<T> GetAllUnderPath(string pathRoot)
		{
			foreach (KeyValuePair<string, T> content in contentList)
			{
				if (content.Key.StartsWith(pathRoot))
				{
					yield return content.Value;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00d4:
			/*Error near IL_00d5: Unexpected return in MoveNext()*/;
		}
	}
}
