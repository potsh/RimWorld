using RimWorld;
using System.IO;
using UnityEngine;

namespace Verse
{
	public class Dialog_RenamePackage : Window
	{
		private DefPackage renamingPackage;

		private string proposedName;

		public override Vector2 InitialSize => new Vector2(400f, 200f);

		public override bool IsDebug => true;

		public Dialog_RenamePackage(DefPackage renamingPackage)
		{
			this.renamingPackage = renamingPackage;
			proposedName = renamingPackage.fileName;
			doCloseX = true;
			forcePause = true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			proposedName = Widgets.TextField(new Rect(0f, 0f, 200f, 32f), proposedName);
			if (Widgets.ButtonText(new Rect(0f, 40f, 100f, 32f), "Rename") && TrySave())
			{
				Close();
			}
		}

		private bool TrySave()
		{
			if (string.IsNullOrEmpty(proposedName) || proposedName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
			{
				Messages.Message("Invalid filename.", MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			if (Path.GetExtension(proposedName) != ".xml")
			{
				Messages.Message("Data package file names must end with .xml", MessageTypeDefOf.RejectInput, historical: false);
				return false;
			}
			renamingPackage.fileName = proposedName;
			return true;
		}
	}
}
