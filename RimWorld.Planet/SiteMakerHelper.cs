using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld.Planet
{
	public static class SiteMakerHelper
	{
		private static List<Faction> possibleFactions = new List<Faction>();

		public static bool TryFindSiteParams_SingleSitePart(SiteCoreDef core, IEnumerable<SitePartDef> singleSitePartCandidates, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			faction = factionToUse;
			if (singleSitePartCandidates != null)
			{
				if (!TryFindNewRandomSitePartFor(core, null, singleSitePartCandidates, faction, out sitePart, disallowNonHostileFactions, extraFactionValidator))
				{
					return false;
				}
			}
			else
			{
				sitePart = null;
			}
			if (faction == null)
			{
				IEnumerable<SitePartDef> parts = (sitePart == null) ? null : Gen.YieldSingle(sitePart);
				if (!TryFindRandomFactionFor(core, parts, out faction, disallowNonHostileFactions, extraFactionValidator))
				{
					return false;
				}
			}
			return true;
		}

		public static bool TryFindSiteParams_SingleSitePart(SiteCoreDef core, string singleSitePartTag, out SitePartDef sitePart, out Faction faction, Faction factionToUse = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			IEnumerable<SitePartDef> singleSitePartCandidates = (singleSitePartTag == null) ? null : (from x in DefDatabase<SitePartDef>.AllDefsListForReading
			where x.tags.Contains(singleSitePartTag)
			select x);
			return TryFindSiteParams_SingleSitePart(core, singleSitePartCandidates, out sitePart, out faction, factionToUse, disallowNonHostileFactions, extraFactionValidator);
		}

		public static bool TryFindNewRandomSitePartFor(SiteCoreDef core, IEnumerable<SitePartDef> existingSiteParts, IEnumerable<SitePartDef> possibleSiteParts, Faction faction, out SitePartDef sitePart, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			if (faction != null)
			{
				if ((from x in possibleSiteParts
				where x == null || FactionCanOwn(x, faction, disallowNonHostileFactions, extraFactionValidator)
				select x).TryRandomElement(out sitePart))
				{
					return true;
				}
			}
			else
			{
				possibleFactions.Clear();
				possibleFactions.Add(null);
				possibleFactions.AddRange(Find.FactionManager.AllFactionsListForReading);
				if ((from x in possibleSiteParts
				where x == null || possibleFactions.Any((Faction fac) => FactionCanOwn(core, existingSiteParts, fac, disallowNonHostileFactions, extraFactionValidator) && FactionCanOwn(x, fac, disallowNonHostileFactions, extraFactionValidator))
				select x).TryRandomElement(out sitePart))
				{
					possibleFactions.Clear();
					return true;
				}
				possibleFactions.Clear();
			}
			sitePart = null;
			return false;
		}

		public static bool TryFindRandomFactionFor(SiteCoreDef core, IEnumerable<SitePartDef> parts, out Faction faction, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null)
		{
			if (FactionCanOwn(core, parts, null, disallowNonHostileFactions, extraFactionValidator))
			{
				faction = null;
				return true;
			}
			if ((from x in Find.FactionManager.AllFactionsListForReading
			where FactionCanOwn(core, parts, x, disallowNonHostileFactions, extraFactionValidator)
			select x).TryRandomElement(out faction))
			{
				return true;
			}
			faction = null;
			return false;
		}

		public static bool FactionCanOwn(SiteCoreDef core, IEnumerable<SitePartDef> parts, Faction faction, bool disallowNonHostileFactions, Predicate<Faction> extraFactionValidator)
		{
			if (!FactionCanOwn(core, faction, disallowNonHostileFactions, extraFactionValidator))
			{
				return false;
			}
			if (parts != null)
			{
				foreach (SitePartDef part in parts)
				{
					if (!FactionCanOwn(part, faction, disallowNonHostileFactions, extraFactionValidator))
					{
						return false;
					}
				}
			}
			return true;
		}

		private static bool FactionCanOwn(SiteCoreOrPartDefBase siteDefBase, Faction faction, bool disallowNonHostileFactions, Predicate<Faction> extraFactionValidator)
		{
			if (siteDefBase == null)
			{
				Log.Error("Called FactionCanOwn() with null SiteDefBase.");
				return false;
			}
			if (!siteDefBase.FactionCanOwn(faction))
			{
				return false;
			}
			if (disallowNonHostileFactions && faction != null && !faction.HostileTo(Faction.OfPlayer))
			{
				return false;
			}
			if (extraFactionValidator != null && !extraFactionValidator(faction))
			{
				return false;
			}
			return true;
		}
	}
}
