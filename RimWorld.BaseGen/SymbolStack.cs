using System.Collections.Generic;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolStack
	{
		private Stack<Pair<string, ResolveParams>> stack = new Stack<Pair<string, ResolveParams>>();

		public bool Empty => stack.Count == 0;

		public int Count => stack.Count;

		public void Push(string symbol, ResolveParams resolveParams)
		{
			stack.Push(new Pair<string, ResolveParams>(symbol, resolveParams));
		}

		public void Push(string symbol, CellRect rect)
		{
			ResolveParams resolveParams = default(ResolveParams);
			resolveParams.rect = rect;
			Push(symbol, resolveParams);
		}

		public void PushMany(ResolveParams resolveParams, params string[] symbols)
		{
			for (int i = 0; i < symbols.Length; i++)
			{
				Push(symbols[i], resolveParams);
			}
		}

		public void PushMany(CellRect rect, params string[] symbols)
		{
			for (int i = 0; i < symbols.Length; i++)
			{
				Push(symbols[i], rect);
			}
		}

		public Pair<string, ResolveParams> Pop()
		{
			return stack.Pop();
		}

		public void Clear()
		{
			stack.Clear();
		}
	}
}
