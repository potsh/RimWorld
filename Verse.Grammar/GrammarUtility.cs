using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse.Grammar
{
	public static class GrammarUtility
	{
		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Pawn pawn, Dictionary<string, string> constants = null)
		{
			if (pawn == null)
			{
				Log.ErrorOnce($"Tried to insert rule {pawnSymbol} for null pawn", 16015097);
				return Enumerable.Empty<Rule>();
			}
			return RulesForPawn(pawnSymbol, pawn.Name, (pawn.story == null) ? null : pawn.story.Title, pawn.kindDef, pawn.gender, pawn.Faction, constants);
		}

		public static IEnumerable<Rule> RulesForPawn(string pawnSymbol, Name name, string title, PawnKindDef kind, Gender gender, Faction faction, Dictionary<string, string> constants = null)
		{
			yield return (Rule)new Rule_String(output: (name == null) ? Find.ActiveLanguageWorker.WithIndefiniteArticle(kind.label, gender) : Find.ActiveLanguageWorker.WithIndefiniteArticle(name.ToStringFull, gender, plural: false, name: true), keyword: pawnSymbol + "_nameFull");
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public static IEnumerable<Rule> RulesForDef(string prefix, Def def)
		{
			if (def != null)
			{
				yield return (Rule)new Rule_String(prefix + "_label", def.label);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			Log.ErrorOnce($"Tried to insert rule {prefix} for null def", 79641686);
		}

		public static IEnumerable<Rule> RulesForBodyPartRecord(string prefix, BodyPartRecord part)
		{
			if (part != null)
			{
				yield return (Rule)new Rule_String(prefix + "_label", part.Label);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			Log.ErrorOnce($"Tried to insert rule {prefix} for null body part", 394876778);
		}

		public static IEnumerable<Rule> RulesForHediffDef(string prefix, HediffDef def, BodyPartRecord part)
		{
			using (IEnumerator<Rule> enumerator = RulesForDef(prefix, def).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Rule rule = enumerator.Current;
					yield return rule;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			string noun = def.labelNoun;
			if (noun.NullOrEmpty())
			{
				noun = def.label;
			}
			yield return (Rule)new Rule_String(prefix + "_labelNoun", noun);
			/*Error: Unable to find new state assignment for yield return*/;
			IL_018a:
			/*Error near IL_018b: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<Rule> RulesForFaction(string prefix, Faction faction)
		{
			if (faction != null)
			{
				yield return (Rule)new Rule_String(prefix + "_name", faction.Name);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return (Rule)new Rule_String(prefix + "_name", "FactionUnaffiliated".Translate());
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
