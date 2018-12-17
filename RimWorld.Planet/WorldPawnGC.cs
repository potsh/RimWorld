using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld.Planet
{
	public class WorldPawnGC : IExposable
	{
		private int lastSuccessfulGCTick;

		private int currentGCRate = 1;

		private const int AdditionalStoryRelevantPawns = 20;

		private const int GCUpdateInterval = 15000;

		private IEnumerator activeGCProcess;

		private StringBuilder logDotgraph;

		private HashSet<string> logDotgraphUniqueLinks;

		[CompilerGenerated]
		private static Func<char, bool> _003C_003Ef__mg_0024cache0;

		public void WorldPawnGCTick()
		{
			if (lastSuccessfulGCTick < Find.TickManager.TicksGame / 15000 * 15000)
			{
				if (activeGCProcess == null)
				{
					activeGCProcess = PawnGCPass().GetEnumerator();
					if (DebugViewSettings.logWorldPawnGC)
					{
						Log.Message($"World pawn GC started at rate {currentGCRate}");
					}
				}
				if (activeGCProcess != null)
				{
					bool flag = false;
					for (int i = 0; i < currentGCRate; i++)
					{
						if (flag)
						{
							break;
						}
						flag = !activeGCProcess.MoveNext();
					}
					if (flag)
					{
						lastSuccessfulGCTick = Find.TickManager.TicksGame;
						currentGCRate = 1;
						activeGCProcess = null;
						if (DebugViewSettings.logWorldPawnGC)
						{
							Log.Message("World pawn GC complete");
						}
					}
				}
			}
		}

		public void CancelGCPass()
		{
			if (activeGCProcess != null)
			{
				activeGCProcess = null;
				currentGCRate = Mathf.Min(currentGCRate * 2, 16777216);
				if (DebugViewSettings.logWorldPawnGC)
				{
					Log.Message("World pawn GC cancelled");
				}
			}
		}

		private IEnumerable AccumulatePawnGCData(Dictionary<Pawn, string> keptPawns)
		{
			_003CAccumulatePawnGCData_003Ec__Iterator0 _003CAccumulatePawnGCData_003Ec__Iterator = (_003CAccumulatePawnGCData_003Ec__Iterator0)/*Error near IL_0032: stateMachine*/;
			foreach (Pawn item in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
			{
				string criticalPawnReason = GetCriticalPawnReason(item);
				if (!criticalPawnReason.NullOrEmpty())
				{
					keptPawns[item] = criticalPawnReason;
					if (logDotgraph != null)
					{
						logDotgraph.AppendLine(string.Format("{0} [label=<{0}<br/><font point-size=\"10\">{1}</font>> color=\"{2}\" shape=\"{3}\"];", DotgraphIdentifier(item), criticalPawnReason, (item.relations == null || !item.relations.everSeenByPlayer) ? "grey" : "black", (!item.RaceProps.Humanlike) ? "box" : "oval"));
					}
				}
				else if (logDotgraph != null)
				{
					logDotgraph.AppendLine(string.Format("{0} [color=\"{1}\" shape=\"{2}\"];", DotgraphIdentifier(item), (item.relations == null || !item.relations.everSeenByPlayer) ? "grey" : "black", (!item.RaceProps.Humanlike) ? "box" : "oval"));
				}
			}
			foreach (Pawn item2 in (from pawn in PawnsFinder.AllMapsWorldAndTemporary_Alive
			where _003CAccumulatePawnGCData_003Ec__Iterator._0024this.AllowedAsStoryPawn(pawn) && !keptPawns.ContainsKey(pawn)
			orderby pawn.records.StoryRelevance descending
			select pawn).Take(20))
			{
				keptPawns[item2] = "StoryRelevant";
			}
			Pawn[] criticalPawns = keptPawns.Keys.ToArray();
			Pawn[] array = criticalPawns;
			int num = 0;
			if (num < array.Length)
			{
				Pawn pawn2 = array[num];
				AddAllRelationships(pawn2, keptPawns);
				yield return (object)null;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			Pawn[] array2 = criticalPawns;
			foreach (Pawn pawn3 in array2)
			{
				AddAllMemories(pawn3, keptPawns);
			}
		}

		private Dictionary<Pawn, string> AccumulatePawnGCDataImmediate()
		{
			Dictionary<Pawn, string> dictionary = new Dictionary<Pawn, string>();
			AccumulatePawnGCData(dictionary).ExecuteEnumerable();
			return dictionary;
		}

		public string PawnGCDebugResults()
		{
			Dictionary<Pawn, string> dictionary = AccumulatePawnGCDataImmediate();
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
			foreach (Pawn item in Find.WorldPawns.AllPawnsAlive)
			{
				string text = "Discarded";
				if (dictionary.ContainsKey(item))
				{
					text = dictionary[item];
				}
				if (!dictionary2.ContainsKey(text))
				{
					dictionary2[text] = 0;
				}
				Dictionary<string, int> dictionary3;
				string key;
				(dictionary3 = dictionary2)[key = text] = dictionary3[key] + 1;
			}
			return (from kvp in dictionary2
			orderby kvp.Value descending
			select $"{kvp.Value}: {kvp.Key}").ToLineList();
		}

		public IEnumerable PawnGCPass()
		{
			Dictionary<Pawn, string> keptPawns = new Dictionary<Pawn, string>();
			Pawn[] worldPawnsSnapshot = Find.WorldPawns.AllPawnsAliveOrDead.ToArray();
			IEnumerator enumerator = AccumulatePawnGCData(keptPawns).GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					yield return (object)null;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			finally
			{
				IDisposable disposable;
				IDisposable disposable2 = disposable = (enumerator as IDisposable);
				if (disposable != null)
				{
					disposable2.Dispose();
				}
			}
			foreach (Pawn pawn in worldPawnsSnapshot)
			{
				if (pawn.IsWorldPawn() && !keptPawns.ContainsKey(pawn))
				{
					Find.WorldPawns.RemoveAndDiscardPawnViaGC(pawn);
				}
			}
			yield break;
			IL_0135:
			/*Error near IL_0136: Unexpected return in MoveNext()*/;
		}

		private string GetCriticalPawnReason(Pawn pawn)
		{
			if (pawn.Discarded)
			{
				return null;
			}
			if (PawnUtility.EverBeenColonistOrTameAnimal(pawn) && pawn.RaceProps.Humanlike)
			{
				return "Colonist";
			}
			if (PawnGenerator.IsBeingGenerated(pawn))
			{
				return "Generating";
			}
			if (PawnUtility.IsFactionLeader(pawn))
			{
				return "FactionLeader";
			}
			if (PawnUtility.IsKidnappedPawn(pawn))
			{
				return "Kidnapped";
			}
			if (pawn.IsCaravanMember())
			{
				return "CaravanMember";
			}
			if (PawnUtility.IsTravelingInTransportPodWorldObject(pawn))
			{
				return "TransportPod";
			}
			if (PawnUtility.ForSaleBySettlement(pawn))
			{
				return "ForSale";
			}
			if (Find.WorldPawns.ForcefullyKeptPawns.Contains(pawn))
			{
				return "ForceKept";
			}
			if (pawn.SpawnedOrAnyParentSpawned)
			{
				return "Spawned";
			}
			if (!pawn.Corpse.DestroyedOrNull())
			{
				return "CorpseExists";
			}
			if (pawn.RaceProps.Humanlike && Current.ProgramState == ProgramState.Playing)
			{
				if (Find.PlayLog.AnyEntryConcerns(pawn))
				{
					return "InPlayLog";
				}
				if (Find.BattleLog.AnyEntryConcerns(pawn))
				{
					return "InBattleLog";
				}
			}
			if (Current.ProgramState == ProgramState.Playing && Find.TaleManager.AnyActiveTaleConcerns(pawn))
			{
				return "InActiveTale";
			}
			return null;
		}

		private bool AllowedAsStoryPawn(Pawn pawn)
		{
			if (!pawn.RaceProps.Humanlike)
			{
				return false;
			}
			return true;
		}

		public void AddAllRelationships(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.relations != null)
			{
				foreach (Pawn relatedPawn in pawn.relations.RelatedPawns)
				{
					if (logDotgraph != null)
					{
						string text = $"{DotgraphIdentifier(pawn)}->{DotgraphIdentifier(relatedPawn)} [label=<{pawn.GetRelations(relatedPawn).FirstOrDefault().ToString()}> color=\"purple\"];";
						if (!logDotgraphUniqueLinks.Contains(text))
						{
							logDotgraphUniqueLinks.Add(text);
							logDotgraph.AppendLine(text);
						}
					}
					if (!keptPawns.ContainsKey(relatedPawn))
					{
						keptPawns[relatedPawn] = "Relationship";
					}
				}
			}
		}

		public void AddAllMemories(Pawn pawn, Dictionary<Pawn, string> keptPawns)
		{
			if (pawn.needs != null && pawn.needs.mood != null && pawn.needs.mood.thoughts != null && pawn.needs.mood.thoughts.memories != null)
			{
				foreach (Thought_Memory memory in pawn.needs.mood.thoughts.memories.Memories)
				{
					if (memory.otherPawn != null)
					{
						if (logDotgraph != null)
						{
							string text = $"{DotgraphIdentifier(pawn)}->{DotgraphIdentifier(memory.otherPawn)} [label=<{memory.def}> color=\"orange\"];";
							if (!logDotgraphUniqueLinks.Contains(text))
							{
								logDotgraphUniqueLinks.Add(text);
								logDotgraph.AppendLine(text);
							}
						}
						if (!keptPawns.ContainsKey(memory.otherPawn))
						{
							keptPawns[memory.otherPawn] = "Memory";
						}
					}
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref lastSuccessfulGCTick, "lastSuccessfulGCTick", 0);
			Scribe_Values.Look(ref currentGCRate, "nextGCRate", 1);
		}

		public void LogGC()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("======= GC =======");
			stringBuilder.AppendLine(PawnGCDebugResults());
			Log.Message(stringBuilder.ToString());
		}

		public void RunGC()
		{
			CancelGCPass();
			PerfLogger.Reset();
			IEnumerator enumerator = PawnGCPass().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
			float num = PerfLogger.Duration() * 1000f;
			PerfLogger.Flush();
			Log.Message($"World pawn GC run complete in {num} ms");
		}

		public void LogDotgraph()
		{
			logDotgraph = new StringBuilder();
			logDotgraphUniqueLinks = new HashSet<string>();
			logDotgraph.AppendLine("digraph { rankdir=LR;");
			AccumulatePawnGCDataImmediate();
			logDotgraph.AppendLine("}");
			GUIUtility.systemCopyBuffer = logDotgraph.ToString();
			Log.Message("Dotgraph copied to clipboard");
			logDotgraph = null;
			logDotgraphUniqueLinks = null;
		}

		public static string DotgraphIdentifier(Pawn pawn)
		{
			return new string(pawn.LabelShort.Where(char.IsLetter).ToArray()) + "_" + pawn.thingIDNumber.ToString();
		}
	}
}
