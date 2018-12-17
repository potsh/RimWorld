using System;
using System.Collections.Generic;
using Verse;

namespace RimWorld.Planet
{
	public static class SiteMaker
	{
		public static Site MakeSite(SiteCoreDef core, SitePartDef sitePart, int tile, Faction faction, bool ifHostileThenMustRemainHostile = true, float? threatPoints = default(float?))
		{
			IEnumerable<SitePartDef> siteParts = (sitePart == null) ? null : Gen.YieldSingle(sitePart);
			return MakeSite(core, siteParts, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
		}

		public static Site MakeSite(SiteCoreDef core, IEnumerable<SitePartDef> siteParts, int tile, Faction faction, bool ifHostileThenMustRemainHostile = true, float? threatPoints = default(float?))
		{
			Site site = (Site)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Site);
			site.Tile = tile;
			site.SetFaction(faction);
			if (ifHostileThenMustRemainHostile && faction != null && faction.HostileTo(Faction.OfPlayer))
			{
				site.factionMustRemainHostile = true;
			}
			float num = site.desiredThreatPoints = ((!threatPoints.HasValue) ? StorytellerUtility.DefaultSiteThreatPointsNow() : threatPoints.Value);
			int num2 = 0;
			if (core.wantsThreatPoints)
			{
				num2++;
			}
			if (siteParts != null)
			{
				foreach (SitePartDef sitePart in siteParts)
				{
					if (sitePart.wantsThreatPoints)
					{
						num2++;
					}
				}
			}
			float num3 = (num2 == 0) ? 0f : (num / (float)num2);
			float myThreatPoints = (!core.wantsThreatPoints) ? 0f : num3;
			site.core = new SiteCore(core, core.Worker.GenerateDefaultParams(site, myThreatPoints));
			if (siteParts != null)
			{
				foreach (SitePartDef sitePart2 in siteParts)
				{
					float myThreatPoints2 = (!sitePart2.wantsThreatPoints) ? 0f : num3;
					site.parts.Add(new SitePart(sitePart2, sitePart2.Worker.GenerateDefaultParams(site, myThreatPoints2)));
				}
				return site;
			}
			return site;
		}

		public static Site TryMakeSite_SingleSitePart(SiteCoreDef core, IEnumerable<SitePartDef> singleSitePartCandidates, int tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = default(float?))
		{
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(core, singleSitePartCandidates, out SitePartDef sitePart, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
			{
				return null;
			}
			return MakeSite(core, sitePart, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
		}

		public static Site TryMakeSite_SingleSitePart(SiteCoreDef core, string singleSitePartTag, int tile, Faction faction = null, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = default(float?))
		{
			if (!SiteMakerHelper.TryFindSiteParams_SingleSitePart(core, singleSitePartTag, out SitePartDef sitePart, out faction, faction, disallowNonHostileFactions, extraFactionValidator))
			{
				return null;
			}
			return MakeSite(core, sitePart, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
		}

		public static Site TryMakeSite(SiteCoreDef core, IEnumerable<SitePartDef> siteParts, int tile, bool disallowNonHostileFactions = true, Predicate<Faction> extraFactionValidator = null, bool ifHostileThenMustRemainHostile = true, float? threatPoints = default(float?))
		{
			if (!SiteMakerHelper.TryFindRandomFactionFor(core, siteParts, out Faction faction, disallowNonHostileFactions, extraFactionValidator))
			{
				return null;
			}
			return MakeSite(core, siteParts, tile, faction, ifHostileThenMustRemainHostile, threatPoints);
		}
	}
}
