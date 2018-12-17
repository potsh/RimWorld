using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Verse
{
	public class HediffDef : Def
	{
		public Type hediffClass = typeof(Hediff);

		public List<HediffCompProperties> comps;

		public float initialSeverity = 0.5f;

		public float lethalSeverity = -1f;

		public List<HediffStage> stages;

		public bool tendable;

		public bool isBad = true;

		public ThingDef spawnThingOnRemoved;

		public float chanceToCauseNoPain;

		public bool makesSickThought;

		public bool makesAlert = true;

		public NeedDef causesNeed;

		public float minSeverity;

		public float maxSeverity = 3.40282347E+38f;

		public bool scenarioCanAdd;

		public List<HediffGiver> hediffGivers;

		public bool cureAllAtOnceIfCuredByItem;

		public TaleDef taleOnVisible;

		public bool everCurableByItem = true;

		public string battleStateLabel;

		public string labelNounPretty;

		public bool displayWound;

		public Color defaultLabelColor = Color.white;

		public InjuryProps injuryProps;

		public AddedBodyPartProps addedPartProps;

		[MustTranslate]
		public string labelNoun;

		private bool alwaysAllowMothballCached;

		private bool alwaysAllowMothball;

		private Hediff concreteExampleInt;

		public bool IsAddiction => typeof(Hediff_Addiction).IsAssignableFrom(hediffClass);

		public bool AlwaysAllowMothball
		{
			get
			{
				if (!alwaysAllowMothballCached)
				{
					alwaysAllowMothball = true;
					if (comps != null && comps.Count > 0)
					{
						alwaysAllowMothball = false;
					}
					if (stages != null)
					{
						for (int i = 0; i < stages.Count; i++)
						{
							HediffStage hediffStage = stages[i];
							if (hediffStage.deathMtbDays > 0f || (hediffStage.hediffGivers != null && hediffStage.hediffGivers.Count > 0))
							{
								alwaysAllowMothball = false;
							}
						}
					}
					alwaysAllowMothballCached = true;
				}
				return alwaysAllowMothball;
			}
		}

		public Hediff ConcreteExample
		{
			get
			{
				if (concreteExampleInt == null)
				{
					concreteExampleInt = HediffMaker.Debug_MakeConcreteExampleHediff(this);
				}
				return concreteExampleInt;
			}
		}

		public bool HasComp(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return true;
					}
				}
			}
			return false;
		}

		public HediffCompProperties CompPropsFor(Type compClass)
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					if (comps[i].compClass == compClass)
					{
						return comps[i];
					}
				}
			}
			return null;
		}

		public T CompProps<T>() where T : HediffCompProperties
		{
			if (comps != null)
			{
				for (int i = 0; i < comps.Count; i++)
				{
					T val = comps[i] as T;
					if (val != null)
					{
						return val;
					}
				}
			}
			return (T)null;
		}

		public bool PossibleToDevelopImmunityNaturally()
		{
			HediffCompProperties_Immunizable hediffCompProperties_Immunizable = CompProps<HediffCompProperties_Immunizable>();
			if (hediffCompProperties_Immunizable != null && (hediffCompProperties_Immunizable.immunityPerDayNotSick > 0f || hediffCompProperties_Immunizable.immunityPerDaySick > 0f))
			{
				return true;
			}
			return false;
		}

		public string PrettyTextForPart(BodyPartRecord bodyPart)
		{
			if (labelNounPretty.NullOrEmpty())
			{
				return null;
			}
			return string.Format(labelNounPretty, label, bodyPart.Label);
		}

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string err = enumerator.Current;
					yield return err;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (hediffClass == null)
			{
				yield return "hediffClass is null";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!comps.NullOrEmpty() && !typeof(HediffWithComps).IsAssignableFrom(hediffClass))
			{
				yield return "has comps but hediffClass is not HediffWithComps or subclass thereof";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (minSeverity > initialSeverity)
			{
				yield return "minSeverity is greater than initialSeverity";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (maxSeverity < initialSeverity)
			{
				yield return "maxSeverity is lower than initialSeverity";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (!tendable && HasComp(typeof(HediffComp_TendDuration)))
			{
				yield return "has HediffComp_TendDuration but tendable = false";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (comps != null)
			{
				for (int k = 0; k < comps.Count; k++)
				{
					using (IEnumerator<string> enumerator2 = comps[k].ConfigErrors(this).GetEnumerator())
					{
						if (enumerator2.MoveNext())
						{
							yield return string.Concat(arg2: enumerator2.Current, arg0: comps[k], arg1: ": ");
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
			}
			if (stages != null)
			{
				if (!typeof(Hediff_Addiction).IsAssignableFrom(hediffClass))
				{
					for (int j = 0; j < stages.Count; j++)
					{
						if (j >= 1 && stages[j].minSeverity <= stages[j - 1].minSeverity)
						{
							yield return "stages are not in order of minSeverity";
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				int i = 0;
				while (true)
				{
					if (i >= stages.Count)
					{
						yield break;
					}
					if (stages[i].makeImmuneTo != null && !stages[i].makeImmuneTo.Any((HediffDef im) => im.HasComp(typeof(HediffComp_Immunizable))))
					{
						break;
					}
					i++;
				}
				yield return "makes immune to hediff which doesn't have comp immunizable";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield break;
			IL_04a3:
			/*Error near IL_04a4: Unexpected return in MoveNext()*/;
		}

		public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
		{
			if (stages != null && stages.Count == 1)
			{
				using (IEnumerator<StatDrawEntry> enumerator = stages[0].SpecialDisplayStats().GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						StatDrawEntry de = enumerator.Current;
						yield return de;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			yield break;
			IL_00ea:
			/*Error near IL_00eb: Unexpected return in MoveNext()*/;
		}

		public static HediffDef Named(string defName)
		{
			return DefDatabase<HediffDef>.GetNamed(defName);
		}
	}
}
