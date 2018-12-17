using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class StatWorker_MeleeAverageDPS : StatWorker
	{
		public override bool ShouldShowFor(StatRequest req)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return false;
			}
			if (!thingDef.IsWeapon && !thingDef.isTechHediff)
			{
				return false;
			}
			GetVerbsAndTools(thingDef, out List<VerbProperties> verbs, out List<Tool> tools);
			if (!tools.NullOrEmpty())
			{
				return true;
			}
			if (verbs != null)
			{
				for (int i = 0; i < verbs.Count; i++)
				{
					if (verbs[i].IsMeleeAttack)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return 0f;
			}
			GetVerbsAndTools(thingDef, out List<VerbProperties> verbs, out List<Tool> tools);
			Pawn attacker = GetCurrentWeaponUser(req.Thing);
			float num = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeDamageAmount(x.tool, attacker, req.Thing, null));
			float num2 = (from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x).AverageWeighted((VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedMeleeSelectionWeight(x.tool, attacker, req.Thing, null, comesFromPawnNativeVerbs: false), (VerbUtility.VerbPropertiesWithSource x) => x.verbProps.AdjustedCooldown(x.tool, attacker, req.Thing));
			if (num2 == 0f)
			{
				return 0f;
			}
			return num / num2;
		}

		public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
		{
			ThingDef thingDef = req.Def as ThingDef;
			if (thingDef == null)
			{
				return null;
			}
			GetVerbsAndTools(thingDef, out List<VerbProperties> verbs, out List<Tool> tools);
			Pawn currentWeaponUser = GetCurrentWeaponUser(req.Thing);
			IEnumerable<VerbUtility.VerbPropertiesWithSource> enumerable = from x in VerbUtility.GetAllVerbProperties(verbs, tools)
			where x.verbProps.IsMeleeAttack
			select x;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (VerbUtility.VerbPropertiesWithSource item in enumerable)
			{
				VerbUtility.VerbPropertiesWithSource current = item;
				float num = current.verbProps.AdjustedMeleeDamageAmount(current.tool, currentWeaponUser, req.Thing, null);
				float num2 = current.verbProps.AdjustedCooldown(current.tool, currentWeaponUser, req.Thing);
				if (current.tool != null)
				{
					stringBuilder.AppendLine(string.Format("  {0}: {1} ({2})", "Tool".Translate(), current.tool.LabelCap, current.ToolCapacity.defName));
				}
				else
				{
					stringBuilder.AppendLine(string.Format("  {0}:", "StatsReport_NonToolAttack".Translate()));
				}
				stringBuilder.AppendLine(string.Format("    {0} {1}", num.ToString("F1"), "DamageLower".Translate()));
				stringBuilder.AppendLine(string.Format("    {0} {1}", num2.ToString("F2"), "SecondsPerAttackLower".Translate()));
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString();
		}

		public static Pawn GetCurrentWeaponUser(Thing weapon)
		{
			if (weapon == null)
			{
				return null;
			}
			Pawn_EquipmentTracker pawn_EquipmentTracker = weapon.ParentHolder as Pawn_EquipmentTracker;
			if (pawn_EquipmentTracker != null)
			{
				return pawn_EquipmentTracker.pawn;
			}
			return (weapon.ParentHolder as Pawn_ApparelTracker)?.pawn;
		}

		private void GetVerbsAndTools(ThingDef def, out List<VerbProperties> verbs, out List<Tool> tools)
		{
			if (def.isTechHediff)
			{
				HediffDef hediffDef = FindTechHediffHediff(def);
				if (hediffDef == null)
				{
					verbs = null;
					tools = null;
				}
				else
				{
					HediffCompProperties_VerbGiver hediffCompProperties_VerbGiver = hediffDef.CompProps<HediffCompProperties_VerbGiver>();
					if (hediffCompProperties_VerbGiver == null)
					{
						verbs = null;
						tools = null;
					}
					else
					{
						verbs = hediffCompProperties_VerbGiver.verbs;
						tools = hediffCompProperties_VerbGiver.tools;
					}
				}
			}
			else
			{
				verbs = def.Verbs;
				tools = def.tools;
			}
		}

		private HediffDef FindTechHediffHediff(ThingDef techHediff)
		{
			List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
			for (int i = 0; i < allDefsListForReading.Count; i++)
			{
				if (allDefsListForReading[i].addsHediff != null && allDefsListForReading[i].IsIngredient(techHediff))
				{
					return allDefsListForReading[i].addsHediff;
				}
			}
			return null;
		}
	}
}
