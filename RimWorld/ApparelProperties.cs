using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class ApparelProperties
	{
		public List<BodyPartGroupDef> bodyPartGroups = new List<BodyPartGroupDef>();

		public List<ApparelLayerDef> layers = new List<ApparelLayerDef>();

		[NoTranslate]
		public string wornGraphicPath = string.Empty;

		[NoTranslate]
		public List<string> tags = new List<string>();

		[NoTranslate]
		public List<string> defaultOutfitTags;

		public float wearPerDay = 0.4f;

		public bool careIfWornByCorpse = true;

		public bool hatRenderedFrontOfFace;

		public bool useDeflectMetalEffect;

		[Unsaved]
		private float cachedHumanBodyCoverage = -1f;

		[Unsaved]
		private BodyPartGroupDef[][] interferingBodyPartGroups;

		private static BodyPartGroupDef[] apparelRelevantGroups;

		public ApparelLayerDef LastLayer
		{
			get
			{
				if (layers.Count > 0)
				{
					return layers[layers.Count - 1];
				}
				Log.ErrorOnce("Failed to get last layer on apparel item (see your config errors)", 31234937);
				return ApparelLayerDefOf.Belt;
			}
		}

		public float HumanBodyCoverage
		{
			get
			{
				if (cachedHumanBodyCoverage < 0f)
				{
					cachedHumanBodyCoverage = 0f;
					List<BodyPartRecord> allParts = BodyDefOf.Human.AllParts;
					for (int i = 0; i < allParts.Count; i++)
					{
						if (CoversBodyPart(allParts[i]))
						{
							cachedHumanBodyCoverage += allParts[i].coverageAbs;
						}
					}
				}
				return cachedHumanBodyCoverage;
			}
		}

		public static void ResetStaticData()
		{
			apparelRelevantGroups = (from td in DefDatabase<ThingDef>.AllDefs
			where td.IsApparel
			select td).SelectMany((ThingDef td) => td.apparel.bodyPartGroups).Distinct().ToArray();
		}

		public IEnumerable<string> ConfigErrors(ThingDef parentDef)
		{
			if (layers.NullOrEmpty())
			{
				yield return parentDef.defName + " apparel has no layers.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		public bool CoversBodyPart(BodyPartRecord partRec)
		{
			for (int i = 0; i < partRec.groups.Count; i++)
			{
				if (bodyPartGroups.Contains(partRec.groups[i]))
				{
					return true;
				}
			}
			return false;
		}

		public string GetCoveredOuterPartsString(BodyDef body)
		{
			IEnumerable<BodyPartRecord> source = from x in body.AllParts
			where x.depth == BodyPartDepth.Outside && x.groups.Any((BodyPartGroupDef y) => bodyPartGroups.Contains(y))
			select x;
			return (from part in source.Distinct()
			select part.Label).ToCommaList(useAnd: true).CapitalizeFirst();
		}

		public string GetLayersString()
		{
			return (from layer in layers
			select layer.label).ToCommaList(useAnd: true).CapitalizeFirst();
		}

		public BodyPartGroupDef[] GetInterferingBodyPartGroups(BodyDef body)
		{
			if (interferingBodyPartGroups == null || interferingBodyPartGroups.Length != DefDatabase<BodyDef>.DefCount)
			{
				interferingBodyPartGroups = new BodyPartGroupDef[DefDatabase<BodyDef>.DefCount][];
			}
			if (interferingBodyPartGroups[body.index] == null)
			{
				BodyPartRecord[] source = (from part in body.AllParts
				where part.groups.Any((BodyPartGroupDef @group) => bodyPartGroups.Contains(@group))
				select part).ToArray();
				BodyPartGroupDef[] array = (from bpgd in source.SelectMany((BodyPartRecord bpr) => bpr.groups).Distinct()
				where apparelRelevantGroups.Contains(bpgd)
				select bpgd).ToArray();
				interferingBodyPartGroups[body.index] = array;
			}
			return interferingBodyPartGroups[body.index];
		}
	}
}
