using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class CompLongRangeMineralScanner : ThingComp
	{
		private ThingDef targetMineable;

		private float daysWorkingSinceLastMinerals;

		private CompPowerTrader powerComp;

		public CompProperties_LongRangeMineralScanner Props => (CompProperties_LongRangeMineralScanner)props;

		public bool CanUseNow
		{
			get
			{
				if (!parent.Spawned)
				{
					return false;
				}
				if (powerComp != null && !powerComp.PowerOn)
				{
					return false;
				}
				return parent.Faction == Faction.OfPlayer;
			}
		}

		public override void PostExposeData()
		{
			Scribe_Defs.Look(ref targetMineable, "targetMineable");
			Scribe_Values.Look(ref daysWorkingSinceLastMinerals, "daysWorkingSinceLastMinerals", 0f);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && targetMineable == null)
			{
				SetDefaultTargetMineral();
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			SetDefaultTargetMineral();
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			base.PostSpawnSetup(respawningAfterLoad);
			powerComp = parent.GetComp<CompPowerTrader>();
		}

		private void SetDefaultTargetMineral()
		{
			targetMineable = ThingDefOf.MineableGold;
		}

		public void Used(Pawn worker)
		{
			if (!CanUseNow)
			{
				Log.Error("Used while CanUseNow is false.");
			}
			float statValue = worker.GetStatValue(StatDefOf.ResearchSpeed);
			daysWorkingSinceLastMinerals += statValue / 60000f;
			if (Find.TickManager.TicksGame % 59 == 0)
			{
				float mtb = Props.mtbDays / statValue;
				if (daysWorkingSinceLastMinerals >= Props.guaranteedToFindLumpAfterDaysWorking || Rand.MTBEventOccurs(mtb, 60000f, 59f))
				{
					FoundMinerals(worker);
				}
			}
		}

		private void FoundMinerals(Pawn worker)
		{
			daysWorkingSinceLastMinerals = 0f;
			IntRange preciousLumpSiteDistanceRange = SiteTuning.PreciousLumpSiteDistanceRange;
			int min = preciousLumpSiteDistanceRange.min;
			int max = preciousLumpSiteDistanceRange.max;
			int tile = parent.Tile;
			int tile2 = default(int);
			if (TileFinder.TryFindNewSiteTile(out tile2, min, max, allowCaravans: false, preferCloserTiles: true, tile))
			{
				Site site = SiteMaker.TryMakeSite_SingleSitePart(SiteCoreDefOf.PreciousLump, (!Rand.Chance(0.6f)) ? "MineralScannerPreciousLumpThreat" : null, tile2);
				if (site != null)
				{
					site.sitePartsKnown = true;
					site.core.parms.preciousLumpResources = targetMineable;
					int randomInRange = SiteTuning.MineralScannerPreciousLumpTimeoutDaysRange.RandomInRange;
					site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * 60000);
					Find.WorldObjects.Add(site);
					Find.LetterStack.ReceiveLetter("LetterLabelFoundPreciousLump".Translate(), "LetterFoundPreciousLump".Translate(targetMineable.label, randomInRange, SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()).CapitalizeFirst(), worker.LabelShort, worker.Named("WORKER")), LetterDefOf.PositiveEvent, site);
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (parent.Faction == Faction.OfPlayer)
			{
				ThingDef resource = targetMineable.building.mineableThing;
				ThingDef localD;
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "CommandSelectMineralToScanFor".Translate() + ": " + resource.LabelCap,
					icon = resource.uiIcon,
					iconAngle = resource.uiIconAngle,
					iconOffset = resource.uiIconOffset,
					action = delegate
					{
						List<ThingDef> mineables = ((GenStep_PreciousLump)GenStepDefOf.PreciousLump.genStep).mineables;
						List<FloatMenuOption> list = new List<FloatMenuOption>();
						foreach (ThingDef item2 in mineables)
						{
							localD = item2;
							FloatMenuOption item = new FloatMenuOption(localD.building.mineableThing.LabelCap, delegate
							{
								foreach (object selectedObject in Find.Selector.SelectedObjects)
								{
									Thing thing = selectedObject as Thing;
									if (thing != null)
									{
										CompLongRangeMineralScanner compLongRangeMineralScanner = thing.TryGetComp<CompLongRangeMineralScanner>();
										if (compLongRangeMineralScanner != null)
										{
											compLongRangeMineralScanner.targetMineable = localD;
										}
									}
								}
							}, MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, localD.building.mineableThing));
							list.Add(item);
						}
						Find.WindowStack.Add(new FloatMenu(list));
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (Prefs.DevMode)
			{
				yield return (Gizmo)new Command_Action
				{
					defaultLabel = "Dev: Find resources now",
					action = delegate
					{
						((_003CCompGetGizmosExtra_003Ec__Iterator0)/*Error near IL_0144: stateMachine*/)._0024this.FoundMinerals(PawnsFinder.AllMaps_FreeColonists.FirstOrDefault());
					}
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
