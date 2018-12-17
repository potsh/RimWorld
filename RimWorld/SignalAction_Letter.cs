using System.Linq;
using Verse;

namespace RimWorld
{
	public class SignalAction_Letter : SignalAction
	{
		public Letter letter;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref letter, "letter");
		}

		protected override void DoAction(object[] args)
		{
			Pawn pawn = null;
			if (args != null && args.Any())
			{
				pawn = (args[0] as Pawn);
			}
			if (pawn != null)
			{
				ChoiceLetter choiceLetter = letter as ChoiceLetter;
				if (choiceLetter != null)
				{
					choiceLetter.text = choiceLetter.text.Formatted(pawn.LabelShort, pawn.Named("PAWN")).AdjustedFor(pawn);
				}
				if (!letter.lookTargets.IsValid())
				{
					letter.lookTargets = pawn;
				}
			}
			Find.LetterStack.ReceiveLetter(letter);
		}
	}
}
