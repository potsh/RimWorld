using RimWorld;

namespace Verse
{
	public class Verb_Shoot : Verb_LaunchProjectile
	{
		protected override int ShotsPerBurst => verbProps.burstShotCount;

		public override void WarmupComplete()
		{
			base.WarmupComplete();
			Pawn pawn = currentTarget.Thing as Pawn;
			if (pawn != null && !pawn.Downed && base.CasterIsPawn && base.CasterPawn.skills != null)
			{
				float num = (!pawn.HostileTo(caster)) ? 20f : 170f;
				float num2 = verbProps.AdjustedFullCycleTime(this, base.CasterPawn);
				base.CasterPawn.skills.Learn(SkillDefOf.Shooting, num * num2);
			}
		}

		protected override bool TryCastShot()
		{
			bool flag = base.TryCastShot();
			if (flag && base.CasterIsPawn)
			{
				base.CasterPawn.records.Increment(RecordDefOf.ShotsFired);
			}
			return flag;
		}
	}
}
