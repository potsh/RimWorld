using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DeathLetter : ChoiceLetter
	{
		protected DiaOption ReadMore
		{
			get
			{
				GlobalTargetInfo target = lookTargets.TryGetPrimaryTarget();
				DiaOption diaOption = new DiaOption("ReadMore".Translate());
				diaOption.action = delegate
				{
					CameraJumper.TryJumpAndSelect(target);
					Find.LetterStack.RemoveLetter(this);
					InspectPaneUtility.OpenTab(typeof(ITab_Pawn_Log));
				};
				diaOption.resolveTree = true;
				if (!target.IsValid)
				{
					diaOption.Disable(null);
				}
				return diaOption;
			}
		}

		public override IEnumerable<DiaOption> Choices
		{
			get
			{
				yield return base.Option_Close;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public override void OpenLetter()
		{
			Pawn targetPawn = lookTargets.TryGetPrimaryTarget().Thing as Pawn;
			string text = base.text;
			string text2 = (from entry in (from entry in (from battle in Find.BattleLog.Battles
			where battle.Concerns(targetPawn)
			select battle).SelectMany((Battle battle) => from entry in battle.Entries
			where entry.Concerns(targetPawn) && entry.ShowInCompactView()
			select entry)
			orderby entry.Age
			select entry).Take(5).Reverse()
			select "  " + entry.ToGameStringFromPOV(null)).ToLineList();
			if (text2.Length > 0)
			{
				text = string.Format("{0}\n\n{1}\n{2}", text, "LastEventsInLife".Translate(targetPawn.LabelDefinite(), targetPawn.Named("PAWN")) + ":", text2);
			}
			DiaNode diaNode = new DiaNode(text);
			diaNode.options.AddRange(Choices);
			WindowStack windowStack = Find.WindowStack;
			DiaNode nodeRoot = diaNode;
			Faction relatedFaction = base.relatedFaction;
			bool radioMode = base.radioMode;
			windowStack.Add(new Dialog_NodeTreeWithFactionInfo(nodeRoot, relatedFaction, delayInteractivity: false, radioMode, title));
		}
	}
}
