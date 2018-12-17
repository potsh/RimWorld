using Verse;

namespace RimWorld
{
	public class CompReportWorkSpeed : ThingComp
	{
		public override string CompInspectStringExtra()
		{
			bool flag = StatPart_WorkTableOutdoors.Applies(parent.def, parent.Map, parent.Position);
			bool flag2 = StatPart_WorkTableTemperature.Applies(parent);
			bool flag3 = StatPart_WorkTableUnpowered.Applies(parent);
			if (flag || flag2 || flag3)
			{
				string str = "WorkSpeedPenalty".Translate() + ": ";
				string text = string.Empty;
				if (flag)
				{
					text += "Outdoors".Translate().ToLower();
				}
				if (flag2)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "BadTemperature".Translate().ToLower();
				}
				if (flag3)
				{
					if (!text.NullOrEmpty())
					{
						text += ", ";
					}
					text += "NoPower".Translate().ToLower();
				}
				return str + text.CapitalizeFirst();
			}
			return null;
		}
	}
}
