using System;
using System.Collections.Generic;
using Verse.Grammar;

namespace Verse
{
	public class BattleLogEntry_Event : LogEntry
	{
		private RulePackDef eventDef;

		private Pawn subjectPawn;

		private ThingDef subjectThing;

		private Pawn initiatorPawn;

		private ThingDef initiatorThing;

		private string SubjectName => (subjectPawn == null) ? "null" : subjectPawn.LabelShort;

		public BattleLogEntry_Event()
		{
		}

		public BattleLogEntry_Event(Thing subject, RulePackDef eventDef, Thing initiator)
		{
			if (subject is Pawn)
			{
				subjectPawn = (subject as Pawn);
			}
			else if (subject != null)
			{
				subjectThing = subject.def;
			}
			if (initiator is Pawn)
			{
				initiatorPawn = (initiator as Pawn);
			}
			else if (initiator != null)
			{
				initiatorThing = initiator.def;
			}
			this.eventDef = eventDef;
		}

		public override bool Concerns(Thing t)
		{
			return t == subjectPawn || t == initiatorPawn;
		}

		public override IEnumerable<Thing> GetConcerns()
		{
			if (subjectPawn != null)
			{
				yield return (Thing)subjectPawn;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (initiatorPawn != null)
			{
				yield return (Thing)initiatorPawn;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void ClickedFromPOV(Thing pov)
		{
			if (pov == subjectPawn)
			{
				CameraJumper.TryJumpAndSelect(initiatorPawn);
			}
			else
			{
				if (pov != initiatorPawn)
				{
					throw new NotImplementedException();
				}
				CameraJumper.TryJumpAndSelect(subjectPawn);
			}
		}

		protected override GrammarRequest GenerateGrammarRequest()
		{
			GrammarRequest result = base.GenerateGrammarRequest();
			result.Includes.Add(eventDef);
			if (subjectPawn != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForPawn("SUBJECT", subjectPawn, result.Constants));
			}
			else if (subjectThing != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef("SUBJECT", subjectThing));
			}
			if (initiatorPawn != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForPawn("INITIATOR", initiatorPawn, result.Constants));
			}
			else if (initiatorThing != null)
			{
				result.Rules.AddRange(GrammarUtility.RulesForDef("INITIATOR", initiatorThing));
			}
			return result;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref eventDef, "eventDef");
			Scribe_References.Look(ref subjectPawn, "subjectPawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref subjectThing, "subjectThing");
			Scribe_References.Look(ref initiatorPawn, "initiatorPawn", saveDestroyedThings: true);
			Scribe_Defs.Look(ref initiatorThing, "initiatorThing");
		}

		public override string ToString()
		{
			return eventDef.defName + ": " + subjectPawn;
		}
	}
}
