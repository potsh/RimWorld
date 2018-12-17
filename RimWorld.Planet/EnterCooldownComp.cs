using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class EnterCooldownComp : WorldObjectComp
	{
		private int ticksLeft;

		public WorldObjectCompProperties_EnterCooldown Props => (WorldObjectCompProperties_EnterCooldown)props;

		public bool Active => ticksLeft > 0;

		public bool BlocksEntering => Active && !base.ParentHasMap;

		public int TicksLeft => Active ? ticksLeft : 0;

		public float DaysLeft => (float)TicksLeft / 60000f;

		public void Start(float? durationDays = default(float?))
		{
			float num = (!durationDays.HasValue) ? Props.durationDays : durationDays.Value;
			ticksLeft = Mathf.RoundToInt(num * 60000f);
		}

		public void Stop()
		{
			ticksLeft = 0;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (Active)
			{
				ticksLeft--;
			}
		}

		public override void PostMapGenerate()
		{
			base.PostMapGenerate();
			if (Active)
			{
				Stop();
			}
		}

		public override void PostMyMapRemoved()
		{
			base.PostMyMapRemoved();
			if (Props.autoStartOnMapRemoved)
			{
				Start();
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
		}
	}
}
