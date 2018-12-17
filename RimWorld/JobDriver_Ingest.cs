using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobDriver_Ingest : JobDriver
	{
		private bool usingNutrientPasteDispenser;

		private bool eatingFromInventory;

		public const float EatCorpseBodyPartsUntilFoodLevelPct = 0.9f;

		public const TargetIndex IngestibleSourceInd = TargetIndex.A;

		private const TargetIndex TableCellInd = TargetIndex.B;

		private const TargetIndex ExtraIngestiblesToCollectInd = TargetIndex.C;

		private Thing IngestibleSource => job.GetTarget(TargetIndex.A).Thing;

		private float ChewDurationMultiplier
		{
			get
			{
				Thing ingestibleSource = IngestibleSource;
				if (ingestibleSource.def.ingestible != null && !ingestibleSource.def.ingestible.useEatingSpeedStat)
				{
					return 1f;
				}
				return 1f / pawn.GetStatValue(StatDefOf.EatingSpeed);
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref usingNutrientPasteDispenser, "usingNutrientPasteDispenser", defaultValue: false);
			Scribe_Values.Look(ref eatingFromInventory, "eatingFromInventory", defaultValue: false);
		}

		public override string GetReport()
		{
			if (usingNutrientPasteDispenser)
			{
				return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label);
			}
			Thing thing = job.targetA.Thing;
			if (thing != null && thing.def.ingestible != null)
			{
				if (!thing.def.ingestible.ingestReportStringEat.NullOrEmpty() && (thing.def.ingestible.ingestReportString.NullOrEmpty() || (int)pawn.RaceProps.intelligence < 1))
				{
					return string.Format(thing.def.ingestible.ingestReportStringEat, job.targetA.Thing.LabelShort);
				}
				if (!thing.def.ingestible.ingestReportString.NullOrEmpty())
				{
					return string.Format(thing.def.ingestible.ingestReportString, job.targetA.Thing.LabelShort);
				}
			}
			return base.GetReport();
		}

		public override void Notify_Starting()
		{
			base.Notify_Starting();
			usingNutrientPasteDispenser = (IngestibleSource is Building_NutrientPasteDispenser);
			eatingFromInventory = (pawn.inventory != null && pawn.inventory.Contains(IngestibleSource));
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			if (base.pawn.Faction != null && !(IngestibleSource is Building_NutrientPasteDispenser))
			{
				Thing ingestibleSource = IngestibleSource;
				int num = FoodUtility.WillIngestStackCountOf(base.pawn, ingestibleSource.def, ingestibleSource.GetStatValue(StatDefOf.Nutrition));
				if (num >= ingestibleSource.stackCount && ingestibleSource.Spawned)
				{
					Pawn pawn = base.pawn;
					LocalTargetInfo target = ingestibleSource;
					Job job = base.job;
					bool errorOnFailed2 = errorOnFailed;
					if (!pawn.Reserve(target, job, 1, -1, null, errorOnFailed2))
					{
						return false;
					}
				}
			}
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			if (!usingNutrientPasteDispenser)
			{
				this.FailOn(() => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0045: stateMachine*/)._0024this.IngestibleSource.Destroyed && !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0045: stateMachine*/)._0024this.IngestibleSource.IngestibleNow);
			}
			Toil chew = Toils_Ingest.ChewIngestible(pawn, ChewDurationMultiplier, TargetIndex.A, TargetIndex.B).FailOn((Toil x) => !((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0075: stateMachine*/)._0024this.IngestibleSource.Spawned && (((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0075: stateMachine*/)._0024this.pawn.carryTracker == null || ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0075: stateMachine*/)._0024this.pawn.carryTracker.CarriedThing != ((_003CMakeNewToils_003Ec__Iterator0)/*Error near IL_0075: stateMachine*/)._0024this.IngestibleSource)).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
			using (IEnumerator<Toil> enumerator = PrepareToIngestToils(chew).GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					Toil toil = enumerator.Current;
					yield return toil;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield return chew;
			/*Error: Unable to find new state assignment for yield return*/;
			IL_01aa:
			/*Error near IL_01ab: Unexpected return in MoveNext()*/;
		}

		private IEnumerable<Toil> PrepareToIngestToils(Toil chewToil)
		{
			if (usingNutrientPasteDispenser)
			{
				return PrepareToIngestToils_Dispenser();
			}
			if (pawn.RaceProps.ToolUser)
			{
				return PrepareToIngestToils_ToolUser(chewToil);
			}
			return PrepareToIngestToils_NonToolUser();
		}

		private IEnumerable<Toil> PrepareToIngestToils_Dispenser()
		{
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private IEnumerable<Toil> PrepareToIngestToils_ToolUser(Toil chewToil)
		{
			if (!eatingFromInventory)
			{
				yield return ReserveFoodIfWillIngestWholeStack();
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, TargetIndex.A);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private IEnumerable<Toil> PrepareToIngestToils_NonToolUser()
		{
			yield return ReserveFoodIfWillIngestWholeStack();
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private Toil ReserveFoodIfWillIngestWholeStack()
		{
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				if (pawn.Faction != null)
				{
					Thing thing = job.GetTarget(TargetIndex.A).Thing;
					if (pawn.carryTracker.CarriedThing != thing)
					{
						int num = FoodUtility.WillIngestStackCountOf(pawn, thing.def, thing.GetStatValue(StatDefOf.Nutrition));
						if (num >= thing.stackCount)
						{
							if (!thing.Spawned)
							{
								pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
							}
							else
							{
								pawn.Reserve(thing, job);
							}
						}
					}
				}
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			toil.atomicWithPrevious = true;
			return toil;
		}

		public override bool ModifyCarriedThingDrawPos(ref Vector3 drawPos, ref bool behind, ref bool flip)
		{
			IntVec3 cell = job.GetTarget(TargetIndex.B).Cell;
			return ModifyCarriedThingDrawPosWorker(ref drawPos, ref behind, ref flip, cell, pawn);
		}

		public static bool ModifyCarriedThingDrawPosWorker(ref Vector3 drawPos, ref bool behind, ref bool flip, IntVec3 placeCell, Pawn pawn)
		{
			if (pawn.pather.Moving)
			{
				return false;
			}
			Thing carriedThing = pawn.carryTracker.CarriedThing;
			if (carriedThing == null || !carriedThing.IngestibleNow)
			{
				return false;
			}
			if (placeCell.IsValid && placeCell.AdjacentToCardinal(pawn.Position) && placeCell.HasEatSurface(pawn.Map) && carriedThing.def.ingestible.ingestHoldUsesTable)
			{
				drawPos = new Vector3((float)placeCell.x + 0.5f, drawPos.y, (float)placeCell.z + 0.5f);
				return true;
			}
			if (carriedThing.def.ingestible.ingestHoldOffsetStanding != null)
			{
				HoldOffset holdOffset = carriedThing.def.ingestible.ingestHoldOffsetStanding.Pick(pawn.Rotation);
				if (holdOffset != null)
				{
					drawPos += holdOffset.offset;
					behind = holdOffset.behind;
					flip = holdOffset.flip;
					return true;
				}
			}
			return false;
		}
	}
}
