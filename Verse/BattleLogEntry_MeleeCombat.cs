using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_MeleeCombat : LogEntry_DamageResult
	{
		private RulePackDef ruleDef;

		private Pawn initiator;

		private Pawn recipientPawn;

		private ThingDef recipientThing;

		private ImplementOwnerTypeDef implementType;

		private ThingDef ownerEquipmentDef;

		private HediffDef ownerHediffDef;

		private string toolLabel;

		public bool alwaysShowInCompact;

		[TweakValue("LogFilter", 0f, 1f)]
		private static float DisplayChanceOnMiss = 0.5f;

		private string InitiatorName => (initiator == null) ? "null" : initiator.LabelShort;

		private string RecipientName => (recipientPawn == null) ? "null" : recipientPawn.LabelShort;

		public RulePackDef RuleDef
		{
			get
			{
				return ruleDef;
			}
			set
			{
				ruleDef = value;
				ResetCache();
			}
		}

		public BattleLogEntry_MeleeCombat()
		{
		}

		public BattleLogEntry_MeleeCombat(RulePackDef ruleDef, bool alwaysShowInCompact, Pawn initiator, Thing recipient, ImplementOwnerTypeDef implementType, string toolLabel, ThingDef ownerEquipmentDef = null, HediffDef ownerHediffDef = null, LogEntryDef def = null)
			: base(def)
		{
			this.ruleDef = ruleDef;
			this.alwaysShowInCompact = alwaysShowInCompact;
			this.initiator = initiator;
			this.implementType = implementType;
			this.ownerEquipmentDef = ownerEquipmentDef;
			this.ownerHediffDef = ownerHediffDef;
			this.toolLabel = toolLabel;
			if (recipient is Pawn)
			{
				recipientPawn = (recipient as Pawn);
			}
			else if (recipient != null)
			{
				recipientThing = recipient.def;
			}
			if (ownerEquipmentDef != null && ownerHediffDef != null)
			{
				Log.ErrorOnce($"Combat log owned by both equipment {ownerEquipmentDef.label} and hediff {ownerHediffDef.label}, may produce unexpected results", 96474669);
			}
		}

		public override bool Concerns(Thing t)
		{
			return t == initiator || t == recipientPawn;
		}

		public override IEnumerable<Thing> GetConcerns()
		{
			if (initiator != null)
			{
				yield return (Thing)initiator;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (recipientPawn != null)
			{
				yield return (Thing)recipientPawn;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == initiator && recipientPawn != null)
			{
				CameraJumper.TryJumpAndSelect(recipientPawn);
			}
			else if (pov == recipientPawn)
			{
				CameraJumper.TryJumpAndSelect(initiator);
			}
			else if (recipientPawn != null)
			{
				throw new NotImplementedException();
			}
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			if (damagedParts.NullOrEmpty())
			{
				return def.iconMissTex;
			}
			if (deflected)
			{
				return def.iconMissTex;
			}
			if (pov == null || pov == recipientPawn)
			{
				return def.iconDamagedTex;
			}
			if (pov == initiator)
			{
				return def.iconDamagedFromInstigatorTex;
			}
			return def.iconDamagedTex;
		}

		protected override BodyDef DamagedBody()
		{
			return (recipientPawn == null) ? null : recipientPawn.RaceProps.body;
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			result.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, result.Constants));
			if (recipientPawn != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipientPawn, result.Constants));
			}
			else if (recipientThing != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef("RECIPIENT", recipientThing));
			}
			result.Includes.Add(ruleDef);
			if (!toolLabel.NullOrEmpty())
			{
				result.Rules.Add(new Rule_String("TOOL_label", toolLabel));
			}
			if (implementType != null && !implementType.implementOwnerRuleName.NullOrEmpty())
			{
				if (ownerEquipmentDef != null)
				{
					result.Rules.AddRange(GrammarUtility.RulesForDef(implementType.implementOwnerRuleName, ownerEquipmentDef));
				}
				else if (ownerHediffDef != null)
				{
					result.Rules.AddRange(GrammarUtility.RulesForDef(implementType.implementOwnerRuleName, ownerHediffDef));
				}
			}
			if (implementType != null && !implementType.implementOwnerTypeValue.NullOrEmpty())
			{
				result.Constants["IMPLEMENTOWNER_type"] = implementType.implementOwnerTypeValue;
			}
			return result;
		}

		public override bool ShowInCompactView()
		{
			if (alwaysShowInCompact)
			{
				return true;
			}
			return Rand.ChanceSeeded(DisplayChanceOnMiss, logID);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref ruleDef, "ruleDef");
			Scribe_Values.Look(ref alwaysShowInCompact, "alwaysShowInCompact", defaultValue: false);
			Scribe_References.Look(ref initiator, "initiator", saveDestroyedThings: true);
			Scribe_References.Look(ref recipientPawn, "recipientPawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref recipientThing, "recipientThing");
			Scribe_Defs.Look(ref implementType, "implementType");
			Scribe_Defs.Look(ref ownerEquipmentDef, "ownerDef");
			Scribe_Values.Look(ref toolLabel, "toolLabel");
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				BackCompatibility.BattleLogEntry_MeleeCombat_LoadingVars(this);
			}
		}

		public override string ToString()
		{
			return ruleDef.defName + ": " + InitiatorName + "->" + RecipientName;
		}
	}
}
