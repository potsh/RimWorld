using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public sealed class ResearchManager : IExposable
	{
		public ResearchProjectDef currentProj;

		private Dictionary<ResearchProjectDef, float> progress = new Dictionary<ResearchProjectDef, float>();

		private float ResearchPointsPerWorkTick = 0.00825f;

		public bool AnyProjectIsAvailable => DefDatabase<ResearchProjectDef>.AllDefsListForReading.Find((ResearchProjectDef x) => x.CanStartNow) != null;

		public void ExposeData()
		{
			Scribe_Defs.Look(ref currentProj, "currentProj");
			Scribe_Collections.Look(ref progress, "progress", LookMode.Def, LookMode.Value);
		}

		public float GetProgress(ResearchProjectDef proj)
		{
			if (progress.TryGetValue(proj, out float value))
			{
				return value;
			}
			progress.Add(proj, 0f);
			return 0f;
		}

		public void ResearchPerformed(float amount, Pawn researcher)
		{
			if (currentProj == null)
			{
				Log.Error("Researched without having an active project.");
			}
			else
			{
				amount *= ResearchPointsPerWorkTick;
				amount *= Find.Storyteller.difficulty.researchSpeedFactor;
				if (researcher != null && researcher.Faction != null)
				{
					amount /= currentProj.CostFactor(researcher.Faction.def.techLevel);
				}
				if (DebugSettings.fastResearch)
				{
					amount *= 500f;
				}
				researcher?.records.AddTo(RecordDefOf.ResearchPointsResearched, amount);
				float num = GetProgress(currentProj);
				num += amount;
				progress[currentProj] = num;
				if (currentProj.IsFinished)
				{
					FinishProject(currentProj, doCompletionDialog: true, researcher);
				}
			}
		}

		public void ReapplyAllMods()
		{
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				if (allDef.IsFinished)
				{
					allDef.ReapplyAllMods();
				}
			}
		}

		public void FinishProject(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null)
		{
			if (proj.prerequisites != null)
			{
				for (int i = 0; i < proj.prerequisites.Count; i++)
				{
					if (!proj.prerequisites[i].IsFinished)
					{
						FinishProject(proj.prerequisites[i]);
					}
				}
			}
			progress[proj] = proj.baseCost;
			if (researcher != null)
			{
				TaleRecorder.RecordTale(TaleDefOf.FinishedResearchProject, researcher, currentProj);
			}
			ReapplyAllMods();
			if (doCompletionDialog)
			{
				string text = "ResearchFinished".Translate(currentProj.LabelCap) + "\n\n" + currentProj.description;
				DiaNode diaNode = new DiaNode(text);
				diaNode.options.Add(DiaOption.DefaultOK);
				DiaOption diaOption = new DiaOption("ResearchScreen".Translate());
				diaOption.resolveTree = true;
				diaOption.action = delegate
				{
					Find.MainTabsRoot.SetCurrentTab(MainButtonDefOf.Research);
				};
				diaNode.options.Add(diaOption);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, delayInteractivity: true));
			}
			if (!proj.discoveredLetterTitle.NullOrEmpty() && Find.Storyteller.difficulty.difficulty >= proj.discoveredLetterMinDifficulty)
			{
				Find.LetterStack.ReceiveLetter(proj.discoveredLetterTitle, proj.discoveredLetterText, LetterDefOf.NeutralEvent);
			}
			if (proj.unlockExtremeDifficulty && Find.Storyteller.difficulty.difficulty >= DifficultyDefOf.Rough.difficulty)
			{
				Prefs.ExtremeDifficultyUnlocked = true;
				Prefs.Save();
			}
			if (currentProj == proj)
			{
				currentProj = null;
			}
		}

		public void DebugSetAllProjectsFinished()
		{
			progress.Clear();
			foreach (ResearchProjectDef allDef in DefDatabase<ResearchProjectDef>.AllDefs)
			{
				progress.Add(allDef, allDef.baseCost);
			}
			ReapplyAllMods();
		}
	}
}
