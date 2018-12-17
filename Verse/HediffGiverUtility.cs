using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public static class HediffGiverUtility
	{
		public static bool TryApply(Pawn pawn, HediffDef hediff, List<BodyPartDef> partsToAffect, bool canAffectAnyLivePart = false, int countToAffect = 1, List<Hediff> outAddedHediffs = null)
		{
			if (canAffectAnyLivePart || partsToAffect != null)
			{
				bool result = false;
				for (int i = 0; i < countToAffect; i++)
				{
					IEnumerable<BodyPartRecord> source = pawn.health.hediffSet.GetNotMissingParts();
					if (partsToAffect != null)
					{
						source = from p in source
						where partsToAffect.Contains(p.def)
						select p;
					}
					if (canAffectAnyLivePart)
					{
						source = from p in source
						where p.def.alive
						select p;
					}
					source = from p in source
					where !pawn.health.hediffSet.HasHediff(hediff, p) && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(p)
					select p;
					if (!source.Any())
					{
						break;
					}
					BodyPartRecord partRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					Hediff hediff2 = HediffMaker.MakeHediff(hediff, pawn, partRecord);
					pawn.health.AddHediff(hediff2);
					outAddedHediffs?.Add(hediff2);
					result = true;
				}
				return result;
			}
			if (!pawn.health.hediffSet.HasHediff(hediff))
			{
				Hediff hediff3 = HediffMaker.MakeHediff(hediff, pawn);
				pawn.health.AddHediff(hediff3);
				outAddedHediffs?.Add(hediff3);
				return true;
			}
			return false;
		}
	}
}
