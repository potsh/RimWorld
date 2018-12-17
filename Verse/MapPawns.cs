using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse.AI;

namespace Verse
{
	public sealed class MapPawns
	{
		private Map map;

		private List<Pawn> pawnsSpawned = new List<Pawn>();

		private Dictionary<Faction, List<Pawn>> pawnsInFactionSpawned = new Dictionary<Faction, List<Pawn>>();

		private List<Pawn> prisonersOfColonySpawned = new List<Pawn>();

		private List<Thing> tmpThings = new List<Thing>();

		private List<Pawn> tmpUnspawnedPawns = new List<Pawn>();

		public IEnumerable<Pawn> AllPawns
		{
			get
			{
				int i = 0;
				if (i < pawnsSpawned.Count)
				{
					yield return pawnsSpawned[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
				using (IEnumerator<Pawn> enumerator = AllPawnsUnspawned.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p = enumerator.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_0122:
				/*Error near IL_0123: Unexpected return in MoveNext()*/;
			}
		}

		public IEnumerable<Pawn> AllPawnsUnspawned
		{
			get
			{
				tmpUnspawnedPawns.Clear();
				ThingOwnerUtility.GetAllThingsRecursively(map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), tmpUnspawnedPawns, allowUnreal: true, null, alsoGetSpawnedThings: false);
				for (int num = tmpUnspawnedPawns.Count - 1; num >= 0; num--)
				{
					if (tmpUnspawnedPawns[num].Dead)
					{
						tmpUnspawnedPawns.RemoveAt(num);
					}
				}
				int i = tmpUnspawnedPawns.Count - 1;
				if (i >= 0)
				{
					yield return tmpUnspawnedPawns[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public IEnumerable<Pawn> FreeColonists => FreeHumanlikesOfFaction(Faction.OfPlayer);

		public IEnumerable<Pawn> PrisonersOfColony => from x in AllPawns
		where x.IsPrisonerOfColony
		select x;

		public IEnumerable<Pawn> FreeColonistsAndPrisoners => FreeColonists.Concat(PrisonersOfColony);

		public int ColonistCount
		{
			get
			{
				if (Current.ProgramState != ProgramState.Playing)
				{
					Log.Error("ColonistCount while not playing. This should get the starting player pawn count.");
					return 3;
				}
				return AllPawns.Count((Pawn x) => x.IsColonist);
			}
		}

		public int AllPawnsCount => AllPawns.Count();

		public int AllPawnsUnspawnedCount => AllPawnsUnspawned.Count();

		public int FreeColonistsCount => FreeColonists.Count();

		public int PrisonersOfColonyCount => PrisonersOfColony.Count();

		public int FreeColonistsAndPrisonersCount => PrisonersOfColony.Count();

		public bool AnyPawnBlockingMapRemoval
		{
			get
			{
				Faction ofPlayer = Faction.OfPlayer;
				for (int i = 0; i < pawnsSpawned.Count; i++)
				{
					if (!pawnsSpawned[i].Downed && pawnsSpawned[i].IsColonist)
					{
						return true;
					}
					if (pawnsSpawned[i].relations != null && pawnsSpawned[i].relations.relativeInvolvedInRescueQuest != null)
					{
						return true;
					}
					if (pawnsSpawned[i].Faction == ofPlayer || pawnsSpawned[i].HostFaction == ofPlayer)
					{
						Job curJob = pawnsSpawned[i].CurJob;
						if (curJob != null && curJob.exitMapOnArrival)
						{
							return true;
						}
					}
					if (CaravanExitMapUtility.FindCaravanToJoinFor(pawnsSpawned[i]) != null && !pawnsSpawned[i].Downed)
					{
						return true;
					}
				}
				List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						tmpThings.Clear();
						ThingOwnerUtility.GetAllThingsRecursively(holder, tmpThings);
						for (int k = 0; k < tmpThings.Count; k++)
						{
							Pawn pawn = tmpThings[k] as Pawn;
							if (pawn != null && !pawn.Dead && !pawn.Downed && pawn.IsColonist)
							{
								tmpThings.Clear();
								return true;
							}
						}
					}
				}
				tmpThings.Clear();
				return false;
			}
		}

		public List<Pawn> AllPawnsSpawned => pawnsSpawned;

		public IEnumerable<Pawn> FreeColonistsSpawned => FreeHumanlikesSpawnedOfFaction(Faction.OfPlayer);

		public List<Pawn> PrisonersOfColonySpawned => prisonersOfColonySpawned;

		public IEnumerable<Pawn> FreeColonistsAndPrisonersSpawned => FreeColonistsSpawned.Concat(PrisonersOfColonySpawned);

		public int AllPawnsSpawnedCount => pawnsSpawned.Count;

		public int FreeColonistsSpawnedCount => FreeColonistsSpawned.Count();

		public int PrisonersOfColonySpawnedCount => PrisonersOfColonySpawned.Count;

		public int FreeColonistsAndPrisonersSpawnedCount => FreeColonistsAndPrisonersSpawned.Count();

		public int ColonistsSpawnedCount
		{
			get
			{
				int num = 0;
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						num++;
					}
				}
				return num;
			}
		}

		public int FreeColonistsSpawnedOrInPlayerEjectablePodsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < pawnsSpawned.Count; i++)
				{
					if (pawnsSpawned[i].IsFreeColonist)
					{
						num++;
					}
				}
				List<Thing> list = map.listerThings.ThingsInGroup(ThingRequestGroup.ThingHolder);
				for (int j = 0; j < list.Count; j++)
				{
					Building_CryptosleepCasket building_CryptosleepCasket = list[j] as Building_CryptosleepCasket;
					if ((building_CryptosleepCasket != null && building_CryptosleepCasket.def.building.isPlayerEjectable) || list[j] is IActiveDropPod || list[j].TryGetComp<CompTransporter>() != null)
					{
						IThingHolder holder = list[j].TryGetComp<CompTransporter>() ?? ((IThingHolder)list[j]);
						tmpThings.Clear();
						ThingOwnerUtility.GetAllThingsRecursively(holder, tmpThings);
						for (int k = 0; k < tmpThings.Count; k++)
						{
							Pawn pawn = tmpThings[k] as Pawn;
							if (pawn != null && !pawn.Dead && pawn.IsFreeColonist)
							{
								num++;
							}
						}
					}
				}
				tmpThings.Clear();
				return num;
			}
		}

		public bool AnyColonistSpawned
		{
			get
			{
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AnyFreeColonistSpawned
		{
			get
			{
				List<Pawn> list = SpawnedPawnsInFaction(Faction.OfPlayer);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].IsFreeColonist)
					{
						return true;
					}
				}
				return false;
			}
		}

		public MapPawns(Map map)
		{
			this.map = map;
		}

		private void EnsureFactionsListsInit()
		{
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				if (!pawnsInFactionSpawned.ContainsKey(allFactionsListForReading[i]))
				{
					pawnsInFactionSpawned.Add(allFactionsListForReading[i], new List<Pawn>());
				}
			}
		}

		public IEnumerable<Pawn> PawnsInFaction(Faction faction)
		{
			if (faction == null)
			{
				Log.Error("Called PawnsInFaction with null faction.");
				return new List<Pawn>();
			}
			return from x in AllPawns
			where x.Faction == faction
			select x;
		}

		public List<Pawn> SpawnedPawnsInFaction(Faction faction)
		{
			EnsureFactionsListsInit();
			if (faction == null)
			{
				Log.Error("Called SpawnedPawnsInFaction with null faction.");
				return new List<Pawn>();
			}
			return pawnsInFactionSpawned[faction];
		}

		public IEnumerable<Pawn> FreeHumanlikesOfFaction(Faction faction)
		{
			return from p in PawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		public IEnumerable<Pawn> FreeHumanlikesSpawnedOfFaction(Faction faction)
		{
			return from p in SpawnedPawnsInFaction(faction)
			where p.HostFaction == null && p.RaceProps.Humanlike
			select p;
		}

		public void RegisterPawn(Pawn p)
		{
			if (p.Dead)
			{
				Log.Warning("Tried to register dead pawn " + p + " in " + GetType() + ".");
			}
			else if (!p.Spawned)
			{
				Log.Warning("Tried to register despawned pawn " + p + " in " + GetType() + ".");
			}
			else if (p.Map != map)
			{
				Log.Warning("Tried to register pawn " + p + " but his Map is not this one.");
			}
			else if (p.mindState.Active)
			{
				EnsureFactionsListsInit();
				if (!pawnsSpawned.Contains(p))
				{
					pawnsSpawned.Add(p);
				}
				if (p.Faction != null && !pawnsInFactionSpawned[p.Faction].Contains(p))
				{
					pawnsInFactionSpawned[p.Faction].Add(p);
					if (p.Faction == Faction.OfPlayer)
					{
						pawnsInFactionSpawned[Faction.OfPlayer].InsertionSort(delegate(Pawn a, Pawn b)
						{
							int num = (a.playerSettings != null) ? a.playerSettings.joinTick : 0;
							int value = (b.playerSettings != null) ? b.playerSettings.joinTick : 0;
							return num.CompareTo(value);
						});
					}
				}
				if (p.IsPrisonerOfColony && !prisonersOfColonySpawned.Contains(p))
				{
					prisonersOfColonySpawned.Add(p);
				}
				DoListChangedNotifications();
			}
		}

		public void DeRegisterPawn(Pawn p)
		{
			EnsureFactionsListsInit();
			pawnsSpawned.Remove(p);
			List<Faction> allFactionsListForReading = Find.FactionManager.AllFactionsListForReading;
			for (int i = 0; i < allFactionsListForReading.Count; i++)
			{
				Faction key = allFactionsListForReading[i];
				pawnsInFactionSpawned[key].Remove(p);
			}
			prisonersOfColonySpawned.Remove(p);
			DoListChangedNotifications();
		}

		public void UpdateRegistryForPawn(Pawn p)
		{
			DeRegisterPawn(p);
			if (p.Spawned && p.Map == map)
			{
				RegisterPawn(p);
			}
			DoListChangedNotifications();
		}

		private void DoListChangedNotifications()
		{
			MainTabWindowUtility.NotifyAllPawnTables_PawnsChanged();
			if (Find.ColonistBar != null)
			{
				Find.ColonistBar.MarkColonistsDirty();
			}
		}

		public void LogListedPawns()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MapPawns:");
			stringBuilder.AppendLine("pawnsSpawned");
			foreach (Pawn item in pawnsSpawned)
			{
				stringBuilder.AppendLine("    " + item.ToString());
			}
			stringBuilder.AppendLine("AllPawnsUnspawned");
			foreach (Pawn item2 in AllPawnsUnspawned)
			{
				stringBuilder.AppendLine("    " + item2.ToString());
			}
			foreach (KeyValuePair<Faction, List<Pawn>> item3 in pawnsInFactionSpawned)
			{
				stringBuilder.AppendLine("pawnsInFactionSpawned[" + item3.Key.ToString() + "]");
				foreach (Pawn item4 in item3.Value)
				{
					stringBuilder.AppendLine("    " + item4.ToString());
				}
			}
			stringBuilder.AppendLine("prisonersOfColonySpawned");
			foreach (Pawn item5 in prisonersOfColonySpawned)
			{
				stringBuilder.AppendLine("    " + item5.ToString());
			}
			Log.Message(stringBuilder.ToString());
		}
	}
}
