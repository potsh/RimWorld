using Verse;

namespace RimWorld
{
	public class GenStep_Snow : GenStep
	{
		public override int SeedPart => 306693816;

		public override void Generate(Map map, GenStepParams parms)
		{
			int num = 0;
			for (int i = (int)(GenLocalDate.Twelfth(map) - 2); i <= (int)GenLocalDate.Twelfth(map); i++)
			{
				int num2 = i;
				if (num2 < 0)
				{
					num2 += 12;
				}
				Twelfth twelfth = (Twelfth)num2;
				float num3 = GenTemperature.AverageTemperatureAtTileForTwelfth(map.Tile, twelfth);
				if (num3 < 0f)
				{
					num++;
				}
			}
			float num4 = 0f;
			switch (num)
			{
			case 0:
				return;
			case 1:
				num4 = 0.3f;
				break;
			case 2:
				num4 = 0.7f;
				break;
			case 3:
				num4 = 1f;
				break;
			}
			if (map.mapTemperature.SeasonalTemp > 0f)
			{
				num4 *= 0.4f;
			}
			if (!((double)num4 < 0.3))
			{
				foreach (IntVec3 allCell in map.AllCells)
				{
					if (!allCell.Roofed(map))
					{
						map.steadyEnvironmentEffects.AddFallenSnowAt(allCell, num4);
					}
				}
			}
		}
	}
}
