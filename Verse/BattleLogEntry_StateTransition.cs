using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_StateTransition : LogEntry
	{
		private RulePackDef transitionDef;

		private Pawn subjectPawn;

		private ThingDef subjectThing;

		private Pawn initiator;

		private HediffDef culpritHediffDef;

		private BodyPartRecord culpritHediffTargetPart;

		private BodyPartRecord culpritTargetPart;

		private string SubjectName => (subjectPawn == null) ? "null" : subjectPawn.LabelShort;

		public BattleLogEntry_StateTransition()
		{
		}

		public BattleLogEntry_StateTransition(Thing subject, RulePackDef transitionDef, Pawn initiator, Hediff culpritHediff, BodyPartRecord culpritTargetDef)
		{
			if (subject is Pawn)
			{
				subjectPawn = (subject as Pawn);
			}
			else if (subject != null)
			{
				subjectThing = subject.def;
			}
			this.transitionDef = transitionDef;
			this.initiator = initiator;
			if (culpritHediff != null)
			{
				culpritHediffDef = culpritHediff.def;
				if (culpritHediff.Part != null)
				{
					culpritHediffTargetPart = culpritHediff.Part;
				}
			}
			culpritTargetPart = culpritTargetDef;
		}

		public override bool Concerns(Thing t)
		{
			return t == subjectPawn || t == initiator;
		}

		public override IEnumerable<Thing> GetConcerns()
		{
			if (initiator != null)
			{
				yield return (Thing)initiator;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (subjectPawn != null)
			{
				yield return (Thing)subjectPawn;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == subjectPawn)
			{
				CameraJumper.TryJumpAndSelect(initiator);
			}
			else
			{
				if (pov != initiator)
				{
					throw new NotImplementedException();
				}
				CameraJumper.TryJumpAndSelect(subjectPawn);
			}
		}

		public override Texture2D IconFromPOV(Thing pov)
		{
			if (pov == null || pov == subjectPawn)
			{
				return (transitionDef != RulePackDefOf.Transition_Downed) ? LogEntry.Skull : LogEntry.Downed;
			}
			if (pov == initiator)
			{
				return (transitionDef != RulePackDefOf.Transition_Downed) ? LogEntry.SkullTarget : LogEntry.DownedTarget;
			}
			return null;
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			if (subjectPawn != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForPawn("SUBJECT", subjectPawn, result.Constants));
			}
			else if (subjectThing != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef("SUBJECT", subjectThing));
			}
			result.Includes.Add(transitionDef);
			if (initiator != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiator, result.Constants));
			}
			if (culpritHediffDef != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForHediffDef("CULPRITHEDIFF", culpritHediffDef, culpritHediffTargetPart));
			}
			if (culpritHediffTargetPart != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForBodyPartRecord("CULPRITHEDIFF_target", culpritHediffTargetPart));
			}
			if (culpritTargetPart != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForBodyPartRecord("CULPRITHEDIFF_originaltarget", culpritTargetPart));
			}
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref transitionDef, "transitionDef");
			Scribe_References.Look(ref subjectPawn, "subjectPawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref subjectThing, "subjectThing");
			Scribe_References.Look(ref initiator, "initiator", saveDestroyedThings: true);
			Scribe_Defs.Look(ref culpritHediffDef, "culpritHediffDef");
			Scribe_BodyParts.Look(ref culpritHediffTargetPart, "culpritHediffTargetPart");
			Scribe_BodyParts.Look(ref culpritTargetPart, "culpritTargetPart");
		}

		public override string ToString()
		{
			return transitionDef.defName + ": " + subjectPawn;
		}
	}
}
