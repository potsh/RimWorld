using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace Verse
{
	public abstract class ChoiceLetter : LetterWithTimeout
	{
		public string title;

		public string text;

		public bool radioMode;

		public abstract IEnumerable<DiaOption> Choices
		{
			get;
		}

		protected DiaOption Option_Close
		{
			get
			{
				DiaOption diaOption = new DiaOption("Close".Translate());
				diaOption.action = delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				return diaOption;
			}
		}

		protected DiaOption Option_JumpToLocation
		{
			get
			{
				GlobalTargetInfo target = lookTargets.TryGetPrimaryTarget();
				DiaOption diaOption = new DiaOption("JumpToLocation".Translate());
				diaOption.action = delegate
				{
					CameraJumper.TryJumpAndSelect(target);
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				if (!CameraJumper.CanJump(target))
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		protected DiaOption Option_Reject
		{
			get
			{
				DiaOption diaOption = new DiaOption("RejectLetter".Translate());
				diaOption.action = delegate
				{
					Find.LetterStack.RemoveLetter(this);
				};
				diaOption.resolveTree = true;
				return diaOption;
			}
		}

		protected DiaOption Option_Postpone
		{
			get
			{
				DiaOption diaOption = new DiaOption("PostponeLetter".Translate());
				diaOption.resolveTree = true;
				if (base.TimeoutActive && disappearAtTick <= Find.TickManager.TicksGame + 1)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref title, "title");
			Scribe_Values.Look(ref text, "text");
			Scribe_Values.Look(ref radioMode, "radioMode", defaultValue: false);
		}

		protected override string GetMouseoverText()
		{
			return text;
		}

		public override void OpenLetter()
		{
			DiaNode diaNode = new DiaNode(text);
			diaNode.options.AddRange(Choices);
			DiaNode nodeRoot = diaNode;
			Faction relatedFaction = base.relatedFaction;
			bool flag = radioMode;
			Dialog_NodeTreeWithFactionInfo window = new Dialog_NodeTreeWithFactionInfo(nodeRoot, relatedFaction, delayInteractivity: false, flag, title);
			Find.WindowStack.Add(window);
		}
	}
}
