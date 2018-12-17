using Verse;

namespace RimWorld
{
	public static class ApparelGraphicRecordGetter
	{
		public static bool TryGetGraphicApparel(Apparel apparel, BodyTypeDef bodyType, out ApparelGraphicRecord rec)
		{
			if (bodyType == null)
			{
				Log.Error("Getting apparel graphic with undefined body type.");
				bodyType = BodyTypeDefOf.Male;
			}
			if (apparel.def.apparel.wornGraphicPath.NullOrEmpty())
			{
				rec = new ApparelGraphicRecord(null, null);
				return false;
			}
			string path = (apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead) ? (apparel.def.apparel.wornGraphicPath + "_" + bodyType.defName) : apparel.def.apparel.wornGraphicPath;
			Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, ShaderDatabase.Cutout, apparel.def.graphicData.drawSize, apparel.DrawColor);
			rec = new ApparelGraphicRecord(graphic, apparel);
			return true;
		}
	}
}
