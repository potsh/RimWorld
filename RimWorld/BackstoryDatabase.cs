using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace RimWorld
{
	public static class BackstoryDatabase
	{
		public static Dictionary<string, Backstory> allBackstories = new Dictionary<string, Backstory>();

		private static Dictionary<Pair<BackstorySlot, string>, List<Backstory>> shuffleableBackstoryList = new Dictionary<Pair<BackstorySlot, string>, List<Backstory>>();

		private static HashSet<Backstory> tmpUniqueBackstories = new HashSet<Backstory>();

		private static Regex regex = new Regex("^[^0-9]*");

		public static void Clear()
		{
			allBackstories.Clear();
		}

		public static void ReloadAllBackstories()
		{
			foreach (Backstory item in DirectXmlLoader.LoadXmlDataInResourcesFolder<Backstory>("Backstories/Shuffled"))
			{
				item.PostLoad();
				item.ResolveReferences();
				foreach (string item2 in item.ConfigErrors(ignoreNoSpawnCategories: false))
				{
					Log.Error(item.title + ": " + item2);
				}
				AddBackstory(item);
			}
			SolidBioDatabase.LoadAllBios();
		}

		public static void AddBackstory(Backstory bs)
		{
			BackstoryHardcodedData.InjectHardcodedData(bs);
			if (allBackstories.ContainsKey(bs.identifier))
			{
				if (bs == allBackstories[bs.identifier])
				{
					Log.Error("Tried to add the same backstory twice " + bs.identifier);
				}
				else
				{
					Log.Error("Backstory " + bs.title + " has same unique save key " + bs.identifier + " as old backstory " + allBackstories[bs.identifier].title);
				}
			}
			else
			{
				allBackstories.Add(bs.identifier, bs);
				shuffleableBackstoryList.Clear();
			}
		}

		public static bool TryGetWithIdentifier(string identifier, out Backstory bs, bool closestMatchWarning = true)
		{
			identifier = GetIdentifierClosestMatch(identifier, closestMatchWarning);
			return allBackstories.TryGetValue(identifier, out bs);
		}

		public static string GetIdentifierClosestMatch(string identifier, bool closestMatchWarning = true)
		{
			if (allBackstories.ContainsKey(identifier))
			{
				return identifier;
			}
			string b = StripNumericSuffix(identifier);
			foreach (KeyValuePair<string, Backstory> allBackstory in allBackstories)
			{
				Backstory value = allBackstory.Value;
				if (StripNumericSuffix(value.identifier) == b)
				{
					if (closestMatchWarning)
					{
						Log.Warning("Couldn't find exact match for backstory " + identifier + ", using closest match " + value.identifier);
					}
					return value.identifier;
				}
			}
			Log.Warning("Couldn't find exact match for backstory " + identifier + ", or any close match.");
			return identifier;
		}

		public static Backstory RandomBackstory(BackstorySlot slot)
		{
			return (from bs in allBackstories
			where bs.Value.slot == slot
			select bs).RandomElement().Value;
		}

		public static List<Backstory> ShuffleableBackstoryList(BackstorySlot slot, string tag)
		{
			Pair<BackstorySlot, string> key = new Pair<BackstorySlot, string>(slot, tag);
			if (!shuffleableBackstoryList.ContainsKey(key))
			{
				shuffleableBackstoryList[key] = (from bs in allBackstories.Values
				where bs.shuffleable && bs.slot == slot && bs.spawnCategories.Contains(tag)
				select bs).ToList();
			}
			return shuffleableBackstoryList[key];
		}

		public static void ShuffleableBackstoryList(BackstorySlot slot, List<string> tags, List<Backstory> outBackstories)
		{
			outBackstories.Clear();
			if (tags.Count != 0)
			{
				if (tags.Count == 1)
				{
					outBackstories.AddRange(ShuffleableBackstoryList(slot, tags[0]));
				}
				else
				{
					tmpUniqueBackstories.Clear();
					for (int i = 0; i < tags.Count; i++)
					{
						List<Backstory> list = ShuffleableBackstoryList(slot, tags[i]);
						for (int j = 0; j < list.Count; j++)
						{
							tmpUniqueBackstories.Add(list[j]);
						}
					}
					foreach (Backstory tmpUniqueBackstory in tmpUniqueBackstories)
					{
						outBackstories.Add(tmpUniqueBackstory);
					}
					tmpUniqueBackstories.Clear();
				}
			}
		}

		public static string StripNumericSuffix(string key)
		{
			return regex.Match(key).Captures[0].Value;
		}
	}
}
