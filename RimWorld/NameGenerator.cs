using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public static class NameGenerator
	{
		public static string GenerateName(RulePackDef rootPack, IEnumerable<string> extantNames, bool appendNumberIfNameUsed = false, string rootKeyword = null)
		{
			return GenerateName(rootPack, (string x) => !extantNames.Contains(x), appendNumberIfNameUsed, rootKeyword);
		}

		public static string GenerateName(RulePackDef rootPack, Predicate<string> validator = null, bool appendNumberIfNameUsed = false, string rootKeyword = null, string testPawnNameSymbol = null)
		{
			GrammarRequest grammarRequest = default(GrammarRequest);
			grammarRequest.Includes.Add(rootPack);
			if (testPawnNameSymbol != null)
			{
				grammarRequest.Rules.Add(new Rule_String("ANYPAWN_nameDef", testPawnNameSymbol));
				grammarRequest.Rules.Add(new Rule_String("ANYPAWN_nameIndef", testPawnNameSymbol));
			}
			string text = (rootKeyword == null) ? rootPack.FirstRuleKeyword : rootKeyword;
			string text2 = (rootKeyword == null) ? rootPack.FirstUntranslatedRuleKeyword : rootKeyword;
			if (appendNumberIfNameUsed)
			{
				string untranslatedRootKeyword;
				GrammarRequest request;
				string rootKeyword2;
				for (int i = 0; i < 100; i++)
				{
					for (int j = 0; j < 5; j++)
					{
						rootKeyword2 = text;
						request = grammarRequest;
						untranslatedRootKeyword = text2;
						string text3 = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(rootKeyword2, request, null, forceLog: false, untranslatedRootKeyword));
						if (i != 0)
						{
							text3 = text3 + " " + (i + 1);
						}
						if (validator == null || validator(text3))
						{
							return text3;
						}
					}
				}
				untranslatedRootKeyword = text;
				request = grammarRequest;
				rootKeyword2 = text2;
				return GenText.ToTitleCaseSmart(GrammarResolver.Resolve(untranslatedRootKeyword, request, null, forceLog: false, rootKeyword2));
			}
			for (int k = 0; k < 150; k++)
			{
				string rootKeyword2 = text;
				GrammarRequest request = grammarRequest;
				string untranslatedRootKeyword = text2;
				string text4 = GenText.ToTitleCaseSmart(GrammarResolver.Resolve(rootKeyword2, request, null, forceLog: false, untranslatedRootKeyword));
				if (validator == null || validator(text4))
				{
					return text4;
				}
			}
			Log.Error("Could not get new name (rule pack: " + rootPack + ")");
			return "Errorname";
		}
	}
}
