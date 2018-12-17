using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Building_ShipReactor : Building
	{
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
			yield break;
			IL_014a:
			/*Error near IL_014b: Unexpected return in MoveNext()*/;
		}
	}
}
