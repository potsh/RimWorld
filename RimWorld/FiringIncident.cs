using Verse;

namespace RimWorld
{
	public class FiringIncident : IExposable
	{
		public IncidentDef def;

		public IncidentParms parms = new IncidentParms();

		public StorytellerComp source;

		public FiringIncident()
		{
		}

		public FiringIncident(IncidentDef def, StorytellerComp source, IncidentParms parms = null)
		{
			this.def = def;
			if (parms != null)
			{
				this.parms = parms;
			}
			this.source = source;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Deep.Look(ref parms, "parms");
		}

		public override string ToString()
		{
			string text = def.ToString();
			text = text.PadRight(17);
			string text2 = text;
			if (parms != null)
			{
				text2 = text2 + " " + parms.ToString();
			}
			if (source != null)
			{
				text2 = text2 + ", source=" + source;
			}
			return text2;
		}
	}
}
