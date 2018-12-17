using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class InstallationDesignatorDatabase
	{
		private static Dictionary<ThingDef, Designator_Install> designators = new Dictionary<ThingDef, Designator_Install>();

		public static Designator_Install DesignatorFor(ThingDef artDef)
		{
			if (designators.TryGetValue(artDef, out Designator_Install value))
			{
				return value;
			}
			value = NewDesignatorFor(artDef);
			designators.Add(artDef, value);
			return value;
		}

		private static Designator_Install NewDesignatorFor(ThingDef artDef)
		{
			Designator_Install designator_Install = new Designator_Install();
			designator_Install.hotKey = KeyBindingDefOf.Misc1;
			return designator_Install;
		}
	}
}
