using System.Linq;

namespace RimWorld
{
	public class Alert_MinorBreakRisk : Alert
	{
		public Alert_MinorBreakRisk()
		{
			defaultPriority = AlertPriority.High;
		}

		public override string GetLabel()
		{
			return BreakRiskAlertUtility.AlertLabel;
		}

		public override string GetExplanation()
		{
			return BreakRiskAlertUtility.AlertExplanation;
		}

		public override AlertReport GetReport()
		{
			if (BreakRiskAlertUtility.PawnsAtRiskExtreme.Any() || BreakRiskAlertUtility.PawnsAtRiskMajor.Any())
			{
				return false;
			}
			return AlertReport.CulpritsAre(BreakRiskAlertUtility.PawnsAtRiskMinor);
		}
	}
}
