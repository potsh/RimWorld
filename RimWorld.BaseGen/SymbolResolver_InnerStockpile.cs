using UnityEngine;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_InnerStockpile : SymbolResolver
	{
		private const int DefaultSize = 3;

		public override void Resolve(ResolveParams rp)
		{
			int? innerStockpileSize = rp.innerStockpileSize;
			CellRect rect;
			if (innerStockpileSize.HasValue)
			{
				if (!TryFindPerfectPlaceThenBest(rp.rect, rp.innerStockpileSize.Value, out rect))
				{
					return;
				}
			}
			else if (rp.stockpileConcreteContents != null)
			{
				int num = Mathf.CeilToInt(Mathf.Sqrt((float)rp.stockpileConcreteContents.Count));
				if (!TryFindRandomInnerRect(rp.rect, num, out rect, num * num, out int _))
				{
					rect = rp.rect;
				}
			}
			else if (!TryFindPerfectPlaceThenBest(rp.rect, 3, out rect))
			{
				return;
			}
			ResolveParams resolveParams = rp;
			resolveParams.rect = rect;
			BaseGen.symbolStack.Push("stockpile", resolveParams);
		}

		private bool TryFindPerfectPlaceThenBest(CellRect outerRect, int size, out CellRect rect)
		{
			if (!TryFindRandomInnerRect(outerRect, size, out rect, size * size, out int maxValidCellsFound))
			{
				if (maxValidCellsFound == 0)
				{
					return false;
				}
				if (!TryFindRandomInnerRect(outerRect, size, out rect, maxValidCellsFound, out int _))
				{
					return false;
				}
			}
			return true;
		}

		private bool TryFindRandomInnerRect(CellRect outerRect, int size, out CellRect rect, int minValidCells, out int maxValidCellsFound)
		{
			Map map = BaseGen.globalSettings.map;
			size = Mathf.Min(size, Mathf.Min(outerRect.Width, outerRect.Height));
			int maxValidCellsFoundLocal = 0;
			bool result = outerRect.TryFindRandomInnerRect(new IntVec2(size, size), out rect, delegate(CellRect x)
			{
				int num = 0;
				CellRect.CellRectIterator iterator = x.GetIterator();
				while (!iterator.Done())
				{
					if (iterator.Current.Standable(map) && iterator.Current.GetFirstItem(map) == null && iterator.Current.GetFirstBuilding(map) == null)
					{
						num++;
					}
					iterator.MoveNext();
				}
				maxValidCellsFoundLocal = Mathf.Max(maxValidCellsFoundLocal, num);
				return num >= minValidCells;
			});
			maxValidCellsFound = maxValidCellsFoundLocal;
			return result;
		}
	}
}
