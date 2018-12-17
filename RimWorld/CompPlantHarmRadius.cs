using Verse;

namespace RimWorld
{
	public class CompPlantHarmRadius : ThingComp
	{
		private int plantHarmAge;

		private int ticksToPlantHarm;

		private float LeaflessPlantKillChance = 0.09f;

		protected CompProperties_PlantHarmRadius PropsPlantHarmRadius => (CompProperties_PlantHarmRadius)props;

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref plantHarmAge, "plantHarmAge", 0);
			Scribe_Values.Look(ref ticksToPlantHarm, "ticksToPlantHarm", 0);
		}

		public override void CompTick()
		{
			if (parent.Spawned)
			{
				plantHarmAge++;
				ticksToPlantHarm--;
				if (ticksToPlantHarm <= 0)
				{
					float x = (float)plantHarmAge / 60000f;
					float num = PropsPlantHarmRadius.radiusPerDayCurve.Evaluate(x);
					float num2 = 3.14159274f * num * num;
					float num3 = num2 * PropsPlantHarmRadius.harmFrequencyPerArea;
					float num4 = 60f / num3;
					int num5;
					if (num4 >= 1f)
					{
						ticksToPlantHarm = GenMath.RoundRandom(num4);
						num5 = 1;
					}
					else
					{
						ticksToPlantHarm = 1;
						num5 = GenMath.RoundRandom(1f / num4);
					}
					for (int i = 0; i < num5; i++)
					{
						HarmRandomPlantInRadius(num);
					}
				}
			}
		}

		private void HarmRandomPlantInRadius(float radius)
		{
			IntVec3 c = parent.Position + (Rand.InsideUnitCircleVec3 * radius).ToIntVec3();
			if (c.InBounds(parent.Map))
			{
				Plant plant = c.GetPlant(parent.Map);
				if (plant != null)
				{
					if (plant.LeaflessNow)
					{
						if (Rand.Value < LeaflessPlantKillChance)
						{
							plant.Kill();
						}
					}
					else
					{
						plant.MakeLeafless(Plant.LeaflessCause.Poison);
					}
				}
			}
		}
	}
}
