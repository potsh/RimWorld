using Verse;

namespace RimWorld
{
	public class CompTargetEffect_BrainDamageChance : CompTargetEffect
	{
		protected CompProperties_TargetEffect_BrainDamageChance PropsBrainDamageChance => (CompProperties_TargetEffect_BrainDamageChance)props;

		public override void DoEffectOn(Pawn user, Thing target)
		{
			Pawn pawn = (Pawn)target;
			if (!pawn.Dead && Rand.Value <= PropsBrainDamageChance.brainDamageChance)
			{
				BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
				if (brain != null)
				{
					int num = Rand.RangeInclusive(1, 5);
					Pawn pawn2 = pawn;
					DamageDef flame = DamageDefOf.Flame;
					float amount = (float)num;
					BodyPartRecord hitPart = brain;
					pawn2.TakeDamage(new DamageInfo(flame, amount, 0f, -1f, user, hitPart, parent.def));
				}
			}
		}
	}
}
