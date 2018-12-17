using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class BodyPartRecord
	{
		public BodyDef body;

		[TranslationHandle]
		public BodyPartDef def;

		[MustTranslate]
		public string customLabel;

		[Unsaved]
		[TranslationHandle(Priority = 100)]
		public string untranslatedCustomLabel;

		public List<BodyPartRecord> parts = new List<BodyPartRecord>();

		public BodyPartHeight height;

		public BodyPartDepth depth;

		public float coverage = 1f;

		public List<BodyPartGroupDef> groups = new List<BodyPartGroupDef>();

		[Unsaved]
		public BodyPartRecord parent;

		[Unsaved]
		public float coverageAbsWithChildren;

		[Unsaved]
		public float coverageAbs;

		public bool IsCorePart => parent == null;

		public string Label => (!customLabel.NullOrEmpty()) ? customLabel : def.label;

		public string LabelCap => Label.CapitalizeFirst();

		public string LabelShort => def.LabelShort;

		public string LabelShortCap => def.LabelShortCap;

		public int Index => body.GetIndexOfPart(this);

		public override string ToString()
		{
			return "BodyPartRecord(" + ((def == null) ? "NULL_DEF" : def.defName) + " parts.Count=" + parts.Count + ")";
		}

		public void PostLoad()
		{
			untranslatedCustomLabel = customLabel;
		}

		public bool IsInGroup(BodyPartGroupDef group)
		{
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i] == group)
				{
					return true;
				}
			}
			return false;
		}

		public IEnumerable<BodyPartRecord> GetChildParts(BodyPartTagDef tag)
		{
			if (def.tags.Contains(tag))
			{
				yield return this;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			for (int i = 0; i < parts.Count; i++)
			{
				using (IEnumerator<BodyPartRecord> enumerator = parts[i].GetChildParts(tag).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						BodyPartRecord record = enumerator.Current;
						yield return record;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0148:
			/*Error near IL_0149: Unexpected return in MoveNext()*/;
		}

		public IEnumerable<BodyPartRecord> GetDirectChildParts()
		{
			int i = 0;
			if (i < parts.Count)
			{
				yield return parts[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool HasChildParts(BodyPartTagDef tag)
		{
			return GetChildParts(tag).Any();
		}

		public IEnumerable<BodyPartRecord> GetConnectedParts(BodyPartTagDef tag)
		{
			BodyPartRecord ancestor = this;
			while (ancestor.parent != null && ancestor.parent.def.tags.Contains(tag))
			{
				ancestor = ancestor.parent;
			}
			using (IEnumerator<BodyPartRecord> enumerator = ancestor.GetChildParts(tag).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					BodyPartRecord child = enumerator.Current;
					yield return child;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0116:
			/*Error near IL_0117: Unexpected return in MoveNext()*/;
		}
	}
}
