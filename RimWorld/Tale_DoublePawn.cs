using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawn : Tale
	{
		public TaleData_Pawn firstPawnData;

		public TaleData_Pawn secondPawnData;

		public override Pawn DominantPawn => firstPawnData.pawn;

		public override string ShortSummary
		{
			get
			{
				string text = def.LabelCap + ": " + firstPawnData.name;
				if (secondPawnData != null)
				{
					text = text + ", " + secondPawnData.name;
				}
				return text;
			}
		}

		public Tale_DoublePawn()
		{
		}

		public Tale_DoublePawn(Pawn firstPawn, Pawn secondPawn)
		{
			firstPawnData = TaleData_Pawn.GenerateFrom(firstPawn);
			if (secondPawn != null)
			{
				secondPawnData = TaleData_Pawn.GenerateFrom(secondPawn);
			}
			if (firstPawn.SpawnedOrAnyParentSpawned)
			{
				surroundings = TaleData_Surroundings.GenerateFrom(firstPawn.PositionHeld, firstPawn.MapHeld);
			}
		}

		public override bool Concerns(Thing th)
		{
			if (secondPawnData != null && secondPawnData.pawn == th)
			{
				return true;
			}
			return base.Concerns(th) || firstPawnData.pawn == th;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref firstPawnData, "firstPawnData");
			Scribe_Deep.Look(ref secondPawnData, "secondPawnData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			if (def.firstPawnSymbol.NullOrEmpty() || def.secondPawnSymbol.NullOrEmpty())
			{
				Log.Error(def + " uses DoublePawn tale class but firstPawnSymbol and secondPawnSymbol are not both set");
			}
			using (IEnumerator<Rule> enumerator = firstPawnData.GetRules("ANYPAWN").GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Rule r4 = enumerator.Current;
					yield return r4;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Rule> enumerator2 = firstPawnData.GetRules(def.firstPawnSymbol).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Rule r3 = enumerator2.Current;
					yield return r3;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (secondPawnData != null)
			{
				using (IEnumerator<Rule> enumerator3 = firstPawnData.GetRules("ANYPAWN").GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						Rule r2 = enumerator3.Current;
						yield return r2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				using (IEnumerator<Rule> enumerator4 = secondPawnData.GetRules(def.secondPawnSymbol).GetEnumerator())
				{
					if (enumerator4.MoveNext())
					{
						Rule r = enumerator4.Current;
						yield return r;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_0309:
			/*Error near IL_030a: Unexpected return in MoveNext()*/;
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			firstPawnData = TaleData_Pawn.GenerateRandom();
			secondPawnData = TaleData_Pawn.GenerateRandom();
		}
	}
}
