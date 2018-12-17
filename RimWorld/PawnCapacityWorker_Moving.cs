using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_Moving : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			float functionalPercentage = 0f;
			float num = PawnCapacityUtility.CalculateLimbEfficiency(diffSet, BodyPartTagDefOf.MovingLimbCore, BodyPartTagDefOf.MovingLimbSegment, BodyPartTagDefOf.MovingLimbDigit, 0.4f, out functionalPercentage, impactors);
			if (functionalPercentage < 0.4999f)
			{
				return 0f;
			}
			float num2 = num;
			BodyPartTagDef pelvis = BodyPartTagDefOf.Pelvis;
			num = num2 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, pelvis, 3.40282347E+38f, default(FloatRange), impactors);
			float num3 = num;
			pelvis = BodyPartTagDefOf.Spine;
			num = num3 * PawnCapacityUtility.CalculateTagEfficiency(diffSet, pelvis, 3.40282347E+38f, default(FloatRange), impactors);
			num = Mathf.Lerp(num, num * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Breathing, impactors), 0.2f);
			num = Mathf.Lerp(num, num * CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.BloodPumping, impactors), 0.2f);
			return num * Mathf.Min(CalculateCapacityAndRecord(diffSet, PawnCapacityDefOf.Consciousness, impactors), 1f);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return body.HasPartWithTag(BodyPartTagDefOf.MovingLimbCore);
		}
	}
}
