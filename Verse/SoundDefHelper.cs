using RimWorld.Planet;
using System;
using Verse.Sound;

namespace Verse
{
	public static class SoundDefHelper
	{
		public static bool NullOrUndefined(this SoundDef def)
		{
			return def?.isUndefined ?? true;
		}

		public static bool CorrectContextNow(SoundDef def, Map sourceMap)
		{
			if (sourceMap == null || (Find.CurrentMap == sourceMap && !WorldRendererUtility.WorldRenderedNow))
			{
				switch (def.context)
				{
				case SoundContext.Any:
					return true;
				case SoundContext.MapOnly:
					return Current.ProgramState == ProgramState.Playing && !WorldRendererUtility.WorldRenderedNow;
				case SoundContext.WorldOnly:
					return WorldRendererUtility.WorldRenderedNow;
				default:
					throw new NotImplementedException();
				}
			}
			return false;
		}
	}
}
