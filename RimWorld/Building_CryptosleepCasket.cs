using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Building_CryptosleepCasket : Building_Casket
	{
		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				if (allowSpecialEffects)
				{
					SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map));
				}
				return true;
			}
			return false;
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
		{
			_003CGetFloatMenuOptions_003Ec__Iterator0 _003CGetFloatMenuOptions_003Ec__Iterator = (_003CGetFloatMenuOptions_003Ec__Iterator0)/*Error near IL_003c: stateMachine*/;
			using (IEnumerator<FloatMenuOption> enumerator = base.GetFloatMenuOptions(myPawn).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					FloatMenuOption o = enumerator.Current;
					yield return o;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (innerContainer.Count == 0)
			{
				if (myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
				{
					_003CGetFloatMenuOptions_003Ec__Iterator0 _003CGetFloatMenuOptions_003Ec__Iterator2 = (_003CGetFloatMenuOptions_003Ec__Iterator0)/*Error near IL_017a: stateMachine*/;
					JobDef jobDef = JobDefOf.EnterCryptosleepCasket;
					string jobStr = "EnterCryptosleepCasket".Translate();
					Action jobAction = delegate
					{
						Job job = new Job(jobDef, _003CGetFloatMenuOptions_003Ec__Iterator2._0024this);
						myPawn.jobs.TryTakeOrderedJob(job);
					};
					yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn, this);
					/*Error: Unable to find new state assignment for yield return*/;
				}
				FloatMenuOption failer = new FloatMenuOption("CannotUseNoPath".Translate(), null);
				yield return failer;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0226:
			/*Error near IL_0227: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo c = enumerator.Current;
					yield return c;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (base.Faction == Faction.OfPlayer && innerContainer.Count > 0 && def.building.isPlayerEjectable)
			{
				Command_Action eject = new Command_Action
				{
					action = ((Building_Casket)this).EjectContents,
					defaultLabel = "CommandPodEject".Translate(),
					defaultDesc = "CommandPodEjectDesc".Translate()
				};
				if (innerContainer.Count == 0)
				{
					eject.Disable("CommandPodEjectFailEmpty".Translate());
				}
				eject.hotKey = KeyBindingDefOf.Misc1;
				eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject");
				yield return (Gizmo)eject;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01c4:
			/*Error near IL_01c5: Unexpected return in MoveNext()*/;
		}

		public override void EjectContents()
		{
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			foreach (Thing item in (IEnumerable<Thing>)innerContainer)
			{
				Pawn pawn = item as Pawn;
				if (pawn != null)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filth_Slime);
					if (pawn.RaceProps.IsFlesh)
					{
						pawn.health.AddHediff(HediffDefOf.CryptosleepSickness);
					}
				}
			}
			if (!base.Destroyed)
			{
				SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map)));
			}
			base.EjectContents();
		}

		public static Building_CryptosleepCasket FindCryptosleepCasketFor(Pawn p, Pawn traveler, bool ignoreOtherReservations = false)
		{
			IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
			where typeof(Building_CryptosleepCasket).IsAssignableFrom(def.thingClass)
			select def;
			foreach (ThingDef item in enumerable)
			{
				Building_CryptosleepCasket building_CryptosleepCasket = (Building_CryptosleepCasket)GenClosest.ClosestThingReachable(p.Position, p.Map, ThingRequest.ForDef(item), PathEndMode.InteractionCell, TraverseParms.For(traveler), 9999f, delegate(Thing x)
				{
					int result;
					if (!((Building_CryptosleepCasket)x).HasAnyContents)
					{
						Pawn p2 = traveler;
						LocalTargetInfo target = x;
						bool ignoreOtherReservations2 = ignoreOtherReservations;
						result = (p2.CanReserve(target, 1, -1, null, ignoreOtherReservations2) ? 1 : 0);
					}
					else
					{
						result = 0;
					}
					return (byte)result != 0;
				});
				if (building_CryptosleepCasket != null)
				{
					return building_CryptosleepCasket;
				}
			}
			return null;
		}
	}
}
