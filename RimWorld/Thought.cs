using UnityEngine;
using Verse;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public abstract class Thought : IExposable
	{
		public Pawn pawn;

		public ThoughtDef def;

		private static readonly Texture2D DefaultGoodIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericGood");

		private static readonly Texture2D DefaultBadIcon = ContentFinder<Texture2D>.Get("Things/Mote/ThoughtSymbol/GenericBad");

		public abstract int CurStageIndex
		{
			get;
		}

		public ThoughtStage CurStage => def.stages[CurStageIndex];

		public virtual bool VisibleInNeedsTab => CurStage.visible;

		public virtual string LabelCap => CurStage.label.CapitalizeFirst();

		protected virtual float BaseMoodOffset => CurStage.baseMoodEffect;

		public string LabelCapSocial
		{
			get
			{
				if (CurStage.labelSocial != null)
				{
					return CurStage.labelSocial.CapitalizeFirst();
				}
				return LabelCap;
			}
		}

		public string Description
		{
			get
			{
				string description = CurStage.description;
				if (description != null)
				{
					return description;
				}
				return def.description;
			}
		}

		public Texture2D Icon
		{
			get
			{
				if (def.Icon != null)
				{
					return def.Icon;
				}
				if (MoodOffset() > 0f)
				{
					return DefaultGoodIcon;
				}
				return DefaultBadIcon;
			}
		}

		public virtual void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
		}

		public virtual float MoodOffset()
		{
			if (CurStage == null)
			{
				Log.Error("CurStage is null while ShouldDiscard is false on " + def.defName + " for " + pawn);
				return 0f;
			}
			float num = BaseMoodOffset;
			if (def.effectMultiplyingStat != null)
			{
				num *= pawn.GetStatValue(def.effectMultiplyingStat);
			}
			return num;
		}

		public virtual bool GroupsWith(Thought other)
		{
			return def == other.def;
		}

		public virtual void Init()
		{
		}

		public override string ToString()
		{
			return "(" + def.defName + ")";
		}
	}
}
