using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class Dialog_ModSettings : Window
	{
		private Mod selMod;

		private const float TopAreaHeight = 40f;

		private const float TopButtonHeight = 35f;

		private const float TopButtonWidth = 150f;

		public override Vector2 InitialSize => new Vector2(900f, 700f);

		public Dialog_ModSettings()
		{
			forcePause = true;
			doCloseX = true;
			doCloseButton = true;
			closeOnClickedOutside = true;
			absorbInputAroundWindow = true;
		}

		public override void PreClose()
		{
			base.PreClose();
			if (selMod != null)
			{
				selMod.WriteSettings();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = new Rect(0f, 0f, 150f, 35f);
			if (Widgets.ButtonText(rect, "SelectMod".Translate()))
			{
				if (HasSettings())
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					foreach (Mod item in from mod in LoadedModManager.ModHandles
					where !mod.SettingsCategory().NullOrEmpty()
					orderby mod.SettingsCategory()
					select mod)
					{
						Mod localMod = item;
						list.Add(new FloatMenuOption(item.SettingsCategory(), delegate
						{
							if (selMod != null)
							{
								selMod.WriteSettings();
							}
							selMod = localMod;
						}));
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				else
				{
					List<FloatMenuOption> list2 = new List<FloatMenuOption>();
					list2.Add(new FloatMenuOption("NoConfigurableMods".Translate(), null));
					Find.WindowStack.Add(new FloatMenu(list2));
				}
			}
			if (selMod != null)
			{
				Text.Font = GameFont.Medium;
				Widgets.Label(new Rect(167f, 0f, inRect.width - 150f - 17f, 35f), selMod.SettingsCategory());
				Text.Font = GameFont.Small;
				float width = inRect.width;
				float num = inRect.height - 40f;
				Vector2 closeButSize = CloseButSize;
				Rect inRect2 = new Rect(0f, 40f, width, num - closeButSize.y);
				selMod.DoSettingsWindowContents(inRect2);
			}
		}

		public static bool HasSettings()
		{
			return LoadedModManager.ModHandles.Any((Mod mod) => !mod.SettingsCategory().NullOrEmpty());
		}
	}
}
