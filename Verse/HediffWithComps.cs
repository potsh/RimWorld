using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
	public class HediffWithComps : Hediff
	{
		public List<HediffComp> comps = new List<HediffComp>();

		public override string LabelInBrackets
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.LabelInBrackets);
				if (comps != null)
				{
					for (int i = 0; i < comps.Count; i++)
					{
						string compLabelInBracketsExtra = comps[i].CompLabelInBracketsExtra;
						if (!compLabelInBracketsExtra.NullOrEmpty())
						{
							if (stringBuilder.Length != 0)
							{
								stringBuilder.Append(", ");
							}
							stringBuilder.Append(compLabelInBracketsExtra);
						}
					}
				}
				return stringBuilder.ToString();
			}
		}

		public override bool ShouldRemove
		{
			get
			{
				if (comps != null)
				{
					for (int i = 0; i < comps.Count; i++)
					{
						if (comps[i].CompShouldRemove)
						{
							return true;
						}
					}
				}
				return base.ShouldRemove;
			}
		}

		public override bool Visible
		{
			get
			{
				if (comps != null)
				{
					for (int i = 0; i < comps.Count; i++)
					{
						if (comps[i].CompDisallowVisible())
						{
							return false;
						}
					}
				}
				return base.Visible;
			}
		}

		public override string TipStringExtra
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.TipStringExtra);
				if (comps != null)
				{
					for (int i = 0; i < comps.Count; i++)
					{
						string compTipStringExtra = comps[i].CompTipStringExtra;
						if (!compTipStringExtra.NullOrEmpty())
						{
							stringBuilder.AppendLine(compTipStringExtra);
						}
					}
				}
				return stringBuilder.ToString();
			}
		}

		public override TextureAndColor StateIcon
		{
			get
			{
				for (int i = 0; i < comps.Count; i++)
				{
					TextureAndColor compStateIcon = comps[i].CompStateIcon;
					if (compStateIcon.HasValue)
					{
						return compStateIcon;
					}
				}
				return TextureAndColor.None;
			}
		}

		public override void PostAdd(DamageInfo? dinfo)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostPostAdd(dinfo);
				}
			}
		}

		public override void PostRemoved()
		{
			base.PostRemoved();
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostPostRemoved();
				}
			}
		}

		public override void PostTick()
		{
			base.PostTick();
			if (comps != null)
			{
				float severityAdjustment = 0f;
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostTick(ref severityAdjustment);
				}
				if (severityAdjustment != 0f)
				{
					Severity += severityAdjustment;
				}
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				InitializeComps();
			}
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompExposeData();
				}
			}
		}

		public override void Tended(float quality, int batchPosition = 0)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompTended(quality, batchPosition);
			}
		}

		public override bool TryMergeWith(Hediff other)
		{
			if (base.TryMergeWith(other))
			{
				for (int i = 0; i < comps.Count; i++)
				{
					comps[i].CompPostMerged(other);
				}
				return true;
			}
			return false;
		}

		public override void Notify_PawnDied()
		{
			base.Notify_PawnDied();
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].Notify_PawnDied();
			}
		}

		public override void ModifyChemicalEffect(ChemicalDef chem, ref float effect)
		{
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompModifyChemicalEffect(chem, ref effect);
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			InitializeComps();
			for (int i = 0; i < comps.Count; i++)
			{
				comps[i].CompPostMake();
			}
		}

		private void InitializeComps()
		{
			if (def.comps != null)
			{
				comps = new List<HediffComp>();
				for (int i = 0; i < def.comps.Count; i++)
				{
					HediffComp hediffComp = (HediffComp)Activator.CreateInstance(def.comps[i].compClass);
					hediffComp.props = def.comps[i];
					hediffComp.parent = this;
					comps.Add(hediffComp);
				}
			}
		}

		public override string DebugString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(base.DebugString());
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					string str = (!comps[i].ToString().Contains('_')) ? comps[i].ToString() : comps[i].ToString().Split('_')[1];
					stringBuilder.AppendLine("--" + str);
					string text = comps[i].CompDebugString();
					if (!text.NullOrEmpty())
					{
						stringBuilder.AppendLine(text.TrimEnd().Indented());
					}
				}
			}
			return stringBuilder.ToString();
		}
	}
}
