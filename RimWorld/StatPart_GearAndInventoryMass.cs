using System;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public class StatPart_GearAndInventoryMass : StatPart
	{
		[CompilerGenerated]
		private static Func<Pawn, float> _003C_003Ef__mg_0024cache0;

		public override void TransformValue(StatRequest req, ref float val)
		{
			if (TryGetValue(req, out float value))
			{
				val += value;
			}
		}

		public override string ExplanationPart(StatRequest req)
		{
			if (TryGetValue(req, out float value))
			{
				return "StatsReport_GearAndInventoryMass".Translate() + ": " + value.ToStringMassOffset();
			}
			return null;
		}

		private bool TryGetValue(StatRequest req, out float value)
		{
			return PawnOrCorpseStatUtility.TryGetPawnOrCorpseStat(req, MassUtility.GearAndInventoryMass, (ThingDef x) => 0f, out value);
		}
	}
}
