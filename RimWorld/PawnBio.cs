using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	[CaseInsensitiveXMLParsing]
	public class PawnBio
	{
		public GenderPossibility gender;

		public NameTriple name;

		public Backstory childhood;

		public Backstory adulthood;

		public bool pirateKing;

		public PawnBioType BioType
		{
			get
			{
				if (pirateKing)
				{
					return PawnBioType.PirateKing;
				}
				if (adulthood != null)
				{
					return PawnBioType.BackstoryInGame;
				}
				return PawnBioType.Undefined;
			}
		}

		public void PostLoad()
		{
			if (childhood != null)
			{
				childhood.PostLoad();
			}
			if (adulthood != null)
			{
				adulthood.PostLoad();
			}
		}

		public void ResolveReferences()
		{
			if (adulthood.spawnCategories.Count == 1 && adulthood.spawnCategories[0] == "Trader")
			{
				adulthood.spawnCategories.Add("Civil");
			}
			if (childhood != null)
			{
				childhood.ResolveReferences();
			}
			if (adulthood != null)
			{
				adulthood.ResolveReferences();
			}
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (childhood != null)
			{
				using (IEnumerator<string> enumerator = childhood.ConfigErrors(ignoreNoSpawnCategories: true).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						string error2 = enumerator.Current;
						yield return name + ", " + childhood.title + ": " + error2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (adulthood != null)
			{
				using (IEnumerator<string> enumerator2 = adulthood.ConfigErrors(ignoreNoSpawnCategories: false).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						string error = enumerator2.Current;
						yield return name + ", " + adulthood.title + ": " + error;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_01f4:
			/*Error near IL_01f5: Unexpected return in MoveNext()*/;
		}

		public override string ToString()
		{
			return "PawnBio(" + name + ")";
		}
	}
}
