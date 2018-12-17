using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class Listing_TreeThingFilter : Listing_Tree
	{
		private ThingFilter filter;

		private ThingFilter parentFilter;

		private List<SpecialThingFilterDef> hiddenSpecialFilters;

		private List<ThingDef> forceHiddenDefs;

		private List<SpecialThingFilterDef> tempForceHiddenSpecialFilters;

		private List<ThingDef> suppressSmallVolumeTags;

		public Listing_TreeThingFilter(ThingFilter filter, ThingFilter parentFilter, IEnumerable<ThingDef> forceHiddenDefs, IEnumerable<SpecialThingFilterDef> forceHiddenFilters, List<ThingDef> suppressSmallVolumeTags)
		{
			this.filter = filter;
			this.parentFilter = parentFilter;
			if (forceHiddenDefs != null)
			{
				this.forceHiddenDefs = forceHiddenDefs.ToList();
			}
			if (forceHiddenFilters != null)
			{
				tempForceHiddenSpecialFilters = forceHiddenFilters.ToList();
			}
			this.suppressSmallVolumeTags = suppressSmallVolumeTags;
		}

		public void DoCategoryChildren(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map, bool isRoot = false)
		{
			if (isRoot)
			{
				foreach (SpecialThingFilterDef parentsSpecialThingFilterDef in node.catDef.ParentsSpecialThingFilterDefs)
				{
					if (Visible(parentsSpecialThingFilterDef))
					{
						DoSpecialFilter(parentsSpecialThingFilterDef, indentLevel);
					}
				}
			}
			List<SpecialThingFilterDef> childSpecialFilters = node.catDef.childSpecialFilters;
			for (int i = 0; i < childSpecialFilters.Count; i++)
			{
				if (Visible(childSpecialFilters[i]))
				{
					DoSpecialFilter(childSpecialFilters[i], indentLevel);
				}
			}
			foreach (TreeNode_ThingCategory childCategoryNode in node.ChildCategoryNodes)
			{
				if (Visible(childCategoryNode))
				{
					DoCategory(childCategoryNode, indentLevel, openMask, map);
				}
			}
			foreach (ThingDef item in from n in node.catDef.childThingDefs
			orderby n.label
			select n)
			{
				if (Visible(item))
				{
					DoThingDef(item, indentLevel, map);
				}
			}
		}

		private void DoSpecialFilter(SpecialThingFilterDef sfDef, int nestLevel)
		{
			if (sfDef.configurable)
			{
				LabelLeft("*" + sfDef.LabelCap, sfDef.description, nestLevel);
				bool checkOn = filter.Allows(sfDef);
				bool flag = checkOn;
				Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
				if (checkOn != flag)
				{
					filter.SetAllow(sfDef, checkOn);
				}
				EndLine();
			}
		}

		public void DoCategory(TreeNode_ThingCategory node, int indentLevel, int openMask, Map map)
		{
			OpenCloseWidget(node, indentLevel, openMask);
			LabelLeft(node.LabelCap, node.catDef.description, indentLevel);
			MultiCheckboxState multiCheckboxState = AllowanceStateOf(node);
			MultiCheckboxState multiCheckboxState2 = Widgets.CheckboxMulti(new Rect(LabelWidth, curY, lineHeight, lineHeight), multiCheckboxState, paintable: true);
			if (multiCheckboxState != multiCheckboxState2)
			{
				filter.SetAllow(node.catDef, multiCheckboxState2 == MultiCheckboxState.On, forceHiddenDefs, hiddenSpecialFilters);
			}
			EndLine();
			if (node.IsOpen(openMask))
			{
				DoCategoryChildren(node, indentLevel + 1, openMask, map);
			}
		}

		private void DoThingDef(ThingDef tDef, int nestLevel, Map map)
		{
			bool flag = (suppressSmallVolumeTags == null || !suppressSmallVolumeTags.Contains(tDef)) && tDef.IsStuff && tDef.smallVolume;
			string text = tDef.DescriptionDetailed;
			if (flag)
			{
				text = text + "\n\n" + "ThisIsSmallVolume".Translate(10.ToStringCached());
			}
			float num = -4f;
			if (flag)
			{
				Rect rect = new Rect(LabelWidth - 19f, curY, 19f, 20f);
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.UpperRight;
				GUI.color = Color.gray;
				Widgets.Label(rect, "/" + 10.ToStringCached());
				Text.Font = GameFont.Small;
				GenUI.ResetLabelAlign();
				GUI.color = Color.white;
			}
			num -= 19f;
			if (map != null)
			{
				int count = map.resourceCounter.GetCount(tDef);
				if (count > 0)
				{
					string text2 = count.ToStringCached();
					Rect rect2 = new Rect(0f, curY, LabelWidth + num, 40f);
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.UpperRight;
					GUI.color = new Color(0.5f, 0.5f, 0.1f);
					Widgets.Label(rect2, text2);
					float num2 = num;
					Vector2 vector = Text.CalcSize(text2);
					num = num2 - vector.x;
					GenUI.ResetLabelAlign();
					Text.Font = GameFont.Small;
					GUI.color = Color.white;
				}
			}
			LabelLeft(tDef.LabelCap, text, nestLevel, num);
			bool checkOn = filter.Allows(tDef);
			bool flag2 = checkOn;
			Widgets.Checkbox(new Vector2(LabelWidth, curY), ref checkOn, lineHeight, disabled: false, paintable: true);
			if (checkOn != flag2)
			{
				filter.SetAllow(tDef, checkOn);
			}
			EndLine();
		}

		public MultiCheckboxState AllowanceStateOf(TreeNode_ThingCategory cat)
		{
			int num = 0;
			int num2 = 0;
			foreach (ThingDef descendantThingDef in cat.catDef.DescendantThingDefs)
			{
				if (Visible(descendantThingDef))
				{
					num++;
					if (filter.Allows(descendantThingDef))
					{
						num2++;
					}
				}
			}
			bool flag = false;
			foreach (SpecialThingFilterDef descendantSpecialThingFilterDef in cat.catDef.DescendantSpecialThingFilterDefs)
			{
				if (Visible(descendantSpecialThingFilterDef) && !filter.Allows(descendantSpecialThingFilterDef))
				{
					flag = true;
					break;
				}
			}
			if (num2 == 0)
			{
				return MultiCheckboxState.Off;
			}
			if (num == num2 && !flag)
			{
				return MultiCheckboxState.On;
			}
			return MultiCheckboxState.Partial;
		}

		private bool Visible(ThingDef td)
		{
			if (td.menuHidden)
			{
				return false;
			}
			if (forceHiddenDefs != null && forceHiddenDefs.Contains(td))
			{
				return false;
			}
			if (parentFilter != null)
			{
				if (!parentFilter.Allows(td))
				{
					return false;
				}
				if (parentFilter.IsAlwaysDisallowedDueToSpecialFilters(td))
				{
					return false;
				}
			}
			return true;
		}

		private bool Visible(TreeNode_ThingCategory node)
		{
			return node.catDef.DescendantThingDefs.Any(Visible);
		}

		private bool Visible(SpecialThingFilterDef filter)
		{
			if (parentFilter != null && !parentFilter.Allows(filter))
			{
				return false;
			}
			if (hiddenSpecialFilters == null)
			{
				CalculateHiddenSpecialFilters();
			}
			for (int i = 0; i < hiddenSpecialFilters.Count; i++)
			{
				if (hiddenSpecialFilters[i] == filter)
				{
					return false;
				}
			}
			return true;
		}

		private void CalculateHiddenSpecialFilters()
		{
			hiddenSpecialFilters = new List<SpecialThingFilterDef>();
			if (tempForceHiddenSpecialFilters != null)
			{
				hiddenSpecialFilters.AddRange(tempForceHiddenSpecialFilters);
			}
			IEnumerable<SpecialThingFilterDef> enumerable = filter.DisplayRootCategory.catDef.DescendantSpecialThingFilterDefs.Concat(filter.DisplayRootCategory.catDef.ParentsSpecialThingFilterDefs);
			IEnumerable<ThingDef> enumerable2 = filter.DisplayRootCategory.catDef.DescendantThingDefs;
			if (parentFilter != null)
			{
				enumerable2 = from x in enumerable2
				where parentFilter.Allows(x)
				select x;
			}
			foreach (SpecialThingFilterDef item in enumerable)
			{
				bool flag = false;
				foreach (ThingDef item2 in enumerable2)
				{
					if (item.Worker.CanEverMatch(item2))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					hiddenSpecialFilters.Add(item);
				}
			}
		}
	}
}
