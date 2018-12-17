using System.Collections.Generic;
using Verse;

namespace RimWorld
{
	public class Apparel : ThingWithComps
	{
		private bool wornByCorpseInt;

		public Pawn Wearer => (base.ParentHolder as Pawn_ApparelTracker)?.pawn;

		public bool WornByCorpse => wornByCorpseInt;

		public override string DescriptionDetailed
		{
			get
			{
				string text = base.DescriptionDetailed;
				if (WornByCorpse)
				{
					text = text + "\n" + "WasWornByCorpse".Translate();
				}
				return text;
			}
		}

		public void Notify_PawnKilled()
		{
			if (def.apparel.careIfWornByCorpse)
			{
				wornByCorpseInt = true;
			}
		}

		public void Notify_PawnResurrected()
		{
			wornByCorpseInt = false;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref wornByCorpseInt, "wornByCorpse", defaultValue: false);
		}

		public virtual void DrawWornExtras()
		{
		}

		public virtual bool CheckPreAbsorbDamage(DamageInfo dinfo)
		{
			return false;
		}

		public virtual bool AllowVerbCast(IntVec3 root, Map map, LocalTargetInfo targ, Verb verb)
		{
			return true;
		}

		public virtual IEnumerable<Gizmo> GetWornGizmos()
		{
			yield break;
		}

		public override string GetInspectString()
		{
			string text = base.GetInspectString();
			if (WornByCorpse)
			{
				if (text.Length > 0)
				{
					text += "\n";
				}
				text += "WasWornByCorpse".Translate();
			}
			return text;
		}

		public virtual float GetSpecialApparelScoreOffset()
		{
			return 0f;
		}
	}
}
