using UnityEngine;

namespace Verse
{
	public class HediffComp_GetsPermanent : HediffComp
	{
		public float permanentDamageThreshold = 9999f;

		public bool isPermanentInt;

		private float painFactor = 1f;

		private const float NonActivePermanentDamageThresholdValue = 9999f;

		public HediffCompProperties_GetsPermanent Props => (HediffCompProperties_GetsPermanent)props;

		public bool IsPermanent
		{
			get
			{
				return isPermanentInt;
			}
			set
			{
				if (value != isPermanentInt)
				{
					isPermanentInt = value;
					if (isPermanentInt)
					{
						painFactor = Mathf.Max(0f, Rand.ByCurve(HealthTuning.PermanentInjuryPainFactorRandomCurve));
						permanentDamageThreshold = 9999f;
					}
				}
			}
		}

		public float PainFactor => painFactor;

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref isPermanentInt, "isPermanent", defaultValue: false);
			Scribe_Values.Look(ref permanentDamageThreshold, "permanentDamageThreshold", 9999f);
			Scribe_Values.Look(ref painFactor, "painFactor", 1f);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				BackCompatibility.HediffComp_GetsPermanentLoadingVars(this);
			}
		}

		public void PreFinalizeInjury()
		{
			if (!base.Pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(parent.Part))
			{
				float num = 0.02f * parent.Part.def.permanentInjuryChanceFactor * Props.becomePermanentChanceFactor;
				if (!parent.Part.def.delicate)
				{
					num *= HealthTuning.BecomePermanentChanceFactorBySeverityCurve.Evaluate(parent.Severity);
				}
				if (Rand.Chance(num))
				{
					if (parent.Part.def.delicate)
					{
						IsPermanent = true;
					}
					else
					{
						permanentDamageThreshold = Rand.Range(1f, parent.Severity / 2f);
					}
				}
			}
		}

		public override void CompPostInjuryHeal(float amount)
		{
			if (!(permanentDamageThreshold >= 9999f) && !IsPermanent && parent.Severity <= permanentDamageThreshold && parent.Severity >= permanentDamageThreshold - amount)
			{
				parent.Severity = permanentDamageThreshold;
				IsPermanent = true;
				base.Pawn.health.Notify_HediffChanged(parent);
			}
		}

		public override string CompDebugString()
		{
			return "isPermanent: " + isPermanentInt + "\npermanentDamageThreshold: " + permanentDamageThreshold + "\npainFactor: " + painFactor;
		}
	}
}
