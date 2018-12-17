using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class FactionManager : IExposable
	{
		private List<Faction> allFactions = new List<Faction>();

		private Faction ofPlayer;

		private Faction ofMechanoids;

		private Faction ofInsects;

		private Faction ofAncients;

		private Faction ofAncientsHostile;

		public List<Faction> AllFactionsListForReading => allFactions;

		public IEnumerable<Faction> AllFactions => allFactions;

		public IEnumerable<Faction> AllFactionsVisible => from fa in allFactions
		where !fa.def.hidden
		select fa;

		public IEnumerable<Faction> AllFactionsVisibleInViewOrder => GetInViewOrder(AllFactionsVisible);

		public IEnumerable<Faction> AllFactionsInViewOrder => GetInViewOrder(AllFactions);

		public Faction OfPlayer => ofPlayer;

		public Faction OfMechanoids => ofMechanoids;

		public Faction OfInsects => ofInsects;

		public Faction OfAncients => ofAncients;

		public Faction OfAncientsHostile => ofAncientsHostile;

		public void ExposeData()
		{
			Scribe_Collections.Look(ref allFactions, "allFactions", LookMode.Deep);
			if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				RecacheFactions();
			}
		}

		public void Add(Faction faction)
		{
			if (!allFactions.Contains(faction))
			{
				allFactions.Add(faction);
				RecacheFactions();
			}
		}

		public void Remove(Faction faction)
		{
			if (allFactions.Contains(faction))
			{
				allFactions.Remove(faction);
				RecacheFactions();
			}
		}

		public void FactionManagerTick()
		{
			SettlementProximityGoodwillUtility.CheckSettlementProximityGoodwillChange();
			for (int i = 0; i < allFactions.Count; i++)
			{
				allFactions[i].FactionTick();
			}
		}

		public Faction FirstFactionOfDef(FactionDef facDef)
		{
			for (int i = 0; i < allFactions.Count; i++)
			{
				if (allFactions[i].def == facDef)
				{
					return allFactions[i];
				}
			}
			return null;
		}

		public bool TryGetRandomNonColonyHumanlikeFaction(out Faction faction, bool tryMedievalOrBetter, bool allowDefeated = false, TechLevel minTechLevel = TechLevel.Undefined)
		{
			IEnumerable<Faction> source = from x in AllFactions
			where !x.IsPlayer && !x.def.hidden && x.def.humanlikeFaction && (allowDefeated || !x.defeated) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel)
			select x;
			return source.TryRandomElementByWeight(delegate(Faction x)
			{
				if (tryMedievalOrBetter && (int)x.def.techLevel < 3)
				{
					return 0.1f;
				}
				return 1f;
			}, out faction);
		}

		public Faction RandomEnemyFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in AllFactions
			where !x.IsPlayer && (allowHidden || !x.def.hidden) && (allowDefeated || !x.defeated) && (allowNonHumanlike || x.def.humanlikeFaction) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel) && x.HostileTo(Faction.OfPlayer)
			select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomNonHostileFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in AllFactions
			where !x.IsPlayer && (allowHidden || !x.def.hidden) && (allowDefeated || !x.defeated) && (allowNonHumanlike || x.def.humanlikeFaction) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel) && !x.HostileTo(Faction.OfPlayer)
			select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public Faction RandomAlliedFaction(bool allowHidden = false, bool allowDefeated = false, bool allowNonHumanlike = true, TechLevel minTechLevel = TechLevel.Undefined)
		{
			if ((from x in AllFactions
			where !x.IsPlayer && (allowHidden || !x.def.hidden) && (allowDefeated || !x.defeated) && (allowNonHumanlike || x.def.humanlikeFaction) && (minTechLevel == TechLevel.Undefined || (int)x.def.techLevel >= (int)minTechLevel) && x.PlayerRelationKind == FactionRelationKind.Ally
			select x).TryRandomElement(out Faction result))
			{
				return result;
			}
			return null;
		}

		public void LogKidnappedPawns()
		{
			Log.Message("Kidnapped pawns:");
			for (int i = 0; i < allFactions.Count; i++)
			{
				allFactions[i].kidnapped.LogKidnappedPawns();
			}
		}

		public static IEnumerable<Faction> GetInViewOrder(IEnumerable<Faction> factions)
		{
			return from x in factions
			orderby x.defeated, x.def.listOrderPriority descending
			select x;
		}

		private void RecacheFactions()
		{
			ofPlayer = null;
			for (int i = 0; i < allFactions.Count; i++)
			{
				if (allFactions[i].IsPlayer)
				{
					ofPlayer = allFactions[i];
					break;
				}
			}
			ofMechanoids = FirstFactionOfDef(FactionDefOf.Mechanoid);
			ofInsects = FirstFactionOfDef(FactionDefOf.Insect);
			ofAncients = FirstFactionOfDef(FactionDefOf.Ancients);
			ofAncientsHostile = FirstFactionOfDef(FactionDefOf.AncientsHostile);
		}
	}
}
