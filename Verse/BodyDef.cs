using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class BodyDef : Def
	{
		public BodyPartRecord corePart;

		[Unsaved]
		private List<BodyPartRecord> cachedAllParts = new List<BodyPartRecord>();

		[Unsaved]
		private List<BodyPartRecord> cachedPartsVulnerableToFrostbite;

		public List<BodyPartRecord> AllParts => cachedAllParts;

		public List<BodyPartRecord> AllPartsVulnerableToFrostbite => cachedPartsVulnerableToFrostbite;

		public IEnumerable<BodyPartRecord> GetPartsWithTag(BodyPartTagDef tag)
		{
			int i = 0;
			BodyPartRecord part;
			while (true)
			{
				if (i >= AllParts.Count)
				{
					yield break;
				}
				part = AllParts[i];
				if (part.def.tags.Contains(tag))
				{
					break;
				}
				i++;
			}
			yield return part;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public IEnumerable<BodyPartRecord> GetPartsWithDef(BodyPartDef def)
		{
			int i = 0;
			BodyPartRecord part;
			while (true)
			{
				if (i >= AllParts.Count)
				{
					yield break;
				}
				part = AllParts[i];
				if (part.def == def)
				{
					break;
				}
				i++;
			}
			yield return part;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public bool HasPartWithTag(BodyPartTagDef tag)
		{
			for (int i = 0; i < AllParts.Count; i++)
			{
				BodyPartRecord bodyPartRecord = AllParts[i];
				if (bodyPartRecord.def.tags.Contains(tag))
				{
					return true;
				}
			}
			return false;
		}

		public BodyPartRecord GetPartAtIndex(int index)
		{
			if (index < 0 || index >= cachedAllParts.Count)
			{
				return null;
			}
			return cachedAllParts[index];
		}

		public int GetIndexOfPart(BodyPartRecord rec)
		{
			for (int i = 0; i < cachedAllParts.Count; i++)
			{
				if (cachedAllParts[i] == rec)
				{
					return i;
				}
			}
			return -1;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (cachedPartsVulnerableToFrostbite.NullOrEmpty())
			{
				yield return "no parts vulnerable to frostbite";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			foreach (BodyPartRecord allPart in AllParts)
			{
				if (allPart.def.conceptual && allPart.coverageAbs != 0f)
				{
					yield return $"part {allPart} is tagged conceptual, but has nonzero coverage";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_01b1:
			/*Error near IL_01b2: Unexpected return in MoveNext()*/;
		}

		public override void ResolveReferences()
		{
			if (corePart != null)
			{
				CacheDataRecursive(corePart);
			}
			cachedPartsVulnerableToFrostbite = new List<BodyPartRecord>();
			List<BodyPartRecord> allParts = AllParts;
			for (int i = 0; i < allParts.Count; i++)
			{
				if (allParts[i].def.frostbiteVulnerability > 0f)
				{
					cachedPartsVulnerableToFrostbite.Add(allParts[i]);
				}
			}
		}

		private void CacheDataRecursive(BodyPartRecord node)
		{
			if (node.def == null)
			{
				Log.Error("BodyPartRecord with null def. body=" + this);
			}
			else
			{
				node.body = this;
				for (int i = 0; i < node.parts.Count; i++)
				{
					node.parts[i].parent = node;
				}
				if (node.parent != null)
				{
					node.coverageAbsWithChildren = node.parent.coverageAbsWithChildren * node.coverage;
				}
				else
				{
					node.coverageAbsWithChildren = 1f;
				}
				float num = 1f;
				for (int j = 0; j < node.parts.Count; j++)
				{
					num -= node.parts[j].coverage;
				}
				if (Mathf.Abs(num) < 1E-05f)
				{
					num = 0f;
				}
				if (num < 0f)
				{
					num = 0f;
					Log.Warning("BodyDef " + defName + " has BodyPartRecord of " + node.def.defName + " whose children have more coverage than 1.");
				}
				node.coverageAbs = node.coverageAbsWithChildren * num;
				if (node.height == BodyPartHeight.Undefined)
				{
					node.height = BodyPartHeight.Middle;
				}
				if (node.depth == BodyPartDepth.Undefined)
				{
					node.depth = BodyPartDepth.Outside;
				}
				for (int k = 0; k < node.parts.Count; k++)
				{
					if (node.parts[k].height == BodyPartHeight.Undefined)
					{
						node.parts[k].height = node.height;
					}
					if (node.parts[k].depth == BodyPartDepth.Undefined)
					{
						node.parts[k].depth = node.depth;
					}
				}
				cachedAllParts.Add(node);
				for (int l = 0; l < node.parts.Count; l++)
				{
					CacheDataRecursive(node.parts[l]);
				}
			}
		}
	}
}
