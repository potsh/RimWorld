using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompMannable : ThingComp
	{
		private int lastManTick = -1;

		private Pawn lastManPawn;

		public bool MannedNow => Find.TickManager.TicksGame - lastManTick <= 1 && lastManPawn != null && lastManPawn.Spawned;

		public Pawn ManningPawn
		{
			get
			{
				if (!MannedNow)
				{
					return null;
				}
				return lastManPawn;
			}
		}

		public CompProperties_Mannable Props => (CompProperties_Mannable)props;

		public void ManForATick(Pawn pawn)
		{
			lastManTick = Find.TickManager.TicksGame;
			lastManPawn = pawn;
			pawn.mindState.lastMannedThing = parent;
		}

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
		{
			_003CCompFloatMenuOptions_003Ec__Iterator0 _003CCompFloatMenuOptions_003Ec__Iterator = (_003CCompFloatMenuOptions_003Ec__Iterator0)/*Error near IL_0036: stateMachine*/;
			if (pawn.RaceProps.ToolUser && pawn.CanReserveAndReach(parent, PathEndMode.InteractionCell, Danger.Deadly))
			{
				if (Props.manWorkType == WorkTags.None || pawn.story == null || !pawn.story.WorkTagIsDisabled(Props.manWorkType))
				{
					FloatMenuOption opt = new FloatMenuOption("OrderManThing".Translate(parent.LabelShort, parent), delegate
					{
						Job job = new Job(JobDefOf.ManTurret, _003CCompFloatMenuOptions_003Ec__Iterator._0024this.parent);
						pawn.jobs.TryTakeOrderedJob(job);
					});
					yield return opt;
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (Props.manWorkType == WorkTags.Violent)
				{
					yield return new FloatMenuOption("CannotManThing".Translate(parent.LabelShort, parent) + " (" + "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn) + ")", null);
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}
	}
}
