using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public class CompLaunchable : ThingComp
	{
		private CompTransporter cachedCompTransporter;

		public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment");

		private static readonly Texture2D LaunchCommandTex = ContentFinder<Texture2D>.Get("UI/Commands/LaunchShip");

		private const float FuelPerTile = 2.25f;

		public Building FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map);

		public bool ConnectedToFuelingPort => FuelingPortSource != null;

		public bool FuelingPortSourceHasAnyFuel => ConnectedToFuelingPort && FuelingPortSource.GetComp<CompRefuelable>().HasFuel;

		public bool LoadingInProgressOrReadyToLaunch => Transporter.LoadingInProgressOrReadyToLaunch;

		public bool AnythingLeftToLoad => Transporter.AnythingLeftToLoad;

		public Thing FirstThingLeftToLoad => Transporter.FirstThingLeftToLoad;

		public List<CompTransporter> TransportersInGroup => Transporter.TransportersInGroup(parent.Map);

		public bool AnyInGroupHasAnythingLeftToLoad => Transporter.AnyInGroupHasAnythingLeftToLoad;

		public Thing FirstThingLeftToLoadInGroup => Transporter.FirstThingLeftToLoadInGroup;

		public bool AnyInGroupIsUnderRoof
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (transportersInGroup[i].parent.Position.Roofed(parent.Map))
					{
						return true;
					}
				}
				return false;
			}
		}

		public CompTransporter Transporter
		{
			get
			{
				if (cachedCompTransporter == null)
				{
					cachedCompTransporter = parent.GetComp<CompTransporter>();
				}
				return cachedCompTransporter;
			}
		}

		public float FuelingPortSourceFuel
		{
			get
			{
				if (!ConnectedToFuelingPort)
				{
					return 0f;
				}
				return FuelingPortSource.GetComp<CompRefuelable>().Fuel;
			}
		}

		public bool AllInGroupConnectedToFuelingPort
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (!transportersInGroup[i].Launchable.ConnectedToFuelingPort)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool AllFuelingPortSourcesInGroupHaveAnyFuel
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					if (!transportersInGroup[i].Launchable.FuelingPortSourceHasAnyFuel)
					{
						return false;
					}
				}
				return true;
			}
		}

		private float FuelInLeastFueledFuelingPortSource
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				float num = 0f;
				bool flag = false;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					float fuelingPortSourceFuel = transportersInGroup[i].Launchable.FuelingPortSourceFuel;
					if (!flag || fuelingPortSourceFuel < num)
					{
						num = fuelingPortSourceFuel;
						flag = true;
					}
				}
				if (!flag)
				{
					return 0f;
				}
				return num;
			}
		}

		private int MaxLaunchDistance
		{
			get
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					return 0;
				}
				return MaxLaunchDistanceAtFuelLevel(FuelInLeastFueledFuelingPortSource);
			}
		}

		private int MaxLaunchDistanceEverPossible
		{
			get
			{
				if (!LoadingInProgressOrReadyToLaunch)
				{
					return 0;
				}
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				float num = 0f;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					Building fuelingPortSource = transportersInGroup[i].Launchable.FuelingPortSource;
					if (fuelingPortSource != null)
					{
						num = Mathf.Max(num, fuelingPortSource.GetComp<CompRefuelable>().Props.fuelCapacity);
					}
				}
				return MaxLaunchDistanceAtFuelLevel(num);
			}
		}

		private bool PodsHaveAnyPotentialCaravanOwner
		{
			get
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				for (int i = 0; i < transportersInGroup.Count; i++)
				{
					ThingOwner innerContainer = transportersInGroup[i].innerContainer;
					for (int j = 0; j < innerContainer.Count; j++)
					{
						Pawn pawn = innerContainer[j] as Pawn;
						if (pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
						{
							return true;
						}
					}
				}
				return false;
			}
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
			if (LoadingInProgressOrReadyToLaunch)
			{
				Command_Action launch = new Command_Action
				{
					defaultLabel = "CommandLaunchGroup".Translate(),
					defaultDesc = "CommandLaunchGroupDesc".Translate(),
					icon = LaunchCommandTex,
					alsoClickIfOtherInGroupClicked = false,
					action = delegate
					{
						if (((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011b: stateMachine*/)._0024this.AnyInGroupHasAnythingLeftToLoad)
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSendNotCompletelyLoadedPods".Translate(((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011b: stateMachine*/)._0024this.FirstThingLeftToLoadInGroup.LabelCapNoCount, ((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011b: stateMachine*/)._0024this.FirstThingLeftToLoadInGroup), ((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011b: stateMachine*/)._0024this.StartChoosingDestination));
						}
						else
						{
							((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_011b: stateMachine*/)._0024this.StartChoosingDestination();
						}
					}
				};
				if (!AllInGroupConnectedToFuelingPort)
				{
					launch.Disable("CommandLaunchGroupFailNotConnectedToFuelingPort".Translate());
				}
				else if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					launch.Disable("CommandLaunchGroupFailNoFuel".Translate());
				}
				else if (AnyInGroupIsUnderRoof)
				{
					launch.Disable("CommandLaunchGroupFailUnderRoof".Translate());
				}
				yield return (Gizmo)launch;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_01ce:
			/*Error near IL_01cf: Unexpected return in MoveNext()*/;
		}

		public override string CompInspectStringExtra()
		{
			if (LoadingInProgressOrReadyToLaunch)
			{
				if (!AllInGroupConnectedToFuelingPort)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllInGroupConnectedToFuelingPort".Translate() + ".";
				}
				if (!AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					return "NotReadyForLaunch".Translate() + ": " + "NotAllFuelingPortSourcesInGroupHaveAnyFuel".Translate() + ".";
				}
				if (AnyInGroupHasAnythingLeftToLoad)
				{
					return "NotReadyForLaunch".Translate() + ": " + "TransportPodInGroupHasSomethingLeftToLoad".Translate() + ".";
				}
				return "ReadyForLaunch".Translate();
			}
			return null;
		}

		private void StartChoosingDestination()
		{
			CameraJumper.TryJump(CameraJumper.GetWorldTarget(parent));
			Find.WorldSelector.ClearSelection();
			int tile = parent.Map.Tile;
			Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, canTargetTiles: true, TargeterMouseAttachment, closeWorldTabWhenFinished: true, delegate
			{
				GenDraw.DrawWorldRadiusRing(tile, MaxLaunchDistance);
			}, delegate(GlobalTargetInfo target)
			{
				if (!target.IsValid)
				{
					return null;
				}
				int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile);
				if (num > MaxLaunchDistance)
				{
					GUI.color = Color.red;
					if (num > MaxLaunchDistanceEverPossible)
					{
						return "TransportPodDestinationBeyondMaximumRange".Translate();
					}
					return "TransportPodNotEnoughFuel".Translate();
				}
				IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = GetTransportPodsFloatMenuOptionsAt(target.Tile);
				if (!transportPodsFloatMenuOptionsAt.Any())
				{
					return string.Empty;
				}
				if (transportPodsFloatMenuOptionsAt.Count() == 1)
				{
					if (transportPodsFloatMenuOptionsAt.First().Disabled)
					{
						GUI.color = Color.red;
					}
					return transportPodsFloatMenuOptionsAt.First().Label;
				}
				MapParent mapParent = target.WorldObject as MapParent;
				if (mapParent != null)
				{
					return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
				}
				return "ClickToSeeAvailableOrders_Empty".Translate();
			});
		}

		private bool ChoseWorldTarget(GlobalTargetInfo target)
		{
			if (!LoadingInProgressOrReadyToLaunch)
			{
				return true;
			}
			if (!target.IsValid)
			{
				Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			int num = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile);
			if (num > MaxLaunchDistance)
			{
				Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(FuelNeededToLaunchAtDist((float)num).ToString("0.#")), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = GetTransportPodsFloatMenuOptionsAt(target.Tile);
			if (!transportPodsFloatMenuOptionsAt.Any())
			{
				if (Find.World.Impassable(target.Tile))
				{
					Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
				TryLaunch(target.Tile, null);
				return true;
			}
			if (transportPodsFloatMenuOptionsAt.Count() == 1)
			{
				if (!transportPodsFloatMenuOptionsAt.First().Disabled)
				{
					transportPodsFloatMenuOptionsAt.First().action();
				}
				return false;
			}
			Find.WindowStack.Add(new FloatMenu(transportPodsFloatMenuOptionsAt.ToList()));
			return false;
		}

		public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction)
		{
			if (!parent.Spawned)
			{
				Log.Error("Tried to launch " + parent + ", but it's unspawned.");
			}
			else
			{
				List<CompTransporter> transportersInGroup = TransportersInGroup;
				if (transportersInGroup == null)
				{
					Log.Error("Tried to launch " + parent + ", but it's not in any group.");
				}
				else if (LoadingInProgressOrReadyToLaunch && AllInGroupConnectedToFuelingPort && AllFuelingPortSourcesInGroupHaveAnyFuel)
				{
					Map map = parent.Map;
					int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile);
					if (num <= MaxLaunchDistance)
					{
						Transporter.TryRemoveLord(map);
						int groupID = Transporter.groupID;
						float amount = Mathf.Max(FuelNeededToLaunchAtDist((float)num), 1f);
						for (int i = 0; i < transportersInGroup.Count; i++)
						{
							CompTransporter compTransporter = transportersInGroup[i];
							compTransporter.Launchable.FuelingPortSource?.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
							ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();
							ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
							activeDropPod.Contents = new ActiveDropPodInfo();
							activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, canMergeWithExistingStacks: true, destroyLeftover: true);
							DropPodLeaving dropPodLeaving = (DropPodLeaving)SkyfallerMaker.MakeSkyfaller(ThingDefOf.DropPodLeaving, activeDropPod);
							dropPodLeaving.groupID = groupID;
							dropPodLeaving.destinationTile = destinationTile;
							dropPodLeaving.arrivalAction = arrivalAction;
							compTransporter.CleanUpLoadingVars(map);
							compTransporter.parent.Destroy();
							GenSpawn.Spawn(dropPodLeaving, compTransporter.parent.Position, map);
						}
						CameraJumper.TryHideWorld();
					}
				}
			}
		}

		public void Notify_FuelingPortSourceDeSpawned()
		{
			if (Transporter.CancelLoad())
			{
				Messages.Message("MessageTransportersLoadCanceled_FuelingPortGiverDeSpawned".Translate(), parent, MessageTypeDefOf.NegativeEvent);
			}
		}

		public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
		{
			return Mathf.FloorToInt(fuelLevel / 2.25f);
		}

		public static float FuelNeededToLaunchAtDist(float dist)
		{
			return 2.25f * dist;
		}

		public IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile)
		{
			_003CGetTransportPodsFloatMenuOptionsAt_003Ec__Iterator1 _003CGetTransportPodsFloatMenuOptionsAt_003Ec__Iterator = (_003CGetTransportPodsFloatMenuOptionsAt_003Ec__Iterator1)/*Error near IL_003c: stateMachine*/;
			bool anything = false;
			if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(TransportersInGroup.Cast<IThingHolder>(), tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
			{
				yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
				{
					_003CGetTransportPodsFloatMenuOptionsAt_003Ec__Iterator._0024this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan());
				});
				/*Error: Unable to find new state assignment for yield return*/;
			}
			List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
			for (int i = 0; i < worldObjects.Count; i++)
			{
				if (worldObjects[i].Tile == tile)
				{
					using (IEnumerator<FloatMenuOption> enumerator = worldObjects[i].GetTransportPodsFloatMenuOptions(TransportersInGroup.Cast<IThingHolder>(), this).GetEnumerator())
					{
						if (enumerator.MoveNext())
						{
							FloatMenuOption o = enumerator.Current;
							anything = true;
							yield return o;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (!anything && !Find.World.Impassable(tile))
			{
				yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
				{
					_003CGetTransportPodsFloatMenuOptionsAt_003Ec__Iterator._0024this.TryLaunch(tile, null);
				});
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_0290:
			/*Error near IL_0291: Unexpected return in MoveNext()*/;
		}
	}
}
