using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class StorytellerComp_Triggered : StorytellerComp
	{
		private StorytellerCompProperties_Triggered Props => (StorytellerCompProperties_Triggered)props;

		public override void Notify_PawnEvent(Pawn p, AdaptationEvent ev, DamageInfo? dinfo = default(DamageInfo?))
		{
			if (p.RaceProps.Humanlike && p.IsColonist && (ev == AdaptationEvent.Died || ev == AdaptationEvent.Kidnapped || ev == AdaptationEvent.LostBecauseMapClosed || ev == AdaptationEvent.Downed))
			{
				IEnumerable<Pawn> allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
				foreach (Pawn item in allMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
				{
					if (item.RaceProps.Humanlike && !item.Downed)
					{
						return;
					}
				}
				Map anyPlayerHomeMap = Find.AnyPlayerHomeMap;
				if (anyPlayerHomeMap != null)
				{
					IncidentParms parms = StorytellerUtility.DefaultParmsNow(Props.incident.category, anyPlayerHomeMap);
					if (Props.incident.Worker.CanFireNow(parms))
					{
						FiringIncident firingInc = new FiringIncident(Props.incident, this, parms);
						QueuedIncident qi = new QueuedIncident(firingInc, Find.TickManager.TicksGame + Props.delayTicks);
						Find.Storyteller.incidentQueue.Add(qi);
					}
				}
			}
		}

		public override string ToString()
		{
			return base.ToString() + " " + Props.incident;
		}
	}
}
