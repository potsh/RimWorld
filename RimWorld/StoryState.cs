using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StoryState : IExposable
	{
		private IIncidentTarget target;

		private int lastThreatBigTick = -1;

		public Dictionary<IncidentDef, int> lastFireTicks = new Dictionary<IncidentDef, int>();

		public int LastThreatBigTick
		{
			get
			{
				if (lastThreatBigTick > Find.TickManager.TicksGame + 1000)
				{
					Log.Error("Latest big threat queue time was " + lastThreatBigTick + " at tick " + Find.TickManager.TicksGame + ". This is too far in the future. Resetting.");
					lastThreatBigTick = Find.TickManager.TicksGame - 1;
				}
				return lastThreatBigTick;
			}
		}

		public StoryState(IIncidentTarget target)
		{
			this.target = target;
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastThreatBigTick, "lastThreatBigTick", 0, forceSave: true);
			Scribe_Collections.Look(ref lastFireTicks, "lastFireTicks", LookMode.Def, LookMode.Value);
		}

		public void Notify_IncidentFired(FiringIncident fi)
		{
			if (!fi.parms.forced && fi.parms.target == target)
			{
				int ticksGame = Find.TickManager.TicksGame;
				if (fi.def.category == IncidentCategoryDefOf.ThreatBig || fi.def.category == IncidentCategoryDefOf.RaidBeacon)
				{
					lastThreatBigTick = ticksGame;
					Find.StoryWatcher.statsRecord.numThreatBigs++;
				}
				if (lastFireTicks.ContainsKey(fi.def))
				{
					lastFireTicks[fi.def] = ticksGame;
				}
				else
				{
					lastFireTicks.Add(fi.def, ticksGame);
				}
			}
		}

		public void CopyTo(StoryState other)
		{
			other.lastThreatBigTick = lastThreatBigTick;
			other.lastFireTicks.Clear();
			foreach (KeyValuePair<IncidentDef, int> lastFireTick in lastFireTicks)
			{
				other.lastFireTicks.Add(lastFireTick.Key, lastFireTick.Value);
			}
		}
	}
}
