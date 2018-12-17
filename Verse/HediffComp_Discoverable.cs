using RimWorld;

namespace Verse
{
	public class HediffComp_Discoverable : HediffComp
	{
		private bool discovered;

		public HediffCompProperties_Discoverable Props => (HediffCompProperties_Discoverable)props;

		public override void CompExposeData()
		{
			Scribe_Values.Look(ref discovered, "discovered", defaultValue: false);
		}

		public override bool CompDisallowVisible()
		{
			return !discovered;
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			if (Find.TickManager.TicksGame % 103 == 0)
			{
				CheckDiscovered();
			}
		}

		public override void CompPostPostAdd(DamageInfo? dinfo)
		{
			CheckDiscovered();
		}

		private void CheckDiscovered()
		{
			if (!discovered && parent.CurStage.becomeVisible)
			{
				discovered = true;
				if (Props.sendLetterWhenDiscovered && PawnUtility.ShouldSendNotificationAbout(base.Pawn))
				{
					if (base.Pawn.RaceProps.Humanlike)
					{
						string label = Props.discoverLetterLabel.NullOrEmpty() ? ("LetterLabelNewDisease".Translate() + " (" + base.Def.label + ")") : string.Format(Props.discoverLetterLabel, base.Pawn.LabelShort.CapitalizeFirst()).CapitalizeFirst();
						string text = (!Props.discoverLetterText.NullOrEmpty()) ? Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst() : ((parent.Part != null) ? "NewPartDisease".Translate(base.Pawn.Named("PAWN"), parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.label).AdjustedFor(base.Pawn).CapitalizeFirst() : "NewDisease".Translate(base.Pawn.Named("PAWN"), base.Def.label, base.Pawn.LabelDefinite()).AdjustedFor(base.Pawn).CapitalizeFirst());
						Find.LetterStack.ReceiveLetter(label, text, (Props.letterType == null) ? LetterDefOf.NegativeEvent : Props.letterType, base.Pawn);
					}
					else
					{
						string text2 = (!Props.discoverLetterText.NullOrEmpty()) ? Props.discoverLetterText.Formatted(base.Pawn.LabelIndefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst() : ((parent.Part != null) ? "NewPartDiseaseAnimal".Translate(base.Pawn.LabelShort, parent.Part.Label, base.Pawn.LabelDefinite(), base.Def.LabelCap, base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst() : "NewDiseaseAnimal".Translate(base.Pawn.LabelShort, base.Def.LabelCap, base.Pawn.LabelDefinite(), base.Pawn.Named("PAWN")).AdjustedFor(base.Pawn).CapitalizeFirst());
						Messages.Message(text2, base.Pawn, (Props.messageType == null) ? MessageTypeDefOf.NegativeHealthEvent : Props.messageType);
					}
				}
			}
		}

		public override void Notify_PawnDied()
		{
			CheckDiscovered();
		}

		public override string CompDebugString()
		{
			return "discovered: " + discovered;
		}
	}
}
