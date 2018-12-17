using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Building_Grave : Building_Casket, IStoreSettingsParent, IAssignableBuilding, IHaulDestination
	{
		private StorageSettings storageSettings;

		private Graphic cachedGraphicFull;

		public Pawn assignedPawn;

		public override Graphic Graphic
		{
			get
			{
				if (HasCorpse)
				{
					if (def.building.fullGraveGraphicData == null)
					{
						return base.Graphic;
					}
					if (cachedGraphicFull == null)
					{
						cachedGraphicFull = def.building.fullGraveGraphicData.GraphicColoredFor(this);
					}
					return cachedGraphicFull;
				}
				return base.Graphic;
			}
		}

		public bool HasCorpse => Corpse != null;

		public Corpse Corpse
		{
			get
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					Corpse corpse = innerContainer[i] as Corpse;
					if (corpse != null)
					{
						return corpse;
					}
				}
				return null;
			}
		}

		public IEnumerable<Pawn> AssigningCandidates
		{
			get
			{
				if (!base.Spawned)
				{
					return Enumerable.Empty<Pawn>();
				}
				IEnumerable<Pawn> second = from Corpse x in base.Map.listerThings.ThingsInGroup(ThingRequestGroup.Corpse)
				where x.InnerPawn.IsColonist
				select x.InnerPawn;
				return base.Map.mapPawns.FreeColonistsSpawned.Concat(second);
			}
		}

		public IEnumerable<Pawn> AssignedPawns
		{
			get
			{
				if (assignedPawn != null)
				{
					yield return assignedPawn;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public int MaxAssignedPawnsCount => 1;

		public bool StorageTabVisible => assignedPawn == null && !HasCorpse;

		public void TryAssignPawn(Pawn pawn)
		{
			pawn.ownership.ClaimGrave(this);
		}

		public void TryUnassignPawn(Pawn pawn)
		{
			if (pawn == assignedPawn)
			{
				pawn.ownership.UnclaimGrave();
			}
		}

		public bool AssignedAnything(Pawn pawn)
		{
			return pawn.ownership.AssignedGrave != null;
		}

		public StorageSettings GetStoreSettings()
		{
			return storageSettings;
		}

		public StorageSettings GetParentStoreSettings()
		{
			return def.building.fixedStorageSettings;
		}

		public override void PostMake()
		{
			base.PostMake();
			storageSettings = new StorageSettings(this);
			if (def.building.defaultStorageSettings != null)
			{
				storageSettings.CopyFrom(def.building.defaultStorageSettings);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		}

		public override void EjectContents()
		{
			base.EjectContents();
			if (base.Spawned)
			{
				base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
			}
		}

		public virtual void Notify_CorpseBuried(Pawn worker)
		{
			CompArt comp = GetComp<CompArt>();
			if (comp != null && !comp.Active)
			{
				comp.JustCreatedBy(worker);
				comp.InitializeArt(Corpse.InnerPawn);
			}
			base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things | MapMeshFlag.Buildings);
			worker.records.Increment(RecordDefOf.CorpsesBuried);
			TaleRecorder.RecordTale(TaleDefOf.BuriedCorpse, worker, (Corpse == null) ? null : Corpse.InnerPawn);
		}

		public override bool Accepts(Thing thing)
		{
			if (!base.Accepts(thing))
			{
				return false;
			}
			if (HasCorpse)
			{
				return false;
			}
			if (assignedPawn != null)
			{
				Corpse corpse = thing as Corpse;
				if (corpse == null)
				{
					return false;
				}
				if (corpse.InnerPawn != assignedPawn)
				{
					return false;
				}
			}
			else if (!storageSettings.AllowedToAccept(thing))
			{
				return false;
			}
			return true;
		}

		public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
		{
			if (base.TryAcceptThing(thing, allowSpecialEffects))
			{
				Corpse corpse = thing as Corpse;
				if (corpse != null && corpse.InnerPawn.ownership != null && corpse.InnerPawn.ownership.AssignedGrave != this)
				{
					corpse.InnerPawn.ownership.UnclaimGrave();
				}
				if (base.Spawned)
				{
					base.Map.mapDrawer.MapMeshDirty(base.Position, MapMeshFlag.Things);
				}
				return true;
			}
			return false;
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			using (IEnumerator<Gizmo> enumerator = base.GetGizmos().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g2 = enumerator.Current;
					yield return g2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (StorageTabVisible)
			{
				using (IEnumerator<Gizmo> enumerator2 = StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings).GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						Gizmo g = enumerator2.Current;
						yield return g;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (!HasCorpse)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandGraveAssignColonistLabel".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/AssignOwner"),
					defaultDesc = "CommandGraveAssignColonistDesc".Translate(),
					action = delegate
					{
						Find.WindowStack.Add(new Dialog_AssignBuildingOwner(((_003CGetGizmos_003Ec__Iterator1)/*Error near IL_01bb: stateMachine*/)._0024this));
					},
					hotKey = KeyBindingDefOf.Misc3
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0205:
			/*Error near IL_0206: Unexpected return in MoveNext()*/;
		}

		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(base.GetInspectString());
			if (HasCorpse)
			{
				if (base.Tile != -1)
				{
					string value = GenDate.DateFullStringAt(GenDate.TickGameToAbs(Corpse.timeOfDeath), Find.WorldGrid.LongLatOf(base.Tile));
					stringBuilder.AppendLine();
					stringBuilder.Append("DiedOn".Translate(value));
				}
			}
			else if (assignedPawn != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.Append("AssignedColonist".Translate());
				stringBuilder.Append(": ");
				stringBuilder.Append(assignedPawn.LabelCap);
			}
			return stringBuilder.ToString();
		}
	}
}
