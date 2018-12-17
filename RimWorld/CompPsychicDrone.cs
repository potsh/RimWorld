using Verse;
using Verse.Sound;

namespace RimWorld
{
	public class CompPsychicDrone : ThingComp
	{
		private int ticksToIncreaseDroneLevel;

		private PsychicDroneLevel droneLevel = PsychicDroneLevel.BadLow;

		public CompProperties_PsychicDrone Props => (CompProperties_PsychicDrone)props;

		public PsychicDroneLevel DroneLevel => droneLevel;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			if (!respawningAfterLoad)
			{
				ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
			}
			SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksToIncreaseDroneLevel, "ticksToIncreaseDroneLevel", 0);
			Scribe_Values.Look(ref droneLevel, "droneLevel", PsychicDroneLevel.None);
		}

		public override void CompTick()
		{
			if (parent.Spawned)
			{
				ticksToIncreaseDroneLevel--;
				if (ticksToIncreaseDroneLevel <= 0)
				{
					IncreaseDroneLevel();
					ticksToIncreaseDroneLevel = Props.droneLevelIncreaseInterval;
				}
			}
		}

		private void IncreaseDroneLevel()
		{
			if (droneLevel != PsychicDroneLevel.BadExtreme)
			{
				droneLevel++;
				string text = "LetterPsychicDroneLevelIncreased".Translate();
				Find.LetterStack.ReceiveLetter("LetterLabelPsychicDroneLevelIncreased".Translate(), text, LetterDefOf.NegativeEvent);
				SoundDefOf.PsychicPulseGlobal.PlayOneShotOnCamera(parent.Map);
			}
		}

		public override string CompInspectStringExtra()
		{
			return "PsychicDroneLevel".Translate(droneLevel.GetLabelCap());
		}
	}
}
