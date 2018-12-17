using System.Text;

namespace Verse
{
	public class HediffComp_SeverityPerDay : HediffComp
	{
		protected const int SeverityUpdateInterval = 200;

		private HediffCompProperties_SeverityPerDay Props => (HediffCompProperties_SeverityPerDay)props;

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			if (base.Pawn.IsHashIntervalTick(200))
			{
				float num = SeverityChangePerDay();
				num *= 0.00333333341f;
				severityAdjustment += num;
			}
		}

		protected virtual float SeverityChangePerDay()
		{
			return Props.severityPerDay;
		}

		public override string CompDebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.CompDebugString());
			if (!base.Pawn.Dead)
			{
				stringBuilder.AppendLine("severity/day: " + SeverityChangePerDay().ToString("F3"));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
	}
}
