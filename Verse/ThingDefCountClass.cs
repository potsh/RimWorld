using System.Xml;

namespace Verse
{
	public sealed class ThingDefCountClass : IExposable
	{
		public ThingDef thingDef;

		public int count;

		public string Summary => count + "x " + ((thingDef == null) ? "null" : thingDef.label);

		public ThingDefCountClass()
		{
		}

		public ThingDefCountClass(ThingDef thingDef, int count)
		{
			if (count < 0)
			{
				Log.Warning("Tried to set ThingDefCountClass count to " + count + ". thingDef=" + thingDef);
				count = 0;
			}
			this.thingDef = thingDef;
			this.count = count;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref thingDef, "thingDef");
			Scribe_Values.Look(ref count, "count", 1);
		}

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			if (xmlRoot.ChildNodes.Count != 1)
			{
				Log.Error("Misconfigured ThingDefCountClass: " + xmlRoot.OuterXml);
			}
			else
			{
				DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
				count = (int)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(int));
			}
		}

		public override string ToString()
		{
			return "(" + count + "x " + ((thingDef == null) ? "null" : thingDef.defName) + ")";
		}

		public override int GetHashCode()
		{
			return thingDef.shortHash + count << 16;
		}

		public static implicit operator ThingDefCountClass(ThingDefCount t)
		{
			return new ThingDefCountClass(t.ThingDef, t.Count);
		}
	}
}
