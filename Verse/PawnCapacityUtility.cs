using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public static class PawnCapacityUtility
	{
		public abstract class CapacityImpactor
		{
			public virtual bool IsDirect => true;

			public abstract string Readable(Pawn pawn);
		}

		public class CapacityImpactorBodyPartHealth : CapacityImpactor
		{
			public BodyPartRecord bodyPart;

			public override string Readable(Pawn pawn)
			{
				return $"{bodyPart.LabelCap}: {pawn.health.hediffSet.GetPartHealth(bodyPart)} / {bodyPart.def.GetMaxHealth(pawn)}";
			}
		}

		public class CapacityImpactorCapacity : CapacityImpactor
		{
			public PawnCapacityDef capacity;

			public override bool IsDirect => false;

			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}: {1}%", capacity.LabelCap, (pawn.health.capacities.GetLevel(capacity) * 100f).ToString("F0"));
			}
		}

		public class CapacityImpactorHediff : CapacityImpactor
		{
			public Hediff hediff;

			public override string Readable(Pawn pawn)
			{
				return $"{hediff.LabelCap}";
			}
		}

		public class CapacityImpactorPain : CapacityImpactor
		{
			public override bool IsDirect => false;

			public override string Readable(Pawn pawn)
			{
				return string.Format("{0}: {1}%", "Pain".Translate(), (pawn.health.hediffSet.PainTotal * 100f).ToString("F0"));
			}
		}

		public static bool BodyCanEverDoCapacity(BodyDef bodyDef, PawnCapacityDef capacity)
		{
			return capacity.Worker.CanHaveCapacity(bodyDef);
		}

		public static float CalculateCapacityLevel(HediffSet diffSet, PawnCapacityDef capacity, List<CapacityImpactor> impactors = null)
		{
			if (capacity.zeroIfCannotBeAwake && !diffSet.pawn.health.capacities.CanBeAwake)
			{
				impactors?.Add(new CapacityImpactorCapacity
				{
					capacity = PawnCapacityDefOf.Consciousness
				});
				return 0f;
			}
			float num = capacity.Worker.CalculateCapacityLevel(diffSet, impactors);
			if (num > 0f)
			{
				float num2 = 99999f;
				float num3 = 1f;
				for (int i = 0; i < diffSet.hediffs.Count; i++)
				{
					Hediff hediff = diffSet.hediffs[i];
					List<PawnCapacityModifier> capMods = hediff.CapMods;
					if (capMods != null)
					{
						for (int j = 0; j < capMods.Count; j++)
						{
							PawnCapacityModifier pawnCapacityModifier = capMods[j];
							if (pawnCapacityModifier.capacity == capacity)
							{
								num += pawnCapacityModifier.offset;
								num3 *= pawnCapacityModifier.postFactor;
								if (pawnCapacityModifier.setMax < num2)
								{
									num2 = pawnCapacityModifier.setMax;
								}
								impactors?.Add(new CapacityImpactorHediff
								{
									hediff = hediff
								});
							}
						}
					}
				}
				num *= num3;
				num = Mathf.Min(num, num2);
			}
			num = Mathf.Max(num, capacity.minValue);
			return GenMath.RoundedHundredth(num);
		}

		public static float CalculatePartEfficiency(HediffSet diffSet, BodyPartRecord part, bool ignoreAddedParts = false, List<CapacityImpactor> impactors = null)
		{
			BodyPartRecord rec;
			for (rec = part.parent; rec != null; rec = rec.parent)
			{
				if (diffSet.HasDirectlyAddedPartFor(rec))
				{
					Hediff_AddedPart hediff_AddedPart = (from x in diffSet.GetHediffs<Hediff_AddedPart>()
					where x.Part == rec
					select x).First();
					impactors?.Add(new CapacityImpactorHediff
					{
						hediff = hediff_AddedPart
					});
					return hediff_AddedPart.def.addedPartProps.partEfficiency;
				}
			}
			if (part.parent != null && diffSet.PartIsMissing(part.parent))
			{
				return 0f;
			}
			float num = 1f;
			if (!ignoreAddedParts)
			{
				for (int i = 0; i < diffSet.hediffs.Count; i++)
				{
					Hediff_AddedPart hediff_AddedPart2 = diffSet.hediffs[i] as Hediff_AddedPart;
					if (hediff_AddedPart2 != null && hediff_AddedPart2.Part == part)
					{
						num *= hediff_AddedPart2.def.addedPartProps.partEfficiency;
						if (hediff_AddedPart2.def.addedPartProps.partEfficiency != 1f)
						{
							impactors?.Add(new CapacityImpactorHediff
							{
								hediff = hediff_AddedPart2
							});
						}
					}
				}
			}
			float b = -1f;
			float num2 = 0f;
			bool flag = false;
			for (int j = 0; j < diffSet.hediffs.Count; j++)
			{
				if (diffSet.hediffs[j].Part == part && diffSet.hediffs[j].CurStage != null)
				{
					HediffStage curStage = diffSet.hediffs[j].CurStage;
					num2 += curStage.partEfficiencyOffset;
					flag |= curStage.partIgnoreMissingHP;
					if (curStage.partEfficiencyOffset != 0f && curStage.becomeVisible)
					{
						impactors?.Add(new CapacityImpactorHediff
						{
							hediff = diffSet.hediffs[j]
						});
					}
				}
			}
			if (!flag)
			{
				float num3 = diffSet.GetPartHealth(part) / part.def.GetMaxHealth(diffSet.pawn);
				if (num3 != 1f)
				{
					if (DamageWorker_AddInjury.ShouldReduceDamageToPreservePart(part))
					{
						num3 = Mathf.InverseLerp(0.1f, 1f, num3);
					}
					impactors?.Add(new CapacityImpactorBodyPartHealth
					{
						bodyPart = part
					});
					num *= num3;
				}
			}
			num += num2;
			if (num > 0.0001f)
			{
				num = Mathf.Max(num, b);
			}
			return Mathf.Max(num, 0f);
		}

		public static float CalculateImmediatePartEfficiencyAndRecord(HediffSet diffSet, BodyPartRecord part, List<CapacityImpactor> impactors = null)
		{
			if (diffSet.AncestorHasDirectlyAddedParts(part))
			{
				return 1f;
			}
			return CalculatePartEfficiency(diffSet, part, ignoreAddedParts: false, impactors);
		}

		public static float CalculateNaturalPartsAverageEfficiency(HediffSet diffSet, BodyPartGroupDef bodyPartGroup)
		{
			float num = 0f;
			int num2 = 0;
			IEnumerable<BodyPartRecord> enumerable = from x in diffSet.GetNotMissingParts()
			where x.groups.Contains(bodyPartGroup)
			select x;
			foreach (BodyPartRecord item in enumerable)
			{
				if (!diffSet.PartOrAnyAncestorHasDirectlyAddedParts(item))
				{
					num += CalculatePartEfficiency(diffSet, item);
				}
				num2++;
			}
			if (num2 == 0 || num < 0f)
			{
				return 0f;
			}
			return num / (float)num2;
		}

		public static float CalculateTagEfficiency(HediffSet diffSet, BodyPartTagDef tag, float maximum = float.MaxValue, FloatRange lerp = default(FloatRange), List<CapacityImpactor> impactors = null, float bestPartEfficiencySpecialWeight = -1f)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			float totalEfficiency = 0f;
			int partCount = 0;
			float bestPartEfficiency = 0f;
			List<CapacityImpactor> list = null;
			foreach (BodyPartRecord item in body.GetPartsWithTag(tag))
			{
				BodyPartRecord part = item;
				List<CapacityImpactor> impactors2 = list;
				float partEfficiency = CalculatePartEfficiency(diffSet, part, ignoreAddedParts: false, impactors2);
				if (impactors != null && partEfficiency != 1f && list == null)
				{
					list = new List<CapacityImpactor>();
					part = item;
					impactors2 = list;
					CalculatePartEfficiency(diffSet, part, ignoreAddedParts: false, impactors2);
				}
				totalEfficiency += partEfficiency;
				bestPartEfficiency = Mathf.Max(bestPartEfficiency, partEfficiency);
				partCount++;
			}
			if (partCount == 0)
			{
				return 1f;
			}
			float weightedEfficiency = (!(bestPartEfficiencySpecialWeight >= 0f) || partCount < 2) ? (totalEfficiency / (float)partCount) : (bestPartEfficiency * bestPartEfficiencySpecialWeight + (totalEfficiency - bestPartEfficiency) / (float)(partCount - 1) * (1f - bestPartEfficiencySpecialWeight));
			float num6 = weightedEfficiency;
			if (lerp != default(FloatRange))
			{
				num6 = lerp.LerpThroughRange(num6);
			}
			num6 = Mathf.Min(num6, maximum);
			if (impactors != null && list != null && (maximum != 1f || weightedEfficiency <= 1f || num6 == 1f))
			{
				impactors.AddRange(list);
			}
			return num6;
		}

		public static float CalculateLimbEfficiency(HediffSet diffSet, BodyPartTagDef limbCoreTag, BodyPartTagDef limbSegmentTag, BodyPartTagDef limbDigitTag, float appendageWeight, out float functionalPercentage, List<CapacityImpactor> impactors)
		{
			BodyDef body = diffSet.pawn.RaceProps.body;
			float totalEfficiency = 0f;
			int partCount = 0;
			int functionalPartCount = 0;
			foreach (BodyPartRecord item in body.GetPartsWithTag(limbCoreTag))
			{
				float partEfficiency = CalculateImmediatePartEfficiencyAndRecord(diffSet, item, impactors);
				foreach (BodyPartRecord connectedPart in item.GetConnectedParts(limbSegmentTag))
				{
					partEfficiency *= CalculateImmediatePartEfficiencyAndRecord(diffSet, connectedPart, impactors);
				}
				if (item.HasChildParts(limbDigitTag))
				{
					partEfficiency = Mathf.Lerp(partEfficiency, partEfficiency * item.GetChildParts(limbDigitTag).Average((BodyPartRecord digitPart) => CalculateImmediatePartEfficiencyAndRecord(diffSet, digitPart, impactors)), appendageWeight);
				}
				totalEfficiency += partEfficiency;
				partCount++;
				if (partEfficiency > 0f)
				{
					functionalPartCount++;
				}
			}
			if (partCount == 0)
			{
				functionalPercentage = 0f;
				return 0f;
			}
			functionalPercentage = (float)functionalPartCount / (float)partCount;
			return totalEfficiency / (float)partCount;
		}
	}
}
