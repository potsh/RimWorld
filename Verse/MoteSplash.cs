using UnityEngine;

namespace Verse
{
	public class MoteSplash : Mote
	{
		public const float VelocityFootstep = 1.5f;

		public const float SizeFootstep = 2f;

		public const float VelocityGunfire = 4f;

		public const float SizeGunfire = 1f;

		public const float VelocityExplosion = 20f;

		public const float SizeExplosion = 6f;

		private float targetSize;

		private float velocity;

		protected override bool EndOfLife => base.AgeSecs >= targetSize / velocity;

		public override float Alpha
		{
			get
			{
				float num = Mathf.Clamp01(base.AgeSecs * 10f);
				num = 1f;
				float num2 = Mathf.Clamp01(1f - base.AgeSecs / (targetSize / velocity));
				return num * num2 * CalculatedIntensity();
			}
		}

		public void Initialize(Vector3 position, float size, float velocity)
		{
			exactPosition = position;
			targetSize = size;
			this.velocity = velocity;
			base.Scale = 0f;
		}

		protected override void TimeInterval(float deltaTime)
		{
			base.TimeInterval(deltaTime);
			if (!base.Destroyed)
			{
				float num2 = base.Scale = base.AgeSecs * velocity;
				exactPosition += base.Map.waterInfo.GetWaterMovement(exactPosition) * deltaTime;
			}
		}

		public float CalculatedIntensity()
		{
			return Mathf.Sqrt(targetSize) / 10f;
		}

		public float CalculatedShockwaveSpan()
		{
			float a = Mathf.Sqrt(targetSize) * 0.8f;
			a = Mathf.Min(a, exactScale.x);
			return a / exactScale.x;
		}
	}
}
