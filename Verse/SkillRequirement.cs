using RimWorld;
using System.Xml;

namespace Verse
{
	public class SkillRequirement
	{
		public SkillDef skill;

		public int minLevel;

		public string Summary
		{
			get
			{
				if (skill == null)
				{
					return string.Empty;
				}
				return $"{skill.LabelCap} ({minLevel})";
			}
		}

		public bool PawnSatisfies(Pawn pawn)
		{
			if (pawn.skills == null)
			{
				return false;
			}
			return pawn.skills.GetSkill(skill).Level >= minLevel;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "skill", xmlRoot.Name);
			minLevel = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
		}

		public override string ToString()
		{
			if (skill == null)
			{
				return "null-skill-requirement";
			}
			return skill.defName + "-" + minLevel;
		}
	}
}
