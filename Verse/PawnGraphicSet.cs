using RimWorld;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class PawnGraphicSet
	{
		public Pawn pawn;

		public Graphic nakedGraphic;

		public Graphic rottingGraphic;

		public Graphic dessicatedGraphic;

		public Graphic packGraphic;

		public DamageFlasher flasher;

		public Graphic headGraphic;

		public Graphic desiccatedHeadGraphic;

		public Graphic skullGraphic;

		public Graphic headStumpGraphic;

		public Graphic desiccatedHeadStumpGraphic;

		public Graphic hairGraphic;

		public List<ApparelGraphicRecord> apparelGraphics = new List<ApparelGraphicRecord>();

		private List<Material> cachedMatsBodyBase = new List<Material>();

		private int cachedMatsBodyBaseHash = -1;

		public static readonly Color RottingColor = new Color(0.34f, 0.32f, 0.3f);

		public bool AllResolved => nakedGraphic != null;

		public GraphicMeshSet HairMeshSet
		{
			get
			{
				if (pawn.story.crownType == CrownType.Average)
				{
					return MeshPool.humanlikeHairSetAverage;
				}
				if (pawn.story.crownType == CrownType.Narrow)
				{
					return MeshPool.humanlikeHairSetNarrow;
				}
				Log.Error("Unknown crown type: " + pawn.story.crownType);
				return MeshPool.humanlikeHairSetAverage;
			}
		}

		public PawnGraphicSet(Pawn pawn)
		{
			this.pawn = pawn;
			flasher = new DamageFlasher(pawn);
		}

		public List<Material> MatsBodyBaseAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh)
		{
			int num = facing.AsInt + 1000 * (int)bodyCondition;
			if (num != cachedMatsBodyBaseHash)
			{
				cachedMatsBodyBase.Clear();
				cachedMatsBodyBaseHash = num;
				switch (bodyCondition)
				{
				case RotDrawMode.Fresh:
					cachedMatsBodyBase.Add(nakedGraphic.MatAt(facing));
					break;
				default:
					if (dessicatedGraphic != null)
					{
						if (bodyCondition == RotDrawMode.Dessicated)
						{
							cachedMatsBodyBase.Add(dessicatedGraphic.MatAt(facing));
						}
						break;
					}
					goto case RotDrawMode.Rotting;
				case RotDrawMode.Rotting:
					cachedMatsBodyBase.Add(rottingGraphic.MatAt(facing));
					break;
				}
				for (int i = 0; i < apparelGraphics.Count; i++)
				{
					ApparelGraphicRecord apparelGraphicRecord = apparelGraphics[i];
					if (apparelGraphicRecord.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Shell)
					{
						ApparelGraphicRecord apparelGraphicRecord2 = apparelGraphics[i];
						if (apparelGraphicRecord2.sourceApparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead)
						{
							List<Material> list = cachedMatsBodyBase;
							ApparelGraphicRecord apparelGraphicRecord3 = apparelGraphics[i];
							list.Add(apparelGraphicRecord3.graphic.MatAt(facing));
						}
					}
				}
			}
			return cachedMatsBodyBase;
		}

		public Material HeadMatAt(Rot4 facing, RotDrawMode bodyCondition = RotDrawMode.Fresh, bool stump = false)
		{
			Material material = null;
			switch (bodyCondition)
			{
			case RotDrawMode.Fresh:
				material = ((!stump) ? headGraphic.MatAt(facing) : headStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Rotting:
				material = ((!stump) ? desiccatedHeadGraphic.MatAt(facing) : desiccatedHeadStumpGraphic.MatAt(facing));
				break;
			case RotDrawMode.Dessicated:
				if (!stump)
				{
					material = skullGraphic.MatAt(facing);
				}
				break;
			}
			if (material != null)
			{
				material = flasher.GetDamagedMat(material);
			}
			return material;
		}

		public Material HairMatAt(Rot4 facing)
		{
			Material baseMat = hairGraphic.MatAt(facing);
			return flasher.GetDamagedMat(baseMat);
		}

		public void ClearCache()
		{
			cachedMatsBodyBaseHash = -1;
		}

		public void ResolveAllGraphics()
		{
			ClearCache();
			if (pawn.RaceProps.Humanlike)
			{
				nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, pawn.story.SkinColor);
				rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyNakedGraphicPath, ShaderDatabase.CutoutSkin, Vector2.one, RottingColor);
				dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.bodyType.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);
				headGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, pawn.story.SkinColor);
				desiccatedHeadGraphic = GraphicDatabaseHeadRecords.GetHeadNamed(pawn.story.HeadGraphicPath, RottingColor);
				skullGraphic = GraphicDatabaseHeadRecords.GetSkull();
				headStumpGraphic = GraphicDatabaseHeadRecords.GetStump(pawn.story.SkinColor);
				desiccatedHeadStumpGraphic = GraphicDatabaseHeadRecords.GetStump(RottingColor);
				hairGraphic = GraphicDatabase.Get<Graphic_Multi>(pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, pawn.story.hairColor);
				ResolveApparelGraphics();
			}
			else
			{
				PawnKindLifeStage curKindLifeStage = pawn.ageTracker.CurKindLifeStage;
				if (pawn.gender != Gender.Female || curKindLifeStage.femaleGraphicData == null)
				{
					nakedGraphic = curKindLifeStage.bodyGraphicData.Graphic;
				}
				else
				{
					nakedGraphic = curKindLifeStage.femaleGraphicData.Graphic;
				}
				rottingGraphic = nakedGraphic.GetColoredVersion(ShaderDatabase.CutoutSkin, RottingColor, RottingColor);
				if (pawn.RaceProps.packAnimal)
				{
					packGraphic = GraphicDatabase.Get<Graphic_Multi>(nakedGraphic.path + "Pack", ShaderDatabase.Cutout, nakedGraphic.drawSize, Color.white);
				}
				if (curKindLifeStage.dessicatedBodyGraphicData != null)
				{
					if (pawn.gender != Gender.Female || curKindLifeStage.femaleDessicatedBodyGraphicData == null)
					{
						dessicatedGraphic = curKindLifeStage.dessicatedBodyGraphicData.GraphicColoredFor(pawn);
					}
					else
					{
						dessicatedGraphic = curKindLifeStage.femaleDessicatedBodyGraphicData.GraphicColoredFor(pawn);
					}
				}
			}
		}

		public void ResolveApparelGraphics()
		{
			ClearCache();
			apparelGraphics.Clear();
			foreach (Apparel item in pawn.apparel.WornApparel)
			{
				if (ApparelGraphicRecordGetter.TryGetGraphicApparel(item, pawn.story.bodyType, out ApparelGraphicRecord rec))
				{
					apparelGraphics.Add(rec);
				}
			}
		}
	}
}
