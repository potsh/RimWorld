using System;
using System.IO;
using UnityEngine;

namespace Verse
{
	public class Dialog_PackageSelector : Window
	{
		private Action<DefPackage> setPackageCallback;

		private ModContentPack mod;

		private string relFolder;

		public override Vector2 InitialSize => new Vector2(1000f, 700f);

		public override bool IsDebug => true;

		public Dialog_PackageSelector(Action<DefPackage> setPackageCallback, ModContentPack mod, string relFolder)
		{
			this.setPackageCallback = setPackageCallback;
			this.mod = mod;
			this.relFolder = relFolder;
			doCloseX = true;
			onlyOneOfTypeAllowed = true;
			draggable = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.Begin(inRect.AtZero());
			listing_Standard.ColumnWidth = 240f;
			foreach (DefPackage item in mod.GetDefPackagesInFolder(relFolder))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item.fileName);
				if (listing_Standard.ButtonText(fileNameWithoutExtension))
				{
					setPackageCallback(item);
					Close();
				}
			}
			listing_Standard.End();
		}
	}
}
