using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class DefDatabase<T> where T : Def, new()
	{
		private static List<T> defsList = new List<T>();

		private static Dictionary<string, T> defsByName = new Dictionary<string, T>();

		public static IEnumerable<T> AllDefs => defsList;

		public static List<T> AllDefsListForReading => defsList;

		public static int DefCount => defsList.Count;

		public static void AddAllInMods()
		{
			HashSet<string> hashSet = new HashSet<string>();
			foreach (ModContentPack item in (from m in LoadedModManager.RunningMods
			orderby m.OverwritePriority
			select m).ThenBy((ModContentPack x) => LoadedModManager.RunningModsListForReading.IndexOf(x)))
			{
				hashSet.Clear();
				foreach (T item2 in GenDefDatabase.DefsToGoInDatabase<T>(item))
				{
					if (hashSet.Contains(item2.defName))
					{
						Log.Error("Mod " + item + " has multiple " + typeof(T) + "s named " + item2.defName + ". Skipping.");
					}
					else
					{
						if (item2.defName == "UnnamedDef")
						{
							string text = "Unnamed" + typeof(T).Name + Rand.Range(1, 100000).ToString() + "A";
							Log.Error(typeof(T).Name + " in " + item.ToString() + " with label " + item2.label + " lacks a defName. Giving name " + text);
							item2.defName = text;
						}
						if (defsByName.TryGetValue(item2.defName, out T value))
						{
							Remove(value);
						}
						Add(item2);
					}
				}
			}
		}

		public static void Add(IEnumerable<T> defs)
		{
			foreach (T def in defs)
			{
				Add(def);
			}
		}

		public static void Add(T def)
		{
			while (defsByName.ContainsKey(def.defName))
			{
				Log.Error("Adding duplicate " + typeof(T) + " name: " + def.defName);
				def.defName += Mathf.RoundToInt(Rand.Value * 1000f);
			}
			defsList.Add(def);
			defsByName.Add(def.defName, def);
			if (defsList.Count > 65535)
			{
				Log.Error("Too many " + typeof(T) + "; over " + (ushort)ushort.MaxValue);
			}
			def.index = (ushort)(defsList.Count - 1);
		}

		private static void Remove(T def)
		{
			defsByName.Remove(def.defName);
			defsList.Remove(def);
			SetIndices();
		}

		public static void Clear()
		{
			defsList.Clear();
			defsByName.Clear();
		}

		public static void ClearCachedData()
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				T val = defsList[i];
				val.ClearCachedData();
			}
		}

		public static void ResolveAllReferences(bool onlyExactlyMyType = true)
		{
			SetIndices();
			for (int i = 0; i < defsList.Count; i++)
			{
				try
				{
					if (!onlyExactlyMyType)
					{
						goto IL_003f;
					}
					T val = defsList[i];
					if (val.GetType() == typeof(T))
					{
						goto IL_003f;
					}
					goto end_IL_000c;
					IL_003f:
					T val2 = defsList[i];
					val2.ResolveReferences();
					end_IL_000c:;
				}
				catch (Exception ex)
				{
					Log.Error("Error while resolving references for def " + defsList[i] + ": " + ex);
				}
			}
			SetIndices();
		}

		private static void SetIndices()
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				defsList[i].index = (ushort)i;
			}
		}

		public static void ErrorCheckAllDefs()
		{
			foreach (T allDef in AllDefs)
			{
				T current = allDef;
				try
				{
					if (!current.ignoreConfigErrors)
					{
						foreach (string item in current.ConfigErrors())
						{
							Log.Error("Config error in " + current + ": " + item);
						}
					}
				}
				catch (Exception ex)
				{
					Log.Error("Exception in ConfigErrors() of " + current.defName + ": " + ex);
				}
			}
		}

		public static T GetNamed(string defName, bool errorOnFail = true)
		{
			if (errorOnFail)
			{
				if (defsByName.TryGetValue(defName, out T value))
				{
					return value;
				}
				Log.Error("Failed to find " + typeof(T) + " named " + defName + ". There are " + defsList.Count + " defs of this type loaded.");
				return (T)null;
			}
			if (defsByName.TryGetValue(defName, out T value2))
			{
				return value2;
			}
			return (T)null;
		}

		public static T GetNamedSilentFail(string defName)
		{
			return GetNamed(defName, errorOnFail: false);
		}

		public static T GetByShortHash(ushort shortHash)
		{
			for (int i = 0; i < defsList.Count; i++)
			{
				if (defsList[i].shortHash == shortHash)
				{
					return defsList[i];
				}
			}
			return (T)null;
		}

		public static T GetRandom()
		{
			return defsList.RandomElement();
		}
	}
}
