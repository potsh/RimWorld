using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleDef : Def
	{
		public TaleType type;

		public Type taleClass;

		public bool usableForArt = true;

		public bool colonistOnly = true;

		public int maxPerPawn = -1;

		public float ignoreChance;

		public float expireDays = -1f;

		public RulePack rulePack;

		[NoTranslate]
		public string firstPawnSymbol;

		[NoTranslate]
		public string secondPawnSymbol;

		[NoTranslate]
		public string defSymbol;

		public Type defType = typeof(ThingDef);

		public float baseInterest;

		public Color historyGraphColor = Color.white;

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (taleClass == null)
			{
				yield return defName + " taleClass is null.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (expireDays < 0f)
			{
				if (type == TaleType.Expirable)
				{
					yield return "Expirable tale type is used but expireDays<0";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			else if (type != TaleType.Expirable)
			{
				yield return "Non expirable tale type is used but expireDays>=0";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (baseInterest > 1E-06f && !usableForArt)
			{
				yield return "Non-zero baseInterest but not usable for art";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (firstPawnSymbol == "pawn" || secondPawnSymbol == "pawn")
			{
				yield return "pawn symbols should not be 'pawn', this is the default and only choice for SinglePawn tales so using it here is confusing.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_021d:
			/*Error near IL_021e: Unexpected return in MoveNext()*/;
		}

		public static TaleDef Named(string str)
		{
			return DefDatabase<TaleDef>.GetNamed(str);
		}
	}
}
