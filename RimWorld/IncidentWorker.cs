using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class IncidentWorker
	{
		public IncidentDef def;

		public virtual float AdjustedChance => def.baseChance;

		public bool CanFireNow(IncidentParms parms, bool forced = false)
		{
			if (!parms.forced)
			{
				if (!def.TargetAllowed(parms.target))
				{
					return false;
				}
				if (GenDate.DaysPassed < def.earliestDay)
				{
					return false;
				}
				if (Find.Storyteller.difficulty.difficulty < def.minDifficulty)
				{
					return false;
				}
				if (def.allowedBiomes != null)
				{
					BiomeDef biome = Find.WorldGrid[parms.target.Tile].biome;
					if (!def.allowedBiomes.Contains(biome))
					{
						return false;
					}
				}
				Scenario scenario = Find.Scenario;
				for (int i = 0; i < scenario.parts.Count; i++)
				{
					ScenPart_DisableIncident scenPart_DisableIncident = scenario.parts[i] as ScenPart_DisableIncident;
					if (scenPart_DisableIncident != null && scenPart_DisableIncident.Incident == def)
					{
						return false;
					}
				}
				if (def.minPopulation > 0 && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Count() < def.minPopulation)
				{
					return false;
				}
				Dictionary<IncidentDef, int> lastFireTicks = parms.target.StoryState.lastFireTicks;
				int ticksGame = Find.TickManager.TicksGame;
				if (lastFireTicks.TryGetValue(def, out int value))
				{
					float num = (float)(ticksGame - value) / 60000f;
					if (num < def.minRefireDays)
					{
						return false;
					}
				}
				List<IncidentDef> refireCheckIncidents = def.RefireCheckIncidents;
				if (refireCheckIncidents != null)
				{
					for (int j = 0; j < refireCheckIncidents.Count; j++)
					{
						if (lastFireTicks.TryGetValue(refireCheckIncidents[j], out value))
						{
							float num2 = (float)(ticksGame - value) / 60000f;
							if (num2 < def.minRefireDays)
							{
								return false;
							}
						}
					}
				}
				if (def.minGreatestPopulation > 0 && Find.StoryWatcher.statsRecord.greatestPopulation < def.minGreatestPopulation)
				{
					return false;
				}
			}
			if (!CanFireNowSub(parms))
			{
				return false;
			}
			return true;
		}

		protected virtual bool CanFireNowSub(IncidentParms parms)
		{
			return true;
		}

		public bool TryExecute(IncidentParms parms)
		{
			bool flag = TryExecuteWorker(parms);
			if (flag)
			{
				if (def.tale != null)
				{
					Pawn pawn = null;
					if (parms.target is Caravan)
					{
						pawn = ((Caravan)parms.target).RandomOwner();
					}
					else if (parms.target is Map)
					{
						pawn = ((Map)parms.target).mapPawns.FreeColonistsSpawned.RandomElementWithFallback();
					}
					else if (parms.target is World)
					{
						pawn = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep.RandomElementWithFallback();
					}
					if (pawn != null)
					{
						TaleRecorder.RecordTale(def.tale, pawn);
					}
				}
				if (def.category.tale != null)
				{
					Tale tale = TaleRecorder.RecordTale(def.category.tale);
					if (tale != null)
					{
						tale.customLabel = def.label;
					}
				}
			}
			return flag;
		}

		protected virtual bool TryExecuteWorker(IncidentParms parms)
		{
			Log.Error("Unimplemented incident " + this);
			return false;
		}

		protected void SendStandardLetter()
		{
			if (def.letterLabel.NullOrEmpty() || def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			Find.LetterStack.ReceiveLetter(def.letterLabel, def.letterText, def.letterDef);
		}

		protected void SendStandardLetter(LookTargets lookTargets, Faction relatedFaction = null, params string[] textArgs)
		{
			if (def.letterLabel.NullOrEmpty() || def.letterText.NullOrEmpty())
			{
				Log.Error("Sending standard incident letter with no label or text.");
			}
			string text = string.Format(def.letterText, textArgs).CapitalizeFirst();
			Find.LetterStack.ReceiveLetter(def.letterLabel, text, def.letterDef, lookTargets, relatedFaction);
		}
	}
}
