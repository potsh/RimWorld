namespace Verse
{
	public class CompHeatPusher : ThingComp
	{
		private const int HeatPushInterval = 60;

		public CompProperties_HeatPusher Props => (CompProperties_HeatPusher)props;

		protected virtual bool ShouldPushHeatNow
		{
			get
			{
				if (!parent.SpawnedOrAnyParentSpawned)
				{
					return false;
				}
				CompProperties_HeatPusher props = Props;
				float ambientTemperature = parent.AmbientTemperature;
				return ambientTemperature < props.heatPushMaxTemperature && ambientTemperature > props.heatPushMinTemperature;
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			if (parent.IsHashIntervalTick(60) && ShouldPushHeatNow)
			{
				GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond);
			}
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			if (ShouldPushHeatNow)
			{
				GenTemperature.PushHeat(parent.PositionHeld, parent.MapHeld, Props.heatPerSecond * 4.16666651f);
			}
		}
	}
}
