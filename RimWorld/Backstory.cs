using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[CaseInsensitiveXMLParsing]
	public class Backstory
	{
		public string identifier;

		public BackstorySlot slot;

		public string title;

		public string titleFemale;

		public string titleShort;

		public string titleShortFemale;

		public string baseDesc;

		private Dictionary<string, int> skillGains = new Dictionary<string, int>();

		[Unsaved]
		public Dictionary<SkillDef, int> skillGainsResolved = new Dictionary<SkillDef, int>();

		public WorkTags workDisables;

		public WorkTags requiredWorkTags;

		public List<string> spawnCategories = new List<string>();

		[LoadAlias("bodyNameGlobal")]
		private string bodyTypeGlobal;

		[LoadAlias("bodyNameFemale")]
		private string bodyTypeFemale;

		[LoadAlias("bodyNameMale")]
		private string bodyTypeMale;

		[Unsaved]
		private BodyTypeDef bodyTypeGlobalResolved;

		[Unsaved]
		private BodyTypeDef bodyTypeFemaleResolved;

		[Unsaved]
		private BodyTypeDef bodyTypeMaleResolved;

		public List<TraitEntry> forcedTraits;

		public List<TraitEntry> disallowedTraits;

		public bool shuffleable = true;

		[Unsaved]
		public string untranslatedTitle;

		[Unsaved]
		public string untranslatedTitleFemale;

		[Unsaved]
		public string untranslatedTitleShort;

		[Unsaved]
		public string untranslatedTitleShortFemale;

		[Unsaved]
		public string untranslatedDesc;

		[Unsaved]
		public bool titleTranslated;

		[Unsaved]
		public bool titleFemaleTranslated;

		[Unsaved]
		public bool titleShortTranslated;

		[Unsaved]
		public bool titleShortFemaleTranslated;

		[Unsaved]
		public bool descTranslated;

		public IEnumerable<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				List<WorkTypeDef> list = DefDatabase<WorkTypeDef>.AllDefsListForReading;
				int i = 0;
				while (true)
				{
					if (i >= list.Count)
					{
						yield break;
					}
					if (!AllowsWorkType(list[i]))
					{
						break;
					}
					i++;
				}
				yield return list[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public IEnumerable<WorkGiverDef> DisabledWorkGivers
		{
			get
			{
				List<WorkGiverDef> list = DefDatabase<WorkGiverDef>.AllDefsListForReading;
				int i = 0;
				while (true)
				{
					if (i >= list.Count)
					{
						yield break;
					}
					if (!AllowsWorkGiver(list[i]))
					{
						break;
					}
					i++;
				}
				yield return list[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool DisallowsTrait(TraitDef def, int degree)
		{
			if (disallowedTraits == null)
			{
				return false;
			}
			for (int i = 0; i < disallowedTraits.Count; i++)
			{
				if (disallowedTraits[i].def == def && disallowedTraits[i].degree == degree)
				{
					return true;
				}
			}
			return false;
		}

		public string TitleFor(Gender g)
		{
			if (g != Gender.Female || titleFemale.NullOrEmpty())
			{
				return title;
			}
			return titleFemale;
		}

		public string TitleCapFor(Gender g)
		{
			return TitleFor(g).CapitalizeFirst();
		}

		public string TitleShortFor(Gender g)
		{
			if (g == Gender.Female && !titleShortFemale.NullOrEmpty())
			{
				return titleShortFemale;
			}
			if (!titleShort.NullOrEmpty())
			{
				return titleShort;
			}
			return TitleFor(g);
		}

		public string TitleShortCapFor(Gender g)
		{
			return TitleShortFor(g).CapitalizeFirst();
		}

		public BodyTypeDef BodyTypeFor(Gender g)
		{
			if (bodyTypeGlobalResolved == null)
			{
				switch (g)
				{
				case Gender.None:
					break;
				case Gender.Female:
					return bodyTypeFemaleResolved;
				default:
					return bodyTypeMaleResolved;
				}
			}
			return bodyTypeGlobalResolved;
		}

		public string FullDescriptionFor(Pawn p)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(baseDesc.Formatted(p.Named("PAWN")).AdjustedFor(p));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine();
			List<SkillDef> allDefsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				SkillDef skillDef = allDefsListForReading[i];
				if (skillGainsResolved.ContainsKey(skillDef))
				{
					stringBuilder.AppendLine(skillDef.skillLabel.CapitalizeFirst() + ":   " + skillGainsResolved[skillDef].ToString("+##;-##"));
				}
			}
			stringBuilder.AppendLine();
			foreach (WorkTypeDef disabledWorkType in DisabledWorkTypes)
			{
				stringBuilder.AppendLine(disabledWorkType.gerundLabel.CapitalizeFirst() + " " + "DisabledLower".Translate());
			}
			foreach (WorkGiverDef disabledWorkGiver in DisabledWorkGivers)
			{
				stringBuilder.AppendLine(disabledWorkGiver.workType.gerundLabel.CapitalizeFirst() + ": " + disabledWorkGiver.LabelCap + " " + "DisabledLower".Translate());
			}
			string str = stringBuilder.ToString().TrimEndNewlines();
			return Find.ActiveLanguageWorker.PostProcessed(str);
		}

		private bool AllowsWorkType(WorkTypeDef workType)
		{
			return (workDisables & workType.workTags) == WorkTags.None;
		}

		private bool AllowsWorkGiver(WorkGiverDef workGiver)
		{
			return (workDisables & workGiver.workTags) == WorkTags.None;
		}

		internal void AddForcedTrait(TraitDef traitDef, int degree = 0)
		{
			if (forcedTraits == null)
			{
				forcedTraits = new List<TraitEntry>();
			}
			forcedTraits.Add(new TraitEntry(traitDef, degree));
		}

		internal void AddDisallowedTrait(TraitDef traitDef, int degree = 0)
		{
			if (disallowedTraits == null)
			{
				disallowedTraits = new List<TraitEntry>();
			}
			disallowedTraits.Add(new TraitEntry(traitDef, degree));
		}

		public void PostLoad()
		{
			untranslatedTitle = title;
			untranslatedTitleFemale = titleFemale;
			untranslatedTitleShort = titleShort;
			untranslatedTitleShortFemale = titleShortFemale;
			untranslatedDesc = baseDesc;
			baseDesc = baseDesc.TrimEnd();
			baseDesc = baseDesc.Replace("\r", string.Empty);
		}

		public void ResolveReferences()
		{
			int num = Mathf.Abs(GenText.StableStringHash(baseDesc) % 100);
			string s = title.Replace('-', ' ');
			s = GenText.CapitalizedNoSpaces(s);
			identifier = GenText.RemoveNonAlphanumeric(s) + num.ToString();
			foreach (KeyValuePair<string, int> skillGain in skillGains)
			{
				skillGainsResolved.Add(DefDatabase<SkillDef>.GetNamed(skillGain.Key), skillGain.Value);
			}
			skillGains = null;
			if (!bodyTypeGlobal.NullOrEmpty())
			{
				bodyTypeGlobalResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeGlobal);
			}
			if (!bodyTypeFemale.NullOrEmpty())
			{
				bodyTypeFemaleResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeFemale);
			}
			if (!bodyTypeMale.NullOrEmpty())
			{
				bodyTypeMaleResolved = DefDatabase<BodyTypeDef>.GetNamed(bodyTypeMale);
			}
			if (slot == BackstorySlot.Adulthood && bodyTypeGlobalResolved == null)
			{
				if (bodyTypeMaleResolved == null)
				{
					Log.Error("Adulthood backstory " + title + " is missing male body type. Defaulting...");
					bodyTypeMaleResolved = BodyTypeDefOf.Male;
				}
				if (bodyTypeFemaleResolved == null)
				{
					Log.Error("Adulthood backstory " + title + " is missing female body type. Defaulting...");
					bodyTypeFemaleResolved = BodyTypeDefOf.Female;
				}
			}
		}

		public IEnumerable<string> ConfigErrors(bool ignoreNoSpawnCategories)
		{
			if (title.NullOrEmpty())
			{
				yield return "null title, baseDesc is " + baseDesc;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (titleShort.NullOrEmpty())
			{
				yield return "null titleShort, baseDesc is " + baseDesc;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if ((workDisables & WorkTags.Violent) != 0 && spawnCategories.Contains("Raider"))
			{
				yield return "cannot do Violent work but can spawn as a raider";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (spawnCategories.Count == 0 && !ignoreNoSpawnCategories)
			{
				yield return "no spawn categories";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (spawnCategories.Count == 1 && spawnCategories[0] == "Trader")
			{
				yield return "only Trader spawn category";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!baseDesc.NullOrEmpty())
			{
				if (char.IsWhiteSpace(baseDesc[0]))
				{
					yield return "baseDesc starts with whitepspace";
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (char.IsWhiteSpace(baseDesc[baseDesc.Length - 1]))
				{
					yield return "baseDesc ends with whitespace";
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Prefs.DevMode)
			{
				foreach (KeyValuePair<SkillDef, int> item in skillGainsResolved)
				{
					if (item.Key.IsDisabled(workDisables, DisabledWorkTypes))
					{
						yield return "modifies skill " + item.Key + " but also disables this skill";
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				foreach (KeyValuePair<string, Backstory> allBackstory in BackstoryDatabase.allBackstories)
				{
					if (allBackstory.Value != this && allBackstory.Value.identifier == identifier)
					{
						yield return "backstory identifier used more than once: " + identifier;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_03ec:
			/*Error near IL_03ed: Unexpected return in MoveNext()*/;
		}

		public void SetTitle(string newTitle, string newTitleFemale)
		{
			title = newTitle;
			titleFemale = newTitleFemale;
		}

		public void SetTitleShort(string newTitleShort, string newTitleShortFemale)
		{
			titleShort = newTitleShort;
			titleShortFemale = newTitleShortFemale;
		}

		public override string ToString()
		{
			if (title.NullOrEmpty())
			{
				return "(NullTitleBackstory)";
			}
			return "(" + title + ")";
		}

		public override int GetHashCode()
		{
			return identifier.GetHashCode();
		}
	}
}
