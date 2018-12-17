using Verse;

namespace RimWorld
{
	public class IncidentWorker_PsychicDrone : IncidentWorker_PsychicEmanation
	{
		private const float MaxPointsDroneLow = 800f;

		private const float MaxPointsDroneMedium = 2000f;

		protected override void DoConditionAndLetter(Map map, int duration, Gender gender, float points)
		{
			if (points < 0f)
			{
				points = StorytellerUtility.DefaultThreatPointsNow(map);
			}
			PsychicDroneLevel level = (points < 800f) ? PsychicDroneLevel.BadLow : ((!(points < 2000f)) ? PsychicDroneLevel.BadHigh : PsychicDroneLevel.BadMedium);
			GameCondition_PsychicEmanation gameCondition_PsychicEmanation = (GameCondition_PsychicEmanation)GameConditionMaker.MakeCondition(GameConditionDefOf.PsychicDrone, duration);
			gameCondition_PsychicEmanation.gender = gender;
			gameCondition_PsychicEmanation.level = level;
			map.gameConditionManager.RegisterCondition(gameCondition_PsychicEmanation);
			string label = "LetterLabelPsychicDrone".Translate() + " (" + level.GetLabel() + ", " + gender.GetLabel() + ")";
			string text = "LetterIncidentPsychicDrone".Translate(gender.ToString().Translate().ToLower(), level.GetLabel());
			Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent);
		}
	}
}
