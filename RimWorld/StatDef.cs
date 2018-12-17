using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class StatDef : Def
	{
		public StatCategoryDef category;

		public Type workerClass = typeof(StatWorker);

		public float hideAtValue = -2.14748365E+09f;

		public bool alwaysHide;

		public bool showNonAbstract = true;

		public bool showIfUndefined = true;

		public bool showOnPawns = true;

		public bool showOnHumanlikes = true;

		public bool showOnNonWildManHumanlikes = true;

		public bool showOnAnimals = true;

		public bool showOnMechanoids = true;

		public bool showOnNonWorkTables = true;

		public bool neverDisabled;

		public int displayPriorityInCategory;

		public ToStringNumberSense toStringNumberSense = ToStringNumberSense.Absolute;

		public ToStringStyle toStringStyle;

		private ToStringStyle? toStringStyleUnfinalized;

		[MustTranslate]
		public string formatString;

		public float defaultBaseValue = 1f;

		public List<SkillNeed> skillNeedOffsets;

		public float noSkillOffset;

		public List<PawnCapacityOffset> capacityOffsets;

		public List<StatDef> statFactors;

		public bool applyFactorsIfNegative = true;

		public List<SkillNeed> skillNeedFactors;

		public float noSkillFactor = 1f;

		public List<PawnCapacityFactor> capacityFactors;

		public SimpleCurve postProcessCurve;

		public float minValue = -9999999f;

		public float maxValue = 9999999f;

		public bool roundValue;

		public float roundToFiveOver = 3.40282347E+38f;

		public bool minifiedThingInherits;

		public bool scenarioRandomizable;

		public List<StatPart> parts;

		[Unsaved]
		private StatWorker workerInt;

		public StatWorker Worker
		{
			get
			{
				if (workerInt == null)
				{
					if (parts != null)
					{
						for (int i = 0; i < parts.Count; i++)
						{
							parts[i].parentStat = this;
						}
					}
					workerInt = (StatWorker)Activator.CreateInstance(workerClass);
					workerInt.InitSetStat(this);
				}
				return workerInt;
			}
		}

		public ToStringStyle ToStringStyleUnfinalized
		{
			get
			{
				ToStringStyle? toStringStyle = toStringStyleUnfinalized;
				return (!toStringStyle.HasValue) ? this.toStringStyle : toStringStyleUnfinalized.Value;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err2 = enumerator.Current;
					yield return err2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (capacityFactors != null)
			{
				foreach (PawnCapacityFactor capacityFactor in capacityFactors)
				{
					if (capacityFactor.weight > 1f)
					{
						yield return defName + " has activity factor with weight > 1";
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			if (parts != null)
			{
				for (int i = 0; i < parts.Count; i++)
				{
					using (IEnumerator<string> enumerator3 = parts[i].ConfigErrors().GetEnumerator())
					{
						if (enumerator3.MoveNext())
						{
							string err = enumerator3.Current;
							yield return defName + " has error in StatPart " + parts[i].ToString() + ": " + err;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_02a9:
			/*Error near IL_02aa: Unexpected return in MoveNext()*/;
		}

		public string ValueToString(float val, ToStringNumberSense numberSense = ToStringNumberSense.Absolute)
		{
			return Worker.ValueToString(val, finalized: true, numberSense);
		}

		public static StatDef Named(string defName)
		{
			return DefDatabase<StatDef>.GetNamed(defName);
		}

		public override void PostLoad()
		{
			base.PostLoad();
			if (parts != null)
			{
				List<StatPart> partsCopy = parts.ToList();
				parts.SortBy((StatPart x) => 0f - x.priority, (StatPart x) => partsCopy.IndexOf(x));
			}
		}

		public T GetStatPart<T>() where T : StatPart
		{
			return parts.OfType<T>().FirstOrDefault();
		}
	}
}
