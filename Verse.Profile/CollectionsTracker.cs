using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Profile
{
	[HasDebugOutput]
	public static class CollectionsTracker
	{
		private static Dictionary<WeakReference, int> collections = new Dictionary<WeakReference, int>();

		[DebugOutput]
		private static void GrownCollectionsStart()
		{
			if (!MemoryTracker.AnythingTracked)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
			}
			else
			{
				collections.Clear();
				foreach (WeakReference foundCollection in MemoryTracker.FoundCollections)
				{
					if (foundCollection.IsAlive)
					{
						ICollection collection = foundCollection.Target as ICollection;
						collections[foundCollection] = collection.Count;
					}
				}
				Log.Message("Tracking " + collections.Count + " collections.");
			}
		}

		[DebugOutput]
		private static void GrownCollectionsLog()
		{
			if (!MemoryTracker.AnythingTracked)
			{
				Log.Message("No objects tracked, memory tracker markup may not be applied.");
			}
			else
			{
				collections.RemoveAll((KeyValuePair<WeakReference, int> kvp) => !kvp.Key.IsAlive || ((ICollection)kvp.Key.Target).Count <= kvp.Value);
				MemoryTracker.LogObjectHoldPathsFor(from kvp in collections
				select kvp.Key, delegate(WeakReference elem)
				{
					ICollection collection = elem.Target as ICollection;
					return collection.Count - collections[elem];
				});
				collections.Clear();
			}
		}
	}
}
