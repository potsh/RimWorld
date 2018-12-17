using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;
using Verse.Steam;

namespace RimWorld
{
	public class Page_ModsConfig : Page
	{
		public ModMetaData selectedMod;

		private Vector2 modListScrollPosition = Vector2.zero;

		private Vector2 modDescriptionScrollPosition = Vector2.zero;

		private int activeModsWhenOpenedHash = -1;

		private Dictionary<string, string> truncatedModNamesCache = new Dictionary<string, string>();

		protected string filter = string.Empty;

		private const float ModListAreaWidth = 350f;

		private const float ModsListButtonHeight = 30f;

		private const float ModsFolderButHeight = 30f;

		private const float ButtonsGap = 4f;

		private const float UploadRowHeight = 40f;

		private const float PreviewMaxHeight = 300f;

		private const float VersionWidth = 30f;

		private const float ModRowHeight = 26f;

		public Page_ModsConfig()
		{
			doCloseButton = true;
		}

		public override void PreOpen()
		{
			base.PreOpen();
			ModLister.RebuildModList();
			selectedMod = ModsInListOrder().FirstOrDefault();
			activeModsWhenOpenedHash = ModLister.InstalledModsListHash(activeOnly: true);
		}

		private IEnumerable<ModMetaData> ModsInListOrder()
		{
			using (IEnumerator<ModMetaData> enumerator = ModsConfig.ActiveModsInLoadOrder.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					ModMetaData mod2 = enumerator.Current;
					yield return mod2;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			using (IEnumerator<ModMetaData> enumerator2 = (from x in ModLister.AllInstalledMods
			where !x.Active
			select x into m
			orderby m.VersionCompatible descending
			select m).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					ModMetaData mod = enumerator2.Current;
					yield return mod;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0182:
			/*Error near IL_0183: Unexpected return in MoveNext()*/;
		}

		public override void DoWindowContents(Rect rect)
		{
			Rect mainRect = GetMainRect(rect, 0f, ignoreTitle: true);
			GUI.BeginGroup(mainRect);
			Text.Font = GameFont.Small;
			float num = 0f;
			Rect rect2 = new Rect(17f, num, 316f, 30f);
			if (Widgets.ButtonText(rect2, "OpenSteamWorkshop".Translate()))
			{
				SteamUtility.OpenSteamWorkshopPage();
			}
			num += 30f;
			Rect rect3 = new Rect(17f, num, 316f, 30f);
			if (Widgets.ButtonText(rect3, "GetModsFromForum".Translate()))
			{
				Application.OpenURL("http://rimworldgame.com/getmods");
			}
			num += 30f;
			num += 17f;
			filter = Widgets.TextField(new Rect(0f, num, 350f, 30f), filter);
			num += 30f;
			num += 10f;
			Rect rect4 = new Rect(0f, num, 350f, mainRect.height - num);
			Widgets.DrawMenuSection(rect4);
			float height = (float)ModLister.AllInstalledMods.Count() * 26f + 8f;
			Rect rect5 = new Rect(0f, 0f, rect4.width - 16f, height);
			Widgets.BeginScrollView(rect4, ref modListScrollPosition, rect5);
			Rect rect6 = rect5.ContractedBy(4f);
			Listing_Standard listing_Standard = new Listing_Standard();
			listing_Standard.ColumnWidth = rect6.width;
			listing_Standard.Begin(rect6);
			int reorderableGroup = ReorderableWidget.NewGroup(delegate(int from, int to)
			{
				ModsConfig.Reorder(from, to);
			}, ReorderableDirection.Vertical);
			int num2 = 0;
			foreach (ModMetaData item in ModsInListOrder())
			{
				DoModRow(listing_Standard, item, num2, reorderableGroup);
				num2++;
			}
			int downloadingItemsCount = WorkshopItems.DownloadingItemsCount;
			for (int i = 0; i < downloadingItemsCount; i++)
			{
				DoModRowDownloading(listing_Standard, num2);
				num2++;
			}
			listing_Standard.End();
			Widgets.EndScrollView();
			Rect position = new Rect(rect4.xMax + 17f, 0f, mainRect.width - rect4.width - 17f, mainRect.height);
			GUI.BeginGroup(position);
			if (selectedMod != null)
			{
				Text.Font = GameFont.Medium;
				Rect rect7 = new Rect(0f, 0f, position.width, 40f);
				Text.Anchor = TextAnchor.UpperCenter;
				Widgets.Label(rect7, selectedMod.Name.Truncate(rect7.width));
				Text.Anchor = TextAnchor.UpperLeft;
				if (!selectedMod.IsCoreMod)
				{
					Rect rect8 = rect7;
					Text.Font = GameFont.Tiny;
					Text.Anchor = TextAnchor.LowerRight;
					if (!selectedMod.VersionCompatible)
					{
						GUI.color = Color.red;
					}
					Widgets.Label(rect8, "ModTargetVersion".Translate(selectedMod.TargetVersion));
					GUI.color = Color.white;
					Text.Anchor = TextAnchor.UpperLeft;
					Text.Font = GameFont.Small;
				}
				Rect position2 = new Rect(0f, rect7.yMax, 0f, 20f);
				if (selectedMod.previewImage != null)
				{
					position2.width = Mathf.Min((float)selectedMod.previewImage.width, position.width);
					position2.height = (float)selectedMod.previewImage.height * (position2.width / (float)selectedMod.previewImage.width);
					if (position2.height > 300f)
					{
						position2.width *= 300f / position2.height;
						position2.height = 300f;
					}
					position2.x = position.width / 2f - position2.width / 2f;
					GUI.DrawTexture(position2, selectedMod.previewImage, ScaleMode.ScaleToFit);
				}
				Text.Font = GameFont.Small;
				float num3 = position2.yMax + 10f;
				if (!selectedMod.Author.NullOrEmpty())
				{
					Rect rect9 = new Rect(0f, num3, position.width / 2f, 25f);
					Widgets.Label(rect9, "Author".Translate() + ": " + selectedMod.Author);
				}
				if (!selectedMod.Url.NullOrEmpty())
				{
					float a = position.width / 2f;
					Vector2 vector = Text.CalcSize(selectedMod.Url);
					float num4 = Mathf.Min(a, vector.x);
					Rect rect10 = new Rect(position.width - num4, num3, num4, 25f);
					Text.WordWrap = false;
					if (Widgets.ButtonText(rect10, selectedMod.Url, drawBackground: false))
					{
						Application.OpenURL(selectedMod.Url);
					}
					Text.WordWrap = true;
				}
				WidgetRow widgetRow = new WidgetRow(position.width, num3 + 25f, UIDirection.LeftThenUp);
				if (SteamManager.Initialized && selectedMod.OnSteamWorkshop)
				{
					if (widgetRow.ButtonText("Unsubscribe"))
					{
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmUnsubscribe".Translate(selectedMod.Name), delegate
						{
							selectedMod.enabled = false;
							Workshop.Unsubscribe(selectedMod);
							Notify_SteamItemUnsubscribed(selectedMod.GetPublishedFileId());
						}, destructive: true));
					}
					if (widgetRow.ButtonText("WorkshopPage".Translate()))
					{
						SteamUtility.OpenWorkshopPage(selectedMod.GetPublishedFileId());
					}
				}
				float num5 = num3 + 25f + 24f;
				Rect outRect = new Rect(0f, num5, position.width, position.height - num5 - 40f);
				float width = outRect.width - 16f;
				Rect rect11 = new Rect(0f, 0f, width, Text.CalcHeight(selectedMod.Description, width));
				Widgets.BeginScrollView(outRect, ref modDescriptionScrollPosition, rect11);
				Widgets.Label(rect11, selectedMod.Description);
				Widgets.EndScrollView();
				if (Prefs.DevMode && SteamManager.Initialized && selectedMod.CanToUploadToWorkshop())
				{
					Rect rect12 = new Rect(0f, position.yMax - 40f, 200f, 40f);
					if (Widgets.ButtonText(rect12, Workshop.UploadButtonLabel(selectedMod.GetPublishedFileId())))
					{
						if (!VersionControl.IsWellFormattedVersionString(selectedMod.TargetVersion))
						{
							Messages.Message("MessageModNeedsWellFormattedTargetVersion".Translate(VersionControl.CurrentVersionString), MessageTypeDefOf.RejectInput, historical: false);
						}
						else
						{
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmSteamWorkshopUpload".Translate(), delegate
							{
								SoundDefOf.Tick_High.PlayOneShotOnCamera();
								Dialog_MessageBox dialog_MessageBox = Dialog_MessageBox.CreateConfirmation("ConfirmContentAuthor".Translate(), delegate
								{
									SoundDefOf.Tick_High.PlayOneShotOnCamera();
									Workshop.Upload(selectedMod);
								}, destructive: true);
								dialog_MessageBox.buttonAText = "Yes".Translate();
								dialog_MessageBox.buttonBText = "No".Translate();
								dialog_MessageBox.interactionDelay = 6f;
								Find.WindowStack.Add(dialog_MessageBox);
							}, destructive: true));
						}
					}
				}
			}
			GUI.EndGroup();
			GUI.EndGroup();
		}

		private void DoModRow(Listing_Standard listing, ModMetaData mod, int index, int reorderableGroup)
		{
			Rect rect = listing.GetRect(26f);
			if (mod.Active)
			{
				ReorderableWidget.Reorderable(reorderableGroup, rect);
			}
			Action clickAction = null;
			if (mod.Source == ContentSource.SteamWorkshop)
			{
				clickAction = delegate
				{
					SteamUtility.OpenWorkshopPage(mod.GetPublishedFileId());
				};
			}
			ContentSourceUtility.DrawContentSource(rect, mod.Source, clickAction);
			rect.xMin += 28f;
			bool selected = mod == selectedMod;
			bool checkOn = mod.Active;
			Rect rect2 = rect;
			if (mod.enabled)
			{
				string text = string.Empty;
				if (mod.Active)
				{
					text = text + "DragToReorder".Translate() + ".\n\n";
				}
				if (!mod.VersionCompatible)
				{
					GUI.color = Color.red;
					text = ((!mod.MadeForNewerVersion) ? (text + "ModNotMadeForThisVersion".Translate()) : (text + "ModNotMadeForThisVersion_Newer".Translate()));
				}
				GUI.color = FilteredColor(GUI.color, mod.Name);
				if (!text.NullOrEmpty())
				{
					TooltipHandler.TipRegion(rect2, new TipSignal(text, mod.GetHashCode() * 3311));
				}
				float num = rect2.width - 24f;
				if (mod.Active)
				{
					Rect position = new Rect(rect2.xMax - 48f + 2f, rect2.y, 24f, 24f);
					GUI.DrawTexture(position, TexButton.DragHash);
					num -= 24f;
				}
				Text.Font = GameFont.Small;
				string label = mod.Name.Truncate(num, truncatedModNamesCache);
				if (Widgets.CheckboxLabeledSelectable(rect2, label, ref selected, ref checkOn))
				{
					selectedMod = mod;
				}
				if (mod.Active && !checkOn && mod.IsCoreMod)
				{
					ModMetaData coreMod = mod;
					Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ConfirmDisableCoreMod".Translate(), delegate
					{
						coreMod.Active = false;
						truncatedModNamesCache.Clear();
					}));
				}
				else
				{
					mod.Active = checkOn;
					truncatedModNamesCache.Clear();
				}
			}
			else
			{
				GUI.color = FilteredColor(Color.gray, mod.Name);
				Widgets.Label(rect2, mod.Name);
			}
			GUI.color = Color.white;
		}

		private void DoModRowDownloading(Listing_Standard listing, int index)
		{
			Rect rect = listing.GetRect(26f);
			ContentSourceUtility.DrawContentSource(rect, ContentSource.SteamWorkshop);
			rect.xMin += 28f;
			Widgets.Label(rect, "Downloading".Translate() + GenText.MarchingEllipsis());
		}

		public void Notify_ModsListChanged()
		{
			string selModId = selectedMod.Identifier;
			selectedMod = ModLister.AllInstalledMods.FirstOrDefault((ModMetaData m) => m.Identifier == selModId);
		}

		internal void Notify_SteamItemUnsubscribed(PublishedFileId_t pfid)
		{
			if (selectedMod != null && selectedMod.Identifier == pfid.ToString())
			{
				selectedMod = null;
			}
		}

		public override void PostClose()
		{
			ModsConfig.Save();
			if (activeModsWhenOpenedHash != ModLister.InstalledModsListHash(activeOnly: true))
			{
				ModsConfig.RestartFromChangedMods();
			}
		}

		private Color FilteredColor(Color color, string label)
		{
			if (filter.NullOrEmpty())
			{
				return color;
			}
			if (label.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return color;
			}
			return color * new Color(1f, 1f, 1f, 0.3f);
		}
	}
}
