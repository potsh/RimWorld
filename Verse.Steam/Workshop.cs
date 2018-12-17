using RimWorld;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Verse.Steam
{
	[HasDebugOutput]
	public static class Workshop
	{
		private static WorkshopItemHook uploadingHook;

		private static UGCUpdateHandle_t curUpdateHandle;

		private static WorkshopInteractStage curStage;

		private static Callback<RemoteStoragePublishedFileSubscribed_t> subscribedCallback;

		private static Callback<RemoteStoragePublishedFileUnsubscribed_t> unsubscribedCallback;

		private static Callback<ItemInstalled_t> installedCallback;

		private static CallResult<SubmitItemUpdateResult_t> submitResult;

		private static CallResult<CreateItemResult_t> createResult;

		private static CallResult<SteamUGCRequestUGCDetailsResult_t> requestDetailsResult;

		private static UGCQueryHandle_t detailsQueryHandle;

		private static int detailsQueryCount = -1;

		public const uint InstallInfoFolderNameMaxLength = 257u;

		[CompilerGenerated]
		private static Callback<RemoteStoragePublishedFileSubscribed_t>.DispatchDelegate _003C_003Ef__mg_0024cache0;

		[CompilerGenerated]
		private static Callback<ItemInstalled_t>.DispatchDelegate _003C_003Ef__mg_0024cache1;

		[CompilerGenerated]
		private static Callback<RemoteStoragePublishedFileUnsubscribed_t>.DispatchDelegate _003C_003Ef__mg_0024cache2;

		[CompilerGenerated]
		private static CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate _003C_003Ef__mg_0024cache3;

		[CompilerGenerated]
		private static CallResult<CreateItemResult_t>.APIDispatchDelegate _003C_003Ef__mg_0024cache4;

		[CompilerGenerated]
		private static CallResult<SteamUGCRequestUGCDetailsResult_t>.APIDispatchDelegate _003C_003Ef__mg_0024cache5;

		[CompilerGenerated]
		private static CallResult<SubmitItemUpdateResult_t>.APIDispatchDelegate _003C_003Ef__mg_0024cache6;

		public static WorkshopInteractStage CurStage => curStage;

		internal static void Init()
		{
			subscribedCallback = Callback<RemoteStoragePublishedFileSubscribed_t>.Create(OnItemSubscribed);
			installedCallback = Callback<ItemInstalled_t>.Create(OnItemInstalled);
			unsubscribedCallback = Callback<RemoteStoragePublishedFileUnsubscribed_t>.Create(OnItemUnsubscribed);
		}

		internal static void Upload(WorkshopUploadable item)
		{
			if (curStage != 0)
			{
				Messages.Message("UploadAlreadyInProgress".Translate(), MessageTypeDefOf.RejectInput, historical: false);
			}
			else
			{
				uploadingHook = item.GetWorkshopItemHook();
				if (uploadingHook.PublishedFileId != PublishedFileId_t.Invalid)
				{
					if (Prefs.LogVerbose)
					{
						Log.Message("Workshop: Starting item update for mod '" + uploadingHook.Name + "' with PublishedFileId " + uploadingHook.PublishedFileId);
					}
					curStage = WorkshopInteractStage.SubmittingItem;
					curUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), uploadingHook.PublishedFileId);
					SetWorkshopItemDataFrom(curUpdateHandle, uploadingHook, creating: false);
					SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(curUpdateHandle, "[Auto-generated text]: Update on " + DateTime.Now.ToString() + ".");
					submitResult = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
					submitResult.Set(hAPICall);
				}
				else
				{
					if (Prefs.LogVerbose)
					{
						Log.Message("Workshop: Starting item creation for mod '" + uploadingHook.Name + "'.");
					}
					curStage = WorkshopInteractStage.CreatingItem;
					SteamAPICall_t hAPICall2 = SteamUGC.CreateItem(SteamUtils.GetAppID(), EWorkshopFileType.k_EWorkshopFileTypeFirst);
					createResult = CallResult<CreateItemResult_t>.Create(OnItemCreated);
					createResult.Set(hAPICall2);
				}
				Find.WindowStack.Add(new Dialog_WorkshopOperationInProgress());
			}
		}

		internal static void Unsubscribe(WorkshopUploadable item)
		{
			SteamUGC.UnsubscribeItem(item.GetPublishedFileId());
		}

		internal static void RequestItemsDetails(PublishedFileId_t[] publishedFileIds)
		{
			if (detailsQueryCount >= 0)
			{
				Log.Error("Requested Workshop item details while a details request was already pending.");
			}
			else
			{
				detailsQueryCount = publishedFileIds.Length;
				detailsQueryHandle = SteamUGC.CreateQueryUGCDetailsRequest(publishedFileIds, (uint)detailsQueryCount);
				SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(detailsQueryHandle);
				requestDetailsResult = CallResult<SteamUGCRequestUGCDetailsResult_t>.Create(OnGotItemDetails);
				requestDetailsResult.Set(hAPICall);
			}
		}

		internal static void OnItemSubscribed(RemoteStoragePublishedFileSubscribed_t result)
		{
			if (IsOurAppId(result.m_nAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item subscribed: " + result.m_nPublishedFileId);
				}
				WorkshopItems.Notify_Subscribed(result.m_nPublishedFileId);
			}
		}

		internal static void OnItemInstalled(ItemInstalled_t result)
		{
			if (IsOurAppId(result.m_unAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item installed: " + result.m_nPublishedFileId);
				}
				WorkshopItems.Notify_Installed(result.m_nPublishedFileId);
			}
		}

		internal static void OnItemUnsubscribed(RemoteStoragePublishedFileUnsubscribed_t result)
		{
			if (IsOurAppId(result.m_nAppID))
			{
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item unsubscribed: " + result.m_nPublishedFileId);
				}
				Find.WindowStack.WindowOfType<Page_ModsConfig>()?.Notify_SteamItemUnsubscribed(result.m_nPublishedFileId);
				Find.WindowStack.WindowOfType<Page_SelectScenario>()?.Notify_SteamItemUnsubscribed(result.m_nPublishedFileId);
				WorkshopItems.Notify_Unsubscribed(result.m_nPublishedFileId);
			}
		}

		private static void OnItemCreated(CreateItemResult_t result, bool IOFailure)
		{
			if (IOFailure || result.m_eResult != EResult.k_EResultOK)
			{
				uploadingHook = null;
				Dialog_WorkshopOperationInProgress.CloseAll();
				Log.Error("Workshop: OnItemCreated failure. Result: " + result.m_eResult.GetLabel());
				Find.WindowStack.Add(new Dialog_MessageBox("WorkshopSubmissionFailed".Translate(GenText.SplitCamelCase(result.m_eResult.GetLabel()))));
			}
			else
			{
				uploadingHook.PublishedFileId = result.m_nPublishedFileId;
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item created. PublishedFileId: " + uploadingHook.PublishedFileId);
				}
				curUpdateHandle = SteamUGC.StartItemUpdate(SteamUtils.GetAppID(), uploadingHook.PublishedFileId);
				SetWorkshopItemDataFrom(curUpdateHandle, uploadingHook, creating: true);
				curStage = WorkshopInteractStage.SubmittingItem;
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Submitting item.");
				}
				SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(curUpdateHandle, "[Auto-generated text]: Initial upload.");
				submitResult = CallResult<SubmitItemUpdateResult_t>.Create(OnItemSubmitted);
				submitResult.Set(hAPICall);
				createResult = null;
			}
		}

		private static void OnItemSubmitted(SubmitItemUpdateResult_t result, bool IOFailure)
		{
			if (IOFailure || result.m_eResult != EResult.k_EResultOK)
			{
				uploadingHook = null;
				Dialog_WorkshopOperationInProgress.CloseAll();
				Log.Error("Workshop: OnItemSubmitted failure. Result: " + result.m_eResult.GetLabel());
				Find.WindowStack.Add(new Dialog_MessageBox("WorkshopSubmissionFailed".Translate(GenText.SplitCamelCase(result.m_eResult.GetLabel()))));
			}
			else
			{
				SteamUtility.OpenWorkshopPage(uploadingHook.PublishedFileId);
				Messages.Message("WorkshopUploadSucceeded".Translate(uploadingHook.Name), MessageTypeDefOf.TaskCompletion, historical: false);
				if (Prefs.LogVerbose)
				{
					Log.Message("Workshop: Item submit result: " + result.m_eResult);
				}
			}
			curStage = WorkshopInteractStage.None;
			submitResult = null;
		}

		private static void OnGotItemDetails(SteamUGCRequestUGCDetailsResult_t result, bool IOFailure)
		{
			if (IOFailure)
			{
				Log.Error("Workshop: OnGotItemDetails IOFailure.");
				detailsQueryCount = -1;
			}
			else
			{
				if (detailsQueryCount < 0)
				{
					Log.Warning("Got unexpected Steam Workshop item details response.");
				}
				string text = "Steam Workshop Item details received:";
				for (int i = 0; i < detailsQueryCount; i++)
				{
					SteamUGC.GetQueryUGCResult(detailsQueryHandle, (uint)i, out SteamUGCDetails_t pDetails);
					if (pDetails.m_eResult != EResult.k_EResultOK)
					{
						text = text + "\n  Query result: " + pDetails.m_eResult;
					}
					else
					{
						text = text + "\n  Title: " + pDetails.m_rgchTitle;
						text = text + "\n  PublishedFileId: " + pDetails.m_nPublishedFileId;
						text = text + "\n  Created: " + DateTime.FromFileTimeUtc(pDetails.m_rtimeCreated).ToString();
						text = text + "\n  Updated: " + DateTime.FromFileTimeUtc(pDetails.m_rtimeUpdated).ToString();
						text = text + "\n  Added to list: " + DateTime.FromFileTimeUtc(pDetails.m_rtimeAddedToUserList).ToString();
						text = text + "\n  File size: " + pDetails.m_nFileSize.ToStringKilobytes();
						text = text + "\n  Preview size: " + pDetails.m_nPreviewFileSize.ToStringKilobytes();
						text = text + "\n  File name: " + pDetails.m_pchFileName;
						text = text + "\n  CreatorAppID: " + pDetails.m_nCreatorAppID;
						text = text + "\n  ConsumerAppID: " + pDetails.m_nConsumerAppID;
						text = text + "\n  Visibiliy: " + pDetails.m_eVisibility;
						text = text + "\n  FileType: " + pDetails.m_eFileType;
						text = text + "\n  Owner: " + pDetails.m_ulSteamIDOwner;
					}
					text += "\n";
				}
				Log.Message(text.TrimEndNewlines());
				detailsQueryCount = -1;
			}
		}

		public static void GetUpdateStatus(out EItemUpdateStatus updateStatus, out float progPercent)
		{
			updateStatus = SteamUGC.GetItemUpdateProgress(curUpdateHandle, out ulong punBytesProcessed, out ulong punBytesTotal);
			progPercent = (float)(double)punBytesProcessed / (float)(double)punBytesTotal;
		}

		public static string UploadButtonLabel(PublishedFileId_t pfid)
		{
			return (!(pfid != PublishedFileId_t.Invalid)) ? "UploadToSteamWorkshop".Translate() : "UpdateOnSteamWorkshop".Translate();
		}

		private static void SetWorkshopItemDataFrom(UGCUpdateHandle_t updateHandle, WorkshopItemHook hook, bool creating)
		{
			hook.PrepareForWorkshopUpload();
			SteamUGC.SetItemTitle(updateHandle, hook.Name);
			if (creating)
			{
				SteamUGC.SetItemDescription(updateHandle, hook.Description);
			}
			if (!File.Exists(hook.PreviewImagePath))
			{
				Log.Warning("Missing preview file at " + hook.PreviewImagePath);
			}
			else
			{
				SteamUGC.SetItemPreview(updateHandle, hook.PreviewImagePath);
			}
			IList<string> tags = hook.Tags;
			tags.Add(VersionControl.CurrentMajor + "." + VersionControl.CurrentMinor);
			SteamUGC.SetItemTags(updateHandle, tags);
			SteamUGC.SetItemContent(updateHandle, hook.Directory.FullName);
		}

		internal static IEnumerable<PublishedFileId_t> AllSubscribedItems()
		{
			uint numSub = SteamUGC.GetNumSubscribedItems();
			PublishedFileId_t[] subbedItems = new PublishedFileId_t[numSub];
			uint count = SteamUGC.GetSubscribedItems(subbedItems, numSub);
			int i = 0;
			if (i < count)
			{
				PublishedFileId_t pfid = subbedItems[i];
				yield return pfid;
				/*Error: Unable to find new state assignment for yield return*/;
			}
		}

		[DebugOutput]
		[Category("System")]
		internal static void SteamWorkshopStatus()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("All subscribed items (" + SteamUGC.GetNumSubscribedItems() + " total):");
			List<PublishedFileId_t> list = AllSubscribedItems().ToList();
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.AppendLine("   " + ItemStatusString(list[i]));
			}
			stringBuilder.AppendLine("All installed mods:");
			foreach (ModMetaData allInstalledMod in ModLister.AllInstalledMods)
			{
				stringBuilder.AppendLine("   " + allInstalledMod.Identifier + ": " + ItemStatusString(allInstalledMod.GetPublishedFileId()));
			}
			Log.Message(stringBuilder.ToString());
			List<PublishedFileId_t> list2 = AllSubscribedItems().ToList();
			PublishedFileId_t[] array = new PublishedFileId_t[list2.Count];
			for (int j = 0; j < list2.Count; j++)
			{
				ref PublishedFileId_t reference = ref array[j];
				PublishedFileId_t publishedFileId_t = list2[j];
				reference = (PublishedFileId_t)publishedFileId_t.m_PublishedFileId;
			}
			RequestItemsDetails(array);
		}

		private static string ItemStatusString(PublishedFileId_t pfid)
		{
			if (pfid == PublishedFileId_t.Invalid)
			{
				return "[unpublished]";
			}
			string str = "[" + pfid + "] ";
			if (SteamUGC.GetItemInstallInfo(pfid, out ulong punSizeOnDisk, out string pchFolder, 257u, out uint _))
			{
				str += "\n      installed";
				str = str + "\n      folder=" + pchFolder;
				return str + "\n      sizeOnDisk=" + ((float)(double)punSizeOnDisk / 1024f).ToString("F2") + "Kb";
			}
			return str + "\n      not installed";
		}

		private static bool IsOurAppId(AppId_t appId)
		{
			if (appId != SteamUtils.GetAppID())
			{
				return false;
			}
			return true;
		}
	}
}
