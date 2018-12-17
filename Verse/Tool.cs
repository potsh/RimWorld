using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class Tool
	{
		[Unsaved]
		public string id;

		[MustTranslate]
		public string label;

		[Unsaved]
		[TranslationHandle]
		public string untranslatedLabel;

		public bool labelUsedInLogging = true;

		public List<ToolCapacityDef> capacities = new List<ToolCapacityDef>();

		public float power;

		public float armorPenetration = -1f;

		public float cooldownTime;

		public SurpriseAttackProps surpriseAttack;

		public HediffDef hediff;

		public float chanceFactor = 1f;

		public bool alwaysTreatAsWeapon;

		public BodyPartGroupDef linkedBodyPartsGroup;

		public bool ensureLinkedBodyPartsGroupAlwaysUsable;

		public string LabelCap => label.CapitalizeFirst();

		public IEnumerable<ManeuverDef> Maneuvers => from x in DefDatabase<ManeuverDef>.AllDefsListForReading
		where capacities.Contains(x.requiredCapacity)
		select x;

		public IEnumerable<VerbProperties> VerbsProperties => from x in Maneuvers
		select x.verb;

		public float AdjustedBaseMeleeDamageAmount(Thing ownerEquipment, DamageDef damageDef)
		{
			float num = power;
			if (ownerEquipment != null)
			{
				num *= ownerEquipment.GetStatValue(StatDefOf.MeleeWeapon_DamageMultiplier);
				if (ownerEquipment.Stuff != null && damageDef != null)
				{
					num *= ownerEquipment.Stuff.GetStatValueAbstract(damageDef.armorCategory.multStat);
				}
			}
			return num;
		}

		public float AdjustedCooldown(Thing ownerEquipment)
		{
			return cooldownTime * (ownerEquipment?.GetStatValue(StatDefOf.MeleeWeapon_CooldownMultiplier) ?? 1f);
		}

		public override string ToString()
		{
			return label;
		}

		public void PostLoad()
		{
			untranslatedLabel = label;
		}

		public IEnumerable<string> ConfigErrors()
		{
			if (id.NullOrEmpty())
			{
				yield return "tool has null id (power=" + power.ToString("0.##") + ")";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
