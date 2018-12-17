namespace Verse.Grammar
{
	public class Rule_Number : Rule
	{
		private IntRange range = IntRange.zero;

		public int selectionWeight = 1;

		public override float BaseSelectionWeight => (float)selectionWeight;

		public override Rule DeepCopy()
		{
			Rule_Number rule_Number = (Rule_Number)base.DeepCopy();
			rule_Number.range = range;
			rule_Number.selectionWeight = selectionWeight;
			return rule_Number;
		}

		public override string Generate()
		{
			return range.RandomInRange.ToString();
		}

		public override string ToString()
		{
			return keyword + "->(number: " + range.ToString() + ")";
		}
	}
}
