using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public struct AlertReport
	{
		public bool active;

		public IEnumerable<GlobalTargetInfo> culprits;

		private static Func<Thing, GlobalTargetInfo> ThingToTargetInfo = (Thing x) => new GlobalTargetInfo(x);

		private static Func<Pawn, GlobalTargetInfo> PawnToTargetInfo = (Pawn x) => new GlobalTargetInfo(x);

		private static Func<Building, GlobalTargetInfo> BuildingToTargetInfo = (Building x) => new GlobalTargetInfo(x);

		private static Func<WorldObject, GlobalTargetInfo> WorldObjectToTargetInfo = (WorldObject x) => new GlobalTargetInfo(x);

		private static Func<Caravan, GlobalTargetInfo> CaravanToTargetInfo = (Caravan x) => new GlobalTargetInfo(x);

		public bool AnyCulpritValid
		{
			get
			{
				if (culprits == null)
				{
					return false;
				}
				foreach (GlobalTargetInfo culprit in culprits)
				{
					if (culprit.IsValid)
					{
						return true;
					}
				}
				return false;
			}
		}

		public static AlertReport Active
		{
			get
			{
				AlertReport result = default(AlertReport);
				result.active = true;
				return result;
			}
		}

		public static AlertReport Inactive
		{
			get
			{
				AlertReport result = default(AlertReport);
				result.active = false;
				return result;
			}
		}

		public static AlertReport CulpritIs(GlobalTargetInfo culp)
		{
			AlertReport result = default(AlertReport);
			result.active = culp.IsValid;
			if (culp.IsValid)
			{
				result.culprits = Gen.YieldSingle(culp);
			}
			return result;
		}

		public static AlertReport CulpritsAre(IEnumerable<GlobalTargetInfo> culprits)
		{
			AlertReport result = default(AlertReport);
			result.culprits = culprits;
			result.active = result.AnyCulpritValid;
			return result;
		}

		public static AlertReport CulpritsAre(IEnumerable<Thing> culprits)
		{
			return CulpritsAre(culprits?.Select(ThingToTargetInfo));
		}

		public static AlertReport CulpritsAre(IEnumerable<Pawn> culprits)
		{
			return CulpritsAre(culprits?.Select(PawnToTargetInfo));
		}

		public static AlertReport CulpritsAre(IEnumerable<Building> culprits)
		{
			return CulpritsAre(culprits?.Select(BuildingToTargetInfo));
		}

		public static AlertReport CulpritsAre(IEnumerable<WorldObject> culprits)
		{
			return CulpritsAre(culprits?.Select(WorldObjectToTargetInfo));
		}

		public static AlertReport CulpritsAre(IEnumerable<Caravan> culprits)
		{
			return CulpritsAre(culprits?.Select(CaravanToTargetInfo));
		}

		public static implicit operator AlertReport(bool b)
		{
			AlertReport result = default(AlertReport);
			result.active = b;
			return result;
		}

		public static implicit operator AlertReport(Thing culprit)
		{
			return CulpritIs(culprit);
		}

		public static implicit operator AlertReport(WorldObject culprit)
		{
			return CulpritIs(culprit);
		}

		public static implicit operator AlertReport(GlobalTargetInfo culprit)
		{
			return CulpritIs(culprit);
		}
	}
}
