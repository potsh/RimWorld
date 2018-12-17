using RimWorld;
using System.Collections.Generic;

namespace Verse
{
	public class HediffComp_VerbGiver : HediffComp, IVerbOwner
	{
		public VerbTracker verbTracker;

		Thing IVerbOwner.ConstantCaster
		{
			get
			{
				return base.Pawn;
			}
		}

		ImplementOwnerTypeDef IVerbOwner.ImplementOwnerTypeDef
		{
			get
			{
				return ImplementOwnerTypeDefOf.Hediff;
			}
		}

		public HediffCompProperties_VerbGiver Props => (HediffCompProperties_VerbGiver)props;

		public VerbTracker VerbTracker => verbTracker;

		public List<VerbProperties> VerbProperties => Props.verbs;

		public List<Tool> Tools => Props.tools;

		public HediffComp_VerbGiver()
		{
			verbTracker = new VerbTracker(this);
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
			Scribe_Deep.Look(ref verbTracker, "verbTracker", this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit && verbTracker == null)
			{
				verbTracker = new VerbTracker(this);
			}
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			verbTracker.VerbsTick();
		}

		string IVerbOwner.UniqueVerbOwnerID()
		{
			return parent.GetUniqueLoadID() + "_" + parent.comps.IndexOf(this);
		}

		bool IVerbOwner.VerbsStillUsableBy(Pawn p)
		{
			return p.health.hediffSet.hediffs.Contains(parent);
		}
	}
}
