using RimWorld;
using System.Collections.Generic;

namespace Verse.AI
{
	public class AttackTargetsCache
	{
		private Map map;

		private HashSet<IAttackTarget> allTargets = new HashSet<IAttackTarget>();

		private Dictionary<Faction, HashSet<IAttackTarget>> targetsHostileToFaction = new Dictionary<Faction, HashSet<IAttackTarget>>();

		private HashSet<Pawn> pawnsInAggroMentalState = new HashSet<Pawn>();

		private static List<IAttackTarget> targets = new List<IAttackTarget>();

		private static HashSet<IAttackTarget> emptySet = new HashSet<IAttackTarget>();

		private static List<IAttackTarget> tmpTargets = new List<IAttackTarget>();

		private static List<IAttackTarget> tmpToUpdate = new List<IAttackTarget>();

		public HashSet<IAttackTarget> TargetsHostileToColony => TargetsHostileToFaction(Faction.OfPlayer);

		public AttackTargetsCache(Map map)
		{
			this.map = map;
		}

		public static void AttackTargetsCacheStaticUpdate()
		{
			targets.Clear();
		}

		public void UpdateTarget(IAttackTarget t)
		{
			if (allTargets.Contains(t))
			{
				DeregisterTarget(t);
				Thing thing = t.Thing;
				if (thing.Spawned && thing.Map == map)
				{
					RegisterTarget(t);
				}
			}
		}

		public List<IAttackTarget> GetPotentialTargetsFor(IAttackTargetSearcher th)
		{
			Thing thing = th.Thing;
			targets.Clear();
			Faction faction = thing.Faction;
			if (faction != null)
			{
				if (UnityData.isDebugBuild)
				{
					Debug_AssertHostile(faction, TargetsHostileToFaction(faction));
				}
				foreach (IAttackTarget item in TargetsHostileToFaction(faction))
				{
					if (thing.HostileTo(item.Thing))
					{
						targets.Add(item);
					}
				}
			}
			foreach (Pawn item2 in pawnsInAggroMentalState)
			{
				if (thing.HostileTo(item2))
				{
					targets.Add(item2);
				}
			}
			Pawn pawn = th as Pawn;
			if (pawn != null && PrisonBreakUtility.IsPrisonBreaking(pawn))
			{
				Faction hostFaction = pawn.guest.HostFaction;
				List<Pawn> list = map.mapPawns.SpawnedPawnsInFaction(hostFaction);
				for (int i = 0; i < list.Count; i++)
				{
					if (thing.HostileTo(list[i]))
					{
						targets.Add(list[i]);
					}
				}
			}
			return targets;
		}

		public HashSet<IAttackTarget> TargetsHostileToFaction(Faction f)
		{
			if (f == null)
			{
				Log.Warning("Called TargetsHostileToFaction with null faction.");
				return emptySet;
			}
			if (targetsHostileToFaction.ContainsKey(f))
			{
				return targetsHostileToFaction[f];
			}
			return emptySet;
		}

		public void Notify_ThingSpawned(Thing th)
		{
			IAttackTarget attackTarget = th as IAttackTarget;
			if (attackTarget != null)
			{
				RegisterTarget(attackTarget);
			}
		}

		public void Notify_ThingDespawned(Thing th)
		{
			IAttackTarget attackTarget = th as IAttackTarget;
			if (attackTarget != null)
			{
				DeregisterTarget(attackTarget);
			}
		}

		public void Notify_FactionHostilityChanged(Faction f1, Faction f2)
		{
			tmpTargets.Clear();
			foreach (IAttackTarget allTarget in allTargets)
			{
				Thing thing = allTarget.Thing;
				Pawn pawn = thing as Pawn;
				if (thing.Faction == f1 || thing.Faction == f2 || (pawn != null && pawn.HostFaction == f1) || (pawn != null && pawn.HostFaction == f2))
				{
					tmpTargets.Add(allTarget);
				}
			}
			for (int i = 0; i < tmpTargets.Count; i++)
			{
				UpdateTarget(tmpTargets[i]);
			}
			tmpTargets.Clear();
		}

		private void RegisterTarget(IAttackTarget target)
		{
			if (allTargets.Contains(target))
			{
				Log.Warning("Tried to register the same target twice " + target.ToStringSafe() + " in " + GetType());
			}
			else
			{
				Thing thing = target.Thing;
				if (!thing.Spawned)
				{
					Log.Warning("Tried to register unspawned thing " + thing.ToStringSafe() + " in " + GetType());
				}
				else if (thing.Map != map)
				{
					Log.Warning("Tried to register attack target " + thing.ToStringSafe() + " but its Map is not this one.");
				}
				else
				{
					allTargets.Add(target);
					List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
					for (int i = 0; i < allFactionsListForReading.Count; i++)
					{
						if (thing.HostileTo(allFactionsListForReading[i]))
						{
							if (!targetsHostileToFaction.ContainsKey(allFactionsListForReading[i]))
							{
								targetsHostileToFaction.Add(allFactionsListForReading[i], new HashSet<IAttackTarget>());
							}
							targetsHostileToFaction[allFactionsListForReading[i]].Add(target);
						}
					}
					Pawn pawn = target as Pawn;
					if (pawn != null && pawn.InAggroMentalState)
					{
						pawnsInAggroMentalState.Add(pawn);
					}
				}
			}
		}

		private void DeregisterTarget(IAttackTarget target)
		{
			if (!allTargets.Contains(target))
			{
				Log.Warning("Tried to deregister " + target + " but it's not in " + GetType());
			}
			else
			{
				allTargets.Remove(target);
				foreach (KeyValuePair<Faction, HashSet<IAttackTarget>> item in targetsHostileToFaction)
				{
					HashSet<IAttackTarget> value = item.Value;
					value.Remove(target);
				}
				Pawn pawn = target as Pawn;
				if (pawn != null)
				{
					pawnsInAggroMentalState.Remove(pawn);
				}
			}
		}

		private void Debug_AssertHostile(Faction f, HashSet<IAttackTarget> targets)
		{
			tmpToUpdate.Clear();
			foreach (IAttackTarget target in targets)
			{
				if (!target.Thing.HostileTo(f))
				{
					tmpToUpdate.Add(target);
					Log.Error("Target " + target.ToStringSafe() + " is not hostile to " + f.ToStringSafe() + " (in " + GetType().Name + ") but it's in the list (forgot to update the target somewhere?). Trying to update the target...");
				}
			}
			for (int i = 0; i < tmpToUpdate.Count; i++)
			{
				UpdateTarget(tmpToUpdate[i]);
			}
			tmpToUpdate.Clear();
		}

		public bool Debug_CheckIfInAllTargets(IAttackTarget t)
		{
			return t != null && allTargets.Contains(t);
		}

		public bool Debug_CheckIfHostileToFaction(Faction f, IAttackTarget t)
		{
			if (f == null)
			{
				return false;
			}
			return t != null && targetsHostileToFaction[f].Contains(t);
		}
	}
}
