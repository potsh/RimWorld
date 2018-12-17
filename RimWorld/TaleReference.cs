using Verse;

namespace RimWorld
{
	public class TaleReference : IExposable
	{
		private Tale tale;

		private int seed;

		public static TaleReference Taleless => new TaleReference(null);

		public TaleReference()
		{
		}

		public TaleReference(Tale tale)
		{
			this.tale = tale;
			seed = Rand.Range(0, 2147483647);
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref seed, "seed", 0);
			Scribe_References.Look(ref tale, "tale");
		}

		public void ReferenceDestroyed()
		{
			if (tale != null)
			{
				tale.Notify_ReferenceDestroyed();
				tale = null;
			}
		}

		public string GenerateText(TextGenerationPurpose purpose, RulePackDef extraInclude)
		{
			return TaleTextGenerator.GenerateTextFromTale(purpose, tale, seed, extraInclude);
		}

		public override string ToString()
		{
			return "TaleReference(tale=" + ((tale != null) ? tale.ToString() : "null") + ", seed=" + seed + ")";
		}
	}
}
