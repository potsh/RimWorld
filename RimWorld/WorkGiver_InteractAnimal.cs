using UnityEngine;
using Verse;
using Verse.AI;

namespace RimWorld
{
	public abstract class WorkGiver_InteractAnimal : WorkGiver_Scanner
	{
		protected static string NoUsableFoodTrans;

		protected static string AnimalInteractedTooRecentlyTrans;

		private static string CantInteractAnimalDownedTrans;

		private static string CantInteractAnimalAsleepTrans;

		private static string CantInteractAnimalBusyTrans;

		public override PathEndMode PathEndMode => PathEndMode.OnCell;

		public static void ResetStaticData()
		{
			NoUsableFoodTrans = "NoUsableFood".Translate();
			AnimalInteractedTooRecentlyTrans = "AnimalInteractedTooRecently".Translate();
			CantInteractAnimalDownedTrans = "CantInteractAnimalDowned".Translate();
			CantInteractAnimalAsleepTrans = "CantInteractAnimalAsleep".Translate();
			CantInteractAnimalBusyTrans = "CantInteractAnimalBusy".Translate();
		}

		protected virtual bool CanInteractWithAnimal(Pawn pawn, Pawn animal, bool forced)
		{
			LocalTargetInfo target = animal;
			bool ignoreOtherReservations = forced;
			if (!pawn.CanReserve(target, 1, -1, null, ignoreOtherReservations))
			{
				return false;
			}
			if (animal.Downed)
			{
				JobFailReason.Is(CantInteractAnimalDownedTrans);
				return false;
			}
			if (!animal.Awake())
			{
				JobFailReason.Is(CantInteractAnimalAsleepTrans);
				return false;
			}
			if (!animal.CanCasuallyInteractNow())
			{
				JobFailReason.Is(CantInteractAnimalBusyTrans);
				return false;
			}
			int num = TrainableUtility.MinimumHandlingSkill(animal);
			if (num > pawn.skills.GetSkill(SkillDefOf.Animals).Level)
			{
				JobFailReason.Is("AnimalsSkillTooLow".Translate(num));
				return false;
			}
			return true;
		}

		protected bool HasFoodToInteractAnimal(Pawn pawn, Pawn tamee)
		{
			ThingOwner<Thing> innerContainer = pawn.inventory.innerContainer;
			int num = 0;
			float num2 = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee);
			float num3 = 0f;
			for (int i = 0; i < innerContainer.Count; i++)
			{
				Thing thing = innerContainer[i];
				if (tamee.WillEat(thing, pawn) && (int)thing.def.ingestible.preferability <= 5 && !thing.def.IsDrug)
				{
					for (int j = 0; j < thing.stackCount; j++)
					{
						num3 += thing.GetStatValue(StatDefOf.Nutrition);
						if (num3 >= num2)
						{
							num++;
							num3 = 0f;
						}
						if (num >= 2)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		protected Job TakeFoodForAnimalInteractJob(Pawn pawn, Pawn tamee)
		{
			float num = JobDriver_InteractAnimal.RequiredNutritionPerFeed(tamee) * 2f * 4f;
			ThingDef foodDef;
			Thing thing = FoodUtility.BestFoodSourceOnMap(pawn, tamee, desperate: false, out foodDef, FoodPreferability.RawTasty, allowPlant: false, allowDrug: false, allowCorpse: false, allowDispenserFull: false, allowDispenserEmpty: false);
			if (thing == null)
			{
				return null;
			}
			float nutrition = FoodUtility.GetNutrition(thing, foodDef);
			Job job = new Job(JobDefOf.TakeInventory, thing);
			job.count = Mathf.CeilToInt(num / nutrition);
			return job;
		}
	}
}
