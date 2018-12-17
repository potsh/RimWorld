using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class StorytellerUtility
	{
		private const float GlobalPointsMin = 35f;

		private const float GlobalPointsMax = 20000f;

		public const float BuildingWealthFactor = 0.5f;

		private static readonly SimpleCurve PointsPerWealthCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(14000f, 0f),
			new CurvePoint(400000f, 2400f),
			new CurvePoint(700000f, 3600f),
			new CurvePoint(1000000f, 4200f)
		};

		private const float PointsPerTameNonDownedCombatTrainableAnimalCombatPower = 0.08f;

		private const float PointsPerPlayerPawnFactorInContainer = 0.3f;

		private const float PointsPerPlayerPawnHealthSummaryLerpAmount = 0.65f;

		private static readonly SimpleCurve PointsPerColonistByWealthCurve = new SimpleCurve
		{
			new CurvePoint(0f, 15f),
			new CurvePoint(10000f, 15f),
			new CurvePoint(400000f, 140f),
			new CurvePoint(1000000f, 200f)
		};

		public const float CaravanWealthPointsFactor = 0.7f;

		public const float CaravanAnimalPointsFactor = 0.7f;

		public static readonly FloatRange CaravanPointsRandomFactorRange = new FloatRange(0.7f, 0.9f);

		private static readonly SimpleCurve AllyIncidentFractionFromAllyFraction = new SimpleCurve
		{
			new CurvePoint(1f, 1f),
			new CurvePoint(0.25f, 0.6f)
		};

		private static Dictionary<IIncidentTarget, StoryState> tmpOldStoryStates = new Dictionary<IIncidentTarget, StoryState>();

		public static IncidentParms DefaultParmsNow(IncidentCategoryDef incCat, IIncidentTarget target)
		{
			if (incCat == null)
			{
				Log.Warning("Trying to get default parms for null incident category.");
			}
			IncidentParms incidentParms = new IncidentParms();
			incidentParms.target = target;
			if (incCat.needsParmsPoints)
			{
				incidentParms.points = DefaultThreatPointsNow(target);
			}
			return incidentParms;
		}

		public static float DefaultThreatPointsNow(IIncidentTarget target)
		{
			float playerWealthForStoryteller = target.PlayerWealthForStoryteller;
			float num = PointsPerWealthCurve.Evaluate(playerWealthForStoryteller);
			float num2 = 0f;
			foreach (Pawn item in target.PlayerPawnsForStoryteller)
			{
				float num3 = 0f;
				if (item.IsFreeColonist)
				{
					num3 = PointsPerColonistByWealthCurve.Evaluate(playerWealthForStoryteller);
				}
				else if (item.RaceProps.Animal && item.Faction == Faction.OfPlayer && !item.Downed && item.training.CanAssignToTrain(TrainableDefOf.Release).Accepted)
				{
					num3 = 0.08f * item.kindDef.combatPower;
					if (target is Caravan)
					{
						num3 *= 0.7f;
					}
				}
				if (num3 > 0f)
				{
					if (item.ParentHolder != null && item.ParentHolder is Building_CryptosleepCasket)
					{
						num3 *= 0.3f;
					}
					num3 = Mathf.Lerp(num3, num3 * item.health.summaryHealth.SummaryHealthPercent, 0.65f);
					num2 += num3;
				}
			}
			float num4 = num + num2;
			num4 *= target.IncidentPointsRandomFactorRange.RandomInRange;
			float totalThreatPointsFactor = Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor;
			float num5 = Mathf.Lerp(1f, totalThreatPointsFactor, Find.Storyteller.difficulty.adaptationEffectFactor);
			num4 *= num5;
			num4 *= Find.Storyteller.difficulty.threatScale;
			num4 *= Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassed);
			return Mathf.Clamp(num4, 35f, 20000f);
		}

		public static float DefaultSiteThreatPointsNow()
		{
			return SiteTuning.ThreatPointsToSiteThreatPointsCurve.Evaluate(DefaultThreatPointsNow(Find.World)) * SiteTuning.SitePointRandomFactorRange.RandomInRange;
		}

		public static float AllyIncidentFraction(bool fullAlliesOnly)
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!allFactionsListForReading[i].def.hidden && !allFactionsListForReading[i].IsPlayer)
				{
					if (allFactionsListForReading[i].def.CanEverBeNonHostile)
					{
						num2++;
					}
					if (allFactionsListForReading[i].PlayerRelationKind == FactionRelationKind.Ally || (!fullAlliesOnly && !allFactionsListForReading[i].HostileTo(Faction.OfPlayer)))
					{
						num++;
					}
				}
			}
			if (num == 0)
			{
				return -1f;
			}
			float x = (float)num / Mathf.Max((float)num2, 1f);
			return AllyIncidentFractionFromAllyFraction.Evaluate(x);
		}

		public static void ShowFutureIncidentsDebugLogFloatMenu(bool currentMapOnly)
		{
			List<FloatMenuOption> list = new List<FloatMenuOption>();
			list.Add(new FloatMenuOption("-All comps-", delegate
			{
				DebugLogTestFutureIncidents(currentMapOnly);
			}));
			List<StorytellerComp> storytellerComps = Find.Storyteller.storytellerComps;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				StorytellerComp comp = storytellerComps[i];
				list.Add(new FloatMenuOption(comp.ToString(), delegate
				{
					DebugLogTestFutureIncidents(currentMapOnly, comp);
				}));
			}
			Find.WindowStack.Add(new FloatMenu(list));
		}

		public static void DebugLogTestFutureIncidents(bool currentMapOnly, StorytellerComp onlyThisComp = null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = "Test future incidents for " + Find.Storyteller.def;
			string text2;
			if (onlyThisComp != null)
			{
				text2 = text;
				text = text2 + " (" + onlyThisComp + ")";
			}
			text2 = text;
			text = text2 + " (" + Find.TickManager.TicksGame.TicksToDays().ToString("F1") + "d - " + (Find.TickManager.TicksGame + 6000000).TicksToDays().ToString("F1") + "d)";
			stringBuilder.AppendLine(text + ":");
			DebugGetFutureIncidents(100, currentMapOnly, out Dictionary<IIncidentTarget, int> incCountsForTarget, out int[] incCountsForComp, out List<Pair<IncidentDef, IncidentParms>> allIncidents, out int threatBigCount, stringBuilder, onlyThisComp);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Target totals:");
			foreach (KeyValuePair<IIncidentTarget, int> item in from kvp in incCountsForTarget
			orderby kvp.Value
			select kvp)
			{
				stringBuilder.AppendLine("  " + item.Value + ": " + item.Key);
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident totals:");
			for (int i = 0; i < incCountsForComp.Length; i++)
			{
				float f = (float)incCountsForComp[i] / (float)incCountsForComp.Sum();
				float num = (float)incCountsForComp[i] / 100f;
				float num2 = 1f / num;
				stringBuilder.AppendLine("   M" + i + ": " + incCountsForComp[i] + "  (" + f.ToStringPercent("F2") + " of total, avg " + num.ToString("F2") + " per day, avg interval " + num2 + ")");
			}
			stringBuilder.AppendLine("Total threats: " + threatBigCount);
			stringBuilder.AppendLine("Total threats avg per day: " + ((float)threatBigCount / 100f).ToString("F2"));
			stringBuilder.AppendLine("Overall: " + incCountsForComp.Sum());
			stringBuilder.AppendLine("Overall avg per day: " + ((float)incCountsForComp.Sum() / 100f).ToString("F2"));
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Incident defs used:");
			foreach (IncidentDef item2 in from x in (from x in allIncidents
			select x.First).Distinct()
			orderby x.category.defName, x.defName
			select x)
			{
				stringBuilder.AppendLine(item2.defName + " (" + item2.category.defName + ")");
			}
			Log.Message(stringBuilder.ToString());
		}

		public static void DebugGetFutureIncidents(int numTestDays, bool currentMapOnly, out Dictionary<IIncidentTarget, int> incCountsForTarget, out int[] incCountsForComp, out List<Pair<IncidentDef, IncidentParms>> allIncidents, out int threatBigCount, StringBuilder outputSb = null, StorytellerComp onlyThisComp = null)
		{
			int ticksGame = Find.TickManager.TicksGame;
			IncidentQueue incidentQueue = Find.Storyteller.incidentQueue;
			List<IIncidentTarget> allIncidentTargets = Find.Storyteller.AllIncidentTargets;
			tmpOldStoryStates.Clear();
			for (int i = 0; i < allIncidentTargets.Count; i++)
			{
				IIncidentTarget incidentTarget = allIncidentTargets[i];
				tmpOldStoryStates.Add(incidentTarget, incidentTarget.StoryState);
				new StoryState(incidentTarget).CopyTo(incidentTarget.StoryState);
			}
			Find.Storyteller.incidentQueue = new IncidentQueue();
			int num = numTestDays * 60;
			incCountsForComp = new int[Find.Storyteller.storytellerComps.Count];
			incCountsForTarget = new Dictionary<IIncidentTarget, int>();
			allIncidents = new List<Pair<IncidentDef, IncidentParms>>();
			threatBigCount = 0;
			for (int j = 0; j < num; j++)
			{
				IEnumerable<FiringIncident> enumerable = (onlyThisComp == null) ? Find.Storyteller.MakeIncidentsForInterval() : Find.Storyteller.MakeIncidentsForInterval(onlyThisComp, Find.Storyteller.AllIncidentTargets);
				foreach (FiringIncident item in enumerable)
				{
					if (item == null)
					{
						Log.Error("Null incident generated.");
					}
					if (!currentMapOnly || item.parms.target == Find.CurrentMap)
					{
						item.parms.target.StoryState.Notify_IncidentFired(item);
						allIncidents.Add(new Pair<IncidentDef, IncidentParms>(item.def, item.parms));
						item.parms.target.StoryState.Notify_IncidentFired(item);
						if (!incCountsForTarget.ContainsKey(item.parms.target))
						{
							incCountsForTarget[item.parms.target] = 0;
						}
						Dictionary<IIncidentTarget, int> dictionary;
						IIncidentTarget target;
						(dictionary = incCountsForTarget)[target = item.parms.target] = dictionary[target] + 1;
						string text = "  ";
						if (item.def.category == IncidentCategoryDefOf.ThreatBig || item.def.category == IncidentCategoryDefOf.RaidBeacon)
						{
							threatBigCount++;
							text = "T";
						}
						else if (item.def.category == IncidentCategoryDefOf.ThreatSmall)
						{
							text = "S";
						}
						int num2 = Find.Storyteller.storytellerComps.IndexOf(item.source);
						incCountsForComp[num2]++;
						outputSb?.AppendLine("M" + num2 + " " + text + " " + Find.TickManager.TicksGame.TicksToDays().ToString("F1") + "d      [" + Find.TickManager.TicksGame / 1000 + "]" + item);
					}
				}
				Find.TickManager.DebugSetTicksGame(Find.TickManager.TicksGame + 1000);
			}
			Find.TickManager.DebugSetTicksGame(ticksGame);
			Find.Storyteller.incidentQueue = incidentQueue;
			for (int k = 0; k < allIncidentTargets.Count; k++)
			{
				tmpOldStoryStates[allIncidentTargets[k]].CopyTo(allIncidentTargets[k].StoryState);
			}
			tmpOldStoryStates.Clear();
		}

		public static void DebugLogTestIncidentTargets()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Available incident targets:\n");
			foreach (IIncidentTarget allIncidentTarget in Find.Storyteller.AllIncidentTargets)
			{
				stringBuilder.AppendLine(allIncidentTarget.ToString());
				foreach (IncidentTargetTagDef item in allIncidentTarget.IncidentTargetTags())
				{
					stringBuilder.AppendLine("  " + item);
				}
				stringBuilder.AppendLine(string.Empty);
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
