using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_SinglePawn : Tale
	{
		public TaleData_Pawn pawnData;

		public override Pawn DominantPawn => pawnData.pawn;

		public override string ShortSummary => def.LabelCap + ": " + pawnData.name;

		public Tale_SinglePawn()
		{
		}

		public Tale_SinglePawn(Pawn pawn)
		{
			pawnData = TaleData_Pawn.GenerateFrom(pawn);
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				surroundings = TaleData_Surroundings.GenerateFrom(pawn.PositionHeld, pawn.MapHeld);
			}
		}

		public override bool Concerns(Thing th)
		{
			return base.Concerns(th) || pawnData.pawn == th;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref pawnData, "pawnData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			using (IEnumerator<Rule> enumerator = pawnData.GetRules("ANYPAWN").GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Rule r2 = enumerator.Current;
					yield return r2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Rule> enumerator2 = pawnData.GetRules("PAWN").GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Rule r = enumerator2.Current;
					yield return r;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_015e:
			/*Error near IL_015f: Unexpected return in MoveNext()*/;
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			pawnData = TaleData_Pawn.GenerateRandom();
		}
	}
}
