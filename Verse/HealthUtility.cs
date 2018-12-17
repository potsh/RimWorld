using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Verse
{
	public static class HealthUtility
	{
		public static readonly Color GoodConditionColor = new Color(0.6f, 0.8f, 0.65f);

		public static readonly Color DarkRedColor = new Color(0.73f, 0.02f, 0.02f);

		public static readonly Color ImpairedColor = new Color(0.9f, 0.7f, 0f);

		public static readonly Color SlightlyImpairedColor = new Color(0.9f, 0.9f, 0f);

		private static List<Hediff> tmpHediffs = new List<Hediff>();

		public static string GetGeneralConditionLabel(Pawn pawn, bool shortVersion = false)
		{
			if (pawn.health.Dead)
			{
				return "Dead".Translate();
			}
			if (!pawn.health.capacities.CanBeAwake)
			{
				return "Unconscious".Translate();
			}
			if (pawn.health.InPainShock)
			{
				return (!shortVersion || !"PainShockShort".CanTranslate()) ? "PainShock".Translate() : "PainShockShort".Translate();
			}
			if (pawn.Downed)
			{
				return "Incapacitated".Translate();
			}
			bool flag = false;
			for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
			{
				Hediff_Injury hediff_Injury = pawn.health.hediffSet.hediffs[i] as Hediff_Injury;
				if (hediff_Injury != null && !hediff_Injury.IsPermanent())
				{
					flag = true;
				}
			}
			if (flag)
			{
				return "Injured".Translate();
			}
			if (pawn.health.hediffSet.PainTotal > 0.3f)
			{
				return "InPain".Translate();
			}
			return "Healthy".Translate();
		}

		public static Pair<string, Color> GetPartConditionLabel(Pawn pawn, BodyPartRecord part)
		{
			float partHealth = pawn.health.hediffSet.GetPartHealth(part);
			float maxHealth = part.def.GetMaxHealth(pawn);
			float num = partHealth / maxHealth;
			string empty = string.Empty;
			Color white = Color.white;
			if (partHealth <= 0f)
			{
				Hediff_MissingPart hediff_MissingPart = null;
				List<Hediff_MissingPart> missingPartsCommonAncestors = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				for (int i = 0; i < missingPartsCommonAncestors.Count; i++)
				{
					if (missingPartsCommonAncestors[i].Part == part)
					{
						hediff_MissingPart = missingPartsCommonAncestors[i];
						break;
					}
				}
				if (hediff_MissingPart == null)
				{
					bool fresh = false;
					if (hediff_MissingPart != null && hediff_MissingPart.IsFreshNonSolidExtremity)
					{
						fresh = true;
					}
					bool solid = part.def.IsSolid(part, pawn.health.hediffSet.hediffs);
					empty = GetGeneralDestroyedPartLabel(part, fresh, solid);
					white = Color.gray;
				}
				else
				{
					empty = hediff_MissingPart.LabelCap;
					white = hediff_MissingPart.LabelColor;
				}
			}
			else if (num < 0.4f)
			{
				empty = "SeriouslyImpaired".Translate();
				white = DarkRedColor;
			}
			else if (num < 0.7f)
			{
				empty = "Impaired".Translate();
				white = ImpairedColor;
			}
			else if (num < 0.999f)
			{
				empty = "SlightlyImpaired".Translate();
				white = SlightlyImpairedColor;
			}
			else
			{
				empty = "GoodCondition".Translate();
				white = GoodConditionColor;
			}
			return new Pair<string, Color>(empty, white);
		}

		public static string GetGeneralDestroyedPartLabel(BodyPartRecord part, bool fresh, bool solid)
		{
			if (part.parent == null)
			{
				return "SeriouslyImpaired".Translate();
			}
			if (part.depth == BodyPartDepth.Inside || fresh)
			{
				if (solid)
				{
					return "ShatteredBodyPart".Translate();
				}
				return "DestroyedBodyPart".Translate();
			}
			return "MissingBodyPart".Translate();
		}

		private static IEnumerable<BodyPartRecord> HittablePartsViolence(HediffSet bodyModel)
		{
			return from x in bodyModel.GetNotMissingParts()
			where x.depth == BodyPartDepth.Outside || (x.depth == BodyPartDepth.Inside && x.def.IsSolid(x, bodyModel.hediffs))
			select x;
		}

		public static void GiveInjuriesOperationFailureMinor(Pawn p, BodyPartRecord part)
		{
			GiveRandomSurgeryInjuries(p, 20, part);
		}

		public static void GiveInjuriesOperationFailureCatastrophic(Pawn p, BodyPartRecord part)
		{
			GiveRandomSurgeryInjuries(p, 65, part);
		}

		public static void GiveInjuriesOperationFailureRidiculous(Pawn p)
		{
			GiveRandomSurgeryInjuries(p, 65, null);
		}

		public static void HealNonPermanentInjuriesAndRestoreLegs(Pawn p)
		{
			if (!p.Dead)
			{
				tmpHediffs.Clear();
				tmpHediffs.AddRange(p.health.hediffSet.hediffs);
				for (int i = 0; i < tmpHediffs.Count; i++)
				{
					Hediff_Injury hediff_Injury = tmpHediffs[i] as Hediff_Injury;
					if (hediff_Injury != null && !hediff_Injury.IsPermanent())
					{
						p.health.RemoveHediff(hediff_Injury);
					}
					else
					{
						Hediff_MissingPart hediff_MissingPart = tmpHediffs[i] as Hediff_MissingPart;
						if (hediff_MissingPart != null && hediff_MissingPart.Part.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && (hediff_MissingPart.Part.parent == null || p.health.hediffSet.GetNotMissingParts().Contains(hediff_MissingPart.Part.parent)))
						{
							p.health.RestorePart(hediff_MissingPart.Part);
						}
					}
				}
				tmpHediffs.Clear();
			}
		}

		private static void GiveRandomSurgeryInjuries(Pawn p, int totalDamage, BodyPartRecord operatedPart)
		{
			IEnumerable<BodyPartRecord> source = (operatedPart != null) ? (from x in p.health.hediffSet.GetNotMissingParts()
			where !x.def.conceptual
			select x into pa
			where pa == operatedPart || pa.parent == operatedPart || (operatedPart != null && operatedPart.parent == pa)
			select pa) : p.health.hediffSet.GetNotMissingParts().Where((BodyPartRecord x) => !x.def.conceptual);
			source = from x in source
			where GetMinHealthOfPartsWeWantToAvoidDestroying(x, p) >= 2f
			select x;
			BodyPartRecord brain = p.health.hediffSet.GetBrain();
			if (brain != null)
			{
				float maxBrainHealth = brain.def.GetMaxHealth(p);
				source = from x in source
				where x != brain || p.health.hediffSet.GetPartHealth(x) >= maxBrainHealth * 0.5f + 1f
				select x;
			}
			while (totalDamage > 0 && source.Any())
			{
				BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int num = Mathf.Max(3, GenMath.RoundRandom(partHealth * Rand.Range(0.5f, 1f)));
				float minHealthOfPartsWeWantToAvoidDestroying = GetMinHealthOfPartsWeWantToAvoidDestroying(bodyPartRecord, p);
				if (minHealthOfPartsWeWantToAvoidDestroying - (float)num < 1f)
				{
					num = Mathf.RoundToInt(minHealthOfPartsWeWantToAvoidDestroying - 1f);
				}
				if (bodyPartRecord == brain && partHealth - (float)num < brain.def.GetMaxHealth(p) * 0.5f)
				{
					num = Mathf.Max(Mathf.RoundToInt(partHealth - brain.def.GetMaxHealth(p) * 0.5f), 1);
				}
				if (num <= 0)
				{
					break;
				}
				DamageDef damageDef = Rand.Element(DamageDefOf.Cut, DamageDefOf.Scratch, DamageDefOf.Stab, DamageDefOf.Crush);
				Pawn pawn = p;
				DamageDef def = damageDef;
				float amount = (float)num;
				BodyPartRecord hitPart = bodyPartRecord;
				pawn.TakeDamage(new DamageInfo(def, amount, 0f, -1f, null, hitPart));
				totalDamage -= num;
			}
		}

		private static float GetMinHealthOfPartsWeWantToAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			float num = 999999f;
			while (part != null)
			{
				if (ShouldRandomSurgeryInjuriesAvoidDestroying(part, pawn))
				{
					num = Mathf.Min(num, pawn.health.hediffSet.GetPartHealth(part));
				}
				part = part.parent;
			}
			return num;
		}

		private static bool ShouldRandomSurgeryInjuriesAvoidDestroying(BodyPartRecord part, Pawn pawn)
		{
			if (part == pawn.RaceProps.body.corePart)
			{
				return true;
			}
			if (part.def.tags.Any((BodyPartTagDef x) => x.vital))
			{
				return true;
			}
			for (int i = 0; i < part.parts.Count; i++)
			{
				if (ShouldRandomSurgeryInjuriesAvoidDestroying(part.parts[i], pawn))
				{
					return true;
				}
			}
			return false;
		}

		public static void DamageUntilDowned(Pawn p, bool allowBleedingWounds = true)
		{
			if (!p.health.Downed)
			{
				HediffSet hediffSet = p.health.hediffSet;
				p.health.forceIncap = true;
				IEnumerable<BodyPartRecord> source = from x in HittablePartsViolence(hediffSet)
				where !p.health.hediffSet.hediffs.Any((Hediff y) => y.Part == x && y.CurStage != null && y.CurStage.partEfficiencyOffset < 0f)
				select x;
				int num = 0;
				while (num < 300 && !p.Downed && source.Any())
				{
					num++;
					BodyPartRecord bodyPartRecord = source.RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
					int num2 = Mathf.RoundToInt(hediffSet.GetPartHealth(bodyPartRecord)) - 3;
					if (num2 >= 8)
					{
						DamageDef damageDef = (bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : ((allowBleedingWounds || !(bodyPartRecord.def.bleedRate > 0f)) ? RandomViolenceDamageType() : DamageDefOf.Blunt);
						int num3 = Rand.RangeInclusive(Mathf.RoundToInt((float)num2 * 0.65f), num2);
						HediffDef hediffDefFromDamage = GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
						if (!p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num3))
						{
							DamageDef def = damageDef;
							float amount = (float)num3;
							float armorPenetration = 999f;
							BodyPartRecord hitPart = bodyPartRecord;
							DamageInfo dinfo = new DamageInfo(def, amount, armorPenetration, -1f, null, hitPart);
							dinfo.SetAllowDamagePropagation(val: false);
							p.TakeDamage(dinfo);
						}
					}
				}
				if (p.Dead)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine(p + " died during GiveInjuriesToForceDowned");
					for (int i = 0; i < p.health.hediffSet.hediffs.Count; i++)
					{
						stringBuilder.AppendLine("   -" + p.health.hediffSet.hediffs[i].ToString());
					}
					Log.Error(stringBuilder.ToString());
				}
				p.health.forceIncap = false;
			}
		}

		public static void DamageUntilDead(Pawn p)
		{
			HediffSet hediffSet = p.health.hediffSet;
			int num = 0;
			while (!p.Dead && num < 200 && HittablePartsViolence(hediffSet).Any())
			{
				num++;
				BodyPartRecord bodyPartRecord = HittablePartsViolence(hediffSet).RandomElementByWeight((BodyPartRecord x) => x.coverageAbs);
				int num2 = Rand.RangeInclusive(8, 25);
				DamageDef damageDef = (bodyPartRecord.depth != BodyPartDepth.Outside) ? DamageDefOf.Blunt : RandomViolenceDamageType();
				DamageDef def = damageDef;
				float amount = (float)num2;
				float armorPenetration = 999f;
				BodyPartRecord hitPart = bodyPartRecord;
				DamageInfo dinfo = new DamageInfo(def, amount, armorPenetration, -1f, null, hitPart);
				p.TakeDamage(dinfo);
			}
			if (!p.Dead)
			{
				Log.Error(p + " not killed during GiveInjuriesToKill");
			}
		}

		public static void DamageLegsUntilIncapableOfMoving(Pawn p, bool allowBleedingWounds = true)
		{
			int num = 0;
			p.health.forceIncap = true;
			while (p.health.capacities.CapableOf(PawnCapacityDefOf.Moving) && num < 300)
			{
				num++;
				IEnumerable<BodyPartRecord> source = from x in p.health.hediffSet.GetNotMissingParts()
				where x.def.tags.Contains(BodyPartTagDefOf.MovingLimbCore) && p.health.hediffSet.GetPartHealth(x) >= 2f
				select x;
				if (!source.Any())
				{
					break;
				}
				BodyPartRecord bodyPartRecord = source.RandomElement();
				float maxHealth = bodyPartRecord.def.GetMaxHealth(p);
				float partHealth = p.health.hediffSet.GetPartHealth(bodyPartRecord);
				int min = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.12f), 1, (int)partHealth - 1);
				int max = Mathf.Clamp(Mathf.RoundToInt(maxHealth * 0.27f), 1, (int)partHealth - 1);
				int num2 = Rand.RangeInclusive(min, max);
				DamageDef damageDef = (allowBleedingWounds || !(bodyPartRecord.def.bleedRate > 0f)) ? RandomViolenceDamageType() : DamageDefOf.Blunt;
				HediffDef hediffDefFromDamage = GetHediffDefFromDamage(damageDef, p, bodyPartRecord);
				if (p.health.WouldDieAfterAddingHediff(hediffDefFromDamage, bodyPartRecord, (float)num2))
				{
					break;
				}
				DamageDef def = damageDef;
				float amount = (float)num2;
				float armorPenetration = 999f;
				BodyPartRecord hitPart = bodyPartRecord;
				DamageInfo dinfo = new DamageInfo(def, amount, armorPenetration, -1f, null, hitPart);
				dinfo.SetAllowDamagePropagation(val: false);
				p.TakeDamage(dinfo);
			}
			p.health.forceIncap = false;
		}

		public static DamageDef RandomViolenceDamageType()
		{
			switch (Rand.RangeInclusive(0, 4))
			{
			case 0:
				return DamageDefOf.Bullet;
			case 1:
				return DamageDefOf.Blunt;
			case 2:
				return DamageDefOf.Stab;
			case 3:
				return DamageDefOf.Scratch;
			case 4:
				return DamageDefOf.Cut;
			default:
				return null;
			}
		}

		public static HediffDef GetHediffDefFromDamage(DamageDef dam, Pawn pawn, BodyPartRecord part)
		{
			HediffDef result = dam.hediff;
			if (part.def.IsSkinCovered(part, pawn.health.hediffSet) && dam.hediffSkin != null)
			{
				result = dam.hediffSkin;
			}
			if (part.def.IsSolid(part, pawn.health.hediffSet.hediffs) && dam.hediffSolid != null)
			{
				result = dam.hediffSolid;
			}
			return result;
		}

		public static bool TryAnesthetize(Pawn pawn)
		{
			if (!pawn.RaceProps.IsFlesh)
			{
				return false;
			}
			pawn.health.forceIncap = true;
			pawn.health.AddHediff(HediffDefOf.Anesthetic);
			pawn.health.forceIncap = false;
			return true;
		}

		public static void AdjustSeverity(Pawn pawn, HediffDef hdDef, float sevOffset)
		{
			if (sevOffset != 0f)
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(hdDef);
				if (firstHediffOfDef != null)
				{
					firstHediffOfDef.Severity += sevOffset;
				}
				else if (sevOffset > 0f)
				{
					firstHediffOfDef = HediffMaker.MakeHediff(hdDef, pawn);
					firstHediffOfDef.Severity = sevOffset;
					pawn.health.AddHediff(firstHediffOfDef);
				}
			}
		}

		public static BodyPartRemovalIntent PartRemovalIntent(Pawn pawn, BodyPartRecord part)
		{
			if (pawn.health.hediffSet.hediffs.Any((Hediff d) => d.Visible && d.Part == part && d.def.isBad))
			{
				return BodyPartRemovalIntent.Amputate;
			}
			return BodyPartRemovalIntent.Harvest;
		}

		public static int TicksUntilDeathDueToBloodLoss(Pawn pawn)
		{
			float bleedRateTotal = pawn.health.hediffSet.BleedRateTotal;
			if (bleedRateTotal < 0.0001f)
			{
				return 2147483647;
			}
			float num = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss)?.Severity ?? 0f;
			return (int)((1f - num) / bleedRateTotal * 60000f);
		}
	}
}
