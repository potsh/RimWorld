using System.Collections.Generic;

namespace Verse
{
	public class TreeNode_ThingCategory : TreeNode
	{
		public ThingCategoryDef catDef;

		public string Label => catDef.label;

		public string LabelCap => Label.CapitalizeFirst();

		public IEnumerable<TreeNode_ThingCategory> ChildCategoryNodesAndThis
		{
			get
			{
				using (IEnumerator<ThingCategoryDef> enumerator = catDef.ThisAndChildCategoryDefs.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ThingCategoryDef other = enumerator.Current;
						yield return other.treeNode;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public IEnumerable<TreeNode_ThingCategory> ChildCategoryNodes
		{
			get
			{
				using (List<ThingCategoryDef>.Enumerator enumerator = catDef.childCategories.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ThingCategoryDef other = enumerator.Current;
						yield return other.treeNode;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00be:
				/*Error near IL_00bf: Unexpected return in MoveNext()*/;
			}
		}

		public TreeNode_ThingCategory(ThingCategoryDef def)
		{
			catDef = def;
		}

		public override string ToString()
		{
			return catDef.defName;
		}
	}
}
