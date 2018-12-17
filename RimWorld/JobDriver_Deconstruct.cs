using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Deconstruct : JobDriver_RemoveBuilding
	{
		private const float MaxDeconstructWork = 3000f;

		private const float MinDeconstructWork = 20f;

		protected override DesignationDef Designation => DesignationDefOf.Deconstruct;

		protected override float TotalNeededWork
		{
			get
			{
				Building building = base.Building;
				float statValue = building.GetStatValue(StatDefOf.WorkToBuild);
				return Mathf.Clamp(statValue, 20f, 3000f);
			}
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOn(() => ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0029: stateMachine*/)._0024this.Building == null || !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0029: stateMachine*/)._0024this.Building.DeconstructibleBy(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0029: stateMachine*/)._0024this.pawn.Faction));
			using (IEnumerator<Toil> enumerator = base.MakeNewToils().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Toil t = enumerator.Current;
					yield return t;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_00d1:
			/*Error near IL_00d2: Unexpected return in MoveNext()*/;
		}

		protected override void FinishedRemoving()
		{
			base.Target.Destroy(DestroyMode.Deconstruct);
			pawn.records.Increment(RecordDefOf.ThingsDeconstructed);
		}

		protected override void TickAction()
		{
			if (base.Building.def.CostListAdjusted(base.Building.Stuff).Count > 0)
			{
				pawn.skills.Learn(SkillDefOf.Construction, 0.25f);
			}
		}
	}
}
