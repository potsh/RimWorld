using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Storyteller : IExposable
	{
		public StorytellerDef def;

		public DifficultyDef difficulty;

		public List<StorytellerComp> storytellerComps;

		public IncidentQueue incidentQueue = new IncidentQueue();

		public static readonly Vector2 PortraitSizeTiny = new Vector2(116f, 124f);

		public static readonly Vector2 PortraitSizeLarge = new Vector2(580f, 620f);

		public const int IntervalsPerDay = 60;

		public const int CheckInterval = 1000;

		private static List<IIncidentTarget> tmpAllIncidentTargets = new List<IIncidentTarget>();

		private string debugStringCached = "Generating data...";

		public List<IIncidentTarget> AllIncidentTargets
		{
			get
			{
				tmpAllIncidentTargets.Clear();
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					tmpAllIncidentTargets.Add(maps[i]);
				}
				List<Caravan> caravans = Find.WorldObjects.Caravans;
				for (int j = 0; j < caravans.Count; j++)
				{
					if (caravans[j].IsPlayerControlled)
					{
						tmpAllIncidentTargets.Add(caravans[j]);
					}
				}
				tmpAllIncidentTargets.Add(Find.World);
				return tmpAllIncidentTargets;
			}
		}

		public Storyteller()
		{
		}

		public Storyteller(StorytellerDef def, DifficultyDef difficulty)
		{
			this.def = def;
			this.difficulty = difficulty;
			InitializeStorytellerComps();
		}

		public static void StorytellerStaticUpdate()
		{
			tmpAllIncidentTargets.Clear();
		}

		private void InitializeStorytellerComps()
		{
			storytellerComps = new List<StorytellerComp>();
			for (int i = 0; i < def.comps.Count; i++)
			{
				StorytellerComp storytellerComp = (StorytellerComp)Activator.CreateInstance(def.comps[i].compClass);
				storytellerComp.props = def.comps[i];
				storytellerComps.Add(storytellerComp);
			}
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Defs.Look(ref difficulty, "difficulty");
			Scribe_Deep.Look(ref incidentQueue, "incidentQueue");
			if (difficulty == null)
			{
				Log.Error("Loaded storyteller without difficulty");
				difficulty = DefDatabase<DifficultyDef>.AllDefsListForReading[3];
			}
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
			{
				InitializeStorytellerComps();
			}
		}

		public void StorytellerTick()
		{
			incidentQueue.IncidentQueueTick();
			if (Find.TickManager.TicksGame % 1000 == 0 && DebugSettings.enableStoryteller)
			{
				foreach (FiringIncident item in MakeIncidentsForInterval())
				{
					TryFire(item);
				}
			}
		}

		public bool TryFire(FiringIncident fi)
		{
			if (fi.def.Worker.CanFireNow(fi.parms) && fi.def.Worker.TryExecute(fi.parms))
			{
				fi.parms.target.StoryState.Notify_IncidentFired(fi);
				return true;
			}
			return false;
		}

		public IEnumerable<FiringIncident> MakeIncidentsForInterval()
		{
			List<IIncidentTarget> targets = AllIncidentTargets;
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				using (IEnumerator<FiringIncident> enumerator = MakeIncidentsForInterval(storytellerComps[i], targets).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						FiringIncident incident = enumerator.Current;
						yield return incident;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_011b:
			/*Error near IL_011c: Unexpected return in MoveNext()*/;
		}

		public IEnumerable<FiringIncident> MakeIncidentsForInterval(StorytellerComp comp, List<IIncidentTarget> targets)
		{
			if (!(GenDate.DaysPassedFloat <= comp.props.minDaysPassed))
			{
				for (int i = 0; i < targets.Count; i++)
				{
					IIncidentTarget targ = targets[i];
					bool flag = false;
					bool flag2 = comp.props.allowedTargetTags.NullOrEmpty();
					foreach (IncidentTargetTagDef item in targ.IncidentTargetTags())
					{
						if (!comp.props.disallowedTargetTags.NullOrEmpty() && comp.props.disallowedTargetTags.Contains(item))
						{
							flag = true;
							break;
						}
						if (!flag2 && comp.props.allowedTargetTags.Contains(item))
						{
							flag2 = true;
						}
					}
					if (!flag && flag2)
					{
						foreach (FiringIncident item2 in comp.MakeIntervalIncidents(targ))
						{
							if (Find.Storyteller.difficulty.allowBigThreats || (item2.def.category != IncidentCategoryDefOf.ThreatBig && item2.def.category != IncidentCategoryDefOf.RaidBeacon))
							{
								yield return item2;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
			}
			yield break;
			IL_023c:
			/*Error near IL_023d: Unexpected return in MoveNext()*/;
		}

		public void Notify_PawnEvent(Pawn pawn, AdaptationEvent ev, DamageInfo? dinfo = default(DamageInfo?))
		{
			Find.StoryWatcher.watcherAdaptation.Notify_PawnEvent(pawn, ev, dinfo);
			for (int i = 0; i < storytellerComps.Count; i++)
			{
				StorytellerComp storytellerComp = storytellerComps[i];
				storytellerComp.Notify_PawnEvent(pawn, ev, dinfo);
			}
		}

		public void Notify_DefChanged()
		{
			InitializeStorytellerComps();
		}

		public string DebugString()
		{
			if (Time.frameCount % 60 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("Storyteller : " + def.label);
				stringBuilder.AppendLine("------------- Global threats data ---------------");
				stringBuilder.AppendLine("   Adaptation days: " + Find.StoryWatcher.watcherAdaptation.AdaptDays.ToString("F1"));
				stringBuilder.AppendLine("   Adapt points factor: " + Find.StoryWatcher.watcherAdaptation.TotalThreatPointsFactor.ToString("F2"));
				stringBuilder.AppendLine("   Time points factor: " + Find.Storyteller.def.pointsFactorFromDaysPassed.Evaluate((float)GenDate.DaysPassed).ToString("F2"));
				stringBuilder.AppendLine("   Num raids enemy: " + Find.StoryWatcher.statsRecord.numRaidsEnemy);
				stringBuilder.AppendLine("   Ally incident fraction (neutral or ally): " + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: false).ToString("F2"));
				stringBuilder.AppendLine("   Ally incident fraction (ally only): " + StorytellerUtility.AllyIncidentFraction(fullAlliesOnly: true).ToString("F2"));
				stringBuilder.AppendLine();
				stringBuilder.AppendLine("-------------- Global population data --------------");
				stringBuilder.AppendLine(StorytellerUtilityPopulation.DebugReadout().TrimEndNewlines());
				stringBuilder.AppendLine("   Greatest population: " + Find.StoryWatcher.statsRecord.greatestPopulation);
				stringBuilder.AppendLine("------------- All incident targets --------------");
				for (int i = 0; i < AllIncidentTargets.Count; i++)
				{
					stringBuilder.AppendLine("   " + AllIncidentTargets[i].ToString());
				}
				IIncidentTarget incidentTarget = Find.WorldSelector.SingleSelectedObject as IIncidentTarget;
				if (incidentTarget == null)
				{
					incidentTarget = Find.CurrentMap;
				}
				if (incidentTarget != null)
				{
					Map map = incidentTarget as Map;
					stringBuilder.AppendLine();
					stringBuilder.AppendLine("---------- Selected: " + incidentTarget + " --------");
					stringBuilder.AppendLine("   Wealth: " + incidentTarget.PlayerWealthForStoryteller.ToString("F0"));
					if (map != null)
					{
						stringBuilder.AppendLine("   (Items: " + map.wealthWatcher.WealthItems.ToString("F0") + " Buildings: " + map.wealthWatcher.WealthBuildings.ToString("F0") + " (Floors: " + map.wealthWatcher.WealthFloorsOnly.ToString("F0") + ") Pawns: " + map.wealthWatcher.WealthPawns.ToString("F0") + ")");
					}
					stringBuilder.AppendLine("   IncidentPointsRandomFactorRange: " + incidentTarget.IncidentPointsRandomFactorRange);
					stringBuilder.AppendLine("   Pawns-Humanlikes: " + (from p in incidentTarget.PlayerPawnsForStoryteller
					where p.def.race.Humanlike
					select p).Count());
					stringBuilder.AppendLine("   Pawns-Animals: " + (from p in incidentTarget.PlayerPawnsForStoryteller
					where p.def.race.Animal
					select p).Count());
					if (map != null)
					{
						stringBuilder.AppendLine("   StoryDanger: " + map.dangerWatcher.DangerRating);
						stringBuilder.AppendLine("   FireDanger: " + map.fireWatcher.FireDanger.ToString("F2"));
						stringBuilder.AppendLine("   LastThreatBigTick days ago: " + (Find.TickManager.TicksGame - map.storyState.LastThreatBigTick).ToStringTicksToDays());
					}
					stringBuilder.AppendLine("   Current points (ignoring early raid factors): " + StorytellerUtility.DefaultThreatPointsNow(incidentTarget).ToString("F0"));
					stringBuilder.AppendLine("   Current points for specific IncidentMakers:");
					for (int j = 0; j < storytellerComps.Count; j++)
					{
						IncidentParms incidentParms = storytellerComps[j].GenerateParms(IncidentCategoryDefOf.ThreatBig, incidentTarget);
						stringBuilder.AppendLine("      " + storytellerComps[j].GetType().ToString().Substring(23) + ": " + incidentParms.points.ToString("F0"));
					}
				}
				debugStringCached = stringBuilder.ToString();
			}
			return debugStringCached;
		}
	}
}
