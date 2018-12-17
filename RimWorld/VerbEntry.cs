using Verse;

namespace RimWorld
{
	public struct VerbEntry
	{
		public Verb verb;

		private float cachedSelectionWeight;

		public bool IsMeleeAttack => verb.IsMeleeAttack;

		public VerbEntry(Verb verb, Pawn pawn)
		{
			this.verb = verb;
			cachedSelectionWeight = verb.verbProps.AdjustedMeleeSelectionWeight(verb, pawn);
		}

		public float GetSelectionWeight(Thing target)
		{
			if (!verb.IsUsableOn(target))
			{
				return 0f;
			}
			return cachedSelectionWeight;
		}

		public override string ToString()
		{
			return verb.ToString() + " - " + cachedSelectionWeight;
		}
	}
}
