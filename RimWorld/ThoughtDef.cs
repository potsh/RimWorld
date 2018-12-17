using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ThoughtDef : Def
	{
		public Type thoughtClass;

		public Type workerClass;

		public List<ThoughtStage> stages = new List<ThoughtStage>();

		public int stackLimit = 1;

		public float stackedEffectMultiplier = 0.75f;

		public float durationDays;

		public bool invert;

		public bool validWhileDespawned;

		public ThoughtDef nextThought;

		public List<TraitDef> nullifyingTraits;

		public List<TaleDef> nullifyingOwnTales;

		public List<TraitDef> requiredTraits;

		public int requiredTraitsDegree = -2147483648;

		public StatDef effectMultiplyingStat;

		public HediffDef hediff;

		public GameConditionDef gameCondition;

		public bool nullifiedIfNotColonist;

		public ThoughtDef thoughtToMake;

		[NoTranslate]
		private string icon;

		public bool showBubble;

		public int stackLimitForSameOtherPawn = -1;

		public float lerpOpinionToZeroAfterDurationPct = 0.7f;

		public float maxCumulatedOpinionOffset = 3.40282347E+38f;

		public TaleDef taleDef;

		[Unsaved]
		private ThoughtWorker workerInt;

		[Unsaved]
		private BoolUnknown isMemoryCached = BoolUnknown.Unknown;

		private Texture2D iconInt;

		public string Label
		{
			get
			{
				if (!label.NullOrEmpty())
				{
					return label;
				}
				if (!stages.NullOrEmpty())
				{
					if (!stages[0].label.NullOrEmpty())
					{
						return stages[0].label;
					}
					if (!stages[0].labelSocial.NullOrEmpty())
					{
						return stages[0].labelSocial;
					}
				}
				Log.Error("Cannot get good label for ThoughtDef " + defName);
				return defName;
			}
		}

		public int DurationTicks => (int)(durationDays * 60000f);

		public bool IsMemory
		{
			get
			{
				if (isMemoryCached == BoolUnknown.Unknown)
				{
					isMemoryCached = ((!(durationDays > 0f) && !typeof(Thought_Memory).IsAssignableFrom(thoughtClass)) ? BoolUnknown.False : BoolUnknown.True);
				}
				return isMemoryCached == BoolUnknown.True;
			}
		}

		public bool IsSituational => Worker != null;

		public bool IsSocial => typeof(ISocialThought).IsAssignableFrom(ThoughtClass);

		public bool RequiresSpecificTraitsDegree => requiredTraitsDegree != -2147483648;

		public ThoughtWorker Worker
		{
			get
			{
				if (workerInt == null && workerClass != null)
				{
					workerInt = (ThoughtWorker)Activator.CreateInstance(workerClass);
					workerInt.def = this;
				}
				return workerInt;
			}
		}

		public Type ThoughtClass
		{
			get
			{
				if (thoughtClass != null)
				{
					return thoughtClass;
				}
				if (IsMemory)
				{
					return typeof(Thought_Memory);
				}
				return typeof(Thought_Situational);
			}
		}

		public Texture2D Icon
		{
			get
			{
				if (iconInt == null)
				{
					if (icon == null)
					{
						return null;
					}
					iconInt = ContentFinder<Texture2D>.Get(icon);
				}
				return iconInt;
			}
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string error = enumerator.Current;
					yield return error;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (stages.NullOrEmpty())
			{
				yield return "no stages";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (workerClass != null && nextThought != null)
			{
				yield return "has a nextThought but also has a workerClass. nextThought only works for memories";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (IsMemory && workerClass != null)
			{
				yield return "has a workerClass but is a memory. workerClass only works for situational thoughts, not memories";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!IsMemory && workerClass == null && IsSituational)
			{
				yield return "is a situational thought but has no workerClass. Situational thoughts require workerClasses to analyze the situation";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			for (int i = 0; i < stages.Count; i++)
			{
				if (stages[i] != null)
				{
					using (IEnumerator<string> enumerator2 = stages[i].ConfigErrors().GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							string e = enumerator2.Current;
							yield return e;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			yield break;
			IL_02bb:
			/*Error near IL_02bc: Unexpected return in MoveNext()*/;
		}

		public static ThoughtDef Named(string defName)
		{
			return DefDatabase<ThoughtDef>.GetNamed(defName);
		}
	}
}
