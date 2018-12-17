using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;

namespace Verse
{
	public class LookTargets : IExposable
	{
		public List<GlobalTargetInfo> targets;

		public static LookTargets Invalid => null;

		public bool IsValid => PrimaryTarget.IsValid;

		public bool Any => targets.Count != 0;

		public GlobalTargetInfo PrimaryTarget
		{
			get
			{
				for (int i = 0; i < targets.Count; i++)
				{
					if (targets[i].IsValid)
					{
						return targets[i];
					}
				}
				if (targets.Count != 0)
				{
					return targets[0];
				}
				return GlobalTargetInfo.Invalid;
			}
		}

		public LookTargets()
		{
			targets = new List<GlobalTargetInfo>();
		}

		public LookTargets(Thing t)
		{
			targets = new List<GlobalTargetInfo>();
			targets.Add(t);
		}

		public LookTargets(WorldObject o)
		{
			targets = new List<GlobalTargetInfo>();
			targets.Add(o);
		}

		public LookTargets(IntVec3 c, Map map)
		{
			targets = new List<GlobalTargetInfo>();
			targets.Add(new GlobalTargetInfo(c, map));
		}

		public LookTargets(int tile)
		{
			targets = new List<GlobalTargetInfo>();
			targets.Add(new GlobalTargetInfo(tile));
		}

		public LookTargets(IEnumerable<GlobalTargetInfo> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			if (targets != null)
			{
				this.targets.AddRange(targets);
			}
		}

		public LookTargets(params GlobalTargetInfo[] targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			if (targets != null)
			{
				for (int i = 0; i < targets.Length; i++)
				{
					this.targets.Add(targets[i]);
				}
			}
		}

		public LookTargets(IEnumerable<TargetInfo> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			if (targets != null)
			{
				IList<TargetInfo> list = targets as IList<TargetInfo>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						this.targets.Add(list[i]);
					}
				}
				else
				{
					foreach (TargetInfo target in targets)
					{
						this.targets.Add(target);
					}
				}
			}
		}

		public LookTargets(params TargetInfo[] targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			if (targets != null)
			{
				for (int i = 0; i < targets.Length; i++)
				{
					this.targets.Add(targets[i]);
				}
			}
		}

		public LookTargets(IEnumerable<Thing> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendThingTargets(targets);
		}

		public LookTargets(IEnumerable<ThingWithComps> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendThingTargets(targets);
		}

		public LookTargets(IEnumerable<Pawn> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendThingTargets(targets);
		}

		public LookTargets(IEnumerable<Building> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendThingTargets(targets);
		}

		public LookTargets(IEnumerable<Plant> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendThingTargets(targets);
		}

		public LookTargets(IEnumerable<WorldObject> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendWorldObjectTargets(targets);
		}

		public LookTargets(IEnumerable<Caravan> targets)
		{
			this.targets = new List<GlobalTargetInfo>();
			AppendWorldObjectTargets(targets);
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref targets, "targets", LookMode.GlobalTargetInfo);
		}

		public static implicit operator LookTargets(Thing t)
		{
			return new LookTargets(t);
		}

		public static implicit operator LookTargets(WorldObject o)
		{
			return new LookTargets(o);
		}

		public static implicit operator LookTargets(TargetInfo target)
		{
			LookTargets lookTargets = new LookTargets();
			lookTargets.targets = new List<GlobalTargetInfo>();
			lookTargets.targets.Add(target);
			return lookTargets;
		}

		public static implicit operator LookTargets(List<TargetInfo> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(GlobalTargetInfo target)
		{
			LookTargets lookTargets = new LookTargets();
			lookTargets.targets = new List<GlobalTargetInfo>();
			lookTargets.targets.Add(target);
			return lookTargets;
		}

		public static implicit operator LookTargets(List<GlobalTargetInfo> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<Thing> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<ThingWithComps> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<Pawn> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<Building> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<Plant> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<WorldObject> targets)
		{
			return new LookTargets(targets);
		}

		public static implicit operator LookTargets(List<Caravan> targets)
		{
			return new LookTargets(targets);
		}

		public static bool SameTargets(LookTargets a, LookTargets b)
		{
			if (a == null)
			{
				return b == null || !b.Any;
			}
			if (b == null)
			{
				return a == null || !a.Any;
			}
			if (a.targets.Count != b.targets.Count)
			{
				return false;
			}
			for (int i = 0; i < a.targets.Count; i++)
			{
				if (a.targets[i] != b.targets[i])
				{
					return false;
				}
			}
			return true;
		}

		public void Highlight(bool arrow = true, bool colonistBar = true, bool circleOverlay = false)
		{
			for (int i = 0; i < targets.Count; i++)
			{
				TargetHighlighter.Highlight(targets[i], arrow, colonistBar, circleOverlay);
			}
		}

		private void AppendThingTargets<T>(IEnumerable<T> things) where T : Thing
		{
			if (things != null)
			{
				IList<T> list = things as IList<T>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						targets.Add(list[i]);
					}
				}
				else
				{
					foreach (T thing in things)
					{
						targets.Add(thing);
					}
				}
			}
		}

		private void AppendWorldObjectTargets<T>(IEnumerable<T> worldObjects) where T : WorldObject
		{
			if (worldObjects != null)
			{
				IList<T> list = worldObjects as IList<T>;
				if (list != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						targets.Add(list[i]);
					}
				}
				else
				{
					foreach (T worldObject in worldObjects)
					{
						targets.Add(worldObject);
					}
				}
			}
		}

		public void Notify_MapRemoved(Map map)
		{
			targets.RemoveAll((GlobalTargetInfo t) => t.Map == map);
		}
	}
}
