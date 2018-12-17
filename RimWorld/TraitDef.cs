using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimWorld
{
	public class TraitDef : Def
	{
		public List<TraitDegreeData> degreeDatas = new List<TraitDegreeData>();

		public List<TraitDef> conflictingTraits = new List<TraitDef>();

		public List<WorkTypeDef> requiredWorkTypes = new List<WorkTypeDef>();

		public WorkTags requiredWorkTags;

		public List<WorkTypeDef> disabledWorkTypes = new List<WorkTypeDef>();

		public WorkTags disabledWorkTags;

		private float commonality = 1f;

		private float commonalityFemale = -1f;

		public bool allowOnHostileSpawn = true;

		public static TraitDef Named(string defName)
		{
			return DefDatabase<TraitDef>.GetNamed(defName);
		}

		public TraitDegreeData DataAtDegree(int degree)
		{
			for (int i = 0; i < degreeDatas.Count; i++)
			{
				if (degreeDatas[i].degree == degree)
				{
					return degreeDatas[i];
				}
			}
			Log.Error(defName + " found no data at degree " + degree + ", returning first defined.");
			return degreeDatas[0];
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
			if (commonality < 0.001f && commonalityFemale < 0.001f)
			{
				yield return "TraitDef " + defName + " has 0 commonality.";
				/*Error: Unable to find new state assignment for yield return*/;
			}
			if (degreeDatas.Any())
			{
				int i = 0;
				TraitDegreeData dd3;
				while (true)
				{
					if (i >= degreeDatas.Count)
					{
						yield break;
					}
					_003CConfigErrors_003Ec__Iterator0 _003CConfigErrors_003Ec__Iterator = (_003CConfigErrors_003Ec__Iterator0)/*Error near IL_017b: stateMachine*/;
					dd3 = degreeDatas[i];
					if ((from dd2 in degreeDatas
					where dd2.degree == dd3.degree
					select dd2).Count() > 1)
					{
						break;
					}
					i++;
				}
				yield return ">1 datas for degree " + dd3.degree;
				/*Error: Unable to find new state assignment for yield return*/;
			}
			yield return defName + " has no degree datas.";
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0239:
			/*Error near IL_023a: Unexpected return in MoveNext()*/;
		}

		public bool ConflictsWith(Trait other)
		{
			if (other.def.conflictingTraits != null)
			{
				for (int i = 0; i < other.def.conflictingTraits.Count; i++)
				{
					if (other.def.conflictingTraits[i] == this)
					{
						return true;
					}
				}
			}
			return false;
		}

		public float GetGenderSpecificCommonality(Gender gender)
		{
			if (gender == Gender.Female && commonalityFemale >= 0f)
			{
				return commonalityFemale;
			}
			return commonality;
		}
	}
}
