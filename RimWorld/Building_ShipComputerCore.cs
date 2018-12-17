using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_ShipComputerCore : Building
	{
		private bool CanLaunchNow => !ShipUtility.LaunchFailReasons(this).Any();

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c2 = enumerator.Current;
					yield return c2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Gizmo> enumerator2 = ShipUtility.ShipStartupGizmos(this).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Gizmo c = enumerator2.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			Command_Action launch = new Command_Action
			{
				action = TryLaunch,
				defaultLabel = "CommandShipLaunch".Translate(),
				defaultDesc = "CommandShipLaunchDesc".Translate()
			};
			if (!CanLaunchNow)
			{
				launch.Disable(ShipUtility.LaunchFailReasons(this).First());
			}
			if (ShipCountdown.CountingDown)
			{
				launch.Disable();
			}
			launch.hotKey = KeyBindingDefOf.Misc1;
			launch.icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");
			yield return (Gizmo)launch;
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0226:
			/*Error near IL_0227: Unexpected return in MoveNext()*/;
		}

		private void TryLaunch()
		{
			if (CanLaunchNow)
			{
				ShipCountdown.InitiateCountdown(this);
			}
		}
	}
}
