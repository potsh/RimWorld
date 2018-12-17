using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class GameCondition : IExposable
	{
		public GameConditionManager gameConditionManager;

		public GameConditionDef def;

		public int startTick;

		private int duration = -1;

		private bool permanent;

		private List<Map> cachedAffectedMaps = new List<Map>();

		private List<Map> cachedAffectedMapsForMaps = new List<Map>();

		private static List<GameConditionManager> tmpGameConditionManagers = new List<GameConditionManager>();

		protected Map SingleMap => gameConditionManager.ownerMap;

		public virtual string Label => def.label;

		public virtual string LabelCap => Label.CapitalizeFirst();

		public virtual bool Expired => !Permanent && Find.TickManager.TicksGame > startTick + Duration;

		public int TicksPassed => Find.TickManager.TicksGame - startTick;

		public int TicksLeft
		{
			get
			{
				if (Permanent)
				{
					Log.ErrorOnce("Trying to get ticks left of a permanent condition.", 384767654);
					return 360000000;
				}
				return Duration - TicksPassed;
			}
			set
			{
				Duration = TicksPassed + value;
			}
		}

		public bool Permanent
		{
			get
			{
				return permanent;
			}
			set
			{
				if (value)
				{
					duration = -1;
				}
				permanent = value;
			}
		}

		public int Duration
		{
			get
			{
				if (Permanent)
				{
					Log.ErrorOnce("Trying to get duration of a permanent condition.", 100394867);
					return 360000000;
				}
				return duration;
			}
			set
			{
				permanent = false;
				duration = value;
			}
		}

		public virtual string TooltipString
		{
			get
			{
				string labelCap = def.LabelCap;
				if (Permanent)
				{
					labelCap = labelCap + "\n" + "Permanent".Translate().CapitalizeFirst();
				}
				else
				{
					Vector2 location = (SingleMap != null) ? Find.WorldGrid.LongLatOf(SingleMap.Tile) : ((Find.CurrentMap != null) ? Find.WorldGrid.LongLatOf(Find.CurrentMap.Tile) : ((Find.AnyPlayerHomeMap == null) ? Vector2.zero : Find.WorldGrid.LongLatOf(Find.AnyPlayerHomeMap.Tile)));
					string text = labelCap;
					labelCap = text + "\n" + "Started".Translate() + ": " + GenDate.DateFullStringAt(GenDate.TickGameToAbs(startTick), location);
					text = labelCap;
					labelCap = text + "\n" + "Lasted".Translate() + ": " + TicksPassed.ToStringTicksToPeriod();
				}
				labelCap += "\n";
				return labelCap + "\n" + def.description;
			}
		}

		public List<Map> AffectedMaps
		{
			get
			{
				if (!GenCollection.ListsEqual(cachedAffectedMapsForMaps, Find.Maps))
				{
					cachedAffectedMapsForMaps.Clear();
					cachedAffectedMapsForMaps.AddRange(Find.Maps);
					cachedAffectedMaps.Clear();
					if (gameConditionManager.ownerMap != null)
					{
						cachedAffectedMaps.Add(gameConditionManager.ownerMap);
					}
					tmpGameConditionManagers.Clear();
					gameConditionManager.GetChildren(tmpGameConditionManagers);
					for (int i = 0; i < tmpGameConditionManagers.Count; i++)
					{
						if (tmpGameConditionManagers[i].ownerMap != null)
						{
							cachedAffectedMaps.Add(tmpGameConditionManagers[i].ownerMap);
						}
					}
					tmpGameConditionManagers.Clear();
				}
				return cachedAffectedMaps;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref startTick, "startTick", 0);
			Scribe_Values.Look(ref duration, "duration", 0);
			Scribe_Values.Look(ref permanent, "permanent", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.GameConditionPostLoadInit(this);
			}
		}

		public virtual void GameConditionTick()
		{
		}

		public virtual void GameConditionDraw(Map map)
		{
		}

		public virtual void Init()
		{
		}

		public virtual void End()
		{
			if (def.endMessage != null)
			{
				Messages.Message(def.endMessage, MessageTypeDefOf.NeutralEvent);
			}
			gameConditionManager.ActiveConditions.Remove(this);
		}

		public virtual float SkyGazeChanceFactor(Map map)
		{
			return 1f;
		}

		public virtual float SkyGazeJoyGainFactor(Map map)
		{
			return 1f;
		}

		public virtual float TemperatureOffset()
		{
			return 0f;
		}

		public virtual float SkyTargetLerpFactor(Map map)
		{
			return 0f;
		}

		public virtual SkyTarget? SkyTarget(Map map)
		{
			return null;
		}

		public virtual float AnimalDensityFactor(Map map)
		{
			return 1f;
		}

		public virtual float PlantDensityFactor(Map map)
		{
			return 1f;
		}

		public virtual bool AllowEnjoyableOutsideNow(Map map)
		{
			return true;
		}

		public virtual List<SkyOverlay> SkyOverlays(Map map)
		{
			return null;
		}

		public virtual void DoCellSteadyEffects(IntVec3 c, Map map)
		{
		}

		public virtual void PostMake()
		{
		}
	}
}
