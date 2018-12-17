using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;

namespace RimWorld
{
	public class Dialog_LoadTransporters : Window
	{
		private enum Tab
		{
			Pawns,
			Items
		}

		private Map map;

		private List<CompTransporter> transporters;

		private List<TransferableOneWay> transferables;

		private TransferableOneWayWidget pawnsTransfer;

		private TransferableOneWayWidget itemsTransfer;

		private Tab tab;

		private float lastMassFlashTime = -9999f;

		private bool massUsageDirty = true;

		private float cachedMassUsage;

		private bool caravanMassUsageDirty = true;

		private float cachedCaravanMassUsage;

		private bool caravanMassCapacityDirty = true;

		private float cachedCaravanMassCapacity;

		private string cachedCaravanMassCapacityExplanation;

		private bool tilesPerDayDirty = true;

		private float cachedTilesPerDay;

		private string cachedTilesPerDayExplanation;

		private bool daysWorthOfFoodDirty = true;

		private Pair<float, float> cachedDaysWorthOfFood;

		private bool foragedFoodPerDayDirty = true;

		private Pair<ThingDef, float> cachedForagedFoodPerDay;

		private string cachedForagedFoodPerDayExplanation;

		private bool visibilityDirty = true;

		private float cachedVisibility;

		private string cachedVisibilityExplanation;

		private const float TitleRectHeight = 35f;

		private const float BottomAreaHeight = 55f;

		private readonly Vector2 BottomButtonSize = new Vector2(160f, 40f);

		private static List<TabRecord> tabsList = new List<TabRecord>();

		public override Vector2 InitialSize => new Vector2(1024f, (float)UI.screenHeight);

		protected override float Margin => 0f;

		private float MassCapacity
		{
			get
			{
				float num = 0f;
				for (int i = 0; i < transporters.Count; i++)
				{
					num += transporters[i].Props.massCapacity;
				}
				return num;
			}
		}

		private float CaravanMassCapacity
		{
			get
			{
				if (caravanMassCapacityDirty)
				{
					caravanMassCapacityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedCaravanMassCapacity = CollectionsMassCalculator.CapacityTransferables(transferables, stringBuilder);
					cachedCaravanMassCapacityExplanation = stringBuilder.ToString();
				}
				return cachedCaravanMassCapacity;
			}
		}

		private string TransportersLabel => Find.ActiveLanguageWorker.Pluralize(transporters[0].parent.Label);

		private string TransportersLabelCap => TransportersLabel.CapitalizeFirst();

		private BiomeDef Biome => map.Biome;

		private float MassUsage
		{
			get
			{
				if (massUsageDirty)
				{
					massUsageDirty = false;
					cachedMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, includePawnsMass: true);
				}
				return cachedMassUsage;
			}
		}

		public float CaravanMassUsage
		{
			get
			{
				if (caravanMassUsageDirty)
				{
					caravanMassUsageDirty = false;
					cachedCaravanMassUsage = CollectionsMassCalculator.MassUsageTransferables(transferables, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload);
				}
				return cachedCaravanMassUsage;
			}
		}

		private float TilesPerDay
		{
			get
			{
				if (tilesPerDayDirty)
				{
					tilesPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedTilesPerDay = TilesPerDayCalculator.ApproxTilesPerDay(transferables, MassUsage, MassCapacity, map.Tile, -1, stringBuilder);
					cachedTilesPerDayExplanation = stringBuilder.ToString();
				}
				return cachedTilesPerDay;
			}
		}

		private Pair<float, float> DaysWorthOfFood
		{
			get
			{
				if (daysWorthOfFoodDirty)
				{
					daysWorthOfFoodDirty = false;
					float first = DaysWorthOfFoodCalculator.ApproxDaysWorthOfFood(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload, Faction.OfPlayer);
					cachedDaysWorthOfFood = new Pair<float, float>(first, DaysUntilRotCalculator.ApproxDaysUntilRot(transferables, map.Tile, IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload));
				}
				return cachedDaysWorthOfFood;
			}
		}

		private Pair<ThingDef, float> ForagedFoodPerDay
		{
			get
			{
				if (foragedFoodPerDayDirty)
				{
					foragedFoodPerDayDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedForagedFoodPerDay = ForagedFoodPerDayCalculator.ForagedFoodPerDay(transferables, Biome, Faction.OfPlayer, stringBuilder);
					cachedForagedFoodPerDayExplanation = stringBuilder.ToString();
				}
				return cachedForagedFoodPerDay;
			}
		}

		private float Visibility
		{
			get
			{
				if (visibilityDirty)
				{
					visibilityDirty = false;
					StringBuilder stringBuilder = new StringBuilder();
					cachedVisibility = CaravanVisibilityCalculator.Visibility(transferables, stringBuilder);
					cachedVisibilityExplanation = stringBuilder.ToString();
				}
				return cachedVisibility;
			}
		}

		public Dialog_LoadTransporters(Map map, List<CompTransporter> transporters)
		{
			this.map = map;
			this.transporters = new List<CompTransporter>();
			this.transporters.AddRange(transporters);
			forcePause = true;
			absorbInputAroundWindow = true;
		}

		public override void PostOpen()
		{
			base.PostOpen();
			CalculateAndRecacheTransferables();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, inRect.width, 35f);
			Text.Font = GameFont.Medium;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect, "LoadTransporters".Translate(TransportersLabel));
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.UpperLeft;
			CaravanUIUtility.DrawCaravanInfo(new CaravanUIUtility.CaravanInfo(MassUsage, MassCapacity, string.Empty, TilesPerDay, cachedTilesPerDayExplanation, DaysWorthOfFood, ForagedFoodPerDay, cachedForagedFoodPerDayExplanation, Visibility, cachedVisibilityExplanation, CaravanMassUsage, CaravanMassCapacity, cachedCaravanMassCapacityExplanation), null, map.Tile, null, lastMassFlashTime, new Rect(12f, 35f, inRect.width - 24f, 40f), lerpMassColor: false);
			tabsList.Clear();
			tabsList.Add(new TabRecord("PawnsTab".Translate(), delegate
			{
				tab = Tab.Pawns;
			}, tab == Tab.Pawns));
			tabsList.Add(new TabRecord("ItemsTab".Translate(), delegate
			{
				tab = Tab.Items;
			}, tab == Tab.Items));
			inRect.yMin += 119f;
			Widgets.DrawMenuSection(inRect);
			TabDrawer.DrawTabs(inRect, tabsList);
			inRect = inRect.ContractedBy(17f);
			GUI.BeginGroup(inRect);
			Rect rect2 = inRect.AtZero();
			DoBottomButtons(rect2);
			Rect inRect2 = rect2;
			inRect2.yMax -= 59f;
			bool anythingChanged = false;
			switch (tab)
			{
			case Tab.Pawns:
				pawnsTransfer.OnGUI(inRect2, out anythingChanged);
				break;
			case Tab.Items:
				itemsTransfer.OnGUI(inRect2, out anythingChanged);
				break;
			}
			if (anythingChanged)
			{
				CountToTransferChanged();
			}
			GUI.EndGroup();
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		private void AddToTransferables(Thing t)
		{
			TransferableOneWay transferableOneWay = TransferableUtility.TransferableMatching(t, transferables, TransferAsOneMode.PodsOrCaravanPacking);
			if (transferableOneWay == null)
			{
				transferableOneWay = new TransferableOneWay();
				transferables.Add(transferableOneWay);
			}
			transferableOneWay.things.Add(t);
		}

		private void DoBottomButtons(Rect rect)
		{
			float num = rect.width / 2f;
			Vector2 bottomButtonSize = BottomButtonSize;
			float x = num - bottomButtonSize.x / 2f;
			float y = rect.height - 55f;
			Vector2 bottomButtonSize2 = BottomButtonSize;
			float x2 = bottomButtonSize2.x;
			Vector2 bottomButtonSize3 = BottomButtonSize;
			Rect rect2 = new Rect(x, y, x2, bottomButtonSize3.y);
			if (Widgets.ButtonText(rect2, "AcceptButton".Translate()))
			{
				if (CaravanMassUsage > CaravanMassCapacity && CaravanMassCapacity != 0f)
				{
					if (CheckForErrors(TransferableUtility.GetPawnsFromTransferables(transferables)))
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("TransportersCaravanWillBeImmobile".Translate(), delegate
						{
							if (TryAccept())
							{
								SoundDefOf.Tick_High.PlayOneShotOnCamera();
								Close(doCloseSound: false);
							}
						}));
					}
				}
				else if (TryAccept())
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Close(doCloseSound: false);
				}
			}
			float num2 = rect2.x - 10f;
			Vector2 bottomButtonSize4 = BottomButtonSize;
			float x3 = num2 - bottomButtonSize4.x;
			float y2 = rect2.y;
			Vector2 bottomButtonSize5 = BottomButtonSize;
			float x4 = bottomButtonSize5.x;
			Vector2 bottomButtonSize6 = BottomButtonSize;
			Rect rect3 = new Rect(x3, y2, x4, bottomButtonSize6.y);
			if (Widgets.ButtonText(rect3, "ResetButton".Translate()))
			{
				SoundDefOf.Tick_Low.PlayOneShotOnCamera();
				CalculateAndRecacheTransferables();
			}
			float x5 = rect2.xMax + 10f;
			float y3 = rect2.y;
			Vector2 bottomButtonSize7 = BottomButtonSize;
			float x6 = bottomButtonSize7.x;
			Vector2 bottomButtonSize8 = BottomButtonSize;
			Rect rect4 = new Rect(x5, y3, x6, bottomButtonSize8.y);
			if (Widgets.ButtonText(rect4, "CancelButton".Translate()))
			{
				Close();
			}
			if (Prefs.DevMode)
			{
				float width = 200f;
				Vector2 bottomButtonSize9 = BottomButtonSize;
				float num3 = bottomButtonSize9.y / 2f;
				Rect rect5 = new Rect(0f, rect.height - 55f, width, num3);
				if (Widgets.ButtonText(rect5, "Dev: Load instantly") && DebugTryLoadInstantly())
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Close(doCloseSound: false);
				}
				Rect rect6 = new Rect(0f, rect.height - 55f + num3, width, num3);
				if (Widgets.ButtonText(rect6, "Dev: Select everything"))
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					SetToLoadEverything();
				}
			}
		}

		private void CalculateAndRecacheTransferables()
		{
			transferables = new List<TransferableOneWay>();
			AddPawnsToTransferables();
			AddItemsToTransferables();
			IEnumerable<TransferableOneWay> enumerable = null;
			string sourceLabel = null;
			string destinationLabel = null;
			string sourceCountDesc = "FormCaravanColonyThingCountTip".Translate();
			bool drawMass = true;
			IgnorePawnsInventoryMode ignorePawnInventoryMass = IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload;
			bool includePawnsMassInMassUsage = true;
			Func<float> availableMassGetter = () => MassCapacity - MassUsage;
			int tile = map.Tile;
			pawnsTransfer = new TransferableOneWayWidget(enumerable, sourceLabel, destinationLabel, sourceCountDesc, drawMass, ignorePawnInventoryMass, includePawnsMassInMassUsage, availableMassGetter, 0f, ignoreSpawnedCorpseGearAndInventoryMass: false, tile, drawMarketValue: true, drawEquippedWeapon: true, drawNutritionEatenPerDay: true, drawItemNutrition: false, drawForagedFoodPerDay: true);
			CaravanUIUtility.AddPawnsSections(pawnsTransfer, transferables);
			enumerable = from x in transferables
			where x.ThingDef.category != ThingCategory.Pawn
			select x;
			sourceCountDesc = null;
			destinationLabel = null;
			sourceLabel = "FormCaravanColonyThingCountTip".Translate();
			includePawnsMassInMassUsage = true;
			ignorePawnInventoryMass = IgnorePawnsInventoryMode.IgnoreIfAssignedToUnload;
			drawMass = true;
			availableMassGetter = (() => MassCapacity - MassUsage);
			tile = map.Tile;
			itemsTransfer = new TransferableOneWayWidget(enumerable, sourceCountDesc, destinationLabel, sourceLabel, includePawnsMassInMassUsage, ignorePawnInventoryMass, drawMass, availableMassGetter, 0f, ignoreSpawnedCorpseGearAndInventoryMass: false, tile, drawMarketValue: true, drawEquippedWeapon: false, drawNutritionEatenPerDay: false, drawItemNutrition: true, drawForagedFoodPerDay: false, drawDaysUntilRot: true);
			CountToTransferChanged();
		}

		private bool DebugTryLoadInstantly()
		{
			CreateAndAssignNewTransportersGroup();
			int i;
			for (i = 0; i < transferables.Count; i++)
			{
				TransferableUtility.Transfer(transferables[i].things, transferables[i].CountToTransfer, delegate(Thing splitPiece, IThingHolder originalThing)
				{
					transporters[i % transporters.Count].GetDirectlyHeldThings().TryAdd(splitPiece);
				});
			}
			return true;
		}

		private bool TryAccept()
		{
			List<Pawn> pawnsFromTransferables = TransferableUtility.GetPawnsFromTransferables(transferables);
			if (!CheckForErrors(pawnsFromTransferables))
			{
				return false;
			}
			int transportersGroup = CreateAndAssignNewTransportersGroup();
			AssignTransferablesToRandomTransporters();
			IEnumerable<Pawn> enumerable = from x in pawnsFromTransferables
			where x.IsColonist && !x.Downed
			select x;
			if (enumerable.Any())
			{
				foreach (Pawn item in enumerable)
				{
					item.GetLord()?.Notify_PawnLost(item, PawnLostCondition.ForcedToJoinOtherLord);
				}
				LordMaker.MakeNewLord(Faction.OfPlayer, new LordJob_LoadAndEnterTransporters(transportersGroup), map, enumerable);
				foreach (Pawn item2 in enumerable)
				{
					if (item2.Spawned)
					{
						item2.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
				}
			}
			Messages.Message("MessageTransportersLoadingProcessStarted".Translate(), transporters[0].parent, MessageTypeDefOf.TaskCompletion, historical: false);
			return true;
		}

		private void AssignTransferablesToRandomTransporters()
		{
			TransferableOneWay transferableOneWay = transferables.MaxBy((TransferableOneWay x) => x.CountToTransfer);
			int num = 0;
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i] != transferableOneWay && transferables[i].CountToTransfer > 0)
				{
					transporters[num % transporters.Count].AddToTheToLoadList(transferables[i], transferables[i].CountToTransfer);
					num++;
				}
			}
			if (num < transporters.Count)
			{
				int num2 = transferableOneWay.CountToTransfer;
				int num3 = num2 / (transporters.Count - num);
				for (int j = num; j < transporters.Count; j++)
				{
					int num4 = (j != transporters.Count - 1) ? num3 : num2;
					if (num4 > 0)
					{
						transporters[j].AddToTheToLoadList(transferableOneWay, num4);
					}
					num2 -= num4;
				}
			}
			else
			{
				transporters[num % transporters.Count].AddToTheToLoadList(transferableOneWay, transferableOneWay.CountToTransfer);
			}
		}

		private int CreateAndAssignNewTransportersGroup()
		{
			int nextTransporterGroupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
			for (int i = 0; i < transporters.Count; i++)
			{
				transporters[i].groupID = nextTransporterGroupID;
			}
			return nextTransporterGroupID;
		}

		private bool CheckForErrors(List<Pawn> pawns)
		{
			if (!transferables.Any((TransferableOneWay x) => x.CountToTransfer != 0))
			{
				Messages.Message("CantSendEmptyTransportPods".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (MassUsage > MassCapacity)
			{
				FlashMass();
				Messages.Message("TooBigTransportersMassUsage".Translate(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			Pawn pawn = pawns.Find((Pawn x) => !x.MapHeld.reachability.CanReach(x.PositionHeld, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)));
			if (pawn != null)
			{
				Messages.Message("PawnCantReachTransporters".Translate(pawn.LabelShort, pawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			Map map = transporters[0].parent.Map;
			for (int i = 0; i < transferables.Count; i++)
			{
				if (transferables[i].ThingDef.category == ThingCategory.Item)
				{
					int countToTransfer = transferables[i].CountToTransfer;
					int num = 0;
					if (countToTransfer > 0)
					{
						for (int j = 0; j < transferables[i].things.Count; j++)
						{
							Thing thing = transferables[i].things[j];
							if (map.reachability.CanReach(thing.Position, transporters[0].parent, PathEndMode.Touch, TraverseParms.For(TraverseMode.PassDoors)))
							{
								num += thing.stackCount;
								if (num >= countToTransfer)
								{
									break;
								}
							}
						}
						if (num < countToTransfer)
						{
							if (countToTransfer == 1)
							{
								Messages.Message("TransporterItemIsUnreachableSingle".Translate(transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
							}
							else
							{
								Messages.Message("TransporterItemIsUnreachableMulti".Translate(countToTransfer, transferables[i].ThingDef.label), MessageTypeDefOf.RejectInput, historical: false);
							}
							return false;
						}
					}
				}
			}
			return true;
		}

		private void AddPawnsToTransferables()
		{
			List<Pawn> list = CaravanFormingUtility.AllSendablePawns(map);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransferables(list[i]);
			}
		}

		private void AddItemsToTransferables()
		{
			List<Thing> list = CaravanFormingUtility.AllReachableColonyItems(map);
			for (int i = 0; i < list.Count; i++)
			{
				AddToTransferables(list[i]);
			}
		}

		private void FlashMass()
		{
			lastMassFlashTime = Time.time;
		}

		private void SetToLoadEverything()
		{
			for (int i = 0; i < transferables.Count; i++)
			{
				transferables[i].AdjustTo(transferables[i].GetMaximumToTransfer());
			}
			CountToTransferChanged();
		}

		private void CountToTransferChanged()
		{
			massUsageDirty = true;
			caravanMassUsageDirty = true;
			caravanMassCapacityDirty = true;
			tilesPerDayDirty = true;
			daysWorthOfFoodDirty = true;
			foragedFoodPerDayDirty = true;
			visibilityDirty = true;
		}
	}
}
