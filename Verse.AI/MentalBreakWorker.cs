using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class MentalBreakWorker
	{
		public MentalBreakDef def;

		public virtual float CommonalityFor(Pawn pawn)
		{
			float num = def.baseCommonality;
			if (pawn.Faction == Faction.OfPlayer && def.commonalityFactorPerPopulationCurve != null)
			{
				num *= def.commonalityFactorPerPopulationCurve.Evaluate((float)PawnsFinder.AllMaps_FreeColonists.Count());
			}
			return num;
		}

		public virtual bool BreakCanOccur(Pawn pawn)
		{
			if (def.requiredTrait != null && (pawn.story == null || !pawn.story.traits.HasTrait(def.requiredTrait)))
			{
				return false;
			}
			if (def.mentalState != null && pawn.story != null && pawn.story.traits.allTraits.Any((Trait tr) => tr.CurrentData.disallowedMentalStates != null && tr.CurrentData.disallowedMentalStates.Contains(def.mentalState)))
			{
				return false;
			}
			if (def.mentalState != null && !def.mentalState.Worker.StateCanOccur(pawn))
			{
				return false;
			}
			if (pawn.story != null)
			{
				IEnumerable<MentalBreakDef> theOnlyAllowedMentalBreaks = pawn.story.traits.TheOnlyAllowedMentalBreaks;
				if (!theOnlyAllowedMentalBreaks.Contains(def) && theOnlyAllowedMentalBreaks.Any((MentalBreakDef x) => x.intensity == def.intensity && x.Worker.BreakCanOccur(pawn)))
				{
					return false;
				}
			}
			if (TutorSystem.TutorialMode && pawn.Faction == Faction.OfPlayer)
			{
				return false;
			}
			return true;
		}

		public virtual bool TryStart(Pawn pawn, string reason, bool causedByMood)
		{
			MentalStateHandler mentalStateHandler = pawn.mindState.mentalStateHandler;
			MentalStateDef mentalState = def.mentalState;
			bool causedByMood2 = causedByMood;
			return mentalStateHandler.TryStartMentalState(mentalState, reason, forceWake: false, causedByMood2);
		}

		protected bool TrySendLetter(Pawn pawn, string textKey, string reason)
		{
			if (!PawnUtility.ShouldSendNotificationAbout(pawn))
			{
				return false;
			}
			string label = def.LabelCap + ": " + pawn.LabelShortCap;
			string text = textKey.Translate(pawn.Label, pawn.Named("PAWN")).CapitalizeFirst();
			if (reason != null)
			{
				text = text + "\n\n" + reason;
			}
			text = text.AdjustedFor(pawn);
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);
			return true;
		}
	}
}
