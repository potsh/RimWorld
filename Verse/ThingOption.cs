using System.Xml;

namespace Verse
{
	public sealed class ThingOption
	{
		public ThingDef thingDef;

		public float weight = 1f;

		public ThingOption()
		{
		}

		public ThingOption(ThingDef thingDef, float weight)
		{
			this.thingDef = thingDef;
			this.weight = weight;
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingOption: " + xmlRoot.OuterXml);
			}
			else
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
				weight = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
			}
		}

		public override string ToString()
		{
			return "(" + ((thingDef == null) ? "null" : thingDef.defName) + ", weight=" + weight.ToString("0.##") + ")";
		}
	}
}
