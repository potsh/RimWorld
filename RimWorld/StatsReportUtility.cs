using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StatsReportUtility
	{
		private static StatDrawEntry selectedEntry;

		private static StatDrawEntry mousedOverEntry;

		private static Vector2 scrollPosition;

		private static float listHeight;

		private static List<StatDrawEntry> cachedDrawEntries = new List<StatDrawEntry>();

		public static void Reset()
		{
			scrollPosition = default(Vector2);
			selectedEntry = null;
			mousedOverEntry = null;
			cachedDrawEntries.Clear();
		}

		public static void DrawStatsReport(Rect rect, Def def, ThingDef stuff)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				BuildableDef buildableDef = def as BuildableDef;
				StatRequest req = (buildableDef == null) ? StatRequest.ForEmpty() : StatRequest.For(buildableDef, stuff);
				cachedDrawEntries.AddRange(def.SpecialDisplayStats(req));
				cachedDrawEntries.AddRange(from r in StatsToDraw(def, stuff)
				where r.ShouldDisplay
				select r);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, null);
		}

		public static void DrawStatsReport(Rect rect, Thing thing)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				cachedDrawEntries.AddRange(thing.def.SpecialDisplayStats(StatRequest.For(thing)));
				cachedDrawEntries.AddRange(from r in StatsToDraw(thing)
				where r.ShouldDisplay
				select r);
				cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, thing, null);
		}

		public static void DrawStatsReport(Rect rect, WorldObject worldObject)
		{
			if (cachedDrawEntries.NullOrEmpty())
			{
				cachedDrawEntries.AddRange(worldObject.def.SpecialDisplayStats(StatRequest.ForEmpty()));
				cachedDrawEntries.AddRange(from r in StatsToDraw(worldObject)
				where r.ShouldDisplay
				select r);
				cachedDrawEntries.RemoveAll((StatDrawEntry de) => de.stat != null && !de.stat.showNonAbstract);
				FinalizeCachedDrawEntries(cachedDrawEntries);
			}
			DrawStatsWorker(rect, null, worldObject);
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(Def def, ThingDef stuff)
		{
			yield return DescriptionEntry(def);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(Thing thing)
		{
			yield return DescriptionEntry(thing);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private static IEnumerable<StatDrawEntry> StatsToDraw(WorldObject worldObject)
		{
			yield return DescriptionEntry(worldObject);
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private static void FinalizeCachedDrawEntries(IEnumerable<StatDrawEntry> original)
		{
			cachedDrawEntries = (from sd in original
			orderby sd.category.displayOrder, sd.DisplayPriorityWithinCategory descending, sd.LabelCap
			select sd).ToList();
		}

		private static StatDrawEntry DescriptionEntry(Def def)
		{
			StatDrawEntry statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999, string.Empty);
			statDrawEntry.overrideReportText = def.description;
			return statDrawEntry;
		}

		private static StatDrawEntry DescriptionEntry(Thing thing)
		{
			StatDrawEntry statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999, string.Empty);
			statDrawEntry.overrideReportText = thing.DescriptionFlavor;
			return statDrawEntry;
		}

		private static StatDrawEntry DescriptionEntry(WorldObject worldObject)
		{
			StatDrawEntry statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "Description".Translate(), string.Empty, 99999, string.Empty);
			statDrawEntry.overrideReportText = worldObject.GetDescription();
			return statDrawEntry;
		}

		private static StatDrawEntry QualityEntry(Thing t)
		{
			if (!t.TryGetQuality(out QualityCategory qc))
			{
				return null;
			}
			StatDrawEntry statDrawEntry = new StatDrawEntry(StatCategoryDefOf.Basics, "Quality".Translate(), qc.GetLabel().CapitalizeFirst(), 99999, string.Empty);
			statDrawEntry.overrideReportText = "QualityDescription".Translate();
			return statDrawEntry;
		}

		private static void SelectEntry(StatDrawEntry rec, bool playSound = true)
		{
			selectedEntry = rec;
			if (playSound)
			{
				SoundDefOf.Tick_High.PlayOneShotOnCamera();
			}
		}

		private static void DrawStatsWorker(Rect rect, Thing optionalThing, WorldObject optionalWorldObject)
		{
			Rect rect2 = new Rect(rect);
			rect2.width *= 0.5f;
			Rect rect3 = new Rect(rect);
			rect3.x = rect2.xMax;
			rect3.width = rect.xMax - rect3.x;
			Text.Font = GameFont.Small;
			Rect viewRect = new Rect(0f, 0f, rect2.width - 16f, listHeight);
			Widgets.BeginScrollView(rect2, ref scrollPosition, viewRect);
			float curY = 0f;
			string b = null;
			mousedOverEntry = null;
			for (int i = 0; i < cachedDrawEntries.Count; i++)
			{
				StatDrawEntry ent = cachedDrawEntries[i];
				if (ent.category.LabelCap != b)
				{
					Widgets.ListSeparator(ref curY, viewRect.width, ent.category.LabelCap);
					b = ent.category.LabelCap;
				}
				curY += ent.Draw(8f, curY, viewRect.width - 8f, selectedEntry == ent, delegate
				{
					SelectEntry(ent);
				}, delegate
				{
					mousedOverEntry = ent;
				}, scrollPosition, rect2);
			}
			listHeight = curY + 100f;
			Widgets.EndScrollView();
			Rect rect4 = rect3.ContractedBy(10f);
			GUI.BeginGroup(rect4);
			StatDrawEntry statDrawEntry = selectedEntry ?? mousedOverEntry ?? cachedDrawEntries.FirstOrDefault();
			if (statDrawEntry != null)
			{
				StatRequest optionalReq = statDrawEntry.hasOptionalReq ? statDrawEntry.optionalReq : ((optionalThing == null) ? StatRequest.ForEmpty() : StatRequest.For(optionalThing));
				string explanationText = statDrawEntry.GetExplanationText(optionalReq);
				Rect rect5 = rect4.AtZero();
				Widgets.Label(rect5, explanationText);
			}
			GUI.EndGroup();
		}
	}
}
