using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class ToolCapacityDef : Def
	{
		public IEnumerable<ManeuverDef> Maneuvers => from x in DefDatabase<ManeuverDef>.AllDefsListForReading
		where x.requiredCapacity == this
		select x;

		public IEnumerable<VerbProperties> VerbsProperties => from x in Maneuvers
		select x.verb;
	}
}
