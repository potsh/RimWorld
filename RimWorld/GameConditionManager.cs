using RimWorld.Planet;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public sealed class GameConditionManager : IExposable
	{
		public Map ownerMap;

		private List<GameCondition> activeConditions = new List<GameCondition>();

		public List<GameCondition> ActiveConditions => activeConditions;

		public GameConditionManager Parent => (ownerMap != null) ? Find.World.gameConditionManager : null;

		public GameConditionManager(Map map)
		{
			ownerMap = map;
		}

		public GameConditionManager(World world)
		{
		}

		public void RegisterCondition(GameCondition cond)
		{
			activeConditions.Add(cond);
			cond.gameConditionManager = this;
			cond.Init();
		}

		public void ExposeData()
		{
			Scribe_Collections.Look(ref activeConditions, "activeConditions", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				for (int i = 0; i < activeConditions.Count; i++)
				{
					activeConditions[i].gameConditionManager = this;
				}
			}
		}

		public void GameConditionManagerTick()
		{
			for (int num = activeConditions.Count - 1; num >= 0; num--)
			{
				GameCondition gameCondition = activeConditions[num];
				if (gameCondition.Expired)
				{
					gameCondition.End();
				}
				else
				{
					gameCondition.GameConditionTick();
				}
			}
		}

		public void GameConditionManagerDraw(Map map)
		{
			for (int num = activeConditions.Count - 1; num >= 0; num--)
			{
				activeConditions[num].GameConditionDraw(map);
			}
			if (Parent != null)
			{
				Parent.GameConditionManagerDraw(map);
			}
		}

		public void DoSteadyEffects(IntVec3 c, Map map)
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				activeConditions[i].DoCellSteadyEffects(c, map);
			}
			if (Parent != null)
			{
				Parent.DoSteadyEffects(c, map);
			}
		}

		public bool ConditionIsActive(GameConditionDef def)
		{
			return GetActiveCondition(def) != null;
		}

		public GameCondition GetActiveCondition(GameConditionDef def)
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				if (def == activeConditions[i].def)
				{
					return activeConditions[i];
				}
			}
			if (Parent != null)
			{
				return Parent.GetActiveCondition(def);
			}
			return null;
		}

		public T GetActiveCondition<T>() where T : GameCondition
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				T val = activeConditions[i] as T;
				if (val != null)
				{
					return val;
				}
			}
			if (Parent != null)
			{
				return Parent.GetActiveCondition<T>();
			}
			return (T)null;
		}

		public void GetChildren(List<GameConditionManager> outChildren)
		{
			if (this == Find.World.gameConditionManager)
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					outChildren.Add(maps[i].gameConditionManager);
				}
			}
		}

		public float TotalHeightAt(float width)
		{
			float num = 0f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num += Text.CalcHeight(activeConditions[i].LabelCap, width);
			}
			if (Parent != null)
			{
				num += Parent.TotalHeightAt(width);
			}
			return num;
		}

		public void DoConditionsUI(Rect rect)
		{
			GUI.BeginGroup(rect);
			float num = 0f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				float width = rect.width - 15f;
				Rect rect2 = new Rect(0f, num, width, Text.CalcHeight(activeConditions[i].LabelCap, width));
				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleRight;
				Widgets.Label(rect2, activeConditions[i].LabelCap);
				GameCondition localCond = activeConditions[i];
				TooltipHandler.TipRegion(rect2, () => localCond.TooltipString, i * 631);
				num += rect2.height;
			}
			rect.yMin += num;
			GUI.EndGroup();
			Text.Anchor = TextAnchor.UpperLeft;
			if (Parent != null)
			{
				Parent.DoConditionsUI(rect);
			}
		}

		public void GetAllGameConditionsAffectingMap(Map map, List<GameCondition> listToFill)
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				listToFill.Add(activeConditions[i]);
			}
			if (Parent != null)
			{
				Parent.GetAllGameConditionsAffectingMap(map, listToFill);
			}
		}

		internal float AggregateTemperatureOffset()
		{
			float num = 0f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num += activeConditions[i].TemperatureOffset();
			}
			if (Parent != null)
			{
				num += Parent.AggregateTemperatureOffset();
			}
			return num;
		}

		internal float AggregateAnimalDensityFactor(Map map)
		{
			float num = 1f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num *= activeConditions[i].AnimalDensityFactor(map);
			}
			if (Parent != null)
			{
				num *= Parent.AggregateAnimalDensityFactor(map);
			}
			return num;
		}

		internal float AggregatePlantDensityFactor(Map map)
		{
			float num = 1f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num *= activeConditions[i].PlantDensityFactor(map);
			}
			if (Parent != null)
			{
				num *= Parent.AggregatePlantDensityFactor(map);
			}
			return num;
		}

		internal float AggregateSkyGazeJoyGainFactor(Map map)
		{
			float num = 1f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num *= activeConditions[i].SkyGazeJoyGainFactor(map);
			}
			if (Parent != null)
			{
				num *= Parent.AggregateSkyGazeJoyGainFactor(map);
			}
			return num;
		}

		internal float AggregateSkyGazeChanceFactor(Map map)
		{
			float num = 1f;
			for (int i = 0; i < activeConditions.Count; i++)
			{
				num *= activeConditions[i].SkyGazeChanceFactor(map);
			}
			if (Parent != null)
			{
				num *= Parent.AggregateSkyGazeChanceFactor(map);
			}
			return num;
		}

		internal bool AllowEnjoyableOutsideNow(Map map)
		{
			GameConditionDef reason;
			return AllowEnjoyableOutsideNow(map, out reason);
		}

		internal bool AllowEnjoyableOutsideNow(Map map, out GameConditionDef reason)
		{
			for (int i = 0; i < activeConditions.Count; i++)
			{
				GameCondition gameCondition = activeConditions[i];
				if (!gameCondition.AllowEnjoyableOutsideNow(map))
				{
					reason = gameCondition.def;
					return false;
				}
			}
			reason = null;
			if (Parent != null)
			{
				return Parent.AllowEnjoyableOutsideNow(map, out reason);
			}
			return true;
		}

		public string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GameCondition activeCondition in activeConditions)
			{
				stringBuilder.AppendLine(Scribe.saver.DebugOutputFor(activeCondition));
			}
			return stringBuilder.ToString();
		}
	}
}
