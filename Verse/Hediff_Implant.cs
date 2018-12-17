namespace Verse
{
	public class Hediff_Implant : HediffWithComps
	{
		public override bool ShouldRemove => false;

		public override void PostAdd(DamageInfo? dinfo)
		{
			if (base.Part == null)
			{
				Log.Error("Part is null. It should be set before PostAdd for " + def + ".");
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && base.Part == null)
			{
				Log.Error("Hediff_Implant has null part after loading.");
				pawn.health.hediffSet.hediffs.Remove(this);
			}
		}
	}
}
