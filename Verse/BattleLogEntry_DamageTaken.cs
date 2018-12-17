using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_DamageTaken : LogEntry_DamageResult
	{
		private Pawn initiatorPawn;

		private Pawn recipientPawn;

		private RulePackDef ruleDef;

		private string RecipientName => (recipientPawn == null) ? "null" : recipientPawn.LabelShort;

		public BattleLogEntry_DamageTaken()
		{
		}

		public BattleLogEntry_DamageTaken(Pawn recipient, RulePackDef ruleDef, Pawn initiator = null)
		{
			initiatorPawn = initiator;
			recipientPawn = recipient;
			this.ruleDef = ruleDef;
		}

		public override bool Concerns(Thing t)
		{
			return t == initiatorPawn || t == recipientPawn;
		}

		public override IEnumerable<Thing> GetConcerns()
		{
			if (initiatorPawn != null)
			{
				yield return (Thing)initiatorPawn;
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
			CameraJumper.TryJumpAndSelect(recipientPawn);
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			return LogEntry.Blood;
		}

		protected override BodyDef DamagedBody()
		{
			return (recipientPawn == null) ? null : recipientPawn.RaceProps.body;
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			if (recipientPawn == null)
			{
				Log.ErrorOnce("BattleLogEntry_DamageTaken has a null recipient.", 60465709);
			}
			result.Includes.Add(ruleDef);
			result.Rules.AddRange(GrammarUtility.RulesForPawn("RECIPIENT", recipientPawn, result.Constants));
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look(ref initiatorPawn, "initiatorPawn", saveDestroyedThings: true);
			Scribe_References.Look(ref recipientPawn, "recipientPawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref ruleDef, "ruleDef");
		}

		public override string ToString()
		{
			return "BattleLogEntry_DamageTaken: " + RecipientName;
		}
	}
}
