using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public static class GrammarResolverSimple
	{
		private static bool working;

		private static StringBuilder tmpResultBuffer = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_objectLabel = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_subSymbol = new StringBuilder();

		private static StringBuilder tmpSymbolBuffer_args = new StringBuilder();

		private static List<string> tmpArgsLabels = new List<string>();

		private static List<object> tmpArgsObjects = new List<object>();

		private static StringBuilder tmpArg = new StringBuilder();

		public static string Formatted(string str, List<string> argsLabelsArg, List<object> argsObjectsArg)
		{
			if (!str.NullOrEmpty())
			{
				bool flag;
				StringBuilder stringBuilder;
				StringBuilder stringBuilder2;
				StringBuilder stringBuilder3;
				StringBuilder stringBuilder4;
				StringBuilder stringBuilder5;
				List<string> list;
				List<object> list2;
				if (working)
				{
					flag = false;
					stringBuilder = new StringBuilder();
					stringBuilder2 = new StringBuilder();
					stringBuilder3 = new StringBuilder();
					stringBuilder4 = new StringBuilder();
					stringBuilder5 = new StringBuilder();
					list = argsLabelsArg.ToList();
					list2 = argsObjectsArg.ToList();
				}
				else
				{
					flag = true;
					stringBuilder = tmpResultBuffer;
					stringBuilder2 = tmpSymbolBuffer;
					stringBuilder3 = tmpSymbolBuffer_objectLabel;
					stringBuilder4 = tmpSymbolBuffer_subSymbol;
					stringBuilder5 = tmpSymbolBuffer_args;
					list = tmpArgsLabels;
					list.Clear();
					list.AddRange(argsLabelsArg);
					list2 = tmpArgsObjects;
					list2.Clear();
					list2.AddRange(argsObjectsArg);
				}
				if (flag)
				{
					working = true;
				}
				try
				{
					stringBuilder.Length = 0;
					for (int i = 0; i < str.Length; i++)
					{
						char c = str[i];
						if (c == '{')
						{
							stringBuilder2.Length = 0;
							stringBuilder3.Length = 0;
							stringBuilder4.Length = 0;
							stringBuilder5.Length = 0;
							bool flag2 = false;
							bool flag3 = false;
							bool flag4 = false;
							i++;
							bool flag5 = i < str.Length && str[i] == '{';
							for (; i < str.Length; i++)
							{
								char c2 = str[i];
								if (c2 == '}')
								{
									flag2 = true;
									break;
								}
								stringBuilder2.Append(c2);
								if (c2 == '_' && !flag3)
								{
									flag3 = true;
								}
								else if (c2 == '?' && !flag4)
								{
									flag4 = true;
								}
								else if (flag4)
								{
									stringBuilder5.Append(c2);
								}
								else if (flag3)
								{
									stringBuilder4.Append(c2);
								}
								else
								{
									stringBuilder3.Append(c2);
								}
							}
							if (!flag2)
							{
								Log.ErrorOnce("Could not find matching '}' in \"" + str + "\".", str.GetHashCode() ^ 0xB9D492D);
							}
							else if (flag5)
							{
								stringBuilder.Append(stringBuilder2);
							}
							else
							{
								if (flag4)
								{
									while (stringBuilder4.Length != 0 && stringBuilder4[stringBuilder4.Length - 1] == ' ')
									{
										stringBuilder4.Length--;
									}
								}
								string text = stringBuilder3.ToString();
								bool flag6 = false;
								int result = -1;
								if (int.TryParse(text, out result))
								{
									if (result >= 0 && result < list2.Count && TryResolveSymbol(list2[result], stringBuilder4.ToString(), stringBuilder5.ToString(), out string resolvedStr, str))
									{
										flag6 = true;
										stringBuilder.Append(resolvedStr);
									}
								}
								else
								{
									for (int j = 0; j < list.Count; j++)
									{
										if (list[j] == text)
										{
											if (TryResolveSymbol(list2[j], stringBuilder4.ToString(), stringBuilder5.ToString(), out string resolvedStr2, str))
											{
												flag6 = true;
												stringBuilder.Append(resolvedStr2);
											}
											break;
										}
									}
								}
								if (!flag6)
								{
									Log.ErrorOnce("Could not resolve symbol \"" + stringBuilder2 + "\" for string \"" + str + "\".", str.GetHashCode() ^ stringBuilder2.ToString().GetHashCode() ^ 0x346E76FE);
								}
							}
						}
						else
						{
							stringBuilder.Append(c);
						}
					}
					string translation = GenText.CapitalizeSentences(stringBuilder.ToString(), str[0] == '{');
					return Find.ActiveLanguageWorker.PostProcessedKeyedTranslation(translation);
				}
				finally
				{
					if (flag)
					{
						working = false;
					}
				}
			}
			return str;
		}

		private static bool TryResolveSymbol(object obj, string subSymbol, string symbolArgs, out string resolvedStr, string fullStringForReference)
		{
			Pawn pawn = obj as Pawn;
			int value;
			if (pawn != null)
			{
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map0 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(40);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("nameFull", 1);
						dictionary.Add("nameFullDef", 2);
						dictionary.Add("label", 3);
						dictionary.Add("labelShort", 4);
						dictionary.Add("definite", 5);
						dictionary.Add("nameDef", 6);
						dictionary.Add("indefinite", 7);
						dictionary.Add("nameIndef", 8);
						dictionary.Add("pronoun", 9);
						dictionary.Add("possessive", 10);
						dictionary.Add("objective", 11);
						dictionary.Add("factionName", 12);
						dictionary.Add("factionPawnSingular", 13);
						dictionary.Add("factionPawnSingularDef", 14);
						dictionary.Add("factionPawnSingularIndef", 15);
						dictionary.Add("factionPawnsPlural", 16);
						dictionary.Add("factionPawnsPluralDef", 17);
						dictionary.Add("factionPawnsPluralIndef", 18);
						dictionary.Add("kind", 19);
						dictionary.Add("kindDef", 20);
						dictionary.Add("kindIndef", 21);
						dictionary.Add("kindPlural", 22);
						dictionary.Add("kindPluralDef", 23);
						dictionary.Add("kindPluralIndef", 24);
						dictionary.Add("kindBase", 25);
						dictionary.Add("kindBaseDef", 26);
						dictionary.Add("kindBaseIndef", 27);
						dictionary.Add("kindBasePlural", 28);
						dictionary.Add("kindBasePluralDef", 29);
						dictionary.Add("kindBasePluralIndef", 30);
						dictionary.Add("lifeStage", 31);
						dictionary.Add("lifeStageDef", 32);
						dictionary.Add("lifeStageIndef", 33);
						dictionary.Add("lifeStageAdjective", 34);
						dictionary.Add("title", 35);
						dictionary.Add("titleDef", 36);
						dictionary.Add("titleIndef", 37);
						dictionary.Add("gender", 38);
						dictionary.Add("humanlike", 39);
						_003C_003Ef__switch_0024map0 = dictionary;
					}
					if (_003C_003Ef__switch_0024map0.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 1:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringFull, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringFull, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = pawn.Label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabel : pawn.Name.ToStringShort);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 6:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelDefinite() : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 7:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 8:
							resolvedStr = ((pawn.Name == null) ? pawn.KindLabelIndefinite() : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 9:
							resolvedStr = pawn.gender.GetPronoun();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 10:
							resolvedStr = pawn.gender.GetPossessive();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 11:
							resolvedStr = pawn.gender.GetObjective();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 12:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.Name);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 13:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.def.pawnSingular);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 14:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnSingular));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 15:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnSingular));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 16:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : pawn.Faction.def.pawnsPlural);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 17:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), plural: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 18:
							resolvedStr = ((pawn.Faction == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.Faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(pawn.Faction.def.pawnsPlural, pawn.Faction.def.pawnSingular), plural: true));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 19:
							resolvedStr = pawn.KindLabel;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 20:
							resolvedStr = pawn.KindLabelDefinite();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 21:
							resolvedStr = pawn.KindLabelIndefinite();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 22:
							resolvedStr = pawn.GetKindLabelPlural();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 23:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.GetKindLabelPlural(), pawn.gender, plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 24:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.GetKindLabelPlural(), pawn.gender, plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 25:
							resolvedStr = pawn.kindDef.label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 26:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 27:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 28:
							resolvedStr = pawn.kindDef.GetLabelPlural();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 29:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(), pawn.kindDef.label), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 30:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.kindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawn.kindDef.GetLabelPlural(), pawn.kindDef.label), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 31:
							resolvedStr = pawn.ageTracker.CurLifeStage.label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 32:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 33:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.ageTracker.CurLifeStage.label, pawn.gender);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 34:
							resolvedStr = pawn.ageTracker.CurLifeStage.Adjective;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 35:
							resolvedStr = ((pawn.story == null) ? string.Empty : pawn.story.Title);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 36:
							resolvedStr = ((pawn.story == null) ? string.Empty : Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.story.Title));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 37:
							resolvedStr = ((pawn.story == null) ? string.Empty : Find.ActiveLanguageWorker.WithIndefiniteArticle(pawn.story.Title));
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 38:
							resolvedStr = ResolveGenderSymbol(pawn.gender, pawn.RaceProps.Animal, symbolArgs, fullStringForReference);
							return true;
						case 39:
							resolvedStr = ResolveHumanlikeSymbol(pawn.RaceProps.Humanlike, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			Thing thing = obj as Thing;
			if (thing != null)
			{
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map1 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(13);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("label", 1);
						dictionary.Add("labelPlural", 2);
						dictionary.Add("labelPluralDef", 3);
						dictionary.Add("labelPluralIndef", 4);
						dictionary.Add("labelShort", 5);
						dictionary.Add("definite", 6);
						dictionary.Add("indefinite", 7);
						dictionary.Add("pronoun", 8);
						dictionary.Add("possessive", 9);
						dictionary.Add("objective", 10);
						dictionary.Add("factionName", 11);
						dictionary.Add("gender", 12);
						_003C_003Ef__switch_0024map1 = dictionary;
					}
					if (_003C_003Ef__switch_0024map1.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 1:
							resolvedStr = thing.Label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), thing.LabelNoCount), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(thing.LabelNoCount), thing.LabelNoCount), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
							resolvedStr = thing.LabelShort;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 6:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(thing.Label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 7:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(thing.Label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 8:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetPronoun();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 9:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetPossessive();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 10:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount).GetObjective();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 11:
							resolvedStr = ((thing.Faction == null) ? string.Empty : thing.Faction.Name);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 12:
							resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(thing.LabelNoCount), animal: false, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			WorldObject worldObject = obj as WorldObject;
			if (worldObject != null)
			{
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map2 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(12);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("label", 1);
						dictionary.Add("labelPlural", 2);
						dictionary.Add("labelPluralDef", 3);
						dictionary.Add("labelPluralIndef", 4);
						dictionary.Add("definite", 5);
						dictionary.Add("indefinite", 6);
						dictionary.Add("pronoun", 7);
						dictionary.Add("possessive", 8);
						dictionary.Add("objective", 9);
						dictionary.Add("factionName", 10);
						dictionary.Add("gender", 11);
						_003C_003Ef__switch_0024map2 = dictionary;
					}
					if (_003C_003Ef__switch_0024map2.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
						{
							LanguageWorker activeLanguageWorker = Find.ActiveLanguageWorker;
							string label = worldObject.Label;
							bool hasName = worldObject.HasName;
							resolvedStr = activeLanguageWorker.WithIndefiniteArticle(label, plural: false, hasName);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						}
						case 1:
							resolvedStr = worldObject.Label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = Find.ActiveLanguageWorker.Pluralize(worldObject.Label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), worldObject.Label), plural: true, worldObject.HasName);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(worldObject.Label), worldObject.Label), plural: true, worldObject.HasName);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
						{
							LanguageWorker activeLanguageWorker3 = Find.ActiveLanguageWorker;
							string label = worldObject.Label;
							bool hasName = worldObject.HasName;
							resolvedStr = activeLanguageWorker3.WithDefiniteArticle(label, plural: false, hasName);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						}
						case 6:
						{
							LanguageWorker activeLanguageWorker2 = Find.ActiveLanguageWorker;
							string label = worldObject.Label;
							bool hasName = worldObject.HasName;
							resolvedStr = activeLanguageWorker2.WithIndefiniteArticle(label, plural: false, hasName);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						}
						case 7:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetPronoun();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 8:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetPossessive();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 9:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label).GetObjective();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 10:
							resolvedStr = ((worldObject.Faction == null) ? string.Empty : worldObject.Faction.Name);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 11:
							resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(worldObject.Label), animal: false, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			Faction faction = obj as Faction;
			if (faction != null)
			{
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map3 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(8);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("name", 1);
						dictionary.Add("pawnSingular", 2);
						dictionary.Add("pawnSingularDef", 3);
						dictionary.Add("pawnSingularIndef", 4);
						dictionary.Add("pawnsPlural", 5);
						dictionary.Add("pawnsPluralDef", 6);
						dictionary.Add("pawnsPluralIndef", 7);
						_003C_003Ef__switch_0024map3 = dictionary;
					}
					if (_003C_003Ef__switch_0024map3.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
							resolvedStr = faction.Name;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 1:
							resolvedStr = faction.Name;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = faction.def.pawnSingular;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnSingular);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnSingular);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
							resolvedStr = faction.def.pawnsPlural;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 6:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 7:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(faction.def.pawnsPlural, LanguageDatabase.activeLanguage.ResolveGender(faction.def.pawnsPlural, faction.def.pawnSingular), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			Def def = obj as Def;
			if (def != null)
			{
				PawnKindDef pawnKindDef = def as PawnKindDef;
				if (pawnKindDef != null && subSymbol != null)
				{
					if (subSymbol == "labelPlural")
					{
						resolvedStr = pawnKindDef.GetLabelPlural();
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "labelPluralDef")
					{
						resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(pawnKindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(), pawnKindDef.label), plural: true);
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "labelPluralIndef")
					{
						resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(pawnKindDef.GetLabelPlural(), LanguageDatabase.activeLanguage.ResolveGender(pawnKindDef.GetLabelPlural(), pawnKindDef.label), plural: true);
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
				}
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map4 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(11);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("label", 1);
						dictionary.Add("labelPlural", 2);
						dictionary.Add("labelPluralDef", 3);
						dictionary.Add("labelPluralIndef", 4);
						dictionary.Add("definite", 5);
						dictionary.Add("indefinite", 6);
						dictionary.Add("pronoun", 7);
						dictionary.Add("possessive", 8);
						dictionary.Add("objective", 9);
						dictionary.Add("gender", 10);
						_003C_003Ef__switch_0024map4 = dictionary;
					}
					if (_003C_003Ef__switch_0024map4.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 1:
							resolvedStr = def.label;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = Find.ActiveLanguageWorker.Pluralize(def.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label), def.label), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(def.label), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(def.label), def.label), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(def.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 6:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(def.label);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 7:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetPronoun();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 8:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetPossessive();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 9:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(def.label).GetObjective();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 10:
							resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(def.label), animal: false, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			string text = obj as string;
			if (text != null)
			{
				if (subSymbol != null)
				{
					if (_003C_003Ef__switch_0024map5 == null)
					{
						Dictionary<string, int> dictionary = new Dictionary<string, int>(10);
						dictionary.Add(string.Empty, 0);
						dictionary.Add("plural", 1);
						dictionary.Add("pluralDef", 2);
						dictionary.Add("pluralIndef", 3);
						dictionary.Add("definite", 4);
						dictionary.Add("indefinite", 5);
						dictionary.Add("pronoun", 6);
						dictionary.Add("possessive", 7);
						dictionary.Add("objective", 8);
						dictionary.Add("gender", 9);
						_003C_003Ef__switch_0024map5 = dictionary;
					}
					if (_003C_003Ef__switch_0024map5.TryGetValue(subSymbol, out value))
					{
						switch (value)
						{
						case 0:
							resolvedStr = text;
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 1:
							resolvedStr = Find.ActiveLanguageWorker.Pluralize(text);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 2:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text), text), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 3:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(Find.ActiveLanguageWorker.Pluralize(text), LanguageDatabase.activeLanguage.ResolveGender(Find.ActiveLanguageWorker.Pluralize(text), text), plural: true);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 4:
							resolvedStr = Find.ActiveLanguageWorker.WithDefiniteArticle(text);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 5:
							resolvedStr = Find.ActiveLanguageWorker.WithIndefiniteArticle(text);
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 6:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text).GetPronoun();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 7:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text).GetPossessive();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 8:
							resolvedStr = LanguageDatabase.activeLanguage.ResolveGender(text).GetObjective();
							EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
							return true;
						case 9:
							resolvedStr = ResolveGenderSymbol(LanguageDatabase.activeLanguage.ResolveGender(text), animal: false, symbolArgs, fullStringForReference);
							return true;
						}
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			if (obj is int || obj is long)
			{
				int number = (int)((!(obj is int)) ? ((long)obj) : ((int)obj));
				if (subSymbol != null)
				{
					if (subSymbol == string.Empty)
					{
						resolvedStr = number.ToString();
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
					if (subSymbol == "ordinal")
					{
						resolvedStr = Find.ActiveLanguageWorker.OrdinalNumber(number).ToString();
						EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
						return true;
					}
				}
				resolvedStr = string.Empty;
				return false;
			}
			if (subSymbol.NullOrEmpty())
			{
				EnsureNoArgs(subSymbol, symbolArgs, fullStringForReference);
				if (obj == null)
				{
					resolvedStr = string.Empty;
				}
				else
				{
					resolvedStr = obj.ToString();
				}
				return true;
			}
			resolvedStr = string.Empty;
			return false;
		}

		private static void EnsureNoArgs(string subSymbol, string symbolArgs, string fullStringForReference)
		{
			if (!symbolArgs.NullOrEmpty())
			{
				Log.ErrorOnce("Symbol \"" + subSymbol + "\" doesn't expect any args but \"" + symbolArgs + "\" args were provided. Full string: \"" + fullStringForReference + "\".", subSymbol.GetHashCode() ^ symbolArgs.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x391B4B8E);
			}
		}

		private static string ResolveGenderSymbol(Gender gender, bool animal, string args, string fullStringForReference)
		{
			if (!args.NullOrEmpty())
			{
				switch (GetArgsCount(args))
				{
				case 2:
					switch (gender)
					{
					case Gender.Male:
						return GetArg(args, 0);
					case Gender.Female:
						return GetArg(args, 1);
					case Gender.None:
						return GetArg(args, 0);
					default:
						return string.Empty;
					}
				case 3:
					switch (gender)
					{
					case Gender.Male:
						return GetArg(args, 0);
					case Gender.Female:
						return GetArg(args, 1);
					case Gender.None:
						return GetArg(args, 2);
					default:
						return string.Empty;
					}
				default:
					Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"gender\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x2EF21A43);
					return string.Empty;
				}
			}
			return gender.GetLabel(animal);
		}

		private static string ResolveHumanlikeSymbol(bool humanlike, string args, string fullStringForReference)
		{
			int argsCount = GetArgsCount(args);
			if (argsCount == 2)
			{
				if (humanlike)
				{
					return GetArg(args, 0);
				}
				return GetArg(args, 1);
			}
			Log.ErrorOnce("Invalid args count in \"" + fullStringForReference + "\" for symbol \"humanlike\".", args.GetHashCode() ^ fullStringForReference.GetHashCode() ^ 0x355A4AD5);
			return string.Empty;
		}

		private static int GetArgsCount(string args)
		{
			int num = 1;
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i] == ':')
				{
					num++;
				}
			}
			return num;
		}

		private static string GetArg(string args, int argIndex)
		{
			tmpArg.Length = 0;
			int num = 0;
			foreach (char c in args)
			{
				if (c == ':')
				{
					num++;
				}
				else if (num == argIndex)
				{
					tmpArg.Append(c);
				}
				else if (num > argIndex)
				{
					break;
				}
			}
			while (tmpArg.Length != 0 && tmpArg[0] == ' ')
			{
				tmpArg.Remove(0, 1);
			}
			while (tmpArg.Length != 0 && tmpArg[tmpArg.Length - 1] == ' ')
			{
				tmpArg.Length--;
			}
			return tmpArg.ToString();
		}
	}
}
