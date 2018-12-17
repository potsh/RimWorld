using Verse;

namespace RimWorld
{
	public class PlaceWorker_ReportWorkSpeedPenalties : PlaceWorker
	{
		public override void PostPlace(Map map, BuildableDef def, IntVec3 loc, Rot4 rot)
		{
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null)
			{
				bool flag = StatPart_WorkTableOutdoors.Applies(thingDef, map, loc);
				bool flag2 = StatPart_WorkTableTemperature.Applies(thingDef, map, loc);
				if (flag || flag2)
				{
					string str = "WillGetWorkSpeedPenalty".Translate(def.label).CapitalizeFirst() + ": ";
					string text = string.Empty;
					if (flag)
					{
						text += "Outdoors".Translate();
					}
					if (flag2)
					{
						if (!text.NullOrEmpty())
						{
							text += ", ";
						}
						text += "BadTemperature".Translate();
					}
					str += text.CapitalizeFirst();
					str += ".";
					Messages.Message(str, new TargetInfo(loc, map), MessageTypeDefOf.CautionInput, historical: false);
				}
			}
		}
	}
}
