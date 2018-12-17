using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace Verse
{
	public class ModContentPack
	{
		private DirectoryInfo rootDirInt;

		public int loadOrder;

		private string nameInt;

		private ModContentHolder<AudioClip> audioClips;

		private ModContentHolder<Texture2D> textures;

		private ModContentHolder<string> strings;

		public ModAssemblyHandler assemblies;

		private List<PatchOperation> patches;

		private List<DefPackage> defPackages = new List<DefPackage>();

		private DefPackage impliedDefPackage;

		public static readonly string CoreModIdentifier = "Core";

		public string RootDir => rootDirInt.FullName;

		public string Identifier => rootDirInt.Name;

		public string Name => nameInt;

		public int OverwritePriority => (!IsCoreMod) ? 1 : 0;

		public bool IsCoreMod => rootDirInt.Name == CoreModIdentifier;

		public IEnumerable<Def> AllDefs => defPackages.SelectMany((DefPackage x) => x.defs);

		public bool LoadedAnyAssembly => assemblies.loadedAssemblies.Count > 0;

		public IEnumerable<PatchOperation> Patches
		{
			get
			{
				if (patches == null)
				{
					LoadPatches();
				}
				return patches;
			}
		}

		public ModContentPack(DirectoryInfo directory, int loadOrder, string name)
		{
			rootDirInt = directory;
			this.loadOrder = loadOrder;
			nameInt = name;
			audioClips = new ModContentHolder<AudioClip>(this);
			textures = new ModContentHolder<Texture2D>(this);
			strings = new ModContentHolder<string>(this);
			assemblies = new ModAssemblyHandler(this);
		}

		public void ClearDestroy()
		{
			audioClips.ClearDestroy();
			textures.ClearDestroy();
		}

		public ModContentHolder<T> GetContentHolder<T>() where T : class
		{
			if (typeof(T) == typeof(Texture2D))
			{
				return (ModContentHolder<T>)textures;
			}
			if (typeof(T) == typeof(AudioClip))
			{
				return (ModContentHolder<T>)audioClips;
			}
			if (typeof(T) == typeof(string))
			{
				return (ModContentHolder<T>)strings;
			}
			Log.Error("Mod lacks manager for asset type " + strings);
			return null;
		}

		public void ReloadContent()
		{
			LongEventHandler.ExecuteWhenFinished(delegate
			{
				audioClips.ReloadAll();
				textures.ReloadAll();
				strings.ReloadAll();
			});
			assemblies.ReloadAll();
		}

		public IEnumerable<LoadableXmlAsset> LoadDefs()
		{
			if (defPackages.Count != 0)
			{
				Log.ErrorOnce("LoadDefs called with already existing def packages", 39029405);
			}
			using (IEnumerator<LoadableXmlAsset> enumerator = DirectXmlLoader.XmlAssetsInModFolder(this, "Defs/").GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					LoadableXmlAsset asset = enumerator.Current;
					DefPackage defPackage = new DefPackage(asset.name, GenFilePaths.FolderPathRelativeToDefsFolder(asset.fullFolderPath, this));
					AddDefPackage(defPackage);
					asset.defPackage = defPackage;
					yield return asset;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			yield break;
			IL_0131:
			/*Error near IL_0132: Unexpected return in MoveNext()*/;
		}

		public IEnumerable<DefPackage> GetDefPackagesInFolder(string relFolder)
		{
			string path = Path.Combine(Path.Combine(RootDir, "Defs/"), relFolder);
			if (!Directory.Exists(path))
			{
				return Enumerable.Empty<DefPackage>();
			}
			string fullPath = Path.GetFullPath(path);
			return from x in defPackages
			where x.GetFullFolderPath(this).StartsWith(fullPath)
			select x;
		}

		public void AddDefPackage(DefPackage defPackage)
		{
			defPackages.Add(defPackage);
		}

		private void LoadPatches()
		{
			DeepProfiler.Start("Loading all patches");
			patches = new List<PatchOperation>();
			List<LoadableXmlAsset> list = DirectXmlLoader.XmlAssetsInModFolder(this, "Patches/").ToList();
			for (int i = 0; i < list.Count; i++)
			{
				XmlElement documentElement = list[i].xmlDoc.DocumentElement;
				if (documentElement.Name != "Patch")
				{
					Log.Error($"Unexpected document element in patch XML; got {documentElement.Name}, expected 'Patch'");
				}
				else
				{
					for (int j = 0; j < documentElement.ChildNodes.Count; j++)
					{
						XmlNode xmlNode = documentElement.ChildNodes[j];
						if (xmlNode.NodeType == XmlNodeType.Element)
						{
							if (xmlNode.Name != "Operation")
							{
								Log.Error($"Unexpected element in patch XML; got {documentElement.ChildNodes[j].Name}, expected 'Operation'");
							}
							else
							{
								PatchOperation patchOperation = DirectXmlToObject.ObjectFromXml<PatchOperation>(xmlNode, doPostLoad: false);
								patchOperation.sourceFile = list[i].FullFilePath;
								patches.Add(patchOperation);
							}
						}
					}
				}
			}
			DeepProfiler.End();
		}

		public void ClearPatchesCache()
		{
			patches = null;
		}

		public void AddImpliedDef(Def def)
		{
			if (impliedDefPackage == null)
			{
				impliedDefPackage = new DefPackage("ImpliedDefs", string.Empty);
				defPackages.Add(impliedDefPackage);
			}
			impliedDefPackage.AddDef(def);
		}

		public override string ToString()
		{
			return Identifier;
		}
	}
}
