using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_Clear : SymbolResolver
	{
		private static List<Thing> tmpThingsToDestroy = new List<Thing>();

		public override void Resolve(ResolveParams rp)
		{
			CellRect.CellRectIterator iterator = rp.rect.GetIterator();
			while (!iterator.Done())
			{
				if (rp.clearEdificeOnly.HasValue && rp.clearEdificeOnly.Value)
				{
					Building edifice = iterator.Current.GetEdifice(BaseGen.globalSettings.map);
					if (edifice != null && edifice.def.destroyable)
					{
						edifice.Destroy();
					}
				}
				else if (rp.clearFillageOnly.HasValue && rp.clearFillageOnly.Value)
				{
					tmpThingsToDestroy.Clear();
					tmpThingsToDestroy.AddRange(iterator.Current.GetThingList(BaseGen.globalSettings.map));
					for (int i = 0; i < tmpThingsToDestroy.Count; i++)
					{
						if (tmpThingsToDestroy[i].def.destroyable && tmpThingsToDestroy[i].def.Fillage != 0)
						{
							tmpThingsToDestroy[i].Destroy();
						}
					}
				}
				else
				{
					tmpThingsToDestroy.Clear();
					tmpThingsToDestroy.AddRange(iterator.Current.GetThingList(BaseGen.globalSettings.map));
					for (int j = 0; j < tmpThingsToDestroy.Count; j++)
					{
						if (tmpThingsToDestroy[j].def.destroyable)
						{
							tmpThingsToDestroy[j].Destroy();
						}
					}
				}
				if (rp.clearRoof.HasValue && rp.clearRoof.Value)
				{
					BaseGen.globalSettings.map.roofGrid.SetRoof(iterator.Current, null);
				}
				iterator.MoveNext();
			}
		}
	}
}
