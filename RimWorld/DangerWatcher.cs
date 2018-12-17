using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RimWorld
{
	public class DangerWatcher
	{
		private Map map;

		private StoryDanger dangerRatingInt;

		private int lastUpdateTick = -10000;

		private int lastColonistHarmedTick = -10000;

		private const int UpdateInterval = 101;

		public StoryDanger DangerRating
		{
			get
			{
				if (Find.TickManager.TicksGame > lastUpdateTick + 101)
				{
					dangerRatingInt = CalculateDangerRating();
					lastUpdateTick = Find.TickManager.TicksGame;
				}
				return dangerRatingInt;
			}
		}

		public DangerWatcher(Map map)
		{
			this.map = map;
		}

		private StoryDanger CalculateDangerRating()
		{
			float num = (from x in map.attackTargetsCache.TargetsHostileToColony
			where AffectsStoryDanger(x)
			select x).Sum((IAttackTarget t) => (!(t is Pawn)) ? 0f : ((Pawn)t).kindDef.combatPower);
			if (num == 0f)
			{
				return StoryDanger.None;
			}
			int num2 = (from p in map.mapPawns.FreeColonistsSpawned
			where !p.Downed
			select p).Count();
			if (num < 150f && num <= (float)num2 * 18f)
			{
				return StoryDanger.Low;
			}
			if (num > 400f)
			{
				return StoryDanger.High;
			}
			if (lastColonistHarmedTick > Find.TickManager.TicksGame - 900)
			{
				return StoryDanger.High;
			}
			foreach (Lord lord in map.lordManager.lords)
			{
				if (lord.faction.HostileTo(Faction.OfPlayer) && lord.CurLordToil.ForceHighStoryDanger && lord.AnyActivePawn)
				{
					return StoryDanger.High;
				}
			}
			return StoryDanger.Low;
		}

		public void Notify_ColonistHarmedExternally()
		{
			lastColonistHarmedTick = Find.TickManager.TicksGame;
		}

		private bool AffectsStoryDanger(IAttackTarget t)
		{
			Pawn pawn = t.Thing as Pawn;
			if (pawn != null)
			{
				Lord lord = pawn.GetLord();
				if (lord != null && lord.LordJob is LordJob_DefendPoint && pawn.CurJobDef != JobDefOf.AttackMelee && pawn.CurJobDef != JobDefOf.AttackStatic)
				{
					return false;
				}
			}
			return GenHostility.IsActiveThreatToPlayer(t);
		}
	}
}
