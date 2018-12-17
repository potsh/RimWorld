using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public static class PawnBioAndNameGenerator
	{
		private const float MinAgeForAdulthood = 20f;

		private const float SolidBioChance = 0.25f;

		private const float SolidNameChance = 0.5f;

		private const float TryPreferredNameChance_Bio = 0.5f;

		private const float TryPreferredNameChance_Name = 0.5f;

		private const float ShuffledNicknameChance = 0.15f;

		private static List<Backstory> tmpBackstories = new List<Backstory>();

		[CompilerGenerated]
		private static Func<Backstory, float> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<PawnBio, float> _003C_003Ef__mg_0024cache1;

		public static void GiveAppropriateBioAndNameTo(Pawn pawn, string requiredLastName, FactionDef factionType)
		{
			List<string> backstoryCategoriesFor = GetBackstoryCategoriesFor(pawn, factionType);
			if ((!(Rand.Value < 0.25f) && !pawn.kindDef.factionLeader) || !TryGiveSolidBioTo(pawn, requiredLastName, backstoryCategoriesFor))
			{
				GiveShuffledBioTo(pawn, factionType, requiredLastName, backstoryCategoriesFor);
			}
		}

		private static void GiveShuffledBioTo(Pawn pawn, FactionDef factionType, string requiredLastName, List<string> backstoryCategories)
		{
			pawn.Name = GeneratePawnName(pawn, NameStyle.Full, requiredLastName);
			FillBackstorySlotShuffled(pawn, BackstorySlot.Childhood, ref pawn.story.childhood, backstoryCategories, factionType);
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				FillBackstorySlotShuffled(pawn, BackstorySlot.Adulthood, ref pawn.story.adulthood, backstoryCategories, factionType);
			}
		}

		private static void FillBackstorySlotShuffled(Pawn pawn, BackstorySlot slot, ref Backstory backstory, List<string> backstoryCategories, FactionDef factionType)
		{
			tmpBackstories.Clear();
			BackstoryDatabase.ShuffleableBackstoryList(slot, backstoryCategories, tmpBackstories);
			if (!tmpBackstories.TakeRandom(20).Where(delegate(Backstory bs)
			{
				if (slot == BackstorySlot.Adulthood && bs.requiredWorkTags.OverlapsWithOnAnyWorkType(pawn.story.childhood.workDisables))
				{
					return false;
				}
				return true;
			}).TryRandomElementByWeight(BackstorySelectionWeight, out backstory))
			{
				Log.Error("No shuffled " + slot + " found for " + pawn.ToStringSafe() + " of " + factionType.ToStringSafe() + ". Defaulting.");
				backstory = (from kvp in BackstoryDatabase.allBackstories
				where kvp.Value.slot == slot
				select kvp).RandomElement().Value;
			}
			tmpBackstories.Clear();
		}

		private static bool TryGiveSolidBioTo(Pawn pawn, string requiredLastName, List<string> backstoryCategories)
		{
			PawnBio pawnBio = TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName);
			if (pawnBio == null)
			{
				return false;
			}
			if (pawnBio.name.First == "Tynan" && pawnBio.name.Last == "Sylvester" && Rand.Value < 0.5f)
			{
				pawnBio = TryGetRandomUnusedSolidBioFor(backstoryCategories, pawn.kindDef, pawn.gender, requiredLastName);
			}
			if (pawnBio == null)
			{
				return false;
			}
			pawn.Name = pawnBio.name;
			pawn.story.childhood = pawnBio.childhood;
			if (pawn.ageTracker.AgeBiologicalYearsFloat >= 20f)
			{
				pawn.story.adulthood = pawnBio.adulthood;
			}
			return true;
		}

		private static PawnBio TryGetRandomUnusedSolidBioFor(List<string> backstoryCategories, PawnKindDef kind, Gender gender, string requiredLastName)
		{
			NameTriple prefName = null;
			if (Rand.Value < 0.5f)
			{
				prefName = Prefs.RandomPreferredName();
				if (prefName != null && (prefName.UsedThisGame || (requiredLastName != null && prefName.Last != requiredLastName)))
				{
					prefName = null;
				}
			}
			PawnBio result;
			while (true)
			{
				result = null;
				if (SolidBioDatabase.allBios.TakeRandom(20).Where(delegate(PawnBio bio)
				{
					if (bio.gender != GenderPossibility.Either)
					{
						if (gender == Gender.Male && bio.gender != 0)
						{
							return false;
						}
						if (gender == Gender.Female && bio.gender != GenderPossibility.Female)
						{
							return false;
						}
					}
					if (!requiredLastName.NullOrEmpty() && bio.name.Last != requiredLastName)
					{
						return false;
					}
					if (prefName != null && !bio.name.Equals(prefName))
					{
						return false;
					}
					if (kind.factionLeader && !bio.pirateKing)
					{
						return false;
					}
					bool flag = false;
					for (int i = 0; i < bio.adulthood.spawnCategories.Count; i++)
					{
						if (backstoryCategories.Contains(bio.adulthood.spawnCategories[i]))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						return false;
					}
					if (bio.name.UsedThisGame)
					{
						return false;
					}
					return true;
				}).TryRandomElementByWeight(BioSelectionWeight, out result) || prefName == null)
				{
					break;
				}
				prefName = null;
			}
			return result;
		}

		public static NameTriple TryGetRandomUnusedSolidName(Gender gender, string requiredLastName = null)
		{
			NameTriple nameTriple = null;
			if (Rand.Value < 0.5f)
			{
				nameTriple = Prefs.RandomPreferredName();
				if (nameTriple != null && (nameTriple.UsedThisGame || (requiredLastName != null && nameTriple.Last != requiredLastName)))
				{
					nameTriple = null;
				}
			}
			List<NameTriple> listForGender = PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Either);
			List<NameTriple> list = (gender != Gender.Male) ? PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Female) : PawnNameDatabaseSolid.GetListForGender(GenderPossibility.Male);
			float num = ((float)listForGender.Count + 0.1f) / ((float)(listForGender.Count + list.Count) + 0.1f);
			List<NameTriple> list2 = (!(Rand.Value < num)) ? list : listForGender;
			if (list2.Count == 0)
			{
				Log.Error("Empty solid pawn name list for gender: " + gender + ".");
				return null;
			}
			if (nameTriple != null && list2.Contains(nameTriple))
			{
				return nameTriple;
			}
			list2.Shuffle();
			return list2.Where(delegate(NameTriple name)
			{
				if (requiredLastName != null && name.Last != requiredLastName)
				{
					return false;
				}
				if (name.UsedThisGame)
				{
					return false;
				}
				return true;
			}).FirstOrDefault();
		}

		private static List<string> GetBackstoryCategoriesFor(Pawn pawn, FactionDef faction)
		{
			List<string> list = new List<string>();
			if (pawn.kindDef.backstoryCategories != null)
			{
				list.AddRange(pawn.kindDef.backstoryCategories);
			}
			if (faction != null && faction.backstoryCategories != null)
			{
				for (int i = 0; i < faction.backstoryCategories.Count; i++)
				{
					string item = faction.backstoryCategories[i];
					if (!list.Contains(item))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		public static Name GeneratePawnName(Pawn pawn, NameStyle style = NameStyle.Full, string forcedLastName = null)
		{
			switch (style)
			{
			case NameStyle.Full:
			{
				RulePackDef nameGenerator = pawn.RaceProps.GetNameGenerator(pawn.gender);
				if (nameGenerator != null)
				{
					string name = NameGenerator.GenerateName(nameGenerator, (string x) => !new NameSingle(x).UsedThisGame);
					return new NameSingle(name);
				}
				if (pawn.Faction != null && pawn.Faction.def.pawnNameMaker != null)
				{
					string rawName = NameGenerator.GenerateName(pawn.Faction.def.pawnNameMaker, delegate(string x)
					{
						NameTriple nameTriple4 = NameTriple.FromString(x);
						nameTriple4.ResolveMissingPieces(forcedLastName);
						return !nameTriple4.UsedThisGame;
					});
					NameTriple nameTriple = NameTriple.FromString(rawName);
					nameTriple.CapitalizeNick();
					nameTriple.ResolveMissingPieces(forcedLastName);
					return nameTriple;
				}
				if (pawn.RaceProps.nameCategory != 0)
				{
					if (Rand.Value < 0.5f)
					{
						NameTriple nameTriple2 = TryGetRandomUnusedSolidName(pawn.gender, forcedLastName);
						if (nameTriple2 != null)
						{
							return nameTriple2;
						}
					}
					return GeneratePawnName_Shuffled(pawn, forcedLastName);
				}
				Log.Error("No name making method for " + pawn);
				NameTriple nameTriple3 = NameTriple.FromString(pawn.def.label);
				nameTriple3.ResolveMissingPieces();
				return nameTriple3;
			}
			case NameStyle.Numeric:
			{
				int num = 1;
				string text;
				while (true)
				{
					text = pawn.KindLabel + " " + num.ToString();
					if (!NameUseChecker.NameSingleIsUsed(text))
					{
						break;
					}
					num++;
				}
				return new NameSingle(text, numerical: true);
			}
			default:
				throw new InvalidOperationException();
			}
		}

		private static NameTriple GeneratePawnName_Shuffled(Pawn pawn, string forcedLastName = null)
		{
			PawnNameCategory pawnNameCategory = pawn.RaceProps.nameCategory;
			if (pawnNameCategory == PawnNameCategory.NoName)
			{
				Log.Message("Can't create a name of type NoName. Defaulting to HumanStandard.");
				pawnNameCategory = PawnNameCategory.HumanStandard;
			}
			NameBank nameBank = PawnNameDatabaseShuffled.BankOf(pawnNameCategory);
			string name = nameBank.GetName(PawnNameSlot.First, pawn.gender);
			string text = (forcedLastName == null) ? nameBank.GetName(PawnNameSlot.Last) : forcedLastName;
			int num = 0;
			string nick;
			do
			{
				num++;
				if (Rand.Value < 0.15f)
				{
					Gender gender = pawn.gender;
					if (Rand.Value < 0.5f)
					{
						gender = Gender.None;
					}
					nick = nameBank.GetName(PawnNameSlot.Nick, gender);
				}
				else if (Rand.Value < 0.5f)
				{
					nick = name;
				}
				else
				{
					nick = text;
				}
			}
			while (num < 50 && NameUseChecker.AllPawnsNamesEverUsed.Any(delegate(Name x)
			{
				NameTriple nameTriple = x as NameTriple;
				return nameTriple != null && nameTriple.Nick == nick;
			}));
			return new NameTriple(name, nick, text);
		}

		private static float BackstorySelectionWeight(Backstory bs)
		{
			return SelectionWeightFactorFromWorkTagsDisabled(bs.workDisables);
		}

		private static float BioSelectionWeight(PawnBio bio)
		{
			return SelectionWeightFactorFromWorkTagsDisabled(bio.adulthood.workDisables | bio.childhood.workDisables);
		}

		private static float SelectionWeightFactorFromWorkTagsDisabled(WorkTags wt)
		{
			float num = 1f;
			if ((wt & WorkTags.ManualDumb) != 0)
			{
				num *= 0.4f;
			}
			if ((wt & WorkTags.ManualSkilled) != 0)
			{
				num *= 1f;
			}
			if ((wt & WorkTags.Violent) != 0)
			{
				num *= 0.5f;
			}
			if ((wt & WorkTags.Caring) != 0)
			{
				num *= 0.9f;
			}
			if ((wt & WorkTags.Social) != 0)
			{
				num *= 0.5f;
			}
			if ((wt & WorkTags.Intellectual) != 0)
			{
				num *= 0.35f;
			}
			if ((wt & WorkTags.Firefighting) != 0)
			{
				num *= 0.7f;
			}
			return num;
		}
	}
}
