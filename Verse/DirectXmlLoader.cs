using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace Verse
{
	public static class DirectXmlLoader
	{
		public static IEnumerable<LoadableXmlAsset> XmlAssetsInModFolder(ModContentPack mod, string folderPath)
		{
			DirectoryInfo di = new DirectoryInfo(Path.Combine(mod.RootDir, folderPath));
			if (di.Exists)
			{
				FileInfo[] files = di.GetFiles("*.xml", SearchOption.AllDirectories);
				FileInfo[] array = files;
				int num = 0;
				if (num < array.Length)
				{
					FileInfo file = array[num];
					yield return new LoadableXmlAsset(file.Name, file.Directory.FullName, File.ReadAllText(file.FullName))
					{
						mod = mod
					};
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
		}

		public static IEnumerable<T> LoadXmlDataInResourcesFolder<T>(string folderPath) where T : new()
		{
			XmlInheritance.Clear();
			List<LoadableXmlAsset> assets = new List<LoadableXmlAsset>();
			object[] textObjects = Resources.LoadAll<TextAsset>(folderPath);
			object[] array = textObjects;
			for (int j = 0; j < array.Length; j++)
			{
				TextAsset textAsset = (TextAsset)array[j];
				LoadableXmlAsset loadableXmlAsset = new LoadableXmlAsset(textAsset.name, string.Empty, textAsset.text);
				XmlInheritance.TryRegisterAllFrom(loadableXmlAsset, null);
				assets.Add(loadableXmlAsset);
			}
			XmlInheritance.Resolve();
			for (int i = 0; i < assets.Count; i++)
			{
				using (IEnumerator<T> enumerator = AllGameItemsFromAsset<T>(assets[i]).GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						T item = enumerator.Current;
						yield return item;
						/*Error: Unable to find new state assignment for yield return*/;
					}
				}
			}
			XmlInheritance.Clear();
			yield break;
			IL_0195:
			/*Error near IL_0196: Unexpected return in MoveNext()*/;
		}

		public static T ItemFromXmlFile<T>(string filePath, bool resolveCrossRefs = true) where T : new()
		{
			if (resolveCrossRefs && DirectXmlCrossRefLoader.LoadingInProgress)
			{
				Log.Error("Cannot call ItemFromXmlFile with resolveCrossRefs=true while loading is already in progress.");
			}
			FileInfo fileInfo = new FileInfo(filePath);
			if (fileInfo.Exists)
			{
				try
				{
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(File.ReadAllText(fileInfo.FullName));
					T result = DirectXmlToObject.ObjectFromXml<T>(xmlDocument.DocumentElement, doPostLoad: false);
					if (resolveCrossRefs)
					{
						DirectXmlCrossRefLoader.ResolveAllWantedCrossReferences(FailMode.LogErrors);
					}
					return result;
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading file at " + filePath + ". Loading defaults instead. Exception was: " + ex.ToString());
					return new T();
				}
			}
			return new T();
		}

		public static Def DefFromNode(XmlNode node, LoadableXmlAsset loadingAsset)
		{
			if (node.NodeType != XmlNodeType.Element)
			{
				return null;
			}
			XmlAttribute xmlAttribute = node.Attributes["Abstract"];
			if (xmlAttribute != null && xmlAttribute.Value.ToLower() == "true")
			{
				return null;
			}
			Type typeInAnyAssembly = GenTypes.GetTypeInAnyAssembly(node.Name);
			if (typeInAnyAssembly == null)
			{
				return null;
			}
			if (typeof(Def).IsAssignableFrom(typeInAnyAssembly))
			{
				MethodInfo method = typeof(DirectXmlToObject).GetMethod("ObjectFromXml");
				MethodInfo methodInfo = method.MakeGenericMethod(typeInAnyAssembly);
				Def result = null;
				try
				{
					result = (Def)methodInfo.Invoke(null, new object[2]
					{
						node,
						true
					});
					return result;
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading def from file " + ((loadingAsset == null) ? "(unknown)" : loadingAsset.name) + ": " + ex);
					return result;
				}
			}
			return null;
		}

		public static IEnumerable<T> AllGameItemsFromAsset<T>(LoadableXmlAsset asset) where T : new()
		{
			if (asset.xmlDoc != null)
			{
				XmlNodeList assetNodes = asset.xmlDoc.DocumentElement.SelectNodes(typeof(T).Name);
				bool gotData = false;
				IEnumerator enumerator = assetNodes.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XmlNode node = (XmlNode)enumerator.Current;
						XmlAttribute abstractAtt = node.Attributes["Abstract"];
						if (abstractAtt == null || !(abstractAtt.Value.ToLower() == "true"))
						{
							T item;
							try
							{
								item = DirectXmlToObject.ObjectFromXml<T>(node, doPostLoad: true);
								gotData = true;
							}
							catch (Exception ex)
							{
								Log.Error("Exception loading data from file " + asset.name + ": " + ex);
								continue;
							}
							yield return item;
							/*Error: Unable to find new state assignment for yield return*/;
						}
					}
				}
				finally
				{
					IDisposable disposable;
					IDisposable disposable2 = disposable = (enumerator as IDisposable);
					if (disposable != null)
					{
						disposable2.Dispose();
					}
				}
				if (!gotData)
				{
					Log.Error("Found no usable data when trying to get " + typeof(T) + "s from file " + asset.name);
				}
			}
			yield break;
			IL_01f5:
			/*Error near IL_01f6: Unexpected return in MoveNext()*/;
		}
	}
}
