using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Pawn_StoryTracker : IExposable
	{
		private Pawn pawn;

		public Backstory childhood;

		public Backstory adulthood;

		public float melanin;

		public Color hairColor = Color.white;

		public CrownType crownType;

		public BodyTypeDef bodyType;

		private string headGraphicPath;

		public HairDef hairDef;

		public TraitSet traits;

		public string title;

		private List<WorkTypeDef> cachedDisabledWorkTypes;

		public string Title
		{
			get
			{
				if (title != null)
				{
					return title;
				}
				return TitleDefault;
			}
			set
			{
				title = null;
				if (value != Title && !value.NullOrEmpty())
				{
					title = value;
				}
			}
		}

		public string TitleCap => Title.CapitalizeFirst();

		public string TitleDefault
		{
			get
			{
				if (adulthood != null)
				{
					return adulthood.TitleFor(pawn.gender);
				}
				if (childhood != null)
				{
					return childhood.TitleFor(pawn.gender);
				}
				return string.Empty;
			}
		}

		public string TitleDefaultCap => TitleDefault.CapitalizeFirst();

		public string TitleShort
		{
			get
			{
				if (title != null)
				{
					return title;
				}
				if (adulthood != null)
				{
					return adulthood.TitleShortFor(pawn.gender);
				}
				if (childhood != null)
				{
					return childhood.TitleShortFor(pawn.gender);
				}
				return string.Empty;
			}
		}

		public string TitleShortCap => TitleShort.CapitalizeFirst();

		public Color SkinColor => PawnSkinColors.GetSkinColor(melanin);

		public IEnumerable<Backstory> AllBackstories
		{
			get
			{
				if (childhood != null)
				{
					yield return childhood;
					/*Error: Unable to find new state assignment for yield return*/;
				}
				if (adulthood != null)
				{
					yield return adulthood;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public string HeadGraphicPath
		{
			get
			{
				if (headGraphicPath == null)
				{
					headGraphicPath = GraphicDatabaseHeadRecords.GetHeadRandom(pawn.gender, pawn.story.SkinColor, pawn.story.crownType).GraphicPath;
				}
				return headGraphicPath;
			}
		}

		public List<WorkTypeDef> DisabledWorkTypes
		{
			get
			{
				if (cachedDisabledWorkTypes == null)
				{
					cachedDisabledWorkTypes = new List<WorkTypeDef>();
					foreach (Backstory allBackstory in AllBackstories)
					{
						foreach (WorkTypeDef disabledWorkType in allBackstory.DisabledWorkTypes)
						{
							if (!cachedDisabledWorkTypes.Contains(disabledWorkType))
							{
								cachedDisabledWorkTypes.Add(disabledWorkType);
							}
						}
					}
					for (int i = 0; i < traits.allTraits.Count; i++)
					{
						foreach (WorkTypeDef disabledWorkType2 in traits.allTraits[i].GetDisabledWorkTypes())
						{
							if (!cachedDisabledWorkTypes.Contains(disabledWorkType2))
							{
								cachedDisabledWorkTypes.Add(disabledWorkType2);
							}
						}
					}
				}
				return cachedDisabledWorkTypes;
			}
		}

		public WorkTags CombinedDisabledWorkTags
		{
			get
			{
				WorkTags workTags = WorkTags.None;
				if (childhood != null)
				{
					workTags |= childhood.workDisables;
				}
				if (adulthood != null)
				{
					workTags |= adulthood.workDisables;
				}
				for (int i = 0; i < traits.allTraits.Count; i++)
				{
					workTags |= traits.allTraits[i].def.disabledWorkTags;
				}
				return workTags;
			}
		}

		public Pawn_StoryTracker(Pawn pawn)
		{
			this.pawn = pawn;
			traits = new TraitSet(pawn);
		}

		public void ExposeData()
		{
			string value = (childhood == null) ? null : childhood.identifier;
			Scribe_Values.Look(ref value, "childhood");
			if (Scribe.mode == LoadSaveMode.LoadingVars && !value.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(value, out childhood))
			{
				Log.Error("Couldn't load child backstory with identifier " + value + ". Giving random.");
				childhood = BackstoryDatabase.RandomBackstory(BackstorySlot.Childhood);
			}
			string value2 = (adulthood == null) ? null : adulthood.identifier;
			Scribe_Values.Look(ref value2, "adulthood");
			if (Scribe.mode == LoadSaveMode.LoadingVars && !value2.NullOrEmpty() && !BackstoryDatabase.TryGetWithIdentifier(value2, out adulthood))
			{
				Log.Error("Couldn't load adult backstory with identifier " + value2 + ". Giving random.");
				adulthood = BackstoryDatabase.RandomBackstory(BackstorySlot.Adulthood);
			}
			Scribe_Defs.Look(ref bodyType, "bodyType");
			Scribe_Values.Look(ref crownType, "crownType", CrownType.Undefined);
			Scribe_Values.Look(ref headGraphicPath, "headGraphicPath");
			Scribe_Defs.Look(ref hairDef, "hairDef");
			Scribe_Values.Look(ref hairColor, "hairColor");
			Scribe_Values.Look(ref melanin, "melanin", 0f);
			Scribe_Deep.Look(ref traits, "traits", pawn);
			Scribe_Values.Look(ref title, "title");
			if (Scribe.mode == LoadSaveMode.PostLoadInit && hairDef == null)
			{
				hairDef = DefDatabase<HairDef>.AllDefs.RandomElement();
			}
		}

		public Backstory GetBackstory(BackstorySlot slot)
		{
			if (slot == BackstorySlot.Childhood)
			{
				return childhood;
			}
			return adulthood;
		}

		public bool WorkTypeIsDisabled(WorkTypeDef w)
		{
			return DisabledWorkTypes.Contains(w);
		}

		public bool OneOfWorkTypesIsDisabled(List<WorkTypeDef> wts)
		{
			for (int i = 0; i < wts.Count; i++)
			{
				if (WorkTypeIsDisabled(wts[i]))
				{
					return true;
				}
			}
			return false;
		}

		public bool WorkTagIsDisabled(WorkTags w)
		{
			return (CombinedDisabledWorkTags & w) != WorkTags.None;
		}

		internal void Notify_TraitChanged()
		{
			cachedDisabledWorkTypes = null;
		}
	}
}
