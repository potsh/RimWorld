using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class CompUsable : ThingComp
	{
		public CompProperties_Usable Props => (CompProperties_Usable)props;

		protected virtual string FloatMenuOptionLabel => Props.useLabel;

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn myPawn)
		{
			_003CCompFloatMenuOptions_003Ec__Iterator0 _003CCompFloatMenuOptions_003Ec__Iterator = (_003CCompFloatMenuOptions_003Ec__Iterator0)/*Error near IL_0042: stateMachine*/;
			if (CanBeUsedBy(myPawn, out string failReason))
			{
				if (myPawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly))
				{
					if (myPawn.CanReserve(parent))
					{
						if (myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
						{
							FloatMenuOption useopt = new FloatMenuOption(FloatMenuOptionLabel, delegate
							{
								if (myPawn.CanReserveAndReach(_003CCompFloatMenuOptions_003Ec__Iterator._0024this.parent, PathEndMode.Touch, Danger.Deadly))
								{
									foreach (CompUseEffect comp in _003CCompFloatMenuOptions_003Ec__Iterator._0024this.parent.GetComps<CompUseEffect>())
									{
										if (comp.SelectedUseOption(myPawn))
										{
											return;
										}
									}
									_003CCompFloatMenuOptions_003Ec__Iterator._0024this.TryStartUseJob(myPawn);
								}
							});
							yield return useopt;
							/*Error: Unable to find new state assignment for yield return*/;
						}
						yield return new FloatMenuOption(FloatMenuOptionLabel + " (" + "Incapable".Translate() + ")", null);
						/*Error: Unable to find new state assignment for yield return*/;
					}
					yield return new FloatMenuOption(FloatMenuOptionLabel + " (" + "Reserved".Translate() + ")", null);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				yield return new FloatMenuOption(FloatMenuOptionLabel + " (" + "NoPath".Translate() + ")", null);
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return new FloatMenuOption(FloatMenuOptionLabel + ((failReason == null) ? string.Empty : (" (" + failReason + ")")), null);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public void TryStartUseJob(Pawn user)
		{
			if (user.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly) && CanBeUsedBy(user, out string _))
			{
				Job job = new Job(Props.useJob, parent);
				user.jobs.TryTakeOrderedJob(job);
			}
		}

		public void UsedBy(Pawn p)
		{
			if (CanBeUsedBy(p, out string _))
			{
				foreach (CompUseEffect item in from x in parent.GetComps<CompUseEffect>()
				orderby x.OrderPriority descending
				select x)
				{
					try
					{
						item.DoEffect(p);
					}
					catch (Exception arg)
					{
						Log.Error("Error in CompUseEffect: " + arg);
					}
				}
			}
		}

		private bool CanBeUsedBy(Pawn p, out string failReason)
		{
			List<ThingComp> allComps = parent.AllComps;
			for (int i = 0; i < allComps.Count; i++)
			{
				CompUseEffect compUseEffect = allComps[i] as CompUseEffect;
				if (compUseEffect != null && !compUseEffect.CanBeUsedBy(p, out failReason))
				{
					return false;
				}
			}
			failReason = null;
			return true;
		}
	}
}
