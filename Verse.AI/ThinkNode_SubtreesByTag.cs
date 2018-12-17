using System.Collections.Generic;
using System.Linq;

namespace Verse.AI
{
	public class ThinkNode_SubtreesByTag : ThinkNode
	{
		[NoTranslate]
		public string insertTag;

		[Unsaved]
		private List<ThinkTreeDef> matchedTrees;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_SubtreesByTag thinkNode_SubtreesByTag = (ThinkNode_SubtreesByTag)base.DeepCopy(resolve);
			thinkNode_SubtreesByTag.insertTag = insertTag;
			return thinkNode_SubtreesByTag;
		}

		protected override void ResolveSubnodes()
		{
		}

		public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
		{
			if (matchedTrees == null)
			{
				matchedTrees = new List<ThinkTreeDef>();
				foreach (ThinkTreeDef allDef in DefDatabase<ThinkTreeDef>.AllDefs)
				{
					if (allDef.insertTag == insertTag)
					{
						matchedTrees.Add(allDef);
					}
				}
				matchedTrees = (from tDef in matchedTrees
				orderby tDef.insertPriority descending
				select tDef).ToList();
			}
			for (int i = 0; i < matchedTrees.Count; i++)
			{
				ThinkResult result = matchedTrees[i].thinkRoot.TryIssueJobPackage(pawn, jobParams);
				if (result.IsValid)
				{
					return result;
				}
			}
			return ThinkResult.NoJob;
		}
	}
}
