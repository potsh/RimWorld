using System.Linq;
using Verse;

namespace RimWorld.BaseGen
{
	public class SymbolResolver_SinglePawn : SymbolResolver
	{
		public override bool CanResolve(ResolveParams rp)
		{
			if (!base.CanResolve(rp))
			{
				return false;
			}
			if (rp.singlePawnToSpawn != null && rp.singlePawnToSpawn.Spawned)
			{
				return true;
			}
			if (!TryFindSpawnCell(rp, out IntVec3 _))
			{
				return false;
			}
			return true;
		}

		public override void Resolve(ResolveParams rp)
		{
			if (rp.singlePawnToSpawn == null || !rp.singlePawnToSpawn.Spawned)
			{
				Map map = BaseGen.globalSettings.map;
				if (!TryFindSpawnCell(rp, out IntVec3 cell))
				{
					if (rp.singlePawnToSpawn != null)
					{
						Find.WorldPawns.PassToWorld(rp.singlePawnToSpawn);
					}
				}
				else
				{
					Pawn pawn;
					if (rp.singlePawnToSpawn == null)
					{
						PawnGenerationRequest request = default(PawnGenerationRequest);
						if (rp.singlePawnGenerationRequest.HasValue)
						{
							request = rp.singlePawnGenerationRequest.Value;
						}
						else
						{
							PawnKindDef pawnKindDef = rp.singlePawnKindDef ?? (from x in DefDatabase<PawnKindDef>.AllDefsListForReading
							where x.defaultFactionType == null || !x.defaultFactionType.isPlayer
							select x).RandomElement();
							Faction result = rp.faction;
							if (result == null && pawnKindDef.RaceProps.Humanlike)
							{
								if (pawnKindDef.defaultFactionType != null)
								{
									result = FactionUtility.DefaultFactionFrom(pawnKindDef.defaultFactionType);
									if (result == null)
									{
										return;
									}
								}
								else if (!(from x in Find.FactionManager.AllFactions
								where !x.IsPlayer
								select x).TryRandomElement(out result))
								{
									return;
								}
							}
							PawnKindDef kind = pawnKindDef;
							Faction faction = result;
							int tile = map.Tile;
							request = new PawnGenerationRequest(kind, faction, PawnGenerationContext.NonPlayer, tile);
						}
						pawn = PawnGenerator.GeneratePawn(request);
						if (rp.postThingGenerate != null)
						{
							rp.postThingGenerate(pawn);
						}
					}
					else
					{
						pawn = rp.singlePawnToSpawn;
					}
					if (!pawn.Dead && rp.disableSinglePawn.HasValue && rp.disableSinglePawn.Value)
					{
						pawn.mindState.Active = false;
					}
					GenSpawn.Spawn(pawn, cell, map);
					if (rp.singlePawnLord != null)
					{
						rp.singlePawnLord.AddPawn(pawn);
					}
					if (rp.postThingSpawn != null)
					{
						rp.postThingSpawn(pawn);
					}
				}
			}
		}

		public static bool TryFindSpawnCell(ResolveParams rp, out IntVec3 cell)
		{
			Map map = BaseGen.globalSettings.map;
			return CellFinder.TryFindRandomCellInsideWith(rp.rect, (IntVec3 x) => x.Standable(map) && (rp.singlePawnSpawnCellExtraPredicate == null || rp.singlePawnSpawnCellExtraPredicate(x)), out cell);
		}
	}
}
