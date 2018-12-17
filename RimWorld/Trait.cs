using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace RimWorld
{
	public class Trait : IExposable
	{
		public TraitDef def;

		private int degree;

		private bool scenForced;

		public int Degree => degree;

		public TraitDegreeData CurrentData => def.DataAtDegree(degree);

		public string Label => CurrentData.label;

		public string LabelCap => Label.CapitalizeFirst();

		public bool ScenForced => scenForced;

		public Trait()
		{
		}

		public Trait(TraitDef def, int degree = 0, bool forced = false)
		{
			this.def = def;
			this.degree = degree;
			scenForced = forced;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref def, "def");
			Scribe_Values.Look(ref degree, "degree", 0);
			Scribe_Values.Look(ref scenForced, "scenForced", defaultValue: false);
			if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && def == null)
			{
				def = DefDatabase<TraitDef>.GetRandom();
				degree = PawnGenerator.RandomTraitDegree(def);
			}
		}

		public float OffsetOfStat(StatDef stat)
		{
			float num = 0f;
			TraitDegreeData currentData = CurrentData;
			if (currentData.statOffsets != null)
			{
				for (int i = 0; i < currentData.statOffsets.Count; i++)
				{
					if (currentData.statOffsets[i].stat == stat)
					{
						num += currentData.statOffsets[i].value;
					}
				}
			}
			return num;
		}

		public float MultiplierOfStat(StatDef stat)
		{
			float num = 1f;
			TraitDegreeData currentData = CurrentData;
			if (currentData.statFactors != null)
			{
				for (int i = 0; i < currentData.statFactors.Count; i++)
				{
					if (currentData.statFactors[i].stat == stat)
					{
						num *= currentData.statFactors[i].value;
					}
				}
			}
			return num;
		}

		public string TipString(Pawn pawn)
		{
			StringBuilder stringBuilder = new StringBuilder();
			TraitDegreeData currentData = CurrentData;
			stringBuilder.Append(currentData.description.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn));
			int count = CurrentData.skillGains.Count;
			if (count > 0)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
			}
			int num = 0;
			foreach (KeyValuePair<SkillDef, int> skillGain in CurrentData.skillGains)
			{
				if (skillGain.Value != 0)
				{
					string value = "    " + skillGain.Key.skillLabel.CapitalizeFirst() + ":   " + skillGain.Value.ToString("+##;-##");
					if (num < count - 1)
					{
						stringBuilder.AppendLine(value);
					}
					else
					{
						stringBuilder.Append(value);
					}
					num++;
				}
			}
			if (GetPermaThoughts().Any())
			{
				stringBuilder.AppendLine();
				foreach (ThoughtDef permaThought in GetPermaThoughts())
				{
					stringBuilder.AppendLine();
					stringBuilder.Append("    " + "PermanentMoodEffect".Translate() + " " + permaThought.stages[0].baseMoodEffect.ToStringByStyle(ToStringStyle.Integer, ToStringNumberSense.Offset));
				}
			}
			if (currentData.statOffsets != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				for (int i = 0; i < currentData.statOffsets.Count; i++)
				{
					StatModifier statModifier = currentData.statOffsets[i];
					string valueToStringAsOffset = statModifier.ValueToStringAsOffset;
					string value2 = "    " + statModifier.stat.LabelCap + " " + valueToStringAsOffset;
					if (i < currentData.statOffsets.Count - 1)
					{
						stringBuilder.AppendLine(value2);
					}
					else
					{
						stringBuilder.Append(value2);
					}
				}
			}
			if (currentData.statFactors != null)
			{
				stringBuilder.AppendLine();
				stringBuilder.AppendLine();
				for (int j = 0; j < currentData.statFactors.Count; j++)
				{
					StatModifier statModifier2 = currentData.statFactors[j];
					string toStringAsFactor = statModifier2.ToStringAsFactor;
					string value3 = "    " + statModifier2.stat.LabelCap + " " + toStringAsFactor;
					if (j < currentData.statFactors.Count - 1)
					{
						stringBuilder.AppendLine(value3);
					}
					else
					{
						stringBuilder.Append(value3);
					}
				}
			}
			return stringBuilder.ToString();
		}

		public override string ToString()
		{
			return "Trait(" + def.ToString() + "-" + degree + ")";
		}

		private IEnumerable<ThoughtDef> GetPermaThoughts()
		{
			TraitDegreeData degree = CurrentData;
			List<ThoughtDef> allThoughts = DefDatabase<ThoughtDef>.AllDefsListForReading;
			int i = 0;
			while (true)
			{
				if (i >= allThoughts.Count)
				{
					yield break;
				}
				if (allThoughts[i].IsSituational && allThoughts[i].Worker is ThoughtWorker_AlwaysActive && allThoughts[i].requiredTraits != null && allThoughts[i].requiredTraits.Contains(def) && (!allThoughts[i].RequiresSpecificTraitsDegree || allThoughts[i].requiredTraitsDegree == degree.degree))
				{
					break;
				}
				i++;
			}
			yield return allThoughts[i];
			/*Error: Unable to find new state assignment for yield return*/;
		}

		private bool AllowsWorkType(WorkTypeDef workDef)
		{
			return (def.disabledWorkTags & workDef.workTags) == WorkTags.None;
		}

		public IEnumerable<WorkTypeDef> GetDisabledWorkTypes()
		{
			int j = 0;
			if (j < def.disabledWorkTypes.Count)
			{
				yield return def.disabledWorkTypes[j];
				/*Error: Unable to find new state assignment for yield return*/;
			}
			List<WorkTypeDef> workTypeDefList = DefDatabase<WorkTypeDef>.AllDefsListForReading;
			int i = 0;
			WorkTypeDef w;
			while (true)
			{
				if (i >= workTypeDefList.Count)
				{
					yield break;
				}
				w = workTypeDefList[i];
				if (!AllowsWorkType(w))
				{
					break;
				}
				i++;
			}
			yield return w;
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
