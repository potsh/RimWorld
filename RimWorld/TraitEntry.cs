using System.Xml;
using Verse;

namespace RimWorld
{
	public class TraitEntry
	{
		public TraitDef def;

		public int degree;

		public TraitEntry()
		{
		}

		public TraitEntry(TraitDef def, int degree)
		{
			this.def = def;
			this.degree = degree;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			def = DefDatabase<TraitDef>.GetNamed(xmlRoot.Name);
			if (xmlRoot.HasChildNodes)
			{
				degree = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
			}
			else
			{
				degree = 0;
			}
		}
	}
}
