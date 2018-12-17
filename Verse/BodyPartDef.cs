using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class BodyPartDef : Def
	{
		[MustTranslate]
		public string labelShort;

		public List<BodyPartTagDef> tags = new List<BodyPartTagDef>();

		public int hitPoints = 10;

		public float permanentInjuryChanceFactor = 1f;

		public float bleedRate = 1f;

		public float frostbiteVulnerability;

		private bool skinCovered;

		private bool solid;

		public bool alive = true;

		public bool delicate;

		public bool beautyRelated;

		public bool conceptual;

		public bool socketed;

		public ThingDef spawnThingOnRemoved;

		public bool pawnGeneratorCanAmputate;

		public bool canSuggestAmputation = true;

		public Dictionary<DamageDef, float> hitChanceFactors;

		public bool destroyableByDamage = true;

		public bool IsSolidInDefinition_Debug => solid;

		public bool IsSkinCoveredInDefinition_Debug => skinCovered;

		public string LabelShort => (!labelShort.NullOrEmpty()) ? labelShort : label;

		public string LabelShortCap => LabelShort.CapitalizeFirst();

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (frostbiteVulnerability > 10f)
			{
				yield return "frostbitePriority > max 10: " + frostbiteVulnerability;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (solid && bleedRate > 0f)
			{
				yield return "solid but bleedRate is not zero";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_014e:
			/*Error near IL_014f: Unexpected return in MoveNext()*/;
		}

		public bool IsSolid(BodyPartRecord part, List<Hediff> hediffs)
		{
			for (BodyPartRecord bodyPartRecord = part; bodyPartRecord != null; bodyPartRecord = bodyPartRecord.parent)
			{
				for (int i = 0; i < hediffs.Count; i++)
				{
					if (hediffs[i].Part == bodyPartRecord && hediffs[i] is Hediff_AddedPart)
					{
						return hediffs[i].def.addedPartProps.solid;
					}
				}
			}
			return solid;
		}

		public bool IsSkinCovered(BodyPartRecord part, HediffSet body)
		{
			if (body.PartOrAnyAncestorHasDirectlyAddedParts(part))
			{
				return false;
			}
			return skinCovered;
		}

		public float GetMaxHealth(Pawn pawn)
		{
			return (float)Mathf.CeilToInt((float)hitPoints * pawn.HealthScale);
		}

		public float GetHitChanceFactorFor(DamageDef damage)
		{
			if (conceptual)
			{
				return 0f;
			}
			if (hitChanceFactors == null)
			{
				return 1f;
			}
			if (hitChanceFactors.TryGetValue(damage, out float value))
			{
				return value;
			}
			return 1f;
		}
	}
}
