using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public static class Autotests_ColonyMaker
	{
		private static CellRect overRect;

		private static BoolGrid usedCells;

		private const int OverRectSize = 100;

		private static Map Map => Find.CurrentMap;

		public static void MakeColony_Full()
		{
			MakeColony(ColonyMakerFlag.ConduitGrid, ColonyMakerFlag.PowerPlants, ColonyMakerFlag.Batteries, ColonyMakerFlag.WorkTables, ColonyMakerFlag.AllBuildings, ColonyMakerFlag.AllItems, ColonyMakerFlag.Filth, ColonyMakerFlag.ColonistsMany, ColonyMakerFlag.ColonistsHungry, ColonyMakerFlag.ColonistsTired, ColonyMakerFlag.ColonistsInjured, ColonyMakerFlag.ColonistsDiseased, ColonyMakerFlag.Beds, ColonyMakerFlag.Stockpiles, ColonyMakerFlag.GrowingZones);
		}

		public static void MakeColony_Animals()
		{
			MakeColony(default(ColonyMakerFlag));
		}

		public static void MakeColony(params ColonyMakerFlag[] flags)
		{
			bool godMode = DebugSettings.godMode;
			DebugSettings.godMode = true;
			Thing.allowDestroyNonDestroyable = true;
			if (usedCells == null)
			{
				usedCells = new BoolGrid(Map);
			}
			else
			{
				usedCells.ClearAndResizeTo(Map);
			}
			IntVec3 center = Map.Center;
			int minX = center.x - 50;
			IntVec3 center2 = Map.Center;
			overRect = new CellRect(minX, center2.z - 50, 100, 100);
			DeleteAllSpawnedPawns();
			GenDebug.ClearArea(overRect, Find.CurrentMap);
			if (flags.Contains(ColonyMakerFlag.Animals))
			{
				foreach (PawnKindDef item in from k in DefDatabase<PawnKindDef>.AllDefs
				where k.RaceProps.Animal
				select k)
				{
					if (!TryGetFreeRect(6, 3, out CellRect result))
					{
						return;
					}
					result = result.ContractedBy(1);
					foreach (IntVec3 item2 in result)
					{
						Map.terrainGrid.SetTerrain(item2, TerrainDefOf.Concrete);
					}
					GenSpawn.Spawn(PawnGenerator.GeneratePawn(item), result.Cells.ElementAt(0), Map);
					IntVec3 intVec = result.Cells.ElementAt(1);
					Pawn p = (Pawn)GenSpawn.Spawn(PawnGenerator.GeneratePawn(item), intVec, Map);
					HealthUtility.DamageUntilDead(p);
					Corpse thing = (Corpse)intVec.GetThingList(Find.CurrentMap).First((Thing t) => t is Corpse);
					CompRottable compRottable = thing.TryGetComp<CompRottable>();
					if (compRottable != null)
					{
						compRottable.RotProgress += 1200000f;
					}
					if (item.RaceProps.leatherDef != null)
					{
						GenSpawn.Spawn(item.RaceProps.leatherDef, result.Cells.ElementAt(2), Map);
					}
					if (item.RaceProps.meatDef != null)
					{
						GenSpawn.Spawn(item.RaceProps.meatDef, result.Cells.ElementAt(3), Map);
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.ConduitGrid))
			{
				Designator_Build designator_Build = new Designator_Build(ThingDefOf.PowerConduit);
				for (int i = overRect.minX; i < overRect.maxX; i++)
				{
					for (int j = overRect.minZ; j < overRect.maxZ; j += 7)
					{
						designator_Build.DesignateSingleCell(new IntVec3(i, 0, j));
					}
				}
				for (int l = overRect.minZ; l < overRect.maxZ; l++)
				{
					for (int m = overRect.minX; m < overRect.maxX; m += 7)
					{
						designator_Build.DesignateSingleCell(new IntVec3(m, 0, l));
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.PowerPlants))
			{
				List<ThingDef> list = new List<ThingDef>();
				list.Add(ThingDefOf.SolarGenerator);
				list.Add(ThingDefOf.WindTurbine);
				List<ThingDef> list2 = list;
				for (int n = 0; n < 8; n++)
				{
					if (TryMakeBuilding(list2[n % list2.Count]) == null)
					{
						Log.Message("Could not make solar generator.");
						break;
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.Batteries))
			{
				for (int num = 0; num < 6; num++)
				{
					Thing thing2 = TryMakeBuilding(ThingDefOf.Battery);
					if (thing2 == null)
					{
						Log.Message("Could not make battery.");
						break;
					}
					((Building_Battery)thing2).GetComp<CompPowerBattery>().AddEnergy(999999f);
				}
			}
			if (flags.Contains(ColonyMakerFlag.WorkTables))
			{
				IEnumerable<ThingDef> enumerable = from def in DefDatabase<ThingDef>.AllDefs
				where typeof(Building_WorkTable).IsAssignableFrom(def.thingClass)
				select def;
				foreach (ThingDef item3 in enumerable)
				{
					Thing thing3 = TryMakeBuilding(item3);
					if (thing3 == null)
					{
						Log.Message("Could not make worktable: " + item3.defName);
						break;
					}
					Building_WorkTable building_WorkTable = thing3 as Building_WorkTable;
					if (building_WorkTable != null)
					{
						foreach (RecipeDef allRecipe in building_WorkTable.def.AllRecipes)
						{
							building_WorkTable.billStack.AddBill(allRecipe.MakeNewBill());
						}
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.AllBuildings))
			{
				IEnumerable<ThingDef> enumerable2 = from def in DefDatabase<ThingDef>.AllDefs
				where def.category == ThingCategory.Building && def.BuildableByPlayer
				select def;
				foreach (ThingDef item4 in enumerable2)
				{
					if (item4 != ThingDefOf.PowerConduit)
					{
						Thing thing4 = TryMakeBuilding(item4);
						if (thing4 == null)
						{
							Log.Message("Could not make building: " + item4.defName);
							break;
						}
					}
				}
			}
			if (!TryGetFreeRect(33, 33, out CellRect result2))
			{
				Log.Error("Could not get wallable rect");
			}
			result2 = result2.ContractedBy(1);
			if (flags.Contains(ColonyMakerFlag.AllItems))
			{
				List<ThingDef> itemDefs = (from def in DefDatabase<ThingDef>.AllDefs
				where DebugThingPlaceHelper.IsDebugSpawnable(def) && def.category == ThingCategory.Item
				select def).ToList();
				FillWithItems(result2, itemDefs);
			}
			else if (flags.Contains(ColonyMakerFlag.ItemsRawFood))
			{
				List<ThingDef> list3 = new List<ThingDef>();
				list3.Add(ThingDefOf.RawPotatoes);
				FillWithItems(result2, list3);
			}
			if (flags.Contains(ColonyMakerFlag.Filth))
			{
				foreach (IntVec3 item5 in result2)
				{
					GenSpawn.Spawn(ThingDefOf.Filth_Dirt, item5, Map);
				}
			}
			if (flags.Contains(ColonyMakerFlag.ItemsWall))
			{
				CellRect cellRect = result2.ExpandedBy(1);
				Designator_Build designator_Build2 = new Designator_Build(ThingDefOf.Wall);
				designator_Build2.SetStuffDef(ThingDefOf.WoodLog);
				foreach (IntVec3 edgeCell in cellRect.EdgeCells)
				{
					designator_Build2.DesignateSingleCell(edgeCell);
				}
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsMany))
			{
				MakeColonists(15, overRect.CenterCell);
			}
			else if (flags.Contains(ColonyMakerFlag.ColonistOne))
			{
				MakeColonists(1, overRect.CenterCell);
			}
			if (flags.Contains(ColonyMakerFlag.Fire))
			{
				if (!TryGetFreeRect(30, 30, out CellRect result3))
				{
					Log.Error("Could not get free rect for fire.");
				}
				ThingDef plant_TreeOak = ThingDefOf.Plant_TreeOak;
				foreach (IntVec3 item6 in result3)
				{
					GenSpawn.Spawn(plant_TreeOak, item6, Map);
				}
				foreach (IntVec3 item7 in result3)
				{
					IntVec3 current9 = item7;
					if (current9.x % 7 == 0 && current9.z % 7 == 0)
					{
						GenExplosion.DoExplosion(current9, Find.CurrentMap, 3.9f, DamageDefOf.Flame, null);
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsHungry))
			{
				DoToColonists(0.4f, delegate(Pawn col)
				{
					col.needs.food.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsTired))
			{
				DoToColonists(0.4f, delegate(Pawn col)
				{
					col.needs.rest.CurLevel = Mathf.Max(0f, Rand.Range(-0.05f, 0.05f));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsInjured))
			{
				DoToColonists(0.4f, delegate(Pawn col)
				{
					DamageDef def2 = (from d in DefDatabase<DamageDef>.AllDefs
					where d.ExternalViolenceFor(null)
					select d).RandomElement();
					col.TakeDamage(new DamageInfo(def2, 10f));
				});
			}
			if (flags.Contains(ColonyMakerFlag.ColonistsDiseased))
			{
				foreach (HediffDef item8 in from d in DefDatabase<HediffDef>.AllDefs
				where d.hediffClass != typeof(Hediff_AddedPart) && (d.HasComp(typeof(HediffComp_Immunizable)) || d.HasComp(typeof(HediffComp_GrowthMode)))
				select d)
				{
					Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
					TryGetFreeRect(1, 1, out CellRect result4);
					GenSpawn.Spawn(pawn, result4.CenterCell, Map);
					pawn.health.AddHediff(item8);
				}
			}
			if (flags.Contains(ColonyMakerFlag.Beds))
			{
				IEnumerable<ThingDef> source = from def in DefDatabase<ThingDef>.AllDefs
				where def.thingClass == typeof(Building_Bed)
				select def;
				int freeColonistsCount = Map.mapPawns.FreeColonistsCount;
				for (int num2 = 0; num2 < freeColonistsCount; num2++)
				{
					if (TryMakeBuilding(source.RandomElement()) == null)
					{
						Log.Message("Could not make beds.");
						break;
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.Stockpiles))
			{
				Designator_ZoneAddStockpile_Resources designator_ZoneAddStockpile_Resources = new Designator_ZoneAddStockpile_Resources();
				IEnumerator enumerator11 = Enum.GetValues(typeof(StoragePriority)).GetEnumerator();
				try
				{
					while (enumerator11.MoveNext())
					{
						StoragePriority priority = (StoragePriority)enumerator11.Current;
						TryGetFreeRect(7, 7, out CellRect result5);
						result5 = result5.ContractedBy(1);
						designator_ZoneAddStockpile_Resources.DesignateMultiCell(result5.Cells);
						Zone_Stockpile zone_Stockpile = (Zone_Stockpile)Map.zoneManager.ZoneAt(result5.CenterCell);
						zone_Stockpile.settings.Priority = priority;
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator11 as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
			}
			if (flags.Contains(ColonyMakerFlag.GrowingZones))
			{
				Zone_Growing dummyZone = new Zone_Growing(Map.zoneManager);
				Map.zoneManager.RegisterZone(dummyZone);
				foreach (ThingDef item9 in from d in DefDatabase<ThingDef>.AllDefs
				where d.plant != null && PlantUtility.CanSowOnGrower(d, dummyZone)
				select d)
				{
					if (!TryGetFreeRect(6, 6, out CellRect result6))
					{
						Log.Error("Could not get growing zone rect.");
					}
					result6 = result6.ContractedBy(1);
					foreach (IntVec3 item10 in result6)
					{
						Map.terrainGrid.SetTerrain(item10, TerrainDefOf.Soil);
					}
					Designator_ZoneAdd_Growing designator_ZoneAdd_Growing = new Designator_ZoneAdd_Growing();
					designator_ZoneAdd_Growing.DesignateMultiCell(result6.Cells);
					(Map.zoneManager.ZoneAt(result6.CenterCell) as Zone_Growing)?.SetPlantDefToGrow(item9);
				}
				dummyZone.Delete();
			}
			ClearAllHomeArea();
			FillWithHomeArea(overRect);
			DebugSettings.godMode = godMode;
			Thing.allowDestroyNonDestroyable = false;
		}

		private static void FillWithItems(CellRect rect, List<ThingDef> itemDefs)
		{
			int num = 0;
			foreach (IntVec3 item in rect)
			{
				IntVec3 current = item;
				if (current.x % 6 != 0 && current.z % 6 != 0)
				{
					ThingDef def = itemDefs[num];
					DebugThingPlaceHelper.DebugSpawn(def, current, -1, direct: true);
					num++;
					if (num >= itemDefs.Count)
					{
						num = 0;
					}
				}
			}
		}

		private static Thing TryMakeBuilding(ThingDef def)
		{
			if (!TryGetFreeRect(def.size.x + 2, def.size.z + 2, out CellRect result))
			{
				return null;
			}
			foreach (IntVec3 item in result)
			{
				Map.terrainGrid.SetTerrain(item, TerrainDefOf.Concrete);
			}
			Designator_Build designator_Build = new Designator_Build(def);
			designator_Build.DesignateSingleCell(result.CenterCell);
			return result.CenterCell.GetEdifice(Find.CurrentMap);
		}

		private static bool TryGetFreeRect(int width, int height, out CellRect result)
		{
			for (int i = overRect.minZ; i <= overRect.maxZ - height; i++)
			{
				for (int j = overRect.minX; j <= overRect.maxX - width; j++)
				{
					CellRect cellRect = new CellRect(j, i, width, height);
					bool flag = true;
					for (int k = cellRect.minZ; k <= cellRect.maxZ; k++)
					{
						for (int l = cellRect.minX; l <= cellRect.maxX; l++)
						{
							if (usedCells[l, k])
							{
								flag = false;
								break;
							}
						}
						if (!flag)
						{
							break;
						}
					}
					if (flag)
					{
						result = cellRect;
						for (int m = cellRect.minZ; m <= cellRect.maxZ; m++)
						{
							for (int n = cellRect.minX; n <= cellRect.maxX; n++)
							{
								IntVec3 c = new IntVec3(n, 0, m);
								usedCells.Set(c, value: true);
								if (c.GetTerrain(Find.CurrentMap).passability == Traversability.Impassable)
								{
									Map.terrainGrid.SetTerrain(c, TerrainDefOf.Concrete);
								}
							}
						}
						return true;
					}
				}
			}
			result = new CellRect(0, 0, width, height);
			return false;
		}

		private static void DoToColonists(float fraction, Action<Pawn> funcToDo)
		{
			int num = Rand.RangeInclusive(1, Mathf.RoundToInt((float)Map.mapPawns.FreeColonistsCount * fraction));
			int num2 = 0;
			foreach (Pawn item in Map.mapPawns.FreeColonists.InRandomOrder())
			{
				funcToDo(item);
				num2++;
				if (num2 >= num)
				{
					break;
				}
			}
		}

		private static void MakeColonists(int count, IntVec3 center)
		{
			for (int i = 0; i < count; i++)
			{
				TryGetFreeRect(1, 1, out CellRect result);
				Pawn pawn = PawnGenerator.GeneratePawn(Faction.OfPlayer.def.basicMemberKind, Faction.OfPlayer);
				foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
				{
					if (!pawn.story.WorkTypeIsDisabled(allDef))
					{
						pawn.workSettings.SetPriority(allDef, 3);
					}
				}
				GenSpawn.Spawn(pawn, result.CenterCell, Map);
			}
		}

		private static void DeleteAllSpawnedPawns()
		{
			foreach (Pawn item in Map.mapPawns.AllPawnsSpawned.ToList())
			{
				item.Destroy();
				item.relations.ClearAllRelations();
			}
			Find.GameEnder.gameEnding = false;
		}

		private static void ClearAllHomeArea()
		{
			foreach (IntVec3 allCell in Map.AllCells)
			{
				Map.areaManager.Home[allCell] = false;
			}
		}

		private static void FillWithHomeArea(CellRect r)
		{
			Designator_AreaHomeExpand designator_AreaHomeExpand = new Designator_AreaHomeExpand();
			designator_AreaHomeExpand.DesignateMultiCell(r.Cells);
		}
	}
}
