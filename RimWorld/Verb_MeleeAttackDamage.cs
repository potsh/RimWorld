using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Verb_MeleeAttackDamage : Verb_MeleeAttack
	{
		private const float MeleeDamageRandomFactorMin = 0.8f;

		private const float MeleeDamageRandomFactorMax = 1.2f;

		private IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
		{
			float damAmount2 = verbProps.AdjustedMeleeDamageAmount(this, base.CasterPawn);
			float armorPenetration = verbProps.AdjustedArmorPenetration(this, base.CasterPawn);
			DamageDef damDef = verbProps.meleeDamageDef;
			BodyPartGroupDef bodyPartGroupDef = null;
			HediffDef hediffDef = null;
			damAmount2 = Rand.Range(damAmount2 * 0.8f, damAmount2 * 1.2f);
			if (base.CasterIsPawn)
			{
				bodyPartGroupDef = verbProps.AdjustedLinkedBodyPartsGroup(tool);
				if (damAmount2 >= 1f)
				{
					if (base.HediffCompSource != null)
					{
						hediffDef = base.HediffCompSource.Def;
					}
				}
				else
				{
					damAmount2 = 1f;
					damDef = DamageDefOf.Blunt;
				}
			}
			ThingDef source = (base.EquipmentSource == null) ? base.CasterPawn.def : base.EquipmentSource.def;
			Vector3 direction = (target.Thing.Position - base.CasterPawn.Position).ToVector3();
			DamageDef def = damDef;
			float amount = damAmount2;
			float armorPenetration2 = armorPenetration;
			Thing caster = base.caster;
			DamageInfo mainDinfo = new DamageInfo(def, amount, armorPenetration2, -1f, caster, null, source);
			mainDinfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			mainDinfo.SetWeaponBodyPartGroup(bodyPartGroupDef);
			mainDinfo.SetWeaponHediff(hediffDef);
			mainDinfo.SetAngle(direction);
			yield return mainDinfo;
			/*Error: Unable to find new state assignment for yield return*/;
		}

		protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
		{
			DamageWorker.DamageResult result = new DamageWorker.DamageResult();
			foreach (DamageInfo item in DamageInfosToApply(target))
			{
				if (target.ThingDestroyed)
				{
					return result;
				}
				result = target.Thing.TakeDamage(item);
			}
			return result;
		}
	}
}
