using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class ScenarioDef : Def
	{
		public Scenario scenario;

		public override void PostLoad()
		{
			base.PostLoad();
			if (scenario.name.NullOrEmpty())
			{
				scenario.name = label;
			}
			if (scenario.description.NullOrEmpty())
			{
				scenario.description = description;
			}
			scenario.Category = ScenarioCategory.FromDef;
		}

		public override IEnumerable<string> ConfigErrors()
		{
			if (scenario == null)
			{
				yield return "null scenario";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			using (IEnumerator<string> enumerator = scenario.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string se = enumerator.Current;
					yield return se;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00f1:
			/*Error near IL_00f2: Unexpected return in MoveNext()*/;
		}
	}
}
