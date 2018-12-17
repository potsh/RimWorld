using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class Building_Turret : Building, IAttackTarget, IAttackTargetSearcher, ILoadReferenceable
	{
		protected StunHandler stunner;

		protected LocalTargetInfo forcedTarget = LocalTargetInfo.Invalid;

		private LocalTargetInfo lastAttackedTarget;

		private int lastAttackTargetTick;

		private const float SightRadiusTurret = 13.4f;

		Thing IAttackTarget.Thing
		{
			get
			{
				return this;
			}
		}

		Thing IAttackTargetSearcher.Thing
		{
			get
			{
				return this;
			}
		}

		public abstract LocalTargetInfo CurrentTarget
		{
			get;
		}

		public abstract Verb AttackVerb
		{
			get;
		}

		public LocalTargetInfo TargetCurrentlyAimingAt => CurrentTarget;

		public Verb CurrentEffectiveVerb => AttackVerb;

		public LocalTargetInfo LastAttackedTarget => lastAttackedTarget;

		public int LastAttackTargetTick => lastAttackTargetTick;

		public Building_Turret()
		{
			stunner = new StunHandler(this);
		}

		public override void Tick()
		{
			base.Tick();
			if (forcedTarget.HasThing && (!forcedTarget.Thing.Spawned || !base.Spawned || forcedTarget.Thing.Map != base.Map))
			{
				forcedTarget = LocalTargetInfo.Invalid;
			}
			stunner.StunHandlerTick();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_TargetInfo.Look(ref forcedTarget, "forcedTarget");
			Scribe_TargetInfo.Look(ref lastAttackedTarget, "lastAttackedTarget");
			Scribe_Deep.Look(ref stunner, "stunner", this);
			Scribe_Values.Look(ref lastAttackTargetTick, "lastAttackTargetTick", 0);
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			base.PreApplyDamage(ref dinfo, out absorbed);
			if (!absorbed)
			{
				stunner.Notify_DamageApplied(dinfo, affectedByEMP: true);
				absorbed = false;
			}
		}

		public abstract void OrderAttack(LocalTargetInfo targ);

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			CompPowerTrader comp = GetComp<CompPowerTrader>();
			if (comp != null && !comp.PowerOn)
			{
				return true;
			}
			CompMannable comp2 = GetComp<CompMannable>();
			if (comp2 != null && !comp2.MannedNow)
			{
				return true;
			}
			return false;
		}

		protected void OnAttackedTarget(LocalTargetInfo target)
		{
			lastAttackTargetTick = Find.TickManager.TicksGame;
			lastAttackedTarget = target;
		}
	}
}
