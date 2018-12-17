using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimWorld
{
	public class Targeter
	{
		public Verb targetingVerb;

		public List<Pawn> targetingVerbAdditionalPawns;

		private Action<LocalTargetInfo> action;

		private Pawn caster;

		private TargetingParameters targetParams;

		private Action actionWhenFinished;

		private Texture2D mouseAttachment;

		public bool IsTargeting => targetingVerb != null || action != null;

		public void BeginTargeting(Verb verb)
		{
			if (verb.verbProps.targetable)
			{
				targetingVerb = verb;
				targetingVerbAdditionalPawns = new List<Pawn>();
			}
			else
			{
				Job job = new Job(JobDefOf.UseVerbOnThing);
				job.verbToUse = verb;
				verb.CasterPawn.jobs.StartJob(job);
			}
			action = null;
			caster = null;
			targetParams = null;
			actionWhenFinished = null;
			mouseAttachment = null;
		}

		public void BeginTargeting(TargetingParameters targetParams, Action<LocalTargetInfo> action, Pawn caster = null, Action actionWhenFinished = null, Texture2D mouseAttachment = null)
		{
			targetingVerb = null;
			targetingVerbAdditionalPawns = null;
			this.action = action;
			this.targetParams = targetParams;
			this.caster = caster;
			this.actionWhenFinished = actionWhenFinished;
			this.mouseAttachment = mouseAttachment;
		}

		public void StopTargeting()
		{
			if (actionWhenFinished != null)
			{
				Action action = actionWhenFinished;
				actionWhenFinished = null;
				action();
			}
			targetingVerb = null;
			this.action = null;
		}

		public void ProcessInputEvents()
		{
			ConfirmStillValid();
			if (IsTargeting)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					if (targetingVerb != null)
					{
						OrderVerbForceTarget();
					}
					if (action != null)
					{
						LocalTargetInfo obj = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: false);
						if (obj.IsValid)
						{
							action(obj);
						}
					}
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					StopTargeting();
					Event.current.Use();
				}
				if ((Event.current.type == EventType.MouseDown && Event.current.button == 1) || KeyBindingDefOf.Cancel.KeyDownEvent)
				{
					SoundDefOf.CancelMode.PlayOneShotOnCamera();
					StopTargeting();
					Event.current.Use();
				}
			}
		}

		public void TargeterOnGUI()
		{
			if (targetingVerb != null)
			{
				Texture2D icon = (!CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true).IsValid) ? TexCommand.CannotShoot : ((!(targetingVerb.UIIcon != BaseContent.BadTex)) ? TexCommand.Attack : targetingVerb.UIIcon);
				GenUI.DrawMouseAttachment(icon);
			}
			if (action != null)
			{
				Texture2D icon2 = mouseAttachment ?? TexCommand.Attack;
				GenUI.DrawMouseAttachment(icon2);
			}
		}

		public void TargeterUpdate()
		{
			if (targetingVerb != null)
			{
				targetingVerb.verbProps.DrawRadiusRing(targetingVerb.caster.Position);
				LocalTargetInfo targ = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
				if (targ.IsValid)
				{
					GenDraw.DrawTargetHighlight(targ);
					bool needLOSToCenter;
					float num = targetingVerb.HighlightFieldRadiusAroundTarget(out needLOSToCenter);
					if (num > 0.2f && targetingVerb.TryFindShootLineFromTo(targetingVerb.caster.Position, targ, out ShootLine resultingLine))
					{
						if (needLOSToCenter)
						{
							GenExplosion.RenderPredictedAreaOfEffect(resultingLine.Dest, num);
						}
						else
						{
							GenDraw.DrawFieldEdges((from x in GenRadial.RadialCellsAround(resultingLine.Dest, num, useCenter: true)
							where x.InBounds(Find.CurrentMap)
							select x).ToList());
						}
					}
				}
			}
			if (action != null)
			{
				LocalTargetInfo targ2 = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: false);
				if (targ2.IsValid)
				{
					GenDraw.DrawTargetHighlight(targ2);
				}
			}
		}

		public bool IsPawnTargeting(Pawn p)
		{
			if (caster == p)
			{
				return true;
			}
			if (targetingVerb != null && targetingVerb.CasterIsPawn)
			{
				if (targetingVerb.CasterPawn == p)
				{
					return true;
				}
				for (int i = 0; i < targetingVerbAdditionalPawns.Count; i++)
				{
					if (targetingVerbAdditionalPawns[i] == p)
					{
						return true;
					}
				}
			}
			return false;
		}

		private void ConfirmStillValid()
		{
			if (caster != null && (caster.Map != Find.CurrentMap || caster.Destroyed || !Find.Selector.IsSelected(caster)))
			{
				StopTargeting();
			}
			if (targetingVerb != null)
			{
				Selector selector = Find.Selector;
				if (targetingVerb.caster.Map != Find.CurrentMap || targetingVerb.caster.Destroyed || !selector.IsSelected(targetingVerb.caster))
				{
					StopTargeting();
				}
				else
				{
					int num = 0;
					while (true)
					{
						if (num >= targetingVerbAdditionalPawns.Count)
						{
							return;
						}
						if (targetingVerbAdditionalPawns[num].Destroyed || !selector.IsSelected(targetingVerbAdditionalPawns[num]))
						{
							break;
						}
						num++;
					}
					StopTargeting();
				}
			}
		}

		private void OrderVerbForceTarget()
		{
			if (targetingVerb.CasterIsPawn)
			{
				OrderPawnForceTarget(targetingVerb);
				for (int i = 0; i < targetingVerbAdditionalPawns.Count; i++)
				{
					Verb verb = GetTargetingVerb(targetingVerbAdditionalPawns[i]);
					if (verb != null)
					{
						OrderPawnForceTarget(verb);
					}
				}
			}
			else
			{
				int numSelected = Find.Selector.NumSelected;
				List<object> selectedObjects = Find.Selector.SelectedObjects;
				for (int j = 0; j < numSelected; j++)
				{
					Building_Turret building_Turret = selectedObjects[j] as Building_Turret;
					if (building_Turret != null && building_Turret.Map == Find.CurrentMap)
					{
						LocalTargetInfo targ = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
						building_Turret.OrderAttack(targ);
					}
				}
			}
		}

		private void OrderPawnForceTarget(Verb verb)
		{
			LocalTargetInfo localTargetInfo = CurrentTargetUnderMouse(mustBeHittableNowIfNotMelee: true);
			if (localTargetInfo.IsValid)
			{
				if (verb.verbProps.IsMeleeAttack)
				{
					Job job = new Job(JobDefOf.AttackMelee, localTargetInfo);
					job.playerForced = true;
					Pawn pawn = localTargetInfo.Thing as Pawn;
					if (pawn != null)
					{
						job.killIncappedTarget = pawn.Downed;
					}
					verb.CasterPawn.jobs.TryTakeOrderedJob(job);
				}
				else
				{
					float num = verb.verbProps.EffectiveMinRange(localTargetInfo, verb.CasterPawn);
					if ((float)verb.CasterPawn.Position.DistanceToSquared(localTargetInfo.Cell) < num * num && verb.CasterPawn.Position.AdjacentTo8WayOrInside(localTargetInfo.Cell))
					{
						Messages.Message("MessageCantShootInMelee".Translate(), verb.CasterPawn, MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						JobDef def = (!verb.verbProps.ai_IsWeapon) ? JobDefOf.UseVerbOnThing : JobDefOf.AttackStatic;
						Job job2 = new Job(def);
						job2.verbToUse = verb;
						job2.targetA = localTargetInfo;
						job2.endIfCantShootInMelee = true;
						verb.CasterPawn.jobs.TryTakeOrderedJob(job2);
					}
				}
			}
		}

		private LocalTargetInfo CurrentTargetUnderMouse(bool mustBeHittableNowIfNotMelee)
		{
			if (!IsTargeting)
			{
				return LocalTargetInfo.Invalid;
			}
			TargetingParameters clickParams = (targetingVerb == null) ? targetParams : targetingVerb.verbProps.targetParams;
			LocalTargetInfo localTargetInfo = LocalTargetInfo.Invalid;
			using (IEnumerator<LocalTargetInfo> enumerator = GenUI.TargetsAtMouse(clickParams).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					LocalTargetInfo current = enumerator.Current;
					localTargetInfo = current;
				}
			}
			if (localTargetInfo.IsValid && mustBeHittableNowIfNotMelee && !(localTargetInfo.Thing is Pawn) && targetingVerb != null && !targetingVerb.verbProps.IsMeleeAttack)
			{
				if (targetingVerbAdditionalPawns != null && targetingVerbAdditionalPawns.Any())
				{
					bool flag = false;
					for (int i = 0; i < targetingVerbAdditionalPawns.Count; i++)
					{
						Verb verb = GetTargetingVerb(targetingVerbAdditionalPawns[i]);
						if (verb != null && verb.CanHitTarget(localTargetInfo))
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						localTargetInfo = LocalTargetInfo.Invalid;
					}
				}
				else if (!targetingVerb.CanHitTarget(localTargetInfo))
				{
					localTargetInfo = LocalTargetInfo.Invalid;
				}
			}
			return localTargetInfo;
		}

		private Verb GetTargetingVerb(Pawn pawn)
		{
			return pawn.equipment.AllEquipmentVerbs.FirstOrDefault((Verb x) => x.verbProps == targetingVerb.verbProps);
		}
	}
}
