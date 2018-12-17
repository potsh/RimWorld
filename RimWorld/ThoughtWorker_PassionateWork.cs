using Verse;
using Verse.AI;

namespace RimWorld
{
	public class ThoughtWorker_PassionateWork : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			JobDriver curDriver = p.jobs.curDriver;
			if (curDriver == null)
			{
				return ThoughtState.Inactive;
			}
			Pawn_SkillTracker skills = p.skills;
			if (skills == null)
			{
				return ThoughtState.Inactive;
			}
			SkillDef activeSkill = curDriver.ActiveSkill;
			if (activeSkill == null)
			{
				return ThoughtState.Inactive;
			}
			SkillRecord skill = p.skills.GetSkill(curDriver.ActiveSkill);
			if (skill == null)
			{
				return ThoughtState.Inactive;
			}
			if (skill.passion == Passion.Minor)
			{
				return ThoughtState.ActiveAtStage(0);
			}
			if (skill.passion == Passion.Major)
			{
				return ThoughtState.ActiveAtStage(1);
			}
			return ThoughtState.Inactive;
		}
	}
}
