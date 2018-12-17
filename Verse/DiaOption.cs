using RimWorld;
using System;
using UnityEngine;
using Verse.Sound;

namespace Verse
{
	public class DiaOption
	{
		public Window dialog;

		protected string text;

		public DiaNode link;

		public Func<DiaNode> linkLateBind;

		public bool resolveTree;

		public Action action;

		public bool disabled;

		public string disabledReason;

		public SoundDef clickSound = SoundDefOf.PageChange;

		protected readonly Color DisabledOptionColor = new Color(0.5f, 0.5f, 0.5f);

		public static DiaOption DefaultOK
		{
			get
			{
				DiaOption diaOption = new DiaOption("OK".Translate());
				diaOption.resolveTree = true;
				return diaOption;
			}
		}

		protected Dialog_NodeTree OwningDialog => (Dialog_NodeTree)dialog;

		public DiaOption()
		{
			text = "OK".Translate();
		}

		public DiaOption(string text)
		{
			this.text = text;
		}

		public DiaOption(DiaOptionMold def)
		{
			text = def.Text;
			DiaNodeMold diaNodeMold = def.RandomLinkNode();
			if (diaNodeMold != null)
			{
				link = new DiaNode(diaNodeMold);
			}
		}

		public void Disable(string newDisabledReason)
		{
			disabled = true;
			disabledReason = newDisabledReason;
		}

		public float OptOnGUI(Rect rect, bool active = true)
		{
			Color textColor = Widgets.NormalOptionColor;
			string text = this.text;
			if (disabled)
			{
				textColor = DisabledOptionColor;
				if (disabledReason != null)
				{
					text = text + " (" + disabledReason + ")";
				}
			}
			rect.height = Text.CalcHeight(text, rect.width);
			if (Widgets.ButtonText(rect, text, drawBackground: false, doMouseoverSound: false, textColor, active && !disabled))
			{
				Activate();
			}
			return rect.height;
		}

		protected void Activate()
		{
			if (clickSound != null && !resolveTree)
			{
				clickSound.PlayOneShotOnCamera();
			}
			if (resolveTree)
			{
				OwningDialog.Close();
			}
			if (action != null)
			{
				action();
			}
			if (linkLateBind != null)
			{
				OwningDialog.GotoNode(linkLateBind());
			}
			else if (link != null)
			{
				OwningDialog.GotoNode(link);
			}
		}
	}
}
