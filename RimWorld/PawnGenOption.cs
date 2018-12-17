using System.Xml;
using Verse;

namespace RimWorld
{
	public class PawnGenOption
	{
		public PawnKindDef kind;

		public float selectionWeight;

		public float Cost => kind.combatPower;

		public override string ToString()
		{
			return "(" + ((kind == null) ? "null" : kind.ToString()) + " w=" + selectionWeight.ToString("F2") + " c=" + ((kind == null) ? "null" : Cost.ToString("F2")) + ")";
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "kind", xmlRoot.Name);
			selectionWeight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
