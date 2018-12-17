using System.Linq;
using Verse;
using Verse.AI.Group;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_RandomMechanoidGroup : SymbolResolver
	{
		private static readonly IntRange DefaultMechanoidCountRange = new IntRange(1, 5);

		public override void Resolve(ResolveParams rp)
		{
			int? mechanoidsCount = rp.mechanoidsCount;
			int num = (!mechanoidsCount.HasValue) ? DefaultMechanoidCountRange.RandomInRange : mechanoidsCount.Value;
			Lord lord = rp.singlePawnLord;
			if (lord == null && num > 0)
			{
				Map map = BaseGen.globalSettings.map;
				lord = LordMaker.MakeNewLord(lordJob: (!Rand.Bool || !(from x in rp.rect.Cells
				where !x.Impassable(map)
				select x).TryRandomElement(out IntVec3 result)) ? ((LordJob)new LordJob_AssaultColony(Faction.OfMechanoids, canKidnap: false, canTimeoutOrFlee: false, sappers: false, useAvoidGridSmart: false, canSteal: false)) : ((LordJob)new LordJob_DefendPoint(result)), faction: Faction.OfMechanoids, map: map);
			}
			for (int i = 0; i < num; i++)
			{
				PawnKindDef pawnKindDef = rp.singlePawnKindDef;
				if (pawnKindDef == null)
				{
					pawnKindDef = (from kind in DefDatabase<PawnKindDef>.AllDefsListForReading
					where kind.RaceProps.IsMechanoid
					select kind).RandomElementByWeight((PawnKindDef kind) => 1f / kind.combatPower);
				}
				ResolveParams resolveParams = rp;
				resolveParams.singlePawnKindDef = pawnKindDef;
				resolveParams.singlePawnLord = lord;
				resolveParams.faction = Faction.OfMechanoids;
				BaseGen.symbolStack.Push("pawn", resolveParams);
			}
		}
	}
}
