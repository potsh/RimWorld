using Verse;
using Verse.AI;

namespace RimWorld
{
	public class JobGiver_GetFood : ThinkNode_JobGiver
	{
		private HungerCategory minCategory;

		public bool forceScanWholeMap;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_GetFood jobGiver_GetFood = (JobGiver_GetFood)base.DeepCopy(resolve);
			jobGiver_GetFood.minCategory = minCategory;
			jobGiver_GetFood.forceScanWholeMap = forceScanWholeMap;
			return jobGiver_GetFood;
		}

		public override float GetPriority(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null)
			{
				return 0f;
			}
			if ((int)pawn.needs.food.CurCategory < 3 && FoodUtility.ShouldBeFedBySomeone(pawn))
			{
				return 0f;
			}
			if ((int)food.CurCategory < (int)minCategory)
			{
				return 0f;
			}
			if (food.CurLevelPercentage < pawn.RaceProps.FoodLevelPercentageWantEat)
			{
				return 9.5f;
			}
			return 0f;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			Need_Food food = pawn.needs.food;
			if (food == null || (int)food.CurCategory < (int)minCategory)
			{
				return null;
			}
			bool flag;
			if (pawn.AnimalOrWildMan())
			{
				flag = true;
			}
			else
			{
				Hediff firstHediffOfDef = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
				flag = (firstHediffOfDef != null && firstHediffOfDef.Severity > 0.4f);
			}
			bool flag2 = pawn.needs.food.CurCategory == HungerCategory.Starving;
			bool desperate = flag2;
			bool canRefillDispenser = true;
			bool canUseInventory = true;
			bool allowCorpse = flag;
			bool flag3 = forceScanWholeMap;
			Thing foodSource = default(Thing);
			ThingDef foodDef = default(ThingDef);
			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn, desperate, out foodSource, out foodDef, canRefillDispenser, canUseInventory, allowForbidden: false, allowCorpse, allowSociallyImproper: false, pawn.IsWildMan(), flag3))
			{
				return null;
			}
			Pawn pawn2 = foodSource as Pawn;
			if (pawn2 != null)
			{
				Job job = new Job(JobDefOf.PredatorHunt, pawn2);
				job.killIncappedTarget = true;
				return job;
			}
			if (foodSource is Plant && foodSource.def.plant.harvestedThingDef == foodDef)
			{
				return new Job(JobDefOf.Harvest, foodSource);
			}
			Building_NutrientPasteDispenser building_NutrientPasteDispenser = foodSource as Building_NutrientPasteDispenser;
			if (building_NutrientPasteDispenser != null && !building_NutrientPasteDispenser.HasEnoughFeedstockInHoppers())
			{
				Building building = building_NutrientPasteDispenser.AdjacentReachableHopper(pawn);
				if (building != null)
				{
					ISlotGroupParent hopperSgp = building as ISlotGroupParent;
					Job job2 = WorkGiver_CookFillHopper.HopperFillFoodJob(pawn, hopperSgp);
					if (job2 != null)
					{
						return job2;
					}
				}
				foodSource = FoodUtility.BestFoodSourceOnMap(pawn, pawn, flag2, out foodDef, FoodPreferability.MealLavish, allowPlant: false, !pawn.IsTeetotaler(), allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false, allowForbidden: false, allowSociallyImproper: false, allowHarvest: false, forceScanWholeMap);
				if (foodSource == null)
				{
					return null;
				}
			}
			float nutrition = FoodUtility.GetNutrition(foodSource, foodDef);
			Job job3 = new Job(JobDefOf.Ingest, foodSource);
			job3.count = FoodUtility.WillIngestStackCountOf(pawn, foodDef, nutrition);
			return job3;
		}
	}
}
