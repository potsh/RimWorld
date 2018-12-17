using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public abstract class Zone : IExposable, ISelectable, ILoadReferenceable
	{
		public ZoneManager zoneManager;

		public int ID = -1;

		public string label;

		public List<IntVec3> cells = new List<IntVec3>();

		private bool cellsShuffled;

		public Color color = Color.white;

		private Material materialInt;

		public bool hidden;

		private int lastStaticFireCheckTick = -9999;

		private bool lastStaticFireCheckResult;

		private const int StaticFireCheckInterval = 1000;

		private static BoolGrid extantGrid;

		private static BoolGrid foundGrid;

		public Map Map => zoneManager.map;

		public IntVec3 Position => (cells.Count == 0) ? IntVec3.Invalid : cells[0];

		public Material Material
		{
			get
			{
				if (materialInt == null)
				{
					materialInt = SolidColorMaterials.SimpleSolidColorMaterial(color);
					materialInt.renderQueue = 3600;
				}
				return materialInt;
			}
		}

		public List<IntVec3> Cells
		{
			get
			{
				if (!cellsShuffled)
				{
					cells.Shuffle();
					cellsShuffled = true;
				}
				return cells;
			}
		}

		public IEnumerable<Thing> AllContainedThings
		{
			get
			{
				ThingGrid grids = Map.thingGrid;
				int j = 0;
				List<Thing> thingList;
				int i;
				while (true)
				{
					if (j >= cells.Count)
					{
						yield break;
					}
					thingList = grids.ThingsListAt(cells[j]);
					i = 0;
					if (i < thingList.Count)
					{
						break;
					}
					j++;
				}
				yield return thingList[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool ContainsStaticFire
		{
			get
			{
				if (Find.TickManager.TicksGame > lastStaticFireCheckTick + 1000)
				{
					lastStaticFireCheckResult = false;
					for (int i = 0; i < cells.Count; i++)
					{
						if (cells[i].ContainsStaticFire(Map))
						{
							lastStaticFireCheckResult = true;
							break;
						}
					}
				}
				return lastStaticFireCheckResult;
			}
		}

		public virtual bool IsMultiselectable => false;

		protected abstract Color NextZoneColor
		{
			get;
		}

		public Zone()
		{
		}

		public Zone(string baseName, ZoneManager zoneManager)
		{
			label = zoneManager.NewZoneName(baseName);
			this.zoneManager = zoneManager;
			ID = Find.UniqueIDsManager.GetNextZoneID();
			color = NextZoneColor;
		}

		public IEnumerator<IntVec3> GetEnumerator()
		{
			int i = 0;
			if (i < cells.Count)
			{
				yield return cells[i];
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Values.Look(ref ID, "ID", -1);
			Scribe_Values.Look(ref label, "label");
			Scribe_Values.Look(ref color, "color");
			Scribe_Values.Look(ref hidden, "hidden", defaultValue: false);
			Scribe_Collections.Look(ref cells, "cells", LookMode.Undefined);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				BackCompatibility.ZonePostLoadInit(this);
				CheckAddHaulDestination();
			}
		}

		public virtual void AddCell(IntVec3 c)
		{
			if (cells.Contains(c))
			{
				Log.Error("Adding cell to zone which already has it. c=" + c + ", zone=" + this);
			}
			else
			{
				List<Thing> list = Map.thingGrid.ThingsListAt(c);
				for (int i = 0; i < list.Count; i++)
				{
					Thing thing = list[i];
					if (!thing.def.CanOverlapZones)
					{
						Log.Error("Added zone over zone-incompatible thing " + thing);
						return;
					}
				}
				cells.Add(c);
				zoneManager.AddZoneGridCell(this, c);
				Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
				AutoHomeAreaMaker.Notify_ZoneCellAdded(c, this);
				cellsShuffled = false;
			}
		}

		public virtual void RemoveCell(IntVec3 c)
		{
			if (!cells.Contains(c))
			{
				Log.Error("Cannot remove cell from zone which doesn't have it. c=" + c + ", zone=" + this);
			}
			else
			{
				cells.Remove(c);
				zoneManager.ClearZoneGridCell(c);
				Map.mapDrawer.MapMeshDirty(c, MapMeshFlag.Zone);
				cellsShuffled = false;
				if (cells.Count == 0)
				{
					Deregister();
				}
			}
		}

		public virtual void Delete()
		{
			SoundDefOf.Designate_ZoneDelete.PlayOneShotOnCamera(Map);
			if (cells.Count != 0)
			{
				while (cells.Count > 0)
				{
					RemoveCell(cells[cells.Count - 1]);
				}
			}
			else
			{
				Deregister();
			}
			Find.Selector.Deselect(this);
		}

		public void Deregister()
		{
			zoneManager.DeregisterZone(this);
		}

		public virtual void PostRegister()
		{
			CheckAddHaulDestination();
		}

		public virtual void PostDeregister()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				Map.haulDestinationManager.RemoveHaulDestination(haulDestination);
			}
		}

		public bool ContainsCell(IntVec3 c)
		{
			for (int i = 0; i < cells.Count; i++)
			{
				if (cells[i] == c)
				{
					return true;
				}
			}
			return false;
		}

		public virtual string GetInspectString()
		{
			return string.Empty;
		}

		public virtual IEnumerable<InspectTabBase> GetInspectTabs()
		{
			yield break;
		}

		public virtual IEnumerable<Gizmo> GetGizmos()
		{
			yield return (Gizmo)new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone"),
				defaultLabel = "CommandRenameZoneLabel".Translate(),
				defaultDesc = "CommandRenameZoneDesc".Translate(),
				action = delegate
				{
					Find.WindowStack.Add(new Dialog_RenameZone(((_003CGetGizmos_003Ec__Iterator3)/*Error near IL_0084: stateMachine*/)._0024this));
				},
				hotKey = KeyBindingDefOf.Misc1
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}

		public virtual IEnumerable<Gizmo> GetZoneAddGizmos()
		{
			yield break;
		}

		public void CheckContiguous()
		{
			if (cells.Count != 0)
			{
				if (extantGrid == null)
				{
					extantGrid = new BoolGrid(Map);
				}
				else
				{
					extantGrid.ClearAndResizeTo(Map);
				}
				if (foundGrid == null)
				{
					foundGrid = new BoolGrid(Map);
				}
				else
				{
					foundGrid.ClearAndResizeTo(Map);
				}
				for (int i = 0; i < cells.Count; i++)
				{
					extantGrid.Set(cells[i], value: true);
				}
				Predicate<IntVec3> passCheck = delegate(IntVec3 c)
				{
					if (!extantGrid[c])
					{
						return false;
					}
					if (foundGrid[c])
					{
						return false;
					}
					return true;
				};
				int numFound = 0;
				Action<IntVec3> processor = delegate(IntVec3 c)
				{
					foundGrid.Set(c, value: true);
					numFound++;
				};
				Map.floodFiller.FloodFill(cells[0], passCheck, processor);
				if (numFound < cells.Count)
				{
					foreach (IntVec3 allCell in Map.AllCells)
					{
						if (extantGrid[allCell] && !foundGrid[allCell])
						{
							RemoveCell(allCell);
						}
					}
				}
			}
		}

		private void CheckAddHaulDestination()
		{
			IHaulDestination haulDestination = this as IHaulDestination;
			if (haulDestination != null)
			{
				Map.haulDestinationManager.AddHaulDestination(haulDestination);
			}
		}

		public override string ToString()
		{
			return label;
		}

		public string GetUniqueLoadID()
		{
			return "Zone_" + ID;
		}
	}
}
