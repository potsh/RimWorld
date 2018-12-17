using RimWorld;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using Verse.Steam;

namespace Verse
{
	public class ModMetaData : WorkshopUploadable
	{
		private class ModMetaDataInternal
		{
			public string name = string.Empty;

			public string author = "Anonymous";

			public string url = string.Empty;

			public string targetVersion = "Unknown";

			public string description = "No description provided.";
		}

		private DirectoryInfo rootDirInt;

		private ContentSource source;

		public Texture2D previewImage;

		public bool enabled = true;

		private ModMetaDataInternal meta = new ModMetaDataInternal();

		private WorkshopItemHook workshopHookInt;

		private PublishedFileId_t publishedFileIdInt = PublishedFileId_t.Invalid;

		private const string AboutFolderName = "About";

		public string Identifier => RootDir.Name;

		public DirectoryInfo RootDir => rootDirInt;

		public bool IsCoreMod => Identifier == ModContentPack.CoreModIdentifier;

		public bool Active
		{
			get
			{
				return ModsConfig.IsActive(this);
			}
			set
			{
				ModsConfig.SetActive(this, value);
			}
		}

		public bool VersionCompatible
		{
			get
			{
				if (IsCoreMod)
				{
					return true;
				}
				if (!VersionControl.IsWellFormattedVersionString(TargetVersion))
				{
					return false;
				}
				return VersionControl.MinorFromVersionString(TargetVersion) == VersionControl.CurrentMinor && VersionControl.MajorFromVersionString(TargetVersion) == VersionControl.CurrentMajor;
			}
		}

		public bool MadeForNewerVersion
		{
			get
			{
				if (VersionCompatible)
				{
					return false;
				}
				if (!VersionControl.IsWellFormattedVersionString(TargetVersion))
				{
					return false;
				}
				int num = VersionControl.MinorFromVersionString(TargetVersion);
				int num2 = VersionControl.MajorFromVersionString(TargetVersion);
				return num2 > VersionControl.CurrentMajor || (num2 == VersionControl.CurrentMajor && num > VersionControl.CurrentMinor);
			}
		}

		public string Name
		{
			get
			{
				return meta.name;
			}
			set
			{
				meta.name = value;
			}
		}

		public string Author => meta.author;

		public string Url => meta.url;

		public string TargetVersion => meta.targetVersion;

		public string Description => meta.description;

		public string PreviewImagePath => rootDirInt.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "Preview.png";

		public ContentSource Source => source;

		public bool OnSteamWorkshop => source == ContentSource.SteamWorkshop;

		private string PublishedFileIdPath => rootDirInt.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "PublishedFileId.txt";

		public ModMetaData(string localAbsPath)
		{
			rootDirInt = new DirectoryInfo(localAbsPath);
			source = ContentSource.LocalFolder;
			Init();
		}

		public ModMetaData(WorkshopItem workshopItem)
		{
			rootDirInt = workshopItem.Directory;
			source = ContentSource.SteamWorkshop;
			Init();
		}

		private void Init()
		{
			meta = DirectXmlLoader.ItemFromXmlFile<ModMetaDataInternal>(RootDir.FullName + Path.DirectorySeparatorChar + "About" + Path.DirectorySeparatorChar + "About.xml");
			if (meta.name.NullOrEmpty())
			{
				if (OnSteamWorkshop)
				{
					meta.name = "Workshop mod " + Identifier;
				}
				else
				{
					meta.name = Identifier;
				}
			}
			if (!IsCoreMod && !OnSteamWorkshop && !VersionControl.IsWellFormattedVersionString(meta.targetVersion))
			{
				Log.ErrorOnce("Mod " + meta.name + " has incorrectly formatted target version '" + meta.targetVersion + "'. For the current version, write: <targetVersion>" + VersionControl.CurrentVersionString + "</targetVersion>", Identifier.GetHashCode());
			}
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				string url = GenFilePaths.SafeURIForUnityWWWFromPath(PreviewImagePath);
				using (WWW wWW = new WWW(url))
				{
					wWW.threadPriority = UnityEngine.ThreadPriority.High;
					while (!wWW.isDone)
					{
						Thread.Sleep(1);
					}
					if (wWW.error == null)
					{
						previewImage = wWW.textureNonReadable;
					}
				}
			});
			string publishedFileIdPath = PublishedFileIdPath;
			if (File.Exists(PublishedFileIdPath))
			{
				string s = File.ReadAllText(publishedFileIdPath);
				publishedFileIdInt = new PublishedFileId_t(ulong.Parse(s));
			}
		}

		internal void DeleteContent()
		{
			rootDirInt.Delete(recursive: true);
			ModLister.RebuildModList();
		}

		public void PrepareForWorkshopUpload()
		{
		}

		public bool CanToUploadToWorkshop()
		{
			if (IsCoreMod)
			{
				return false;
			}
			if (Source != ContentSource.LocalFolder)
			{
				return false;
			}
			if (GetWorkshopItemHook().MayHaveAuthorNotCurrentUser)
			{
				return false;
			}
			return true;
		}

		public PublishedFileId_t GetPublishedFileId()
		{
			return publishedFileIdInt;
		}

		public void SetPublishedFileId(PublishedFileId_t newPfid)
		{
			if (!(publishedFileIdInt == newPfid))
			{
				publishedFileIdInt = newPfid;
				File.WriteAllText(PublishedFileIdPath, newPfid.ToString());
			}
		}

		public string GetWorkshopName()
		{
			return Name;
		}

		public string GetWorkshopDescription()
		{
			return Description;
		}

		public string GetWorkshopPreviewImagePath()
		{
			return PreviewImagePath;
		}

		public IList<string> GetWorkshopTags()
		{
			List<string> list = new List<string>();
			list.Add("Mod");
			return list;
		}

		public DirectoryInfo GetWorkshopUploadDirectory()
		{
			return RootDir;
		}

		public WorkshopItemHook GetWorkshopItemHook()
		{
			if (workshopHookInt == null)
			{
				workshopHookInt = new WorkshopItemHook(this);
			}
			return workshopHookInt;
		}

		public override int GetHashCode()
		{
			return Identifier.GetHashCode();
		}

		public override string ToString()
		{
			return "[" + Identifier + "|" + Name + "]";
		}

		public string ToStringLong()
		{
			return Identifier + "(" + RootDir.ToString() + ")";
		}
	}
}
