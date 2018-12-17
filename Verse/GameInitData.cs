using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;

namespace Verse
{
	public class GameInitData
	{
		public int startingTile = -1;

		public int mapSize = 250;

		public List<Pawn> startingAndOptionalPawns = new List<Pawn>();

		public int startingPawnCount = -1;

		public Faction playerFaction;

		public Season startingSeason;

		public bool permadeathChosen;

		public bool permadeath;

		public bool startedFromEntry;

		public string gameToLoad;

		public const int DefaultMapSize = 250;

		public bool QuickStarted => gameToLoad.NullOrEmpty() && !startedFromEntry;

		public void ChooseRandomStartingTile()
		{
			startingTile = TileFinder.RandomStartingTile();
		}

		public void ResetWorldRelatedMapInitData()
		{
			Current.Game.World = null;
			startingAndOptionalPawns.Clear();
			playerFaction = null;
			startingTile = -1;
		}

		public override string ToString()
		{
			return "startedFromEntry: " + startedFromEntry + "\nstartingAndOptionalPawns: " + startingAndOptionalPawns.Count;
		}

		public void PrepForMapGen()
		{
			while (startingAndOptionalPawns.Count > startingPawnCount)
			{
				PawnComponentsUtility.RemoveComponentsOnDespawned(startingAndOptionalPawns[startingPawnCount]);
				Find.WorldPawns.PassToWorld(startingAndOptionalPawns[startingPawnCount], PawnDiscardDecideMode.KeepForever);
				startingAndOptionalPawns.RemoveAt(startingPawnCount);
			}
			List<Pawn> list = startingAndOptionalPawns;
			foreach (Pawn item in list)
			{
				item.SetFactionDirect(Faction.OfPlayer);
				PawnComponentsUtility.AddAndRemoveDynamicComponents(item);
			}
			foreach (Pawn item2 in list)
			{
				item2.workSettings.DisableAll();
			}
			foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
			{
				if (allDef.alwaysStartActive)
				{
					foreach (Pawn item3 in from col in list
					where !col.story.WorkTypeIsDisabled(allDef)
					select col)
					{
						item3.workSettings.SetPriority(allDef, 3);
					}
				}
				else
				{
					bool flag = false;
					foreach (Pawn item4 in list)
					{
						if (!item4.story.WorkTypeIsDisabled(allDef) && item4.skills.AverageOfRelevantSkillsFor(allDef) >= 6f)
						{
							item4.workSettings.SetPriority(allDef, 3);
							flag = true;
						}
					}
					if (!flag)
					{
						IEnumerable<Pawn> source = from col in list
						where !col.story.WorkTypeIsDisabled(allDef)
						select col;
						if (source.Any())
						{
							Pawn pawn = source.InRandomOrder().MaxBy((Pawn c) => c.skills.AverageOfRelevantSkillsFor(allDef));
							pawn.workSettings.SetPriority(allDef, 3);
						}
					}
				}
			}
		}
	}
}
