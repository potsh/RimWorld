using System;
using System.Collections.Generic;

namespace Verse
{
	public class LoadedObjectDirectory
	{
		private Dictionary<string, ILoadReferenceable> allObjectsByLoadID = new Dictionary<string, ILoadReferenceable>();

		public void Clear()
		{
			allObjectsByLoadID.Clear();
		}

		public void RegisterLoaded(ILoadReferenceable reffable)
		{
			if (Prefs.DevMode)
			{
				string text = "[excepted]";
				try
				{
					text = reffable.GetUniqueLoadID();
				}
				catch (Exception)
				{
				}
				string text2 = "[excepted]";
				try
				{
					text2 = reffable.ToString();
				}
				catch (Exception)
				{
				}
				if (allObjectsByLoadID.TryGetValue(text, out ILoadReferenceable value))
				{
					Log.Error("Cannot register " + reffable.GetType() + " " + text2 + ", (id=" + text + " in loaded object directory. Id already used by " + value.GetType() + " " + value.ToString() + ".");
					return;
				}
			}
			try
			{
				allObjectsByLoadID.Add(reffable.GetUniqueLoadID(), reffable);
			}
			catch (Exception ex5)
			{
				string text3 = "[excepted]";
				try
				{
					text3 = reffable.GetUniqueLoadID();
				}
				catch (Exception)
				{
				}
				string text4 = "[excepted]";
				try
				{
					text4 = reffable.ToString();
				}
				catch (Exception)
				{
				}
				Log.Error("Exception registering " + reffable.GetType() + " " + text4 + " in loaded object directory with unique load ID " + text3 + ": " + ex5);
			}
		}

		public T ObjectWithLoadID<T>(string loadID)
		{
			if (loadID.NullOrEmpty() || loadID == "null")
			{
				return default(T);
			}
			if (allObjectsByLoadID.TryGetValue(loadID, out ILoadReferenceable value))
			{
				if (value != null)
				{
					try
					{
						return (T)value;
					}
					catch (Exception ex)
					{
						Log.Error("Exception getting object with load id " + loadID + " of type " + typeof(T) + ". What we loaded was " + value.ToStringSafe() + ". Exception:\n" + ex);
						return default(T);
					}
				}
				return default(T);
			}
			Log.Warning("Could not resolve reference to object with loadID " + loadID + " of type " + typeof(T) + ". Was it compressed away, destroyed, had no ID number, or not saved/loaded right? curParent=" + Scribe.loader.curParent.ToStringSafe() + " curPathRelToParent=" + Scribe.loader.curPathRelToParent);
			return default(T);
		}
	}
}
