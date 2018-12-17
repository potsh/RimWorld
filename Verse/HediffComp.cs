using RimWorld;

namespace Verse
{
	public class HediffComp
	{
		public HediffWithComps parent;

		public HediffCompProperties props;

		public Pawn Pawn => parent.pawn;

		public HediffDef Def => parent.def;

		public virtual string CompLabelInBracketsExtra => null;

		public virtual string CompTipStringExtra => null;

		public virtual TextureAndColor CompStateIcon => TextureAndColor.None;

		public virtual bool CompShouldRemove => false;

		public virtual void CompPostMake()
		{
		}

		public virtual void CompPostTick(ref float severityAdjustment)
		{
		}

		public virtual void CompExposeData()
		{
		}

		public virtual void CompPostPostAdd(DamageInfo? dinfo)
		{
		}

		public virtual void CompPostPostRemoved()
		{
		}

		public virtual void CompPostMerged(Hediff other)
		{
		}

		public virtual bool CompDisallowVisible()
		{
			return false;
		}

		public virtual void CompModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
		}

		public virtual void CompPostInjuryHeal(float amount)
		{
		}

		public virtual void CompTended(float quality, int batchPosition = 0)
		{
		}

		public virtual void Notify_PawnDied()
		{
		}

		public virtual string CompDebugString()
		{
			return null;
		}
	}
}