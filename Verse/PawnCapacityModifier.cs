namespace Verse
{
	public class PawnCapacityModifier
	{
		public PawnCapacityDef capacity;

		public float offset;

		public float setMax = 999f;

		public float postFactor = 1f;

		public bool SetMaxDefined => setMax != 999f;
	}
}
