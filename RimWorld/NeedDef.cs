using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class NeedDef : Def
	{
		public Type needClass;

		public Intelligence minIntelligence;

		public bool colonistAndPrisonersOnly;

		public bool colonistsOnly;

		public bool onlyIfCausedByHediff;

		public bool neverOnPrisoner;

		public bool showOnNeedList = true;

		public float baseLevel = 0.5f;

		public bool major;

		public int listPriority;

		[NoTranslate]
		public string tutorHighlightTag;

		public bool showForCaravanMembers;

		public bool scaleBar;

		public float fallPerDay = 0.5f;

		public float seekerRisePerHour;

		public float seekerFallPerHour;

		public bool freezeWhileSleeping;

		public bool freezeInMentalState;

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
			if (description.NullOrEmpty() && showOnNeedList)
			{
				yield return "no description";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (needClass == null)
			{
				yield return "needClass is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (needClass == typeof(Need_Seeker) && (seekerRisePerHour == 0f || seekerFallPerHour == 0f))
			{
				yield return "seeker rise/fall rates not set";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_019b:
			/*Error near IL_019c: Unexpected return in MoveNext()*/;
		}
	}
}
