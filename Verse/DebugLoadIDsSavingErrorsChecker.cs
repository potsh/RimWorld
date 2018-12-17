using System;
using System.Collections.Generic;

namespace Verse
{
	public class DebugLoadIDsSavingErrorsChecker
	{
		private struct ReferencedObject : IEquatable<ReferencedObject>
		{
			public string loadID;

			public string label;

			public ReferencedObject(string loadID, string label)
			{
				this.loadID = loadID;
				this.label = label;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is ReferencedObject))
				{
					return false;
				}
				return Equals((ReferencedObject)obj);
			}

			public bool Equals(ReferencedObject other)
			{
				return loadID == other.loadID && label == other.label;
			}

			public override int GetHashCode()
			{
				int seed = 0;
				seed = Gen.HashCombine(seed, loadID);
				return Gen.HashCombine(seed, label);
			}

			public static bool operator ==(ReferencedObject lhs, ReferencedObject rhs)
			{
				return lhs.Equals(rhs);
			}

			public static bool operator !=(ReferencedObject lhs, ReferencedObject rhs)
			{
				return !(lhs == rhs);
			}
		}

		private HashSet<string> deepSaved = new HashSet<string>();

		private HashSet<ReferencedObject> referenced = new HashSet<ReferencedObject>();

		public void Clear()
		{
			if (Prefs.DevMode)
			{
				deepSaved.Clear();
				referenced.Clear();
			}
		}

		public void CheckForErrorsAndClear()
		{
			if (Prefs.DevMode)
			{
				if (!Scribe.saver.savingForDebug)
				{
					foreach (ReferencedObject item in referenced)
					{
						ReferencedObject current = item;
						if (!deepSaved.Contains(current.loadID))
						{
							Log.Warning("Object with load ID " + current.loadID + " is referenced (xml node name: " + current.label + ") but is not deep-saved. This will cause errors during loading.");
						}
					}
				}
				Clear();
			}
		}

		public void RegisterDeepSaved(object obj, string label)
		{
			if (Prefs.DevMode)
			{
				if (Scribe.mode != LoadSaveMode.Saving)
				{
					Log.Error("Registered " + obj + ", but current mode is " + Scribe.mode);
				}
				else if (obj != null)
				{
					ILoadReferenceable loadReferenceable = obj as ILoadReferenceable;
					if (loadReferenceable != null)
					{
						try
						{
							if (!deepSaved.Add(loadReferenceable.GetUniqueLoadID()))
							{
								Log.Warning("DebugLoadIDsSavingErrorsChecker error: tried to register deep-saved object with loadID " + loadReferenceable.GetUniqueLoadID() + ", but it's already here. label=" + label + " (not cleared after the previous save? different objects have the same load ID? the same object is deep-saved twice?)");
							}
						}
						catch (Exception arg)
						{
							Log.Error("Error in GetUniqueLoadID(): " + arg);
						}
					}
				}
			}
		}

		public void RegisterReferenced(ILoadReferenceable obj, string label)
		{
			if (Prefs.DevMode)
			{
				if (Scribe.mode != LoadSaveMode.Saving)
				{
					Log.Error("Registered " + obj + ", but current mode is " + Scribe.mode);
				}
				else if (obj != null)
				{
					try
					{
						referenced.Add(new ReferencedObject(obj.GetUniqueLoadID(), label));
					}
					catch (Exception arg)
					{
						Log.Error("Error in GetUniqueLoadID(): " + arg);
					}
				}
			}
		}
	}
}
