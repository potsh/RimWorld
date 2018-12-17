using RimWorld;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Verse
{
	public class ReachabilityCache
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		private struct CachedEntry : IEquatable<CachedEntry>
		{
			public int FirstRoomID
			{
				get;
				private set;
			}

			public int SecondRoomID
			{
				get;
				private set;
			}

			public TraverseParms TraverseParms
			{
				get;
				private set;
			}

			public CachedEntry(int firstRoomID, int secondRoomID, TraverseParms traverseParms)
			{
				this = default(CachedEntry);
				if (firstRoomID < secondRoomID)
				{
					FirstRoomID = firstRoomID;
					SecondRoomID = secondRoomID;
				}
				else
				{
					FirstRoomID = secondRoomID;
					SecondRoomID = firstRoomID;
				}
				TraverseParms = traverseParms;
			}

			public static bool operator ==(CachedEntry lhs, CachedEntry rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(CachedEntry lhs, CachedEntry rhs)
			{
				return !lhs.Equals(rhs);
			}

			public override bool Equals(object obj)
			{
				if (!(obj is CachedEntry))
				{
					return false;
				}
				return Equals((CachedEntry)obj);
			}

			public bool Equals(CachedEntry other)
			{
				return FirstRoomID == other.FirstRoomID && SecondRoomID == other.SecondRoomID && TraverseParms == other.TraverseParms;
			}

			public override int GetHashCode()
			{
				int seed = Gen.HashCombineInt(FirstRoomID, SecondRoomID);
				return Gen.HashCombineStruct(seed, TraverseParms);
			}
		}

		private Dictionary<CachedEntry, bool> cacheDict = new Dictionary<CachedEntry, bool>();

		private static List<CachedEntry> tmpCachedEntries = new List<CachedEntry>();

		public int Count => cacheDict.Count;

		public BoolUnknown CachedResultFor(Room A, Room B, TraverseParms traverseParams)
		{
			if (cacheDict.TryGetValue(new CachedEntry(A.ID, B.ID, traverseParams), out bool value))
			{
				return (!value) ? BoolUnknown.False : BoolUnknown.True;
			}
			return BoolUnknown.Unknown;
		}

		public void AddCachedResult(Room A, Room B, TraverseParms traverseParams, bool reachable)
		{
			CachedEntry key = new CachedEntry(A.ID, B.ID, traverseParams);
			if (!cacheDict.ContainsKey(key))
			{
				cacheDict.Add(key, reachable);
			}
		}

		public void Clear()
		{
			cacheDict.Clear();
		}

		public void ClearFor(Pawn p)
		{
			tmpCachedEntries.Clear();
			foreach (KeyValuePair<CachedEntry, bool> item in cacheDict)
			{
				TraverseParms traverseParms = item.Key.TraverseParms;
				if (traverseParms.pawn == p)
				{
					tmpCachedEntries.Add(item.Key);
				}
			}
			for (int i = 0; i < tmpCachedEntries.Count; i++)
			{
				cacheDict.Remove(tmpCachedEntries[i]);
			}
			tmpCachedEntries.Clear();
		}

		public void ClearForHostile(Thing hostileTo)
		{
			tmpCachedEntries.Clear();
			foreach (KeyValuePair<CachedEntry, bool> item in cacheDict)
			{
				TraverseParms traverseParms = item.Key.TraverseParms;
				Pawn pawn = traverseParms.pawn;
				if (pawn != null && pawn.HostileTo(hostileTo))
				{
					tmpCachedEntries.Add(item.Key);
				}
			}
			for (int i = 0; i < tmpCachedEntries.Count; i++)
			{
				cacheDict.Remove(tmpCachedEntries[i]);
			}
			tmpCachedEntries.Clear();
		}
	}
}
