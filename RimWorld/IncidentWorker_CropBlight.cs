using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class IncidentWorker_CropBlight : IncidentWorker
	{
		private const float Radius = 15f;

		private const float BaseBlightChance = 0.4f;

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Plant plant;
			return TryFindRandomBlightablePlant((Map)parms.target, out plant);
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!TryFindRandomBlightablePlant(map, out Plant plant))
			{
				return false;
			}
			Room room = plant.GetRoom();
			plant.CropBlighted();
			int i = 0;
			for (int num = GenRadial.NumCellsInRadius(15f); i < num; i++)
			{
				IntVec3 intVec = plant.Position + GenRadial.RadialPattern[i];
				if (intVec.InBounds(map) && intVec.GetRoom(map) == room)
				{
					Plant firstBlightableNowPlant = BlightUtility.GetFirstBlightableNowPlant(intVec, map);
					if (firstBlightableNowPlant != null && firstBlightableNowPlant != plant && Rand.Chance(0.4f * BlightChanceFactor(firstBlightableNowPlant.Position, plant.Position)))
					{
						firstBlightableNowPlant.CropBlighted();
					}
				}
			}
			Find.LetterStack.ReceiveLetter("LetterLabelCropBlight".Translate(), "LetterCropBlight".Translate(), LetterDefOf.NegativeEvent, new TargetInfo(plant.Position, map));
			return true;
		}

		private bool TryFindRandomBlightablePlant(Map map, out Plant plant)
		{
			Thing result;
			bool result2 = (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
			where ((Plant)x).BlightableNow
			select x).TryRandomElement(out result);
			plant = (Plant)result;
			return result2;
		}

		private float BlightChanceFactor(IntVec3 c, IntVec3 root)
		{
			return Mathf.InverseLerp(15f, 7.5f, c.DistanceTo(root));
		}
	}
}
