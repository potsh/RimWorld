using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Verse;

namespace RimWorld
{
	public static class TaleFactory
	{
		[CompilerGenerated]
		private static Func<object, string> _003C_003Ef__mg_0024cache0;

		public static Tale MakeRawTale(TaleDef def, params object[] args)
		{
			try
			{
				Tale tale = (Tale)Activator.CreateInstance(def.taleClass, args);
				tale.def = def;
				tale.id = Find.UniqueIDsManager.GetNextTaleID();
				tale.date = Find.TickManager.TicksAbs;
				return tale;
			}
			catch (Exception arg)
			{
				Log.Error($"Failed to create tale object {def} with parameters {args.Select(Gen.ToStringSafe<object>).ToCommaList()}: {arg}");
				return null;
			}
		}

		public static Tale MakeRandomTestTale(TaleDef def = null)
		{
			if (def == null)
			{
				def = (from d in DefDatabase<TaleDef>.AllDefs
				where d.usableForArt
				select d).RandomElement();
			}
			Tale tale = MakeRawTale(def);
			tale.GenerateTestData();
			return tale;
		}
	}
}
