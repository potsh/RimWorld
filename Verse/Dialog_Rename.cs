using RimWorld;
using UnityEngine;

namespace Verse
{
	public abstract class Dialog_Rename : Window
	{
		protected string curName;

		private bool focusedRenameField;

		protected virtual int MaxNameLength => 28;

		public override Vector2 InitialSize => new Vector2(280f, 175f);

		public Dialog_Rename()
		{
			forcePause = true;
			doCloseX = true;
			absorbInputAroundWindow = true;
			closeOnAccept = false;
			closeOnClickedOutside = true;
		}

		protected virtual AcceptanceReport NameIsValid(string name)
		{
			if (name.Length == 0)
			{
				return false;
			}
			return true;
		}

		public override void DoWindowContents(Rect inRect)
		{
			Text.Font = GameFont.Small;
			bool flag = false;
			if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
			{
				flag = true;
				Event.current.Use();
			}
			GUI.SetNextControlName("RenameField");
			string text = Widgets.TextField(new Rect(0f, 15f, inRect.width, 35f), curName);
			if (text.Length < MaxNameLength)
			{
				curName = text;
			}
			if (!focusedRenameField)
			{
				UI.FocusControl("RenameField", this);
				focusedRenameField = true;
			}
			if (Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width - 15f - 15f, 35f), "OK") || flag)
			{
				AcceptanceReport acceptanceReport = NameIsValid(curName);
				if (!acceptanceReport.Accepted)
				{
					if (acceptanceReport.Reason.NullOrEmpty())
					{
						Messages.Message("NameIsInvalid".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
					else
					{
						Messages.Message(acceptanceReport.Reason, MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				else
				{
					SetName(curName);
					Find.WindowStack.TryRemove(this);
				}
			}
		}

		protected abstract void SetName(string name);
	}
}
