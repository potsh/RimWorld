namespace Verse.AI
{
	public class ThinkNode_ChancePerHour_Constant : ThinkNode_ChancePerHour
	{
		private float mtbHours = -1f;

		private float mtbDays = -1f;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			ThinkNode_ChancePerHour_Constant thinkNode_ChancePerHour_Constant = (ThinkNode_ChancePerHour_Constant)base.DeepCopy(resolve);
			thinkNode_ChancePerHour_Constant.mtbHours = mtbHours;
			thinkNode_ChancePerHour_Constant.mtbDays = mtbDays;
			return thinkNode_ChancePerHour_Constant;
		}

		protected override float MtbHours(Pawn Pawn)
		{
			if (mtbDays > 0f)
			{
				return mtbDays * 24f;
			}
			return mtbHours;
		}
	}
}
