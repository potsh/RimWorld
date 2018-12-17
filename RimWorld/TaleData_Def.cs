using System;
using System.Collections.Generic;
using Verse;
using Verse.Grammar;

namespace RimWorld
{
	public class TaleData_Def : TaleData
	{
		public Def def;

		private string tmpDefName;

		private Type tmpDefType;

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				tmpDefName = ((def == null) ? null : def.defName);
				tmpDefType = ((def == null) ? null : def.GetType());
			}
			Scribe_Values.Look(ref tmpDefName, "defName");
			Scribe_Values.Look(ref tmpDefType, "defType");
			if (Scribe.mode == LoadSaveMode.LoadingVars && tmpDefName != null)
			{
				def = GenDefDatabase.GetDef(tmpDefType, BackCompatibility.BackCompatibleDefName(tmpDefType, tmpDefName));
			}
		}

		public override IEnumerable<Rule> GetRules(string prefix)
		{
			if (def != null)
			{
				yield return (Rule)new Rule_String(prefix + "_label", def.label);
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public static TaleData_Def GenerateFrom(Def def)
		{
			TaleData_Def taleData_Def = new TaleData_Def();
			taleData_Def.def = def;
			return taleData_Def;
		}
	}
}
