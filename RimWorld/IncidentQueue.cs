using System.Collections;
using System.Collections.Generic;
using System.Text;
using Verse;

namespace RimWorld
{
	public class IncidentQueue : IExposable
	{
		private List<QueuedIncident> queuedIncidents = new List<QueuedIncident>();

		public int Count => queuedIncidents.Count;

		public string DebugQueueReadout
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (QueuedIncident queuedIncident in queuedIncidents)
				{
					stringBuilder.AppendLine(queuedIncident.ToString() + " (in " + (queuedIncident.FireTick - Find.TickManager.TicksGame).ToString() + " ticks)");
				}
				return stringBuilder.ToString();
			}
		}

		public IEnumerator GetEnumerator()
		{
			using (List<QueuedIncident>.Enumerator enumerator = queuedIncidents.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					QueuedIncident inc = enumerator.Current;
					yield return (object)inc;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00b4:
			/*Error near IL_00b5: Unexpected return in MoveNext()*/;
		}

		public void Clear()
		{
			queuedIncidents.Clear();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref queuedIncidents, "queuedIncidents", LookMode.Deep);
		}

		public bool Add(QueuedIncident qi)
		{
			queuedIncidents.Add(qi);
			queuedIncidents.Sort((QueuedIncident a, QueuedIncident b) => a.FireTick.CompareTo(b.FireTick));
			return true;
		}

		public bool Add(IncidentDef def, int fireTick, IncidentParms parms = null, int retryDurationTicks = 0)
		{
			FiringIncident firingInc = new FiringIncident(def, null, parms);
			QueuedIncident qi = new QueuedIncident(firingInc, fireTick, retryDurationTicks);
			Add(qi);
			return true;
		}

		public void IncidentQueueTick()
		{
			for (int num = queuedIncidents.Count - 1; num >= 0; num--)
			{
				QueuedIncident queuedIncident = queuedIncidents[num];
				if (!queuedIncident.TriedToFire)
				{
					if (queuedIncident.FireTick <= Find.TickManager.TicksGame)
					{
						bool flag = Find.Storyteller.TryFire(queuedIncident.FiringIncident);
						queuedIncident.Notify_TriedToFire();
						if (flag || queuedIncident.RetryDurationTicks == 0)
						{
							queuedIncidents.Remove(queuedIncident);
						}
					}
				}
				else if (queuedIncident.FireTick + queuedIncident.RetryDurationTicks <= Find.TickManager.TicksGame)
				{
					queuedIncidents.Remove(queuedIncident);
				}
				else if (Find.TickManager.TicksGame % 833 == Rand.RangeSeeded(0, 833, queuedIncident.FireTick))
				{
					bool flag2 = Find.Storyteller.TryFire(queuedIncident.FiringIncident);
					queuedIncident.Notify_TriedToFire();
					if (flag2)
					{
						queuedIncidents.Remove(queuedIncident);
					}
				}
			}
		}

		public void Notify_MapRemoved(Map map)
		{
			queuedIncidents.RemoveAll((QueuedIncident x) => x.FiringIncident.parms.target == map);
		}
	}
}
