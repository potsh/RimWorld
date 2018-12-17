using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public static class ScenarioLister
	{
		private static bool dirty = true;

		public static IEnumerable<Scenario> AllScenarios()
		{
			RecacheIfDirty();
			using (IEnumerator<ScenarioDef> enumerator = DefDatabase<ScenarioDef>.AllDefs.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					ScenarioDef scenDef = enumerator.Current;
					yield return scenDef.scenario;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Scenario> enumerator2 = ScenarioFiles.AllScenariosLocal.GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Scenario scen2 = enumerator2.Current;
					yield return scen2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<Scenario> enumerator3 = ScenarioFiles.AllScenariosWorkshop.GetEnumerator())
			{
				if (enumerator3.MoveNext())
				{
					Scenario scen = enumerator3.Current;
					yield return scen;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_01d3:
			/*Error near IL_01d4: Unexpected return in MoveNext()*/;
		}

		public static IEnumerable<Scenario> ScenariosInCategory(ScenarioCategory cat)
		{
			RecacheIfDirty();
			switch (cat)
			{
			case ScenarioCategory.FromDef:
				using (IEnumerator<ScenarioDef> enumerator3 = DefDatabase<ScenarioDef>.AllDefs.GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						ScenarioDef scenDef = enumerator3.Current;
						yield return scenDef.scenario;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				break;
			case ScenarioCategory.CustomLocal:
				using (IEnumerator<Scenario> enumerator2 = ScenarioFiles.AllScenariosLocal.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Scenario scen2 = enumerator2.Current;
						yield return scen2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				break;
			case ScenarioCategory.SteamWorkshop:
				using (IEnumerator<Scenario> enumerator = ScenarioFiles.AllScenariosWorkshop.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Scenario scen = enumerator.Current;
						yield return scen;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				break;
			}
			yield break;
			IL_0201:
			/*Error near IL_0202: Unexpected return in MoveNext()*/;
		}

		public static bool ScenarioIsListedAnywhere(Scenario scen)
		{
			RecacheIfDirty();
			foreach (ScenarioDef allDef in DefDatabase<ScenarioDef>.AllDefs)
			{
				if (allDef.scenario == scen)
				{
					return true;
				}
			}
			foreach (Scenario item in ScenarioFiles.AllScenariosLocal)
			{
				if (scen == item)
				{
					return true;
				}
			}
			return false;
		}

		public static void MarkDirty()
		{
			dirty = true;
		}

		private static void RecacheIfDirty()
		{
			if (dirty)
			{
				RecacheData();
			}
		}

		private static void RecacheData()
		{
			dirty = false;
			int num = ScenarioListHash();
			ScenarioFiles.RecacheData();
			if (ScenarioListHash() != num && !LongEventHandler.ShouldWaitForEvent)
			{
				Find.WindowStack.WindowOfType<Page_SelectScenario>()?.Notify_ScenarioListChanged();
			}
		}

		public static int ScenarioListHash()
		{
			int num = 9826121;
			foreach (Scenario item in AllScenarios())
			{
				num ^= 791 * item.GetHashCode() * 6121;
			}
			return num;
		}
	}
}
