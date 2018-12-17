using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class CaravanArrivalActionUtility
	{
		public static IEnumerable<FloatMenuOption> GetFloatMenuOptions<T>(Func<FloatMenuAcceptanceReport> acceptanceReportGetter, Func<T> arrivalActionGetter, string label, Caravan caravan, int pathDestination, WorldObject revalidateWorldClickTarget) where T : CaravanArrivalAction
		{
			_003CGetFloatMenuOptions_003Ec__Iterator0<T> _003CGetFloatMenuOptions_003Ec__Iterator = (_003CGetFloatMenuOptions_003Ec__Iterator0<T>)/*Error near IL_003a: stateMachine*/;
			FloatMenuAcceptanceReport rep = acceptanceReportGetter();
			if (rep.Accepted || !rep.FailReason.NullOrEmpty() || !rep.FailMessage.NullOrEmpty())
			{
				if (rep.FailReason.NullOrEmpty())
				{
					Action action = delegate
					{
						FloatMenuAcceptanceReport floatMenuAcceptanceReport = acceptanceReportGetter();
						if (floatMenuAcceptanceReport.Accepted)
						{
							caravan.pather.StartPath(pathDestination, arrivalActionGetter(), repathImmediately: true);
						}
						else if (!floatMenuAcceptanceReport.FailMessage.NullOrEmpty())
						{
							Messages.Message(floatMenuAcceptanceReport.FailMessage, new GlobalTargetInfo(pathDestination), MessageTypeDefOf.RejectInput, historical: false);
						}
					};
					yield return new FloatMenuOption(label, action, MenuOptionPriority.Default, null, null, 0f, null, revalidateWorldClickTarget);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return new FloatMenuOption(label + " (" + rep.FailReason + ")", null);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
