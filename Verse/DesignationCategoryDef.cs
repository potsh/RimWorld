using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class DesignationCategoryDef : Def
	{
		public List<Type> specialDesignatorClasses = new List<Type>();

		public int order;

		public bool showPowerGrid;

		[Unsaved]
		private List<Designator> resolvedDesignators = new List<Designator>();

		[Unsaved]
		public KeyBindingCategoryDef bindingCatDef;

		[Unsaved]
		public string cachedHighlightClosedTag;

		public IEnumerable<Designator> ResolvedAllowedDesignators
		{
			get
			{
				GameRules rules = Current.Game.Rules;
				int i = 0;
				Designator des;
				while (true)
				{
					if (i >= resolvedDesignators.Count)
					{
						yield break;
					}
					des = resolvedDesignators[i];
					if (rules.DesignatorAllowed(des))
					{
						break;
					}
					i++;
				}
				yield return des;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public List<Designator> AllResolvedDesignators => resolvedDesignators;

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				ResolveDesignators();
			});
			cachedHighlightClosedTag = "DesignationCategoryButton-" + defName + "-Closed";
		}

		private void ResolveDesignators()
		{
			resolvedDesignators.Clear();
			foreach (Type specialDesignatorClass in specialDesignatorClasses)
			{
				Designator designator = null;
				try
				{
					designator = (Designator)Activator.CreateInstance(specialDesignatorClass);
				}
				catch (Exception ex)
				{
					Log.Error("DesignationCategoryDef" + defName + " could not instantiate special designator from class " + specialDesignatorClass + ".\n Exception: \n" + ex.ToString());
				}
				if (designator != null)
				{
					resolvedDesignators.Add(designator);
				}
			}
			IEnumerable<BuildableDef> enumerable = from tDef in DefDatabase<TerrainDef>.AllDefs.Cast<BuildableDef>().Concat(DefDatabase<ThingDef>.AllDefs.Cast<BuildableDef>())
			where tDef.designationCategory == this
			select tDef;
			Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown> dictionary = new Dictionary<DesignatorDropdownGroupDef, Designator_Dropdown>();
			foreach (BuildableDef item in enumerable)
			{
				if (item.designatorDropdown != null)
				{
					if (!dictionary.ContainsKey(item.designatorDropdown))
					{
						dictionary[item.designatorDropdown] = new Designator_Dropdown();
						resolvedDesignators.Add(dictionary[item.designatorDropdown]);
					}
					dictionary[item.designatorDropdown].Add(new Designator_Build(item));
				}
				else
				{
					resolvedDesignators.Add(new Designator_Build(item));
				}
			}
		}
	}
}
