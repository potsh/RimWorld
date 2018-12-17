using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class PawnCapacityWorker_BloodFiltration : PawnCapacityWorker
	{
		public override float CalculateCapacityLevel(HediffSet diffSet, List<PawnCapacityUtility.CapacityImpactor> impactors = null)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			BodyPartTagDef bloodFiltrationKidney;
			if (body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney))
			{
				bloodFiltrationKidney = BodyPartTagDefOf.BloodFiltrationKidney;
				float num = PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationKidney, 3.40282347E+38f, default(FloatRange), impactors);
				bloodFiltrationKidney = BodyPartTagDefOf.BloodFiltrationLiver;
				return num * PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationKidney, 3.40282347E+38f, default(FloatRange), impactors);
			}
			bloodFiltrationKidney = BodyPartTagDefOf.BloodFiltrationSource;
			return PawnCapacityUtility.CalculateTagEfficiency(diffSet, bloodFiltrationKidney, 3.40282347E+38f, default(FloatRange), impactors);
		}

		public override bool CanHaveCapacity(BodyDef body)
		{
			return (body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationKidney) && body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationLiver)) || body.HasPartWithTag(BodyPartTagDefOf.BloodFiltrationSource);
		}
	}
}
