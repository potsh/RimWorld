using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class TrainableDef : Def
	{
		public float difficulty = -1f;

		public float minBodySize;

		public List<TrainableDef> prerequisites;

		[NoTranslate]
		public List<string> tags = new List<string>();

		public bool defaultTrainable;

		public TrainabilityDef requiredTrainability;

		public int steps = 1;

		public float listPriority;

		[NoTranslate]
		public string icon;

		[Unsaved]
		public int indent;

		[Unsaved]
		private Texture2D iconTex;

		public Texture2D Icon
		{
			get
			{
				if (iconTex == null)
				{
					iconTex = ContentFinder<Texture2D>.Get(icon);
				}
				return iconTex;
			}
		}

		public bool MatchesTag(string tag)
		{
			if (tag == defName)
			{
				return true;
			}
			for (int i = 0; i < tags.Count; i++)
			{
				if (tags[i] == tag)
				{
					return true;
				}
			}
			return false;
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
			if (difficulty < 0f)
			{
				yield return "difficulty not set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_00f1:
			/*Error near IL_00f2: Unexpected return in MoveNext()*/;
		}
	}
}
