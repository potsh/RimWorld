using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public static class PawnsFinder
	{
		public static IEnumerable<Pawn> AllMapsWorldAndTemporary_AliveOrDead
		{
			get
			{
				using (IEnumerator<Pawn> enumerator = AllMapsWorldAndTemporary_Alive.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p3 = enumerator.Current;
						yield return p3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (Find.World != null)
				{
					using (IEnumerator<Pawn> enumerator2 = Find.WorldPawns.AllPawnsDead.GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							Pawn p2 = enumerator2.Current;
							yield return p2;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				using (IEnumerator<Pawn> enumerator3 = Temporary_Dead.GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						Pawn p = enumerator3.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_01d8:
				/*Error near IL_01d9: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsWorldAndTemporary_Alive
		{
			get
			{
				using (IEnumerator<Pawn> enumerator = AllMaps.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p3 = enumerator.Current;
						yield return p3;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				if (Find.World != null)
				{
					using (IEnumerator<Pawn> enumerator2 = Find.WorldPawns.AllPawnsAlive.GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							Pawn p2 = enumerator2.Current;
							yield return p2;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				using (IEnumerator<Pawn> enumerator3 = Temporary_Alive.GetEnumerator())
				{
					if (enumerator3.MoveNext())
					{
						Pawn p = enumerator3.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_01d8:
				/*Error near IL_01d9: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					for (int i = 0; i < maps.Count; i++)
					{
						using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.AllPawns.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_010e:
				/*Error near IL_010f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_Spawned
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					int j = 0;
					List<Pawn> spawned;
					int i;
					while (true)
					{
						if (j >= maps.Count)
						{
							yield break;
						}
						spawned = maps[j].mapPawns.AllPawnsSpawned;
						i = 0;
						if (i < spawned.Count)
						{
							break;
						}
						j++;
					}
					yield return spawned[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<Pawn> All_AliveOrDead
		{
			get
			{
				using (IEnumerator<Pawn> enumerator = AllMapsWorldAndTemporary_AliveOrDead.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p2 = enumerator.Current;
						yield return p2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				using (IEnumerator<Pawn> enumerator2 = AllCaravansAndTravelingTransportPods_AliveOrDead.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Pawn p = enumerator2.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_013e:
				/*Error near IL_013f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> Temporary
		{
			get
			{
				List<List<Pawn>> makingPawnsList = PawnGroupKindWorker.pawnsBeingGeneratedNow;
				for (int m = 0; m < makingPawnsList.Count; m++)
				{
					List<Pawn> makingPawns = makingPawnsList[m];
					int i = 0;
					if (i < makingPawns.Count)
					{
						yield return makingPawns[i];
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				List<List<Thing>> makingThingsList = ThingSetMaker.thingsBeingGeneratedNow;
				for (int l = 0; l < makingThingsList.Count; l++)
				{
					List<Thing> makingThings = makingThingsList[l];
					for (int j = 0; j < makingThings.Count; j++)
					{
						Pawn p = makingThings[j] as Pawn;
						if (p != null)
						{
							yield return p;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				if (Current.ProgramState != ProgramState.Playing && Find.GameInitData != null)
				{
					List<Pawn> startingAndOptionalPawns = Find.GameInitData.startingAndOptionalPawns;
					int k = 0;
					while (true)
					{
						if (k >= startingAndOptionalPawns.Count)
						{
							yield break;
						}
						if (startingAndOptionalPawns[k] != null)
						{
							break;
						}
						k++;
					}
					yield return startingAndOptionalPawns[k];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<Pawn> Temporary_Alive
		{
			get
			{
				foreach (Pawn item in Temporary)
				{
					if (!item.Dead)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> Temporary_Dead
		{
			get
			{
				foreach (Pawn item in Temporary)
				{
					if (item.Dead)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				using (IEnumerator<Pawn> enumerator = AllMaps.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						Pawn p2 = enumerator.Current;
						yield return p2;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				using (IEnumerator<Pawn> enumerator2 = AllCaravansAndTravelingTransportPods_Alive.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Pawn p = enumerator2.Current;
						yield return p;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_013e:
				/*Error near IL_013f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllCaravansAndTravelingTransportPods_Alive
		{
			get
			{
				foreach (Pawn item in AllCaravansAndTravelingTransportPods_AliveOrDead)
				{
					if (!item.Dead)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllCaravansAndTravelingTransportPods_AliveOrDead
		{
			get
			{
				if (Find.World != null)
				{
					List<Caravan> caravans = Find.WorldObjects.Caravans;
					for (int k = 0; k < caravans.Count; k++)
					{
						List<Pawn> pawns = caravans[k].PawnsListForReading;
						int i = 0;
						if (i < pawns.Count)
						{
							yield return pawns[i];
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
					List<TravelingTransportPods> travelingTransportPods = Find.WorldObjects.TravelingTransportPods;
					for (int j = 0; j < travelingTransportPods.Count; j++)
					{
						using (IEnumerator<Pawn> enumerator = travelingTransportPods[j].Pawns.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_01c9:
				/*Error near IL_01ca: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
		{
			get
			{
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.IsColonist)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists
		{
			get
			{
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.IsFreeColonist)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists_NoCryptosleep
		{
			get
			{
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.IsFreeColonist && !item.Suspended)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00d3:
				/*Error near IL_00d4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction
		{
			get
			{
				Faction playerFaction = Faction.OfPlayer;
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.Faction == playerFaction)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00d4:
				/*Error near IL_00d5: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction_NoCryptosleep
		{
			get
			{
				Faction playerFaction = Faction.OfPlayer;
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.Faction == playerFaction && !item.Suspended)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00e4:
				/*Error near IL_00e5: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony
		{
			get
			{
				foreach (Pawn item in AllMapsCaravansAndTravelingTransportPods_Alive)
				{
					if (item.IsPrisonerOfColony)
					{
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners => AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Concat(AllMapsCaravansAndTravelingTransportPods_Alive_PrisonersOfColony);

		public static IEnumerable<Pawn> AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
		{
			get
			{
				foreach (Pawn allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner in AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners)
				{
					if (!allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner.Suspended)
					{
						yield return allMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoner;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
				yield break;
				IL_00c3:
				/*Error near IL_00c4: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_PrisonersOfColonySpawned
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					int j = 0;
					List<Pawn> prisonersOfColonySpawned;
					int i;
					while (true)
					{
						if (j >= maps.Count)
						{
							yield break;
						}
						prisonersOfColonySpawned = maps[j].mapPawns.PrisonersOfColonySpawned;
						i = 0;
						if (i < prisonersOfColonySpawned.Count)
						{
							break;
						}
						j++;
					}
					yield return prisonersOfColonySpawned[i];
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<Pawn> AllMaps_PrisonersOfColony
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					for (int i = 0; i < maps.Count; i++)
					{
						using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.PrisonersOfColony.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_010e:
				/*Error near IL_010f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonists
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					for (int i = 0; i < maps.Count; i++)
					{
						using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.FreeColonists.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_010e:
				/*Error near IL_010f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsSpawned
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					for (int i = 0; i < maps.Count; i++)
					{
						using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.FreeColonistsSpawned.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_010e:
				/*Error near IL_010f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsAndPrisonersSpawned
		{
			get
			{
				if (Current.ProgramState != 0)
				{
					List<Map> maps = Find.Maps;
					for (int i = 0; i < maps.Count; i++)
					{
						using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.FreeColonistsAndPrisonersSpawned.GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								Pawn p = enumerator.Current;
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
					}
				}
				yield break;
				IL_010e:
				/*Error near IL_010f: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_FreeColonistsAndPrisoners
		{
			get
			{
				List<Map> maps = Find.Maps;
				for (int i = 0; i < maps.Count; i++)
				{
					using (IEnumerator<Pawn> enumerator = maps[i].mapPawns.FreeColonistsAndPrisoners.GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							Pawn p = enumerator.Current;
							yield return p;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				yield break;
				IL_0104:
				/*Error near IL_0105: Unexpected return in MoveNext()*/;
			}
		}

		public static IEnumerable<Pawn> AllMaps_SpawnedPawnsInFaction(Faction faction)
		{
			List<Map> maps = Find.Maps;
			int j = 0;
			List<Pawn> spawnedPawnsInFaction;
			int i;
			while (true)
			{
				if (j >= maps.Count)
				{
					yield break;
				}
				spawnedPawnsInFaction = maps[j].mapPawns.SpawnedPawnsInFaction(faction);
				i = 0;
				if (i < spawnedPawnsInFaction.Count)
				{
					break;
				}
				j++;
			}
			yield return spawnedPawnsInFaction[i];
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
