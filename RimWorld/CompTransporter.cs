using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompTransporter : ThingComp, IThingHolder
	{
		public int groupID = -1;

		public ThingOwner innerContainer;

		public List<TransferableOneWay> leftToLoad;

		private CompLaunchable cachedCompLaunchable;

		private bool notifiedCantLoadMore;

		private static readonly Texture2D CancelLoadCommandTex = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

		private static readonly Texture2D LoadCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LoadTransporter");

		private static readonly Texture2D SelectPreviousInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectPreviousTransporter");

		private static readonly Texture2D SelectAllInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectAllTransporters");

		private static readonly Texture2D SelectNextInGroupCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/SelectNextTransporter");

		private static List<CompTransporter> tmpTransportersInGroup = new List<CompTransporter>();

		public CompProperties_Transporter Props => (CompProperties_Transporter)props;

		public Map Map => parent.MapHeld;

		public bool AnythingLeftToLoad => FirstThingLeftToLoad != null;

		public bool LoadingInProgressOrReadyToLaunch => groupID >= 0;

		public bool AnyInGroupHasAnythingLeftToLoad => FirstThingLeftToLoadInGroup != null;

		public CompLaunchable Launchable
		{
			get
			{
				if (cachedCompLaunchable == null)
				{
					cachedCompLaunchable = parent.GetComp<CompLaunchable>();
				}
				return cachedCompLaunchable;
			}
		}

		public Thing FirstThingLeftToLoad
		{
			get
			{
				if (leftToLoad == null)
				{
					return null;
				}
				for (int i = 0; i < leftToLoad.Count; i++)
				{
					if (leftToLoad[i].CountToTransfer != 0 && leftToLoad[i].HasAnyThing)
					{
						return leftToLoad[i].AnyThing;
					}
				}
				return null;
			}
		}

		public Thing FirstThingLeftToLoadInGroup
		{
			get
			{
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int i = 0; i < list.Count; i++)
				{
					Thing firstThingLeftToLoad = list[i].FirstThingLeftToLoad;
					if (firstThingLeftToLoad != null)
					{
						return firstThingLeftToLoad;
					}
				}
				return null;
			}
		}

		public bool AnyInGroupNotifiedCantLoadMore
		{
			get
			{
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].notifiedCantLoadMore)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool AnyPawnCanLoadAnythingNow
		{
			get
			{
				if (!AnythingLeftToLoad)
				{
					return false;
				}
				if (!parent.Spawned)
				{
					return false;
				}
				List<Pawn> allPawnsSpawned = parent.Map.mapPawns.AllPawnsSpawned;
				for (int i = 0; i < allPawnsSpawned.Count; i++)
				{
					if (allPawnsSpawned[i].CurJobDef == JobDefOf.HaulToTransporter)
					{
						CompTransporter transporter = ((JobDriver_HaulToTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
						if (transporter != null && transporter.groupID == groupID)
						{
							return true;
						}
					}
					if (allPawnsSpawned[i].CurJobDef == JobDefOf.EnterTransporter)
					{
						CompTransporter transporter2 = ((JobDriver_EnterTransporter)allPawnsSpawned[i].jobs.curDriver).Transporter;
						if (transporter2 != null && transporter2.groupID == groupID)
						{
							return true;
						}
					}
				}
				List<CompTransporter> list = TransportersInGroup(parent.Map);
				for (int j = 0; j < allPawnsSpawned.Count; j++)
				{
					if (allPawnsSpawned[j].mindState.duty != null && allPawnsSpawned[j].mindState.duty.transportersGroup == groupID)
					{
						CompTransporter compTransporter = JobGiver_EnterTransporter.FindMyTransporter(list, allPawnsSpawned[j]);
						if (compTransporter != null && allPawnsSpawned[j].CanReach(compTransporter.parent, PathEndMode.Touch, Danger.Deadly))
						{
							return true;
						}
					}
				}
				for (int k = 0; k < allPawnsSpawned.Count; k++)
				{
					if (allPawnsSpawned[k].IsColonist)
					{
						for (int l = 0; l < list.Count; l++)
						{
							if (LoadTransportersJobUtility.HasJobOnTransporter(allPawnsSpawned[k], list[l]))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
		}

		public CompTransporter()
		{
			innerContainer = new ThingOwner<Thing>(this);
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref groupID, "groupID", 0);
			Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
			Scribe_Collections.Look(ref leftToLoad, "leftToLoad", LookMode.Deep);
			Scribe_Values.Look(ref notifiedCantLoadMore, "notifiedCantLoadMore", defaultValue: false);
		}

		public ThingOwner GetDirectlyHeldThings()
		{
			return innerContainer;
		}

		public void GetChildHolders(List<IThingHolder> outChildren)
		{
			ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
		}

		public override void CompTick()
		{
			base.CompTick();
			innerContainer.ThingOwnerTick();
			if (Props.restEffectiveness != 0f)
			{
				for (int i = 0; i < innerContainer.Count; i++)
				{
					Pawn pawn = innerContainer[i] as Pawn;
					if (pawn != null && !pawn.Dead && pawn.needs.rest != null)
					{
						pawn.needs.rest.TickResting(Props.restEffectiveness);
					}
				}
			}
			if (parent.IsHashIntervalTick(60) && parent.Spawned && LoadingInProgressOrReadyToLaunch && AnyInGroupHasAnythingLeftToLoad && !AnyInGroupNotifiedCantLoadMore && !AnyPawnCanLoadAnythingNow)
			{
				notifiedCantLoadMore = true;
				Messages.Message("MessageCantLoadMoreIntoTransporters".Translate(FirstThingLeftToLoadInGroup.LabelNoCount, Faction.OfPlayer.def.pawnsPlural, FirstThingLeftToLoadInGroup), parent, MessageTypeDefOf.CautionInput);
			}
		}

		public List<CompTransporter> TransportersInGroup(Map map)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return null;
			}
			TransporterUtility.GetTransportersInGroup(groupID, map, tmpTransportersInGroup);
			return tmpTransportersInGroup;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			using (IEnumerator<Gizmo> enumerator = base.CompGetGizmosExtra().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Gizmo g = enumerator.Current;
					yield return g;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (!LoadingInProgressOrReadyToLaunch)
			{
				Command_LoadToTransporter loadGroup = new Command_LoadToTransporter();
				int selectedTransportersCount = 0;
				for (int i = 0; i < Find.Selector.NumSelected; i++)
				{
					Thing thing = Find.Selector.SelectedObjectsListForReading[i] as Thing;
					if (thing != null && thing.def == parent.def)
					{
						CompLaunchable compLaunchable = thing.TryGetComp<CompLaunchable>();
						if (compLaunchable == null || (compLaunchable.FuelingPortSource != null && compLaunchable.FuelingPortSourceHasAnyFuel))
						{
							selectedTransportersCount++;
						}
					}
				}
				loadGroup.defaultLabel = "CommandLoadTransporter".Translate(selectedTransportersCount.ToString());
				loadGroup.defaultDesc = "CommandLoadTransporterDesc".Translate();
				loadGroup.icon = LoadCommandTex;
				loadGroup.transComp = this;
				CompLaunchable launchable = Launchable;
				if (launchable != null)
				{
					if (!launchable.ConnectedToFuelingPort)
					{
						loadGroup.Disable("CommandLoadTransporterFailNotConnectedToFuelingPort".Translate());
					}
					else if (!launchable.FuelingPortSourceHasAnyFuel)
					{
						loadGroup.Disable("CommandLoadTransporterFailNoFuel".Translate());
					}
				}
				yield return (Gizmo)loadGroup;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return (Gizmo)new Command_Action
			{
				defaultLabel = "CommandCancelLoad".Translate(),
				defaultDesc = "CommandCancelLoadDesc".Translate(),
				icon = CancelLoadCommandTex,
				action = delegate
				{
					SoundDefOf.Designate_Cancel.PlayOneShotOnCamera();
					((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011f: stateMachine*/)._0024this.CancelLoad();
				}
			};
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0457:
			/*Error near IL_0458: Unexpected return in MoveNext()*/;
		}

		public override void PostDeSpawn(Map map)
		{
			base.PostDeSpawn(map);
			if (CancelLoad(map))
			{
				Messages.Message("MessageTransportersLoadCanceled_TransporterDestroyed".Translate(), MessageTypeDefOf.NegativeEvent);
			}
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
		}

		public override string CompInspectStringExtra()
		{
			return "Contents".Translate() + ": " + innerContainer.ContentsString.CapitalizeFirst();
		}

		public void AddToTheToLoadList(TransferableOneWay t, int count)
		{
			if (t.HasAnyThing && t.CountToTransfer > 0)
			{
				if (leftToLoad == null)
				{
					leftToLoad = new List<TransferableOneWay>();
				}
				if (TransferableUtility.TransferableMatching(t.AnyThing, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking) != null)
				{
					Log.Error("Transferable already exists.");
				}
				else
				{
					TransferableOneWay transferableOneWay = new TransferableOneWay();
					leftToLoad.Add(transferableOneWay);
					transferableOneWay.things.AddRange(t.things);
					transferableOneWay.AdjustTo(count);
				}
			}
		}

		public void Notify_ThingAdded(Thing t)
		{
			SubtractFromToLoadList(t, t.stackCount);
		}

		public void Notify_ThingAddedAndMergedWith(Thing t, int mergedCount)
		{
			SubtractFromToLoadList(t, mergedCount);
		}

		public bool CancelLoad()
		{
			return CancelLoad(Map);
		}

		public bool CancelLoad(Map map)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return false;
			}
			TryRemoveLord(map);
			List<CompTransporter> list = TransportersInGroup(map);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].CleanUpLoadingVars(map);
			}
			CleanUpLoadingVars(map);
			return true;
		}

		public void TryRemoveLord(Map map)
		{
			if (LoadingInProgressOrReadyToLaunch)
			{
				Lord lord = TransporterUtility.FindLord(groupID, map);
				if (lord != null)
				{
					map.lordManager.RemoveLord(lord);
				}
			}
		}

		public void CleanUpLoadingVars(Map map)
		{
			groupID = -1;
			innerContainer.TryDropAll(parent.Position, map, ThingPlaceMode.Near);
			if (leftToLoad != null)
			{
				leftToLoad.Clear();
			}
		}

		private void SubtractFromToLoadList(Thing t, int count)
		{
			if (leftToLoad != null)
			{
				TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatchingDesperate(t, leftToLoad, TransferAsOneMode.PodsOrCaravanPacking);
				if (transferableOneWay != null)
				{
					transferableOneWay.AdjustBy(-count);
					if (transferableOneWay.CountToTransfer <= 0)
					{
						leftToLoad.Remove(transferableOneWay);
					}
					if (!AnyInGroupHasAnythingLeftToLoad)
					{
						Messages.Message("MessageFinishedLoadingTransporters".Translate(), parent, MessageTypeDefOf.TaskCompletion);
					}
				}
			}
		}

		private void SelectPreviousInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[GenMath.PositiveMod(num - 1, list.Count)].parent);
		}

		private void SelectAllInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			Selector selector = Find.Selector;
			selector.ClearSelection();
			for (int i = 0; i < list.Count; i++)
			{
				selector.Select(list[i].parent);
			}
		}

		private void SelectNextInGroup()
		{
			List<CompTransporter> list = TransportersInGroup(Map);
			int num = list.IndexOf(this);
			CameraJumper.TryJumpAndSelect(list[(num + 1) % list.Count].parent);
		}
	}
}
