using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_AncientShrinesGroup : SymbolResolver
	{
		public static readonly IntVec2 StandardAncientShrineSize = new IntVec2(4, 3);

		private const int MaxNumCaskets = 6;

		private const float SkipShrineChance = 0.25f;

		public const int MarginCells = 1;

		public override void Resolve(ResolveParams rp)
		{
			int num = rp.rect.Width + 1;
			IntVec2 standardAncientShrineSize = StandardAncientShrineSize;
			int num2 = num / (standardAncientShrineSize.x + 1);
			int num3 = rp.rect.Height + 1;
			IntVec2 standardAncientShrineSize2 = StandardAncientShrineSize;
			int num4 = num3 / (standardAncientShrineSize2.z + 1);
			IntVec3 bottomLeft = rp.rect.BottomLeft;
			PodContentsType? podContentsType = rp.podContentsType;
			if (!podContentsType.HasValue)
			{
				float value = Rand.Value;
				podContentsType = ((value < 0.5f) ? null : ((!(value < 0.7f)) ? new PodContentsType?(PodContentsType.AncientHostile) : new PodContentsType?(PodContentsType.Slave)));
			}
			int? ancientCryptosleepCasketGroupID = rp.ancientCryptosleepCasketGroupID;
			int value2 = (!ancientCryptosleepCasketGroupID.HasValue) ? Find.UniqueIDsManager.GetNextAncientCryptosleepCasketGroupID() : ancientCryptosleepCasketGroupID.Value;
			int num5 = 0;
			for (int i = 0; i < num4; i++)
			{
				for (int j = 0; j < num2; j++)
				{
					if (!Rand.Chance(0.25f))
					{
						if (num5 >= 6)
						{
							break;
						}
						int x = bottomLeft.x;
						int num6 = j;
						IntVec2 standardAncientShrineSize3 = StandardAncientShrineSize;
						int minX = x + num6 * (standardAncientShrineSize3.x + 1);
						int z = bottomLeft.z;
						int num7 = i;
						IntVec2 standardAncientShrineSize4 = StandardAncientShrineSize;
						int minZ = z + num7 * (standardAncientShrineSize4.z + 1);
						IntVec2 standardAncientShrineSize5 = StandardAncientShrineSize;
						int x2 = standardAncientShrineSize5.x;
						IntVec2 standardAncientShrineSize6 = StandardAncientShrineSize;
						CellRect rect = new CellRect(minX, minZ, x2, standardAncientShrineSize6.z);
						if (rect.FullyContainedWithin(rp.rect))
						{
							ResolveParams resolveParams = rp;
							resolveParams.rect = rect;
							resolveParams.ancientCryptosleepCasketGroupID = value2;
							resolveParams.podContentsType = podContentsType;
							BaseGen.symbolStack.Push("ancientShrine", resolveParams);
							num5++;
						}
					}
				}
			}
		}
	}
}
