using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class BuildFacilityCommandUtility
	{
		public static IEnumerable<Command> BuildFacilityCommands(BuildableDef building)
		{
			ThingDef thingDef = building as ThingDef;
			if (thingDef != null)
			{
				CompProperties_AffectedByFacilities affectedByFacilities = thingDef.GetCompProperties<CompProperties_AffectedByFacilities>();
				if (affectedByFacilities != null)
				{
					int i = 0;
					Designator_Build des;
					while (true)
					{
						if (i >= affectedByFacilities.linkableFacilities.Count)
						{
							yield break;
						}
						ThingDef facility = affectedByFacilities.linkableFacilities[i];
						des = BuildCopyCommandUtility.FindAllowedDesignator(facility);
						if (des != null)
						{
							break;
						}
						i++;
					}
					yield return (Command)des;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}
	}
}
