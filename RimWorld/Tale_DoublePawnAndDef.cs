using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class Tale_DoublePawnAndDef : Tale_DoublePawn
	{
		public TaleData_Def defData;

		public Tale_DoublePawnAndDef()
		{
		}

		public Tale_DoublePawnAndDef(Pawn firstPawn, Pawn secondPawn, Def def)
			: base(firstPawn, secondPawn)
		{
			defData = TaleData_Def.GenerateFrom(def);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref defData, "defData");
		}

		protected override IEnumerable<Rule> SpecialTextGenerationRules()
		{
			if (def.defSymbol.NullOrEmpty())
			{
				Log.Error(def + " uses tale type with def but defSymbol is not set.");
			}
			using (IEnumerator<Rule> enumerator = base.SpecialTextGenerationRules().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Rule r2 = enumerator.Current;
					yield return r2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Rule> enumerator2 = defData.GetRules(def.defSymbol).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Rule r = enumerator2.Current;
					yield return r;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0194:
			/*Error near IL_0195: Unexpected return in MoveNext()*/;
		}

		public override void GenerateTestData()
		{
			base.GenerateTestData();
			defData = TaleData_Def.GenerateFrom((Def)GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), def.defType, "GetRandom"));
		}
	}
}
