namespace Verse
{
	public class CameraMapConfig_ContinuousPan : CameraMapConfig
	{
		public CameraMapConfig_ContinuousPan()
		{
			dollyRateKeys = 10f;
			dollyRateMouseDrag = 4f;
			dollyRateScreenEdge = 5f;
			camSpeedDecayFactor = 1f;
			moveSpeedScale = 1f;
		}
	}
}
