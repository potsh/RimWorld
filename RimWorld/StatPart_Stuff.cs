using System.Text;
using Verse;

namespace RimWorld
{
	public class StatPart_Stuff : StatPart
	{
		public StatDef stuffPowerStat;

		public StatDef multiplierStat;

		public override string ExplanationPart(StatRequest req)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (req.Def.MadeFromStuff)
			{
				string text = (req.StuffDef == null) ? "None".Translate() : req.StuffDef.LabelCap;
				string text2 = (req.StuffDef == null) ? "0" : req.StuffDef.GetStatValueAbstract(stuffPowerStat).ToStringByStyle(parentStat.ToStringStyleUnfinalized);
				stringBuilder.AppendLine("StatsReport_Material".Translate() + " (" + text + "): " + text2);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("StatsReport_StuffEffectMultiplier".Translate() + ": " + GetMultiplier(req).ToStringPercent("F0"));
				stringBuilder.AppendLine();
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

		public override void TransformValue(StatRequest req, ref float value)
		{
			float num = (req.StuffDef == null) ? 0f : req.StuffDef.GetStatValueAbstract(stuffPowerStat);
			value += GetMultiplier(req) * num;
		}

		private float GetMultiplier(StatRequest req)
		{
			if (req.HasThing)
			{
				return req.Thing.GetStatValue(multiplierStat);
			}
			return req.Def.GetStatValueAbstract(multiplierStat);
		}
	}
}
