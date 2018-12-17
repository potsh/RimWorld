using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public abstract class SiteCoreOrPartDefBase : Def
	{
		public Type workerClass = typeof(SiteCoreOrPartWorkerBase);

		[NoTranslate]
		public string siteTexture;

		[NoTranslate]
		public string expandingIconTexture;

		public bool applyFactionColorToSiteTexture;

		public bool showFactionInInspectString;

		public bool requiresFaction;

		public TechLevel minFactionTechLevel;

		[MustTranslate]
		public string approachOrderString;

		[MustTranslate]
		public string approachingReportString;

		[NoTranslate]
		public List<string> tags = new List<string>();

		[MustTranslate]
		public string arrivedLetter;

		[MustTranslate]
		public string arrivedLetterLabelPart;

		public LetterDef arrivedLetterDef;

		[MustTranslate]
		public string descriptionDialogue;

		public bool wantsThreatPoints;

		[Unsaved]
		private SiteCoreOrPartWorkerBase workerInt;

		[Unsaved]
		private Texture2D expandingIconTextureInt;

		[Unsaved]
		private List<GenStepDef> extraGenSteps;

		public SiteCoreOrPartWorkerBase Worker
		{
			get
			{
				if (workerInt == null)
				{
					workerInt = CreateWorker();
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public Texture2D ExpandingIconTexture
		{
			get
			{
				if (expandingIconTextureInt == null)
				{
					if (!expandingIconTexture.NullOrEmpty())
					{
						expandingIconTextureInt = ContentFinder<Texture2D>.Get(expandingIconTexture);
					}
					else
					{
						expandingIconTextureInt = BaseContent.BadTex;
					}
				}
				return expandingIconTextureInt;
			}
		}

		public List<GenStepDef> ExtraGenSteps
		{
			get
			{
				if (extraGenSteps == null)
				{
					extraGenSteps = new List<GenStepDef>();
					List<GenStepDef> allDefsListForReading = DefDatabase<GenStepDef>.AllDefsListForReading;
					for (int i = 0; i < allDefsListForReading.Count; i++)
					{
						if (allDefsListForReading[i].linkWithSite == this)
						{
							extraGenSteps.Add(allDefsListForReading[i]);
						}
					}
				}
				return extraGenSteps;
			}
		}

		public virtual bool FactionCanOwn(Faction faction)
		{
			if (requiresFaction && faction == null)
			{
				return false;
			}
			if (minFactionTechLevel != 0 && (faction == null || (int)faction.def.techLevel < (int)minFactionTechLevel))
			{
				return false;
			}
			if (faction != null && (faction.IsPlayer || faction.defeated || faction.def.hidden))
			{
				return false;
			}
			return true;
		}

		protected abstract SiteCoreOrPartWorkerBase CreateWorker();
	}
}
