using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class Building_PlantGrower : Building, IPlantToGrowSettable
	{
		private ThingDef plantDefToGrow;

		private CompPowerTrader compPower;

		IEnumerable<IntVec3> IPlantToGrowSettable.Cells
		{
			get
			{
				return this.OccupiedRect().Cells;
			}
		}

		public IEnumerable<Plant> PlantsOnMe
		{
			get
			{
				if (base.Spawned)
				{
					CellRect.CellRectIterator cri = this.OccupiedRect().GetIterator();
					while (!cri.Done())
					{
						List<Thing> thingList = base.Map.thingGrid.ThingsListAt(cri.Current);
						for (int i = 0; i < thingList.Count; i++)
						{
							Plant p = thingList[i] as Plant;
							if (p != null)
							{
								yield return p;
								/*Error: Unable to find new state assignment for yield return*/;
							}
						}
						cri.MoveNext();
					}
				}
			}
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
			yield return (Gizmo)PlantToGrowSettableUtility.SetPlantToGrowCommand(this);
			/*Error: Unable to find new state assignment for yield return*/;
			IL_00e2:
			/*Error near IL_00e3: Unexpected return in MoveNext()*/;
		}

		public override void PostMake()
		{
			base.PostMake();
			plantDefToGrow = def.building.defaultPlantToGrow;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			compPower = GetComp<CompPowerTrader>();
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Defs.Look(ref plantDefToGrow, "plantDefToGrow");
		}

		public override void TickRare()
		{
			if (compPower != null && !compPower.PowerOn)
			{
				foreach (Plant item in PlantsOnMe)
				{
					DamageInfo dinfo = new DamageInfo(DamageDefOf.Rotting, 1f);
					item.TakeDamage(dinfo);
				}
			}
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			foreach (Plant item in PlantsOnMe.ToList())
			{
				item.Destroy();
			}
			base.DeSpawn(mode);
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (base.Spawned)
			{
				text = ((!PlantUtility.GrowthSeasonNow(base.Position, base.Map, forSowing: true)) ? (text + "\n" + "CannotGrowBadSeasonTemperature".Translate()) : (text + "\n" + "GrowSeasonHereNow".Translate()));
			}
			return text;
		}

		public ThingDef GetPlantDefToGrow()
		{
			return plantDefToGrow;
		}

		public void SetPlantDefToGrow(ThingDef plantDef)
		{
			plantDefToGrow = plantDef;
		}

		public bool CanAcceptSowNow()
		{
			if (compPower != null && !compPower.PowerOn)
			{
				return false;
			}
			return true;
		}
	}
}
