using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorld
{
	[StaticConstructorOnStartup]
	public static class MedicalCareUtility
	{
		private static Texture2D[] careTextures;

		public const float CareSetterHeight = 28f;

		public const float CareSetterWidth = 140f;

		private static bool medicalCarePainting;

		[CompilerGenerated]
		private static Func<Pawn, MedicalCareCategory> _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>> _003C_003Ef__mg_0024cache1;

		public static void Reset()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				careTextures = new Texture2D[5];
				careTextures[0] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoCare");
				careTextures[1] = ContentFinder<Texture2D>.Get("UI/Icons/Medical/NoMeds");
				careTextures[2] = ThingDefOf.MedicineHerbal.uiIcon;
				careTextures[3] = ThingDefOf.MedicineIndustrial.uiIcon;
				careTextures[4] = ThingDefOf.MedicineUltratech.uiIcon;
			});
		}

		public static void MedicalCareSetter(Rect rect, ref MedicalCareCategory medCare)
		{
			Rect rect2 = new Rect(rect.x, rect.y, rect.width / 5f, rect.height);
			for (int i = 0; i < 5; i++)
			{
				MedicalCareCategory mc = (MedicalCareCategory)i;
				Widgets.DrawHighlightIfMouseover(rect2);
				GUI.DrawTexture(rect2, careTextures[i]);
				Widgets.DraggableResult draggableResult = Widgets.ButtonInvisibleDraggable(rect2);
				if (draggableResult == Widgets.DraggableResult.Dragged)
				{
					medicalCarePainting = true;
				}
				if ((medicalCarePainting && Mouse.IsOver(rect2) && medCare != mc) || draggableResult.AnyPressed())
				{
					medCare = mc;
					SoundDefOf.Tick_High.PlayOneShotOnCamera();
				}
				if (medCare == mc)
				{
					Widgets.DrawBox(rect2, 3);
				}
				TooltipHandler.TipRegion(rect2, () => mc.GetLabel(), 632165 + i * 17);
				rect2.x += rect2.width;
			}
			if (!Input.GetMouseButton(0))
			{
				medicalCarePainting = false;
			}
		}

		public static string GetLabel(this MedicalCareCategory cat)
		{
			return ("MedicalCareCategory_" + cat).Translate();
		}

		public static bool AllowsMedicine(this MedicalCareCategory cat, ThingDef meds)
		{
			switch (cat)
			{
			case MedicalCareCategory.NoCare:
				return false;
			case MedicalCareCategory.NoMeds:
				return false;
			case MedicalCareCategory.HerbalOrWorse:
				return meds.GetStatValueAbstract(StatDefOf.MedicalPotency) <= ThingDefOf.MedicineHerbal.GetStatValueAbstract(StatDefOf.MedicalPotency);
			case MedicalCareCategory.NormalOrWorse:
				return meds.GetStatValueAbstract(StatDefOf.MedicalPotency) <= ThingDefOf.MedicineIndustrial.GetStatValueAbstract(StatDefOf.MedicalPotency);
			case MedicalCareCategory.Best:
				return true;
			default:
				throw new InvalidOperationException();
			}
		}

		public static void MedicalCareSelectButton(Rect rect, Pawn pawn)
		{
			Func<Pawn, MedicalCareCategory> getPayload = MedicalCareSelectButton_GetMedicalCare;
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>> menuGenerator = MedicalCareSelectButton_GenerateMenu;
			Texture2D buttonIcon = careTextures[(uint)pawn.playerSettings.medCare];
			Widgets.Dropdown(rect, pawn, getPayload, menuGenerator, null, buttonIcon, null, null, null, paintable: true);
		}

		private static MedicalCareCategory MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
		{
			return pawn.playerSettings.medCare;
		}

		private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> MedicalCareSelectButton_GenerateMenu(Pawn p)
		{
			int i = 0;
			if (i < 5)
			{
				MedicalCareCategory mc = (MedicalCareCategory)i;
				yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
				{
					option = new FloatMenuOption(mc.GetLabel(), delegate
					{
						p.playerSettings.medCare = mc;
					}),
					payload = mc
				};
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}
	}
}
