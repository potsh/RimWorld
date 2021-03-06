using Verse;

namespace RimWorld
{
	public class CompArt : ThingComp
	{
		private string authorNameInt;

		private string titleInt;

		private TaleReference taleRef;

		public string AuthorName
		{
			get
			{
				if (authorNameInt.NullOrEmpty())
				{
					return "UnknownLower".Translate().CapitalizeFirst();
				}
				return authorNameInt;
			}
		}

		public string Title
		{
			get
			{
				if (titleInt.NullOrEmpty())
				{
					Log.Error("CompArt got title but it wasn't configured.");
					titleInt = "Error";
				}
				return titleInt;
			}
		}

		public TaleReference TaleRef => taleRef;

		public bool CanShowArt
		{
			get
			{
				if (Props.mustBeFullGrave)
				{
					Building_Grave building_Grave = parent as Building_Grave;
					if (building_Grave == null || !building_Grave.HasCorpse)
					{
						return false;
					}
				}
				if (!parent.TryGetQuality(out QualityCategory qc))
				{
					return true;
				}
				return (int)qc >= (int)Props.minQualityForArtistic;
			}
		}

		public bool Active => taleRef != null;

		public CompProperties_Art Props => (CompProperties_Art)props;

		public void InitializeArt(ArtGenerationContext source)
		{
			InitializeArt(null, source);
		}

		public void InitializeArt(Thing relatedThing)
		{
			InitializeArt(relatedThing, ArtGenerationContext.Colony);
		}

		private void InitializeArt(Thing relatedThing, ArtGenerationContext source)
		{
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
			if (CanShowArt)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					if (relatedThing != null)
					{
						taleRef = Find.TaleManager.GetRandomTaleReferenceForArtConcerning(relatedThing);
					}
					else
					{
						taleRef = Find.TaleManager.GetRandomTaleReferenceForArt(source);
					}
				}
				else
				{
					taleRef = TaleReference.Taleless;
				}
				titleInt = GenerateTitle();
			}
			else
			{
				titleInt = null;
				taleRef = null;
			}
		}

		public void JustCreatedBy(Pawn pawn)
		{
			if (CanShowArt)
			{
				authorNameInt = pawn.Name.ToStringFull;
			}
		}

		public void Clear()
		{
			authorNameInt = null;
			titleInt = null;
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref authorNameInt, "authorName");
			Scribe_Values.Look(ref titleInt, "title");
			Scribe_Deep.Look(ref taleRef, "taleRef");
		}

		public override string CompInspectStringExtra()
		{
			if (!Active)
			{
				return null;
			}
			string text = "Author".Translate() + ": " + AuthorName;
			string text2 = text;
			return text2 + "\n" + "Title".Translate() + ": " + Title;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (taleRef != null)
			{
				taleRef.ReferenceDestroyed();
				taleRef = null;
			}
		}

		public override string GetDescriptionPart()
		{
			if (!Active)
			{
				return null;
			}
			string empty = string.Empty;
			empty += Title;
			empty += "\n\n";
			empty += GenerateImageDescription();
			empty += "\n\n";
			return empty + "Author".Translate() + ": " + AuthorName;
		}

		public override bool AllowStackWith(Thing other)
		{
			if (Active)
			{
				return false;
			}
			return true;
		}

		public string GenerateImageDescription()
		{
			if (taleRef == null)
			{
				Log.Error("Did CompArt.GenerateImageDescription without initializing art: " + parent);
				InitializeArt(ArtGenerationContext.Outsider);
			}
			return taleRef.GenerateText(TextGenerationPurpose.ArtDescription, Props.descriptionMaker);
		}

		private string GenerateTitle()
		{
			if (taleRef == null)
			{
				Log.Error("Did CompArt.GenerateTitle without initializing art: " + parent);
				InitializeArt(ArtGenerationContext.Outsider);
			}
			return GenText.CapitalizeAsTitle(taleRef.GenerateText(TextGenerationPurpose.ArtName, Props.nameMaker));
		}
	}
}
