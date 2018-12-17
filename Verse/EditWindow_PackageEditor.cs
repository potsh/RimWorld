using RimWorld;
using System.Linq;
using UnityEngine;

namespace Verse
{
	public class EditWindow_PackageEditor<TNewDef> : EditWindow where TNewDef : Def, new()
	{
		public ModContentPack curMod = LoadedModManager.RunningMods.First();

		private DefPackage curPackage;

		private Vector2 scrollPosition = default(Vector2);

		private float viewHeight;

		private string relFolder;

		private const float EditButSize = 24f;

		public override Vector2 InitialSize => new Vector2(250f, 600f);

		public override bool IsDebug => true;

		public EditWindow_PackageEditor(string relFolder)
		{
			this.relFolder = relFolder;
			onlyOneOfTypeAllowed = true;
			optionalTitle = "Package Editor: " + relFolder;
		}

		public override void DoWindowContents(Rect selectorInner)
		{
			Text.Font = GameFont.Tiny;
			float width = (selectorInner.width - 4f) / 2f;
			Rect rect = new Rect(0f, 0f, width, 24f);
			string str = curMod.ToString();
			if (Widgets.ButtonText(rect, "Editing: " + str))
			{
				Messages.Message("Mod changing not implemented - it's always Core for now.", MessageTypeDefOf.RejectInput, historical: false);
			}
			TooltipHandler.TipRegion(rect, "Change the mod being edited.");
			Rect rect2 = new Rect(rect.xMax + 4f, 0f, width, 24f);
			string label = "No package loaded";
			if (curPackage != null)
			{
				label = curPackage.fileName;
			}
			if (Widgets.ButtonText(rect2, label))
			{
				Find.WindowStack.Add(new Dialog_PackageSelector(delegate(DefPackage pack)
				{
					if (pack != curPackage)
					{
						curPackage = pack;
					}
				}, curMod, relFolder));
			}
			TooltipHandler.TipRegion(rect2, "Open a Def package for editing.");
			WidgetRow widgetRow = new WidgetRow(0f, 28f);
			if (widgetRow.ButtonIcon(TexButton.NewFile, "Create a new Def package."))
			{
				string name = DefPackage.UnusedPackageName(relFolder, curMod);
				DefPackage defPackage = new DefPackage(name, relFolder);
				curMod.AddDefPackage(defPackage);
				curPackage = defPackage;
			}
			if (curPackage != null)
			{
				if (widgetRow.ButtonIcon(TexButton.Save, "Save the current Def package."))
				{
					curPackage.SaveIn(curMod);
				}
				if (widgetRow.ButtonIcon(TexButton.RenameDev, "Rename the current Def package."))
				{
					Find.WindowStack.Add(new Dialog_RenamePackage(curPackage));
				}
			}
			float num = 56f;
			Rect rect3 = new Rect(0f, num, selectorInner.width, selectorInner.height - num);
			Rect rect4 = new Rect(0f, 0f, rect3.width - 16f, viewHeight);
			Widgets.DrawMenuSection(rect3);
			Widgets.BeginScrollView(rect3, ref scrollPosition, rect4);
			Rect rect5 = rect4.ContractedBy(4f);
			rect5.height = 9999f;
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(rect5);
			Text.Font = GameFont.Tiny;
			if (curPackage == null)
			{
				listing_Standard.Label("(no package open)");
			}
			else
			{
				if (curPackage.defs.Count == 0)
				{
					listing_Standard.Label("(package is empty)");
				}
				else
				{
					Def deletingDef2 = null;
					foreach (Def item in curPackage)
					{
						Def deletingDef;
						if (listing_Standard.SelectableDef(item.defName, selected: false, delegate
						{
							deletingDef = item;
						}))
						{
							bool flag = false;
							WindowStack windowStack = Find.WindowStack;
							for (int i = 0; i < windowStack.Count; i++)
							{
								EditWindow_DefEditor editWindow_DefEditor = windowStack[i] as EditWindow_DefEditor;
								if (editWindow_DefEditor != null && editWindow_DefEditor.def == item)
								{
									flag = true;
								}
							}
							if (!flag)
							{
								Find.WindowStack.Add(new EditWindow_DefEditor(item));
							}
						}
					}
					if (deletingDef2 != null)
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Really delete Def " + deletingDef2.defName + "?", delegate
						{
							curPackage.RemoveDef(deletingDef2);
						}, destructive: true));
					}
				}
				if (listing_Standard.ButtonImage(TexButton.Add, 24f, 24f))
				{
					Def def2 = new TNewDef();
					def2.defName = "New" + typeof(TNewDef).Name;
					curPackage.AddDef(def2);
				}
			}
			if (Event.current.type == EventType.Layout)
			{
				viewHeight = listing_Standard.CurHeight;
			}
			listing_Standard.End();
			Widgets.EndScrollView();
		}
	}
}
