using Verse;

namespace RimWorld.Planet
{
	public class TimeoutComp : WorldObjectComp
	{
		private int timeoutEndTick = -1;

		public bool Active => timeoutEndTick != -1;

		public bool Passed => Active && Find.TickManager.TicksGame >= timeoutEndTick;

		private bool ShouldRemoveWorldObjectNow => Passed && !base.ParentHasMap;

		public int TicksLeft => Active ? (timeoutEndTick - Find.TickManager.TicksGame) : 0;

		public void StartTimeout(int ticks)
		{
			timeoutEndTick = Find.TickManager.TicksGame + ticks;
		}

		public override void CompTick()
		{
			base.CompTick();
			if (ShouldRemoveWorldObjectNow)
			{
				Find.WorldObjects.Remove(parent);
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref timeoutEndTick, "timeoutEndTick", 0);
		}

		public override string CompInspectStringExtra()
		{
			if (Active && !base.ParentHasMap)
			{
				return "WorldObjectTimeout".Translate(TicksLeft.ToStringTicksToPeriod());
			}
			return null;
		}
	}
}
