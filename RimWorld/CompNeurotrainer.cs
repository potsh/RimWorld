using Verse;

namespace RimWorld
{
	public class CompNeurotrainer : CompUsable
	{
		public SkillDef skill;

		protected override string FloatMenuOptionLabel => string.Format(base.Props.useLabel, skill.skillLabel);

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Defs.Look(ref skill, "skill");
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			skill = DefDatabase<SkillDef>.GetRandom();
		}

		public override string TransformLabel(string label)
		{
			return skill.LabelCap + " " + label;
		}

		public override bool AllowStackWith(Thing other)
		{
			if (!base.AllowStackWith(other))
			{
				return false;
			}
			CompNeurotrainer compNeurotrainer = other.TryGetComp<CompNeurotrainer>();
			if (compNeurotrainer == null || compNeurotrainer.skill != skill)
			{
				return false;
			}
			return true;
		}

		public override void PostSplitOff(Thing piece)
		{
			base.PostSplitOff(piece);
			CompNeurotrainer compNeurotrainer = piece.TryGetComp<CompNeurotrainer>();
			if (compNeurotrainer != null)
			{
				compNeurotrainer.skill = skill;
			}
		}
	}
}
