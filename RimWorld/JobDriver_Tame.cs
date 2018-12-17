using System.Collections.Generic;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Tame : JobDriver_InteractAnimal
	{
		protected override bool CanInteractNow => !TameUtility.TriedToTameTooRecently(base.Animal);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			using (IEnumerator<Toil> enumerator = base.MakeNewToils().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Toil toil = enumerator.Current;
					yield return toil;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			this.FailOn(() => ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_00b6: stateMachine*/)._0024this.Map.designationManager.DesignationOn(((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_00b6: stateMachine*/)._0024this.Animal, DesignationDefOf.Tame) == null && !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_00b6: stateMachine*/)._0024this.OnLastToil);
			yield break;
			IL_00d1:
			/*Error near IL_00d2: Unexpected return in MoveNext()*/;
		}

		protected override Toil FinalInteractToil()
		{
			return Toils_Interpersonal.TryRecruit(TargetIndex.A);
		}
	}
}
