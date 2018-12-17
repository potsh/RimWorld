using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	public static class StorageSettingsClipboard
	{
		private static StorageSettings clipboard = new StorageSettings();

		private static bool copied = false;

		public static bool HasCopiedSettings => copied;

		public static void Copy(StorageSettings s)
		{
			clipboard.CopyFrom(s);
			copied = true;
		}

		public static void PasteInto(StorageSettings s)
		{
			s.CopyFrom(clipboard);
		}

		public static IEnumerable<Gizmo> CopyPasteGizmosFor(StorageSettings s)
		{
			yield return (Gizmo)new Command_Action
			{
				icon = ContentFinder<Texture2D>.Get("UI/Commands/CopySettings"),
				defaultLabel = "CommandCopyZoneSettingsLabel".Translate(),
				defaultDesc = "CommandCopyZoneSettingsDesc".Translate(),
				action = delegate
				{
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
					Copy(s);
				},
				hotKey = KeyBindingDefOf.Misc4
			};
			/*Error: Unable to find new state assignment for yield return*/;
		}
	}
}
