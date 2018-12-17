namespace Verse
{
	public class BodyPartGroupDef : Def
	{
		[MustTranslate]
		public string labelShort;

		public int listOrder;

		public string LabelShort => (!labelShort.NullOrEmpty()) ? labelShort : label;

		public string LabelShortCap => LabelShort.CapitalizeFirst();
	}
}
