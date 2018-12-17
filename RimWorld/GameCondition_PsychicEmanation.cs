using Verse;

namespace RimWorld
{
	public class GameCondition_PsychicEmanation : GameCondition
	{
		public Gender gender = Gender.Male;

		public PsychicDroneLevel level = PsychicDroneLevel.BadMedium;

		public override string Label => def.label + " (" + gender.ToString().Translate().ToLower() + ")";

		public override void PostMake()
		{
			base.PostMake();
			level = def.defaultDroneLevel;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref gender, "gender", Gender.None);
			Scribe_Values.Look(ref level, "level", PsychicDroneLevel.None);
		}
	}
}
