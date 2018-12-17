using System;
using System.Collections.Generic;

namespace Verse
{
	public class SpecialThingFilterDef : Def
	{
		public ThingCategoryDef parentCategory;

		public string saveKey;

		public bool allowedByDefault;

		public bool configurable = true;

		public Type workerClass;

		[Unsaved]
		private SpecialThingFilterWorker workerInt;

		public SpecialThingFilterWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = (SpecialThingFilterWorker)Activator.CreateInstance(workerClass);
				}
				return workerInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (workerClass == null)
			{
				yield return "SpecialThingFilterDef " + defName + " has no worker class.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0101:
			/*Error near IL_0102: Unexpected return in MoveNext()*/;
		}

		public static SpecialThingFilterDef Named(string defName)
		{
			return DefDatabase<SpecialThingFilterDef>.GetNamed(defName);
		}
	}
}
