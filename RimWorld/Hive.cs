using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Hive : ThingWithComps
	{
		public bool active = true;

		public int nextPawnSpawnTick = -1;

		private List<Pawn> spawnedPawns = new List<Pawn>();

		public bool caveColony;

		public bool canSpawnPawns = true;

		public const int PawnSpawnRadius = 2;

		public const float MaxSpawnedPawnsPoints = 500f;

		public const float InitialPawnsPoints = 200f;

		private static readonly FloatRange PawnSpawnIntervalDays = new FloatRange(0.85f, 1.15f);

		public static List<PawnKindDef> spawnablePawnKinds = new List<PawnKindDef>();

		public static readonly string MemoAttackedByEnemy = "HiveAttacked";

		public static readonly string MemoDeSpawned = "HiveDeSpawned";

		public static readonly string MemoBurnedBadly = "HiveBurnedBadly";

		public static readonly string MemoDestroyedNonRoofCollapse = "HiveDestroyedNonRoofCollapse";

		private Lord Lord
		{
			get
			{
				Predicate<Pawn> hasDefendHiveLord = delegate(Pawn x)
				{
					Lord lord = x.GetLord();
					return lord != null && lord.LordJob is LordJob_DefendAndExpandHive;
				};
				Pawn foundPawn = spawnedPawns.Find(hasDefendHiveLord);
				if (base.Spawned)
				{
					if (foundPawn == null)
					{
						RegionTraverser.BreadthFirstTraverse(this.GetRegion(), (Region from, Region to) => true, delegate(Region r)
						{
							List<Thing> list = r.ListerThings.ThingsOfDef(ThingDefOf.Hive);
							for (int i = 0; i < list.Count; i++)
							{
								if (list[i] != this && list[i].Faction == base.Faction)
								{
									foundPawn = ((Hive)list[i]).spawnedPawns.Find(hasDefendHiveLord);
									if (foundPawn != null)
									{
										return true;
									}
								}
							}
							return false;
						}, 20);
					}
					if (foundPawn != null)
					{
						return foundPawn.GetLord();
					}
				}
				return null;
			}
		}

		private float SpawnedPawnsPoints
		{
			get
			{
				FilterOutUnspawnedPawns();
				float num = 0f;
				for (int i = 0; i < spawnedPawns.Count; i++)
				{
					num += spawnedPawns[i].kindDef.combatPower;
				}
				return num;
			}
		}

		public static void ResetStaticData()
		{
			spawnablePawnKinds.Clear();
			spawnablePawnKinds.Add(PawnKindDefOf.Megascarab);
			spawnablePawnKinds.Add(PawnKindDefOf.Spelopede);
			spawnablePawnKinds.Add(PawnKindDefOf.Megaspider);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (base.Faction == null)
			{
				SetFaction(Faction.OfInsects);
			}
			if (!respawningAfterLoad && active)
			{
				SpawnInitialPawns();
			}
		}

		private void SpawnInitialPawns()
		{
			SpawnPawnsUntilPoints(200f);
			CalculateNextPawnSpawnTick();
		}

		public void SpawnPawnsUntilPoints(float points)
		{
			int num = 0;
			while (SpawnedPawnsPoints < points)
			{
				num++;
				if (num > 1000)
				{
					Log.Error("Too many iterations.");
					break;
				}
				if (!TrySpawnPawn(out Pawn _))
				{
					break;
				}
			}
			CalculateNextPawnSpawnTick();
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Spawned)
			{
				FilterOutUnspawnedPawns();
				if (!active && !base.Position.Fogged(base.Map))
				{
					Activate();
				}
				if (active && Find.TickManager.TicksGame >= nextPawnSpawnTick)
				{
					Pawn pawn;
					if (SpawnedPawnsPoints < 500f && TrySpawnPawn(out pawn) && pawn.caller != null)
					{
						pawn.caller.DoCall();
					}
					CalculateNextPawnSpawnTick();
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			Map map = base.Map;
			base.DeSpawn(mode);
			List<Lord> lords = map.lordManager.lords;
			for (int i = 0; i < lords.Count; i++)
			{
				lords[i].ReceiveMemo(MemoDeSpawned);
			}
			HiveUtility.Notify_HiveDespawned(this, map);
		}

		public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			if (dinfo.Def.ExternalViolenceFor(this) && dinfo.Instigator != null && dinfo.Instigator.Faction != null)
			{
				Lord?.ReceiveMemo(MemoAttackedByEnemy);
			}
			if (dinfo.Def == DamageDefOf.Flame && (float)HitPoints < (float)base.MaxHitPoints * 0.3f)
			{
				Lord?.ReceiveMemo(MemoBurnedBadly);
			}
			base.PostApplyDamage(dinfo, totalDamageDealt);
		}

		public override void Kill(DamageInfo? dinfo = default(DamageInfo?), Hediff exactCulprit = null)
		{
			if (base.Spawned && (!dinfo.HasValue || dinfo.Value.Category != DamageInfo.SourceCategory.Collapse))
			{
				List<Lord> lords = base.Map.lordManager.lords;
				for (int i = 0; i < lords.Count; i++)
				{
					lords[i].ReceiveMemo(MemoDestroyedNonRoofCollapse);
				}
			}
			base.Kill(dinfo, exactCulprit);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref active, "active", defaultValue: false);
			Scribe_Values.Look(ref nextPawnSpawnTick, "nextPawnSpawnTick", 0);
			Scribe_Collections.Look(ref spawnedPawns, "spawnedPawns", LookMode.Reference);
			Scribe_Values.Look(ref caveColony, "caveColony", defaultValue: false);
			Scribe_Values.Look(ref canSpawnPawns, "canSpawnPawns", defaultValue: true);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				spawnedPawns.RemoveAll((Pawn x) => x == null);
			}
		}

		private void Activate()
		{
			active = true;
			SpawnInitialPawns();
			CalculateNextPawnSpawnTick();
			GetComp<CompSpawnerHives>()?.CalculateNextHiveSpawnTick();
		}

		private void CalculateNextPawnSpawnTick()
		{
			float num = GenMath.LerpDouble(0f, 5f, 1f, 0.5f, (float)spawnedPawns.Count);
			nextPawnSpawnTick = Find.TickManager.TicksGame + (int)(PawnSpawnIntervalDays.RandomInRange * 60000f / (num * Find.Storyteller.difficulty.enemyReproductionRateFactor));
		}

		private void FilterOutUnspawnedPawns()
		{
			for (int num = spawnedPawns.Count - 1; num >= 0; num--)
			{
				if (!spawnedPawns[num].Spawned)
				{
					spawnedPawns.RemoveAt(num);
				}
			}
		}

		private bool TrySpawnPawn(out Pawn pawn)
		{
			if (!canSpawnPawns)
			{
				pawn = null;
				return false;
			}
			float curPoints = SpawnedPawnsPoints;
			IEnumerable<PawnKindDef> source = from x in spawnablePawnKinds
			where curPoints + x.combatPower <= 500f
			select x;
			if (!source.TryRandomElement(out PawnKindDef result))
			{
				pawn = null;
				return false;
			}
			pawn = PawnGenerator.GeneratePawn(result, base.Faction);
			spawnedPawns.Add(pawn);
			GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(base.Position, base.Map, 2), base.Map);
			Lord lord = Lord;
			if (lord == null)
			{
				lord = CreateNewLord();
			}
			lord.AddPawn(pawn);
			SoundDefOf.Hive_Spawn.PlayOneShot(this);
			return true;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "DEBUG: Spawn pawn",
					icon = TexCommand.ReleaseAnimals,
					action = delegate
					{
						((_003CGetGizmos_003Ec__Iterator0)/*Error near IL_00ef: stateMachine*/)._0024this.TrySpawnPawn(out Pawn _);
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0129:
			/*Error near IL_012a: Unexpected return in MoveNext()*/;
		}

		public override bool PreventPlayerSellingThingsNearby(out string reason)
		{
			if (spawnedPawns.Count > 0 && spawnedPawns.Any((Pawn p) => !p.Downed))
			{
				reason = def.label;
				return true;
			}
			reason = null;
			return false;
		}

		private Lord CreateNewLord()
		{
			return LordMaker.MakeNewLord(base.Faction, new LordJob_DefendAndExpandHive(!caveColony), base.Map);
		}
	}
}
